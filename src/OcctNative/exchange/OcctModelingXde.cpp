#include "exchange/OcctModelingXde.h"
#include "exchange/OcctExchangePath.hxx"
#include "exchange/OcctModelingExchangeInternal.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRep_Builder.hxx>
#include <IGESCAFControl_Reader.hxx>
#include <IGESCAFControl_Writer.hxx>
#include <Message_ProgressRange.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <RWGltf_CafReader.hxx>
#include <RWObj_CafReader.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDataStd_Name.hxx>
#include <TDF_LabelSequence.hxx>
#include <TopExp.hxx>
#include <TopoDS_Compound.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <XCAFApp_Application.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFPrs.hxx>
#include <XCAFPrs_DocumentExplorer.hxx>
#include <XCAFPrs_IndexedDataMapOfShapeStyle.hxx>

#include <algorithm>
#include <cstring>
#include <filesystem>
#include <iomanip>
#include <limits>
#include <memory>
#include <sstream>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

using namespace OcctModelingInternal;

struct OcctXdeDocumentHandle_t
{
    Handle(TDocStd_Document) document;
    std::vector<OcctObjectId> leafShapeIds;
    std::string sourceFormat;
};

namespace
{
    std::filesystem::path requiredPath(const char* utf8Path)
    {
        const auto path = OcctBridge::pathFromUtf8(utf8Path);
        if (path.empty()) throw std::invalid_argument("Path is empty.");
        return path;
    }

    OcctXdeDocumentHandle_t& requireDocument(OcctXdeDocumentHandle handle)
    {
        if (handle == nullptr || handle->document.IsNull())
            throw std::invalid_argument("XDE document handle is invalid.");
        return *handle;
    }

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

    void appendColor(std::ostringstream& stream, const Quantity_ColorRGBA& color)
    {
        stream << '['
               << color.GetRGB().Red() << ','
               << color.GetRGB().Green() << ','
               << color.GetRGB().Blue() << ','
               << color.Alpha() << ']';
    }

    void appendStyleJson(std::ostringstream& stream, const XCAFPrs_Style& style)
    {
        stream << "\"visible\":" << (style.IsVisible() ? "true" : "false")
               << ",\"surfaceColor\":";
        if (style.IsSetColorSurf())
            appendColor(stream, style.GetColorSurfRGBA());
        else
            stream << "null";

        stream << ",\"curveColor\":";
        if (style.IsSetColorCurv())
        {
            const Quantity_Color& color = style.GetColorCurv();
            stream << '[' << color.Red() << ',' << color.Green() << ',' << color.Blue() << ",1]";
        }
        else
        {
            stream << "null";
        }
    }

    void appendSubshapeStyles(
        std::ostringstream& stream,
        const XCAFPrs_DocumentNode& node)
    {
        const TDF_Label sourceLabel = node.Label.IsNull() ? node.RefLabel : node.Label;
        if (sourceLabel.IsNull())
        {
            stream << "[]";
            return;
        }

        const TopoDS_Shape rootShape = XCAFDoc_ShapeTool::GetShape(sourceLabel);
        if (rootShape.IsNull())
        {
            stream << "[]";
            return;
        }

        XCAFPrs_IndexedDataMapOfShapeStyle settings;
        XCAFPrs::CollectStyleSettings(sourceLabel, TopLoc_Location(), settings);
        if (settings.IsEmpty())
        {
            stream << "[]";
            return;
        }

        TopTools_IndexedMapOfShape subShapes;
        TopExp::MapShapes(rootShape, subShapes);

        stream << '[';
        bool firstStyle = true;
        for (XCAFPrs_DataMapIteratorOfIndexedDataMapOfShapeStyle iterator(settings);
             iterator.More();
             iterator.Next())
        {
            const TopoDS_Shape& styledShape = iterator.Key();
            if (styledShape.IsNull() || styledShape.IsSame(rootShape)) continue;
            const Standard_Integer mappedIndex = subShapes.FindIndex(styledShape);
            if (mappedIndex <= 0) continue;

            if (!firstStyle) stream << ',';
            firstStyle = false;
            stream << "{\"shapeType\":" << toOcctShapeType(styledShape.ShapeType())
                   << ",\"subshapeIndex\":" << (mappedIndex - 1) << ',';
            appendStyleJson(stream, iterator.Value());
            stream << '}';
        }
        stream << ']';
    }

