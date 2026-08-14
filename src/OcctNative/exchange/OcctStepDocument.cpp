#include "OcctStepDocument.h"
#include "OcctInternal.hxx"

#include <Quantity_ColorRGBA.hxx>
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDataStd_Name.hxx>
#include <TopExp.hxx>
#include <TopLoc_Location.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFPrs.hxx>
#include <XCAFPrs_DocumentExplorer.hxx>
#include <XCAFPrs_IndexedDataMapOfShapeStyle.hxx>

#include <iomanip>
#include <sstream>

using namespace OcctBridge;

namespace
{
    struct StepStyleSnapshot
    {
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
    };

    struct StepSubshapeStyleSnapshot
    {
        int shapeType = OcctShape_Shape;
        int subshapeIndex = -1;
        StepStyleSnapshot style;
    };

    struct StepNodeSnapshot
    {
        std::string id;
        int parent = -1;
        int kind = 2;
        std::string name;
        std::string referenceName;
        OcctObjectId objectId = 0;
        StepStyleSnapshot style;
        gp_Trsf localTransform;
        gp_Trsf globalTransform;
        std::vector<StepSubshapeStyleSnapshot> subshapeStyles;
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

    StepStyleSnapshot captureStyle(const XCAFPrs_Style& style)
    {
        StepStyleSnapshot result;
        result.visible = style.IsVisible();
        if (style.IsSetColorSurf())
        {
            const Quantity_ColorRGBA& rgba = style.GetColorSurfRGBA();
            result.hasSurfaceColor = true;
            result.surfaceR = rgba.GetRGB().Red();
            result.surfaceG = rgba.GetRGB().Green();
            result.surfaceB = rgba.GetRGB().Blue();
            result.surfaceA = rgba.Alpha();
        }
        if (style.IsSetColorCurv())
        {
            const Quantity_Color& rgb = style.GetColorCurv();
            result.hasCurveColor = true;
            result.curveR = rgb.Red();
            result.curveG = rgb.Green();
            result.curveB = rgb.Blue();
        }
        return result;
    }

    void rememberResolvedStyle(ObjectEntry& entry, const XCAFPrs_Style& style)
    {
        if (!entry.hasStoredVisibility)
        {
            entry.storedVisible = style.IsVisible();
            entry.hasStoredVisibility = true;
        }
        if (style.IsSetColorSurf())
        {
            const Quantity_ColorRGBA& rgba = style.GetColorSurfRGBA();
            if (!entry.hasStoredColor)
            {
                entry.hasStoredColor = true;
                entry.storedColorR = rgba.GetRGB().Red();
                entry.storedColorG = rgba.GetRGB().Green();
                entry.storedColorB = rgba.GetRGB().Blue();
            }
            if (!entry.hasStoredAlpha)
            {
                entry.storedColorA = rgba.Alpha();
                entry.hasStoredAlpha = true;
            }
        }
        else if (style.IsSetColorCurv() && !entry.hasStoredColor)
        {
            const Quantity_Color& rgb = style.GetColorCurv();
            entry.hasStoredColor = true;
            entry.storedColorR = rgb.Red();
            entry.storedColorG = rgb.Green();
            entry.storedColorB = rgb.Blue();
        }
    }

    std::vector<StepSubshapeStyleSnapshot> captureSubshapeStyles(const XCAFPrs_DocumentNode& node)
    {
        // Prefer the occurrence/component label so instance-specific styles are
        // represented. Direct Part nodes naturally fall back to their own label.
        const TDF_Label sourceLabel = node.Label.IsNull() ? node.RefLabel : node.Label;
        if (sourceLabel.IsNull()) return {};

        const TopoDS_Shape rootShape = XCAFDoc_ShapeTool::GetShape(sourceLabel);
        if (rootShape.IsNull()) return {};

        XCAFPrs_IndexedDataMapOfShapeStyle settings;
        XCAFPrs::CollectStyleSettings(sourceLabel, TopLoc_Location(), settings);
        if (settings.IsEmpty()) return {};

        TopTools_IndexedMapOfShape subShapes;
        TopExp::MapShapes(rootShape, subShapes);
        std::vector<StepSubshapeStyleSnapshot> result;
        result.reserve(static_cast<std::size_t>(settings.Size()));

        for (XCAFPrs_DataMapIteratorOfIndexedDataMapOfShapeStyle iterator(settings);
             iterator.More();
             iterator.Next())
        {
            const TopoDS_Shape& styledShape = iterator.Key();
            if (styledShape.IsNull() || styledShape.IsSame(rootShape)) continue;
            const Standard_Integer mappedIndex = subShapes.FindIndex(styledShape);
            if (mappedIndex <= 0) continue;

            StepSubshapeStyleSnapshot snapshot;
            snapshot.shapeType = shapeTypeValue(styledShape);
            snapshot.subshapeIndex = mappedIndex - 1;
            snapshot.style = captureStyle(iterator.Value());
            result.push_back(std::move(snapshot));
        }
        return result;
    }

