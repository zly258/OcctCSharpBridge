#include "OcctInternal.hxx"

#include <Quantity_ColorRGBA.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDataStd_Name.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFPrs_DocumentExplorer.hxx>

#include <algorithm>
#include <iomanip>
#include <sstream>

using namespace OcctBridge;

namespace
{
    struct StepNodeSnapshot
    {
        std::string id;
        int parent = -1;
        int kind = 2;
        std::string name;
        std::string referenceName;
        OcctObjectId objectId = 0;
        bool visible = true;
        bool hasSurfaceColor = false;
        double surfaceR = 0.0;
        double surfaceG = 0.0;
        double surfaceB = 0.0;
        double surfaceA = 1.0;
        bool hasCurveColor = false;
        double curveR = 0.0;
        double curveG = 0.0;
        double curveB = 0.0;
        gp_Trsf localTransform;
        gp_Trsf globalTransform;
    };

    std::string extendedStringToUtf8(const TCollection_ExtendedString& value)
    {
        const Standard_Integer byteCount = value.LengthOfCString();
        if (byteCount <= 0) return {};
        std::vector<char> buffer(static_cast<std::size_t>(byteCount) + 1U, '\0');
        Standard_PCharacter destination = buffer.data();
        value.ToUTF8CString(destination);
        return std::string(buffer.data());
    }

    std::string labelName(const TDF_Label& label)
    {
        if (label.IsNull()) return {};
        Handle(TDataStd_Name) attribute;
        if (!label.FindAttribute(TDataStd_Name::GetID(), attribute) || attribute.IsNull()) return {};
        return extendedStringToUtf8(attribute->Get());
    }

    std::string nodeName(const XCAFPrs_DocumentNode& node)
    {
        std::string result = labelName(node.Label);
        if (result.empty()) result = labelName(node.RefLabel);
        if (result.empty()) result = node.IsAssembly ? "Assembly" : "Part";
        return result;
    }

    std::string jsonEscape(const std::string& value)
    {
        std::ostringstream stream;
        for (const unsigned char ch : value)
        {
            switch (ch)
            {
                case '"': stream << "\\\""; break;
                case '\\': stream << "\\\\"; break;
                case '\b': stream << "\\b"; break;
                case '\f': stream << "\\f"; break;
                case '\n': stream << "\\n"; break;
                case '\r': stream << "\\r"; break;
                case '\t': stream << "\\t"; break;
                default:
                    if (ch < 0x20U)
                    {
                        stream << "\\u"
                               << std::hex << std::setw(4) << std::setfill('0')
                               << static_cast<int>(ch)
                               << std::dec << std::setfill(' ');
                    }
                    else
                    {
                        stream << static_cast<char>(ch);
                    }
                    break;
            }
        }
        return stream.str();
    }

    void appendTransform(std::ostringstream& stream, const gp_Trsf& transform)
    {
        stream << '[';
        for (int row = 1; row <= 3; ++row)
        {
            for (int column = 1; column <= 4; ++column)
            {
                if (row != 1 || column != 1) stream << ',';
                stream << transform.Value(row, column);
            }
        }
        stream << ']';
    }

    std::vector<OcctObjectId> latestImportedStepObjectIds(const Engine* engine, std::size_t leafCount)
    {
        std::vector<OcctObjectId> candidates;
        candidates.reserve(engine->objects.size());
        for (const auto& pair : engine->objects)
        {
            if (pair.second.kind == OcctObject_Shape && !pair.second.stepHierarchyPath.empty())
                candidates.push_back(pair.first);
        }
        std::sort(candidates.begin(), candidates.end());
        if (candidates.size() > leafCount)
            candidates.erase(candidates.begin(), candidates.end() - static_cast<std::ptrdiff_t>(leafCount));
        return candidates;
    }