    std::vector<std::string> nodeLayers(
        const Handle(TDocStd_Document)& document,
        const XCAFPrs_DocumentNode& node)
    {
        const Handle(XCAFDoc_LayerTool) layerTool =
            XCAFDoc_DocumentTool::LayerTool(document->Main());
        if (layerTool.IsNull()) return {};

        std::vector<std::string> result;
        const auto appendFromLabel = [&](const TDF_Label& label)
        {
            if (label.IsNull()) return;
            TDF_LabelSequence labels;
            layerTool->GetLayers(label, labels);
            for (Standard_Integer index = 1; index <= labels.Length(); ++index)
            {
                const std::string name = labelName(labels.Value(index));
                if (name.empty()) continue;
                if (std::find(result.begin(), result.end(), name) == result.end())
                    result.push_back(name);
            }
        };
        appendFromLabel(node.Label);
        if (node.RefLabel != node.Label) appendFromLabel(node.RefLabel);
        return result;
    }

    TopoDS_Shape nodeShape(
        const Handle(XCAFDoc_ShapeTool)& shapeTool,
        const XCAFPrs_DocumentNode& node)
    {
        const TDF_Label source = node.RefLabel.IsNull() ? node.Label : node.RefLabel;
        TopoDS_Shape shape = shapeTool->GetShape(source);
        if (!shape.IsNull() && !node.Location.IsIdentity())
            shape = shape.Moved(node.Location);
        return shape;
    }

    std::unique_ptr<OcctXdeDocumentHandle_t> retainDocument(
        ModelSession* model,
        const Handle(TDocStd_Document)& document,
        const std::string& sourceFormat,
        OcctObjectId* primaryShapeId)
    {
        if (document.IsNull()) throw std::runtime_error("XDE document is null.");
        if (primaryShapeId == nullptr) throw std::invalid_argument("Primary shape output is null.");

        const Handle(XCAFDoc_ShapeTool) shapeTool =
            XCAFDoc_DocumentTool::ShapeTool(document->Main());
        if (shapeTool.IsNull()) throw std::runtime_error("XDE shape tool is unavailable.");

        auto resource = std::make_unique<OcctXdeDocumentHandle_t>();
        resource->document = document;
        resource->sourceFormat = sourceFormat;

        BRep_Builder builder;
        TopoDS_Compound compound;
        builder.MakeCompound(compound);
        int leafCount = 0;
        XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            if (node.IsAssembly) continue;

            const TopoDS_Shape shape = nodeShape(shapeTool, node);
            if (shape.IsNull()) continue;

            const OcctObjectId leafId = model->addShape(shape);
            resource->leafShapeIds.push_back(leafId);
            builder.Add(compound, shape);
            ++leafCount;
        }

        if (leafCount <= 0)
            throw std::runtime_error("XDE document contains no transferable leaf shapes.");