    OcctObjectId objectIdOfEntry(Engine* engine, const ObjectEntry& entry)
    {
        if (engine == nullptr) return 0;
        for (const auto& pair : engine->scene.objects)
        {
            if (&pair.second == &entry) return pair.first;
        }
        return 0;
    }

    TDF_Label findStepLabel(Engine* engine, ObjectEntry& entry)
    {
        if (engine == nullptr) return TDF_Label();

        if (entry.stepDocumentIndex >= 0 && !entry.stepNodeId.empty())
        {
            const std::size_t documentIndex = static_cast<std::size_t>(entry.stepDocumentIndex);
            if (documentIndex < engine->documents.stepDocuments.size())
            {
                const Handle(TDocStd_Document)& document = engine->documents.stepDocuments[documentIndex];
                if (!document.IsNull())
                {
                    TopLoc_Location location;
                    const TDF_Label label = XCAFPrs_DocumentExplorer::FindLabelFromPathId(
                        document,
                        TCollection_AsciiString(entry.stepNodeId.c_str()),
                        location);
                    if (!label.IsNull()) return label;
                }
            }
        }

        if (engine->documents.stepDocuments.empty() || engine->documents.lastStepImportObjectIds.empty()) return TDF_Label();
        const OcctObjectId objectId = objectIdOfEntry(engine, entry);
        if (objectId == 0) return TDF_Label();

        std::size_t targetLeaf = engine->documents.lastStepImportObjectIds.size();
        for (std::size_t index = 0; index < engine->documents.lastStepImportObjectIds.size(); ++index)
        {
            if (engine->documents.lastStepImportObjectIds[index] == objectId)
            {
                targetLeaf = index;
                break;
            }
        }
        if (targetLeaf >= engine->documents.lastStepImportObjectIds.size()) return TDF_Label();

        const std::size_t documentIndex = engine->documents.stepDocuments.size() - 1U;
        const Handle(TDocStd_Document)& document = engine->documents.stepDocuments[documentIndex];
        if (document.IsNull()) return TDF_Label();

        std::size_t leafIndex = 0;
        XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            if (node.IsAssembly) continue;
            if (leafIndex++ != targetLeaf) continue;

            entry.stepDocumentIndex = static_cast<int>(documentIndex);
            entry.stepNodeId = node.Id.ToCString();
            rememberResolvedStyle(entry, node.Style);
            return node.Label;
        }
        return TDF_Label();
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

    void appendStyle(std::ostringstream& stream, const StepStyleSnapshot& style)
    {
        stream << "\"visible\":" << (style.visible ? "true" : "false") << ','
               << "\"surfaceColor\":";
        if (style.hasSurfaceColor)
        {
            stream << '[' << style.surfaceR << ',' << style.surfaceG << ','
                   << style.surfaceB << ',' << style.surfaceA << ']';
        }
        else
        {
            stream << "null";
        }
        stream << ",\"curveColor\":";
        if (style.hasCurveColor)
        {
            stream << '[' << style.curveR << ',' << style.curveG << ',' << style.curveB << ']';
        }
        else
        {
            stream << "null";
        }
    }

    std::string buildLastStepDocumentJson(Engine* engine)
    {
        if (engine->documents.stepDocuments.empty() || engine->documents.stepDocuments.back().IsNull())
            throw std::runtime_error("No imported STEP/XDE document is available.");

        const Handle(TDocStd_Document)& document = engine->documents.stepDocuments.back();
        std::vector<StepNodeSnapshot> nodes;
        std::vector<int> nodeAtDepth;
        std::size_t leafIndex = 0;

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
            snapshot.style = captureStyle(node.Style);
            snapshot.localTransform = node.LocalTrsf.Transformation();
            snapshot.globalTransform = node.Location.Transformation();

            if (!node.IsAssembly)
            {
                if (leafIndex >= engine->documents.lastStepImportObjectIds.size())
                    throw std::runtime_error("STEP/XDE leaf-to-object mapping is incomplete.");
                snapshot.objectId = engine->documents.lastStepImportObjectIds[leafIndex++];
                snapshot.subshapeStyles = captureSubshapeStyles(node);
            }

            const int nodeIndex = static_cast<int>(nodes.size());
            nodes.push_back(std::move(snapshot));
            nodeAtDepth[static_cast<std::size_t>(depth)] = nodeIndex;
            nodeAtDepth.resize(static_cast<std::size_t>(depth) + 1U);
        }