    std::string buildLastStepDocumentJson(Engine* engine)
    {
        if (engine->stepDocuments.empty() || engine->stepDocuments.back().IsNull())
            throw std::runtime_error("No imported STEP/XDE document is available.");

        const Handle(TDocStd_Document)& document = engine->stepDocuments.back();
        std::vector<StepNodeSnapshot> nodes;
        std::vector<int> nodeAtDepth;
        std::size_t leafCount = 0;

        XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            const int depth = explorer.CurrentDepth();
            if (depth < 0) continue;
            if (nodeAtDepth.size() <= static_cast<std::size_t>(depth))
                nodeAtDepth.resize(static_cast<std::size_t>(depth) + 1U, -1);

            StepNodeSnapshot snapshot;
            snapshot.id = node.Id.ToCString();
            snapshot.parent = depth == 0 ? -1 : nodeAtDepth[static_cast<std::size_t>(depth - 1)];
            snapshot.kind = node.IsAssembly
                ? 0
                : (XCAFDoc_ShapeTool::IsComponent(node.Label) ? 1 : 2);
            snapshot.name = nodeName(node);
            snapshot.referenceName = labelName(node.RefLabel);
            snapshot.visible = node.Style.IsVisible();
            snapshot.localTransform = node.LocalTrsf.Transformation();
            snapshot.globalTransform = node.Location.Transformation();

            if (node.Style.IsSetColorSurf())
            {
                const Quantity_ColorRGBA& rgba = node.Style.GetColorSurfRGBA();
                snapshot.hasSurfaceColor = true;
                snapshot.surfaceR = rgba.GetRGB().Red();
                snapshot.surfaceG = rgba.GetRGB().Green();
                snapshot.surfaceB = rgba.GetRGB().Blue();
                snapshot.surfaceA = rgba.Alpha();
            }
            if (node.Style.IsSetColorCurv())
            {
                const Quantity_Color& rgb = node.Style.GetColorCurv();
                snapshot.hasCurveColor = true;
                snapshot.curveR = rgb.Red();
                snapshot.curveG = rgb.Green();
                snapshot.curveB = rgb.Blue();
            }
            if (!node.IsAssembly) ++leafCount;

            const int nodeIndex = static_cast<int>(nodes.size());
            nodes.push_back(std::move(snapshot));
            nodeAtDepth[static_cast<std::size_t>(depth)] = nodeIndex;
            nodeAtDepth.resize(static_cast<std::size_t>(depth) + 1U);
        }

        const std::vector<OcctObjectId> leafObjectIds = latestImportedStepObjectIds(engine, leafCount);
        std::size_t leafIndex = 0;
        for (StepNodeSnapshot& node : nodes)
        {
            if (node.kind == 0) continue;
            if (leafIndex < leafObjectIds.size()) node.objectId = leafObjectIds[leafIndex];
            ++leafIndex;
        }

        std::ostringstream stream;
        stream << std::setprecision(17);
        stream << "{\"nodes\":[";
        for (std::size_t index = 0; index < nodes.size(); ++index)
        {
            if (index != 0U) stream << ',';
            const StepNodeSnapshot& node = nodes[index];
            stream << '{'
                   << "\"id\":\"" << jsonEscape(node.id) << "\","
                   << "\"parent\":" << node.parent << ','
                   << "\"kind\":" << node.kind << ','
                   << "\"name\":\"" << jsonEscape(node.name) << "\","
                   << "\"referenceName\":\"" << jsonEscape(node.referenceName) << "\","
                   << "\"objectId\":" << node.objectId << ','
                   << "\"visible\":" << (node.visible ? "true" : "false") << ','
                   << "\"surfaceColor\":";
            if (node.hasSurfaceColor)
            {
                stream << '[' << node.surfaceR << ',' << node.surfaceG << ','
                       << node.surfaceB << ',' << node.surfaceA << ']';
            }
            else
            {
                stream << "null";
            }
            stream << ",\"curveColor\":";
            if (node.hasCurveColor)
            {
                stream << '[' << node.curveR << ',' << node.curveG << ',' << node.curveB << ']';
            }
            else
            {
                stream << "null";
            }
            stream << ",\"localTransform\":";
            appendTransform(stream, node.localTransform);
            stream << ",\"globalTransform\":";
            appendTransform(stream, node.globalTransform);
            stream << '}';
        }
        stream << "]}";
        return stream.str();
    }
}

extern "C" OCCTBRIDGE_API const char* occt_get_last_step_document_json(OcctHandle h)
{
    Engine* engine = engineOf(h);
    if (engine == nullptr) return "";
    engine->clearError();
    try
    {
        engine->scratchString = buildLastStepDocumentJson(engine);
    }
    catch (const Standard_Failure& failure)
    {
        engine->setError(failureMessage(failure));
        engine->scratchString.clear();
    }
    catch (const std::exception& exception)
    {
        engine->setError(exception.what());
        engine->scratchString.clear();
    }
    catch (...)
    {
        engine->setError("Unknown native error while reading the STEP assembly document.");
        engine->scratchString.clear();
    }
    return engine->scratchString.c_str();
}