        *primaryShapeId = leafCount == 1
            ? resource->leafShapeIds.front()
            : model->addShape(compound);
        return resource;
    }

    std::string buildDocumentJson(const OcctXdeDocumentHandle_t& resource)
    {
        const Handle(TDocStd_Document)& document = resource.document;
        std::ostringstream stream;
        stream << std::setprecision(17);
        stream << "{\"format\":\"" << jsonEscape(resource.sourceFormat) << "\",\"nodes\":[";

        const Handle(XCAFDoc_ShapeTool) shapeTool =
            XCAFDoc_DocumentTool::ShapeTool(document->Main());
        if (shapeTool.IsNull())
            throw std::logic_error("XDE shape tool is unavailable.");

        std::vector<int> nodeAtDepth;
        std::size_t leafIndex = 0;
        int nextNodeIndex = 0;
        bool firstNode = true;
        XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            const int depth = explorer.CurrentDepth();
            if (depth < 0) continue;
            if (nodeAtDepth.size() <= static_cast<std::size_t>(depth))
                nodeAtDepth.resize(static_cast<std::size_t>(depth) + 1U, -1);

            const int parent = depth == 0 ? -1 : nodeAtDepth[static_cast<std::size_t>(depth - 1)];
            const int nodeIndex = nextNodeIndex++;
            nodeAtDepth[static_cast<std::size_t>(depth)] = nodeIndex;
            for (std::size_t i = static_cast<std::size_t>(depth) + 1U; i < nodeAtDepth.size(); ++i)
                nodeAtDepth[i] = -1;

            const int kind = node.IsAssembly
                ? 0
                : (XCAFDoc_ShapeTool::IsComponent(node.Label) ? 1 : 2);

            OcctObjectId shapeId = 0;
            if (!node.IsAssembly)
            {
                const TopoDS_Shape shape = nodeShape(shapeTool, node);
                if (!shape.IsNull())
                {
                    if (leafIndex >= resource.leafShapeIds.size())
                        throw std::logic_error("XDE leaf-to-shape mapping is incomplete.");
                    shapeId = resource.leafShapeIds[leafIndex++];
                }
            }

            if (!firstNode) stream << ',';
            firstNode = false;
            stream << "{\"id\":\"" << jsonEscape(node.Id.ToCString()) << "\""
                   << ",\"parent\":" << parent
                   << ",\"kind\":" << kind
                   << ",\"name\":\"" << jsonEscape(nodeName(node)) << "\""
                   << ",\"referenceName\":\"" << jsonEscape(labelName(node.RefLabel)) << "\""
                   << ",\"shapeId\":" << shapeId << ',';
            appendStyleJson(stream, node.Style);
            stream << ",\"subshapeStyles\":";
            appendSubshapeStyles(stream, node);
            stream << ",\"layers\":[";
            const std::vector<std::string> layers = nodeLayers(document, node);
            for (std::size_t index = 0; index < layers.size(); ++index)
            {
                if (index != 0) stream << ',';
                stream << "\"" << jsonEscape(layers[index]) << "\"";
            }
            stream << "],\"localTransform\":";
            appendTransform(stream, node.LocalTrsf.Transformation());
            stream << ",\"globalTransform\":";
            appendTransform(stream, node.Location.Transformation());
            stream << '}';
        }

        if (leafIndex != resource.leafShapeIds.size())
            throw std::logic_error("XDE leaf-to-shape mapping contains stale shapes.");

        stream << "]}";
        return stream.str();
    }

    OcctStatus copyUtf8(
        const std::string& value,
        char* buffer,
        int capacity,
        int* required)
    {
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;
        if (value.size() >= static_cast<std::size_t>(std::numeric_limits<int>::max()))
            return OcctStatus_ErrorOutOfMemory;

        const int size = static_cast<int>(value.size()) + 1;
        *required = size;
        if (buffer == nullptr)
            return capacity == 0 ? OcctStatus_Ok : OcctStatus_ErrorInvalidArgument;
        if (capacity < size) return OcctStatus_ErrorBufferTooSmall;
        std::memcpy(buffer, value.c_str(), static_cast<std::size_t>(size));
        return OcctStatus_Ok;
    }

    template<typename ReaderAction>
    OcctStatus importDocument(
        ModelSession* model,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* documentHandle,
        const std::string& sourceFormat,
        ReaderAction&& action)
    {
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (primaryShapeId == nullptr || documentHandle == nullptr)
            return OcctStatus_ErrorInvalidArgument;

        *primaryShapeId = 0;
        *documentHandle = nullptr;
        return executeStatus(model, [&]
        {
            Handle(TDocStd_Document) document;
            XCAFApp_Application::GetApplication()->NewDocument("MDTV-XCAF", document);
            action(document);
            auto resource = retainDocument(model, document, sourceFormat, primaryShapeId);
            *documentHandle = resource.release();
        });
    }
}