        if (leafIndex != engine->documents.lastStepImportObjectIds.size())
            throw std::runtime_error("STEP/XDE leaf-to-object mapping does not match the imported document.");

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
                   << "\"objectId\":" << node.objectId << ',';
            appendStyle(stream, node.style);
            stream << ",\"localTransform\":";
            appendTransform(stream, node.localTransform);
            stream << ",\"globalTransform\":";
            appendTransform(stream, node.globalTransform);
            stream << ",\"subshapeStyles\":[";
            for (std::size_t styleIndex = 0; styleIndex < node.subshapeStyles.size(); ++styleIndex)
            {
                if (styleIndex != 0U) stream << ',';
                const StepSubshapeStyleSnapshot& subshape = node.subshapeStyles[styleIndex];
                stream << '{'
                       << "\"shapeType\":" << subshape.shapeType << ','
                       << "\"subshapeIndex\":" << subshape.subshapeIndex << ',';
                appendStyle(stream, subshape.style);
                stream << '}';
            }
            stream << "]}";
        }
        stream << "]}";
        return stream.str();
    }
}

namespace OcctBridge
{
    bool syncStepObjectName(Engine* engine, ObjectEntry& entry)
    {
        const TDF_Label label = findStepLabel(engine, entry);
        if (label.IsNull()) return false;
        TDataStd_Name::Set(label, TCollection_ExtendedString(entry.name.c_str(), true));
        return true;
    }

    bool syncStepObjectColor(Engine* engine, ObjectEntry& entry)
    {
        const TDF_Label label = findStepLabel(engine, entry);
        if (label.IsNull()) return false;

        if (!entry.hasStoredColor)
        {
            Quantity_Color current;
            if (!entry.presentation.IsNull()) engine->viewerContext.context->Color(entry.presentation, current);
            entry.hasStoredColor = true;
            entry.storedColorR = current.Red();
            entry.storedColorG = current.Green();
            entry.storedColorB = current.Blue();
        }
        if (!entry.hasStoredAlpha)
        {
            entry.storedColorA = 1.0;
            entry.hasStoredAlpha = true;
        }

        const std::size_t documentIndex = static_cast<std::size_t>(entry.stepDocumentIndex);
        const Handle(TDocStd_Document)& document = engine->documents.stepDocuments[documentIndex];
        const Handle(XCAFDoc_ColorTool) colorTool = XCAFDoc_DocumentTool::ColorTool(document->Main());
        if (colorTool.IsNull()) return false;

        const Quantity_Color rgb = color(entry.storedColorR, entry.storedColorG, entry.storedColorB);
        const Quantity_ColorRGBA rgba(rgb, static_cast<float>(std::clamp(entry.storedColorA, 0.0, 1.0)));
        colorTool->SetColor(label, rgba, XCAFDoc_ColorGen);
        colorTool->SetColor(label, rgba, XCAFDoc_ColorSurf);
        return true;
    }

    bool syncStepObjectVisibility(Engine* engine, ObjectEntry& entry)
    {
        const bool requestedVisibility = entry.storedVisible;
        const TDF_Label label = findStepLabel(engine, entry);
        if (label.IsNull()) return false;
        entry.storedVisible = requestedVisibility;
        entry.hasStoredVisibility = true;
        const std::size_t documentIndex = static_cast<std::size_t>(entry.stepDocumentIndex);
        const Handle(TDocStd_Document)& document = engine->documents.stepDocuments[documentIndex];
        const Handle(XCAFDoc_ColorTool) colorTool = XCAFDoc_DocumentTool::ColorTool(document->Main());
        if (colorTool.IsNull()) return false;
        colorTool->SetVisibility(label, requestedVisibility ? Standard_True : Standard_False);
        return true;
    }
}

extern "C" OCCTBRIDGE_API const char* occt_get_last_step_document_json(OcctHandle h)
{
    Engine* engine = engineOf(h);
    if (engine == nullptr) return "";
    engine->clearError();
    try
    {
        engine->errors.scratch = buildLastStepDocumentJson(engine);
    }
    catch (const Standard_Failure& failure)
    {
        engine->setError(OcctStatus_ErrorOcct, failureMessage(failure));
        engine->errors.scratch.clear();
    }
    catch (const std::exception& exception)
    {
        engine->setError(exception.what());
        engine->errors.scratch.clear();
    }
    catch (...)
    {
        engine->setError("Unknown native error while reading the STEP assembly document.");
        engine->errors.scratch.clear();
    }
    return engine->errors.scratch.c_str();
}
