#include "OcctStepDocument.h"
#include "core/OcctInternal.hxx"

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

#include <algorithm>
#include <cstring>
#include <iomanip>
#include <limits>
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

        if (engine->documents.stepDocuments.empty() || engine->documents.lastStepImportObjectIds.empty())
            return TDF_Label();

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

    TDF_Label findLastStepNode(Engine* engine, const char* nodeId)
    {
        if (engine == nullptr || nodeId == nullptr || nodeId[0] == '\0')
            throw std::invalid_argument("STEP node ID is empty.");
        if (engine->documents.stepDocuments.empty() || engine->documents.stepDocuments.back().IsNull())
            throw std::logic_error("No imported STEP/XDE document is available.");

        TopLoc_Location location;
        const TDF_Label label = XCAFPrs_DocumentExplorer::FindLabelFromPathId(
            engine->documents.stepDocuments.back(),
            TCollection_AsciiString(nodeId),
            location);
        if (label.IsNull()) throw std::invalid_argument("STEP node ID does not exist.");
        return label;
    }

    gp_Trsf stepTransform(const OcctStepTransform3d& value)
    {
        gp_Trsf result;
        result.SetValues(
            value.m00, value.m01, value.m02, value.m03,
            value.m10, value.m11, value.m12, value.m13,
            value.m20, value.m21, value.m22, value.m23);
        return result;
    }

    std::unordered_map<std::string, OcctObjectId> stepLeafObjects(Engine* engine)
    {
        std::unordered_map<std::string, OcctObjectId> result;
        const Handle(TDocStd_Document)& document = engine->documents.stepDocuments.back();
        std::size_t leafIndex = 0;
        XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            if (node.IsAssembly) continue;
            if (leafIndex >= engine->documents.lastStepImportObjectIds.size())
                throw std::logic_error("STEP leaf-to-viewer mapping is incomplete.");
            result.emplace(node.Id.ToCString(), engine->documents.lastStepImportObjectIds[leafIndex++]);
        }
        if (leafIndex != engine->documents.lastStepImportObjectIds.size())
            throw std::logic_error("STEP leaf-to-viewer mapping contains stale objects.");
        return result;
    }

    void rebuildStepLeafObjects(
        Engine* engine,
        const std::unordered_map<std::string, OcctObjectId>& objectsByPath)
    {
        std::vector<OcctObjectId> result;
        const Handle(TDocStd_Document)& document = engine->documents.stepDocuments.back();
        XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            if (node.IsAssembly) continue;
            const auto iterator = objectsByPath.find(node.Id.ToCString());
            if (iterator == objectsByPath.end())
                throw std::logic_error("STEP leaf has no matching Viewer object.");
            result.push_back(iterator->second);
            ObjectEntry* entry = engine->findShape(iterator->second);
            if (entry != nullptr)
            {
                entry->stepDocumentIndex = static_cast<int>(engine->documents.stepDocuments.size() - 1U);
                entry->stepNodeId = node.Id.ToCString();
            }
        }
        engine->documents.lastStepImportObjectIds = std::move(result);
    }

    TDF_Label referredDefinition(const Handle(XCAFDoc_ShapeTool)& shapeTool, const TDF_Label& label)
    {
        TDF_Label definition;
        if (XCAFDoc_ShapeTool::IsReference(label) && shapeTool->GetReferredShape(label, definition))
            return definition;
        return label;
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

extern "C"
{
    OcctStatus occt_engine_step_node_name_set(
        OcctEngineHandle handle,
        const char* nodeId,
        const char* utf8Name)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (!validateInitialized(engine)) return engine == nullptr
            ? OcctStatus_ErrorInvalidHandle
            : engine->currentErrorCode();
        if (utf8Name == nullptr) return OcctStatus_ErrorInvalidArgument;

        return execute(engine, [&]
        {
            const TDF_Label label = findLastStepNode(engine, nodeId);
            TDataStd_Name::Set(label, TCollection_ExtendedString(utf8Name, true));
        }) != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    OcctStatus occt_engine_step_node_visibility_set(
        OcctEngineHandle handle,
        const char* nodeId,
        OcctBool visible)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (!validateInitialized(engine)) return engine == nullptr
            ? OcctStatus_ErrorInvalidHandle
            : engine->currentErrorCode();

        return execute(engine, [&]
        {
            const TDF_Label label = findLastStepNode(engine, nodeId);
            const Handle(TDocStd_Document)& document = engine->documents.stepDocuments.back();
            const Handle(XCAFDoc_ColorTool) colorTool =
                XCAFDoc_DocumentTool::ColorTool(document->Main());
            if (colorTool.IsNull()) throw std::logic_error("XDE color tool is unavailable.");
            colorTool->SetVisibility(label, visible != 0 ? Standard_True : Standard_False);
        }) != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    OcctStatus occt_engine_step_node_transform_set(
        OcctEngineHandle handle,
        const char* nodeId,
        const OcctStepTransform3d* transform)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (!validateInitialized(engine)) return engine == nullptr
            ? OcctStatus_ErrorInvalidHandle
            : engine->currentErrorCode();
        if (transform == nullptr) return OcctStatus_ErrorInvalidArgument;

        return execute(engine, [&]
        {
            const TDF_Label label = findLastStepNode(engine, nodeId);
            if (!XCAFDoc_ShapeTool::IsComponent(label))
                throw std::invalid_argument("Only STEP component occurrences have editable transforms.");

            const Handle(TDocStd_Document)& document = engine->documents.stepDocuments.back();
            const Handle(XCAFDoc_ShapeTool) shapeTool =
                XCAFDoc_DocumentTool::ShapeTool(document->Main());
            if (shapeTool.IsNull()) throw std::logic_error("XDE shape tool is unavailable.");
            shapeTool->SetLocation(label, TopLoc_Location(stepTransform(*transform)));
        }) != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    OcctStatus occt_engine_step_component_add(
        OcctEngineHandle handle,
        const char* parentNodeId,
        const char* referenceNodeId,
        const OcctStepTransform3d* transform,
        OcctObjectId* viewerObjectId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (!validateInitialized(engine)) return engine == nullptr
            ? OcctStatus_ErrorInvalidHandle
            : engine->currentErrorCode();
        if (transform == nullptr || viewerObjectId == nullptr) return OcctStatus_ErrorInvalidArgument;
        *viewerObjectId = 0;

        return execute(engine, [&]
        {
            auto objectsByPath = stepLeafObjects(engine);
            const Handle(TDocStd_Document)& document = engine->documents.stepDocuments.back();
            const Handle(XCAFDoc_ShapeTool) shapeTool =
                XCAFDoc_DocumentTool::ShapeTool(document->Main());
            if (shapeTool.IsNull()) throw std::logic_error("XDE shape tool is unavailable.");

            const TDF_Label parent = referredDefinition(shapeTool, findLastStepNode(engine, parentNodeId));
            const TDF_Label reference = referredDefinition(shapeTool, findLastStepNode(engine, referenceNodeId));
            if (!XCAFDoc_ShapeTool::IsAssembly(parent))
                throw std::invalid_argument("Parent STEP node is not an assembly definition.");

            const gp_Trsf location = stepTransform(*transform);
            const TDF_Label component = shapeTool->AddComponent(parent, reference, TopLoc_Location(location));
            if (component.IsNull()) throw std::runtime_error("XDE component creation failed.");

            OcctObjectId objectId = 0;
            try
            {
                const TopoDS_Shape shape = XCAFDoc_ShapeTool::GetShape(reference);
                objectId = engine->addShape(shape, false, labelName(reference));
                ObjectEntry* entry = engine->findShape(objectId);
                if (entry == nullptr) throw std::logic_error("Created Viewer shape is unavailable.");
                entry->presentation->SetLocalTransformation(location);

                std::string createdPath;
                XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
                for (; explorer.More(); explorer.Next())
                {
                    const XCAFPrs_DocumentNode& node = explorer.Current();
                    if (node.Label == component)
                    {
                        createdPath = node.Id.ToCString();
                        break;
                    }
                }
                if (createdPath.empty()) throw std::logic_error("Created XDE component path was not found.");
                objectsByPath.emplace(createdPath, objectId);
                rebuildStepLeafObjects(engine, objectsByPath);
                engine->documents.pristineStepDocumentMatchesScene = true;
                *viewerObjectId = objectId;
            }
            catch (...)
            {
                if (objectId != 0) engine->erase(objectId);
                shapeTool->RemoveComponent(component);
                throw;
            }
        }) != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    OcctStatus occt_engine_step_component_remove(
        OcctEngineHandle handle,
        const char* componentNodeId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (!validateInitialized(engine)) return engine == nullptr
            ? OcctStatus_ErrorInvalidHandle
            : engine->currentErrorCode();

        return execute(engine, [&]
        {
            auto objectsByPath = stepLeafObjects(engine);
            const auto objectIterator = objectsByPath.find(componentNodeId == nullptr ? "" : componentNodeId);
            if (objectIterator == objectsByPath.end())
                throw std::invalid_argument("STEP component does not map to a Viewer leaf object.");

            const Handle(TDocStd_Document)& document = engine->documents.stepDocuments.back();
            const Handle(XCAFDoc_ShapeTool) shapeTool =
                XCAFDoc_DocumentTool::ShapeTool(document->Main());
            if (shapeTool.IsNull()) throw std::logic_error("XDE shape tool is unavailable.");
            const TDF_Label component = findLastStepNode(engine, componentNodeId);
            if (!XCAFDoc_ShapeTool::IsComponent(component))
                throw std::invalid_argument("STEP node is not a removable component occurrence.");

            shapeTool->RemoveComponent(component);
            const OcctObjectId objectId = objectIterator->second;
            objectsByPath.erase(objectIterator);
            engine->erase(objectId);
            rebuildStepLeafObjects(engine, objectsByPath);
            engine->documents.pristineStepDocument = document;
            engine->documents.pristineStepDocumentMatchesScene = true;
        }) != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    OcctStatus occt_engine_step_document_json_get(
        OcctEngineHandle handle,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || requiredBytes == nullptr) return OcctStatus_ErrorInvalidArgument;

        *requiredBytes = 0;
        engine->clearError();
        try
        {
            const std::string json = buildLastStepDocumentJson(engine);
            if (json.size() >= static_cast<std::size_t>(std::numeric_limits<int>::max()))
            {
                engine->setError(OcctStatus_ErrorOutOfMemory, "STEP assembly snapshot exceeds the ABI buffer size limit.");
                return OcctStatus_ErrorOutOfMemory;
            }

            const int required = static_cast<int>(json.size()) + 1;
            *requiredBytes = required;
            if (utf8Buffer == nullptr)
                return capacity == 0 ? OcctStatus_Ok : OcctStatus_ErrorInvalidArgument;
            if (capacity < required) return OcctStatus_ErrorBufferTooSmall;

            std::memcpy(utf8Buffer, json.c_str(), static_cast<std::size_t>(required));
            return OcctStatus_Ok;
        }
        catch (const Standard_Failure& failure)
        {
            engine->setError(OcctStatus_ErrorOcct, failureMessage(failure));
            return OcctStatus_ErrorOcct;
        }
        catch (const std::invalid_argument& exception)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, exception.what());
            return OcctStatus_ErrorInvalidArgument;
        }
        catch (const std::logic_error& exception)
        {
            engine->setError(OcctStatus_ErrorInvalidState, exception.what());
            return OcctStatus_ErrorInvalidState;
        }
        catch (const std::bad_alloc&)
        {
            engine->setError(OcctStatus_ErrorOutOfMemory, "Native memory allocation failed while building STEP assembly snapshot.");
            return OcctStatus_ErrorOutOfMemory;
        }
        catch (const std::exception& exception)
        {
            engine->setError(OcctStatus_ErrorUnknown, exception.what());
            return OcctStatus_ErrorUnknown;
        }
        catch (...)
        {
            engine->setError(OcctStatus_ErrorUnknown, "Unknown native error while reading the STEP assembly document.");
            return OcctStatus_ErrorUnknown;
        }
    }
}