extern "C"
{
    OcctStatus occt_model_step_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document)
    {
        ModelSession* model = sessionOf(session);
        const std::filesystem::path path = requiredPath(utf8Path);
        return importDocument(model, primaryShapeId, document, "step", [&](const Handle(TDocStd_Document)& xde)
        {
            STEPCAFControl_Reader reader;
            reader.SetColorMode(Standard_True);
            reader.SetNameMode(Standard_True);
            reader.SetLayerMode(Standard_True);
            auto stream = modelInputStream(path);
            if (reader.ReadStream(path.filename().string().c_str(), stream) != IFSelect_RetDone)
                throw std::runtime_error("STEP/XDE file could not be read.");
            if (!reader.Transfer(xde))
                throw std::runtime_error("STEP/XDE document could not be transferred.");
        });
    }

    OcctStatus occt_model_iges_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document)
    {
        ModelSession* model = sessionOf(session);
        const std::filesystem::path path = requiredPath(utf8Path);
        return importDocument(model, primaryShapeId, document, "iges", [&](const Handle(TDocStd_Document)& xde)
        {
            IGESCAFControl_Reader reader;
            reader.SetColorMode(Standard_True);
            reader.SetNameMode(Standard_True);
            reader.SetLayerMode(Standard_True);
            if (!reader.Perform(path.string().c_str(), xde))
                throw std::runtime_error("IGES/XDE document could not be read.");
        });
    }

    OcctStatus occt_model_obj_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document)
    {
        ModelSession* model = sessionOf(session);
        const std::filesystem::path path = requiredPath(utf8Path);
        return importDocument(model, primaryShapeId, document, "obj", [&](const Handle(TDocStd_Document)& xde)
        {
            RWObj_CafReader reader;
            reader.SetDocument(xde);
            if (!reader.Perform(path.string().c_str(), Message_ProgressRange()))
                throw std::runtime_error("OBJ/XDE document could not be read.");
        });
    }

    OcctStatus occt_model_gltf_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document)
    {
        ModelSession* model = sessionOf(session);
        const std::filesystem::path path = requiredPath(utf8Path);
        return importDocument(model, primaryShapeId, document, "gltf", [&](const Handle(TDocStd_Document)& xde)
        {
            RWGltf_CafReader reader;
            reader.SetDocument(xde);
            if (!reader.Perform(path.string().c_str(), Message_ProgressRange()))
                throw std::runtime_error("glTF/XDE document could not be read.");
        });
    }

    void occt_model_xde_document_release(OcctXdeDocumentHandle document)
    {
        delete document;
    }

    OcctStatus occt_model_step_document_export(
        OcctModelingSessionHandle session,
        OcctXdeDocumentHandle document,
        const char* utf8Path)
    {
        ModelSession* model = sessionOf(session);
        return executeStatus(model, [&]
        {
            OcctXdeDocumentHandle_t& resource = requireDocument(document);
            STEPCAFControl_Writer writer;
            writer.SetColorMode(Standard_True);
            writer.SetNameMode(Standard_True);
            writer.SetLayerMode(Standard_True);
            if (!writer.Transfer(resource.document, STEPControl_AsIs))
                throw std::runtime_error("XDE document could not be transferred to STEP.");
            auto stream = modelOutputStream(requiredPath(utf8Path));
            if (writer.WriteStream(stream) != IFSelect_RetDone)
                throw std::runtime_error("STEP/XDE document could not be written.");
        });
    }

    OcctStatus occt_model_iges_document_export(
        OcctModelingSessionHandle session,
        OcctXdeDocumentHandle document,
        const char* utf8Path)
    {
        ModelSession* model = sessionOf(session);
        return executeStatus(model, [&]
        {
            OcctXdeDocumentHandle_t& resource = requireDocument(document);
            const std::filesystem::path path = requiredPath(utf8Path);
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());

            IGESCAFControl_Writer writer;
            writer.SetColorMode(Standard_True);
            writer.SetNameMode(Standard_True);
            writer.SetLayerMode(Standard_True);
            if (!writer.Transfer(resource.document))
                throw std::runtime_error("XDE document could not be transferred to IGES.");
            writer.ComputeModel();
            if (!writer.Write(path.string().c_str()))
                throw std::runtime_error("IGES/XDE document could not be written.");
        });
    }

    OcctStatus occt_model_xde_document_json_get(
        OcctModelingSessionHandle session,
        OcctXdeDocumentHandle document,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes)
    {
        ModelSession* model = sessionOf(session);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;

        std::string json;
        const OcctStatus status = executeStatus(model, [&]
        {
            json = buildDocumentJson(requireDocument(document));
        });
        if (status != OcctStatus_Ok) return status;
        return copyUtf8(json, utf8Buffer, capacity, requiredBytes);
    }
}
