#include "exchange/OcctObjGltfExchange.h"
#include "exchange/OcctModelingExchangeInternal.hxx"
#include "exchange/OcctExchangePath.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRep_Builder.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <TopoDS_Compound.hxx>
#include <RWObj_CafReader.hxx>
#include <RWObj_CafWriter.hxx>
#include <RWGltf_CafReader.hxx>
#include <RWGltf_CafWriter.hxx>
#include <RWGltf_WriterTrsfFormat.hxx>
#include <RWMesh_CoordinateSystemConverter.hxx>
#include <RWMesh_CoordinateSystem.hxx>
#include <XCAFApp_Application.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFPrs_DocumentExplorer.hxx>
#include <TDocStd_Document.hxx>
#include <TDF_Label.hxx>
#include <StlAPI_Writer.hxx>
#include <Message_ProgressRange.hxx>
#include <TColStd_IndexedDataMapOfStringString.hxx>

#include <cmath>
#include <filesystem>
#include <stdexcept>
#include <string>
#include <utility>

using OcctModelingInternal::ModelSession;
using OcctModelingInternal::executeStatus;
using OcctModelingInternal::sessionOf;

namespace
{
    constexpr std::uint32_t GltfExportOptionsApiVersion = 1;

    std::filesystem::path requiredPath(const char* utf8Path)
    {
        const auto path = OcctBridge::pathFromUtf8(utf8Path);
        if (path.empty()) throw std::invalid_argument("Path is empty.");
        return path;
    }

    template<typename Factory>
    OcctStatus importShape(ModelSession* model, OcctObjectId* output, Factory&& factory)
    {
        return executeStatus(model, [&]
        {
            if (output == nullptr)
                throw std::invalid_argument("Result shape ID output is null.");
            *output = 0;
            *output = model->addShape(factory());
        });
    }

    template<typename Action>
    OcctStatus exportShape(ModelSession* model, Action&& action)
    {
        return executeStatus(model, std::forward<Action>(action));
    }

    void validateGltfOptions(const OcctGltfExportOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("glTF export options are null.");
        if (options->structSize < sizeof(OcctGltfExportOptions) ||
            options->apiVersion != GltfExportOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported glTF export options size or version.");
        }
    }

    TopoDS_Shape readDocToCompound(const Handle(TDocStd_Document)& doc)
    {
        Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(doc->Main());
        XCAFPrs_DocumentExplorer explorer(doc, XCAFPrs_DocumentExplorerFlags_OnlyLeafNodes);
        
        BRep_Builder builder;
        TopoDS_Compound compound;
        builder.MakeCompound(compound);
        int count = 0;
        TopoDS_Shape singleShape;
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            TopoDS_Shape shape = shapeTool->GetShape(node.RefLabel.IsNull() ? node.Label : node.RefLabel);
            if (!shape.IsNull())
            {
                if (!node.Location.IsIdentity())
                {
                    shape.Move(node.Location);
                }
                builder.Add(compound, shape);
                singleShape = shape;
                count++;
            }
        }
        if (count == 0) throw std::runtime_error("Document contains no valid shapes.");
        if (count == 1)
        {
            return singleShape;
        }
        return compound;
    }
}

extern "C"
{
    OcctStatus occt_model_obj_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = sessionOf(session);
        return importShape(model, resultShapeId, [&]
        {
            Handle(TDocStd_Document) doc;
            Handle(XCAFApp_Application) app = XCAFApp_Application::GetApplication();
            app->NewDocument("MDTV-XCAF", doc);

            RWObj_CafReader reader;
            reader.SetDocument(doc);
            if (!reader.Perform(requiredPath(utf8Path).string().c_str(), Message_ProgressRange()))
                throw std::runtime_error("OBJ file could not be read.");
            return readDocToCompound(doc);
        });
    }

    OcctStatus occt_model_obj_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path)
    {
        ModelSession* model = sessionOf(session);
        return exportShape(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(shapeId);
            const auto path = requiredPath(utf8Path);
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());

            Handle(TDocStd_Document) doc;
            Handle(XCAFApp_Application) app = XCAFApp_Application::GetApplication();
            app->NewDocument("MDTV-XCAF", doc);
            Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(doc->Main());
            shapeTool->AddShape(shape);

            RWObj_CafWriter writer(path.string().c_str());
            if (!writer.Perform(doc, TColStd_IndexedDataMapOfStringString(), Message_ProgressRange()))
                throw std::runtime_error("OBJ file could not be written.");
        });
    }

    OcctStatus occt_model_gltf_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = sessionOf(session);
        return importShape(model, resultShapeId, [&]
        {
            Handle(TDocStd_Document) doc;
            Handle(XCAFApp_Application) app = XCAFApp_Application::GetApplication();
            app->NewDocument("MDTV-XCAF", doc);

            RWGltf_CafReader reader;
            reader.SetDocument(doc);
            if (!reader.Perform(requiredPath(utf8Path).string().c_str(), Message_ProgressRange()))
                throw std::runtime_error("glTF file could not be read.");
            return readDocToCompound(doc);
        });
    }

    OcctStatus occt_model_gltf_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path,
        const OcctGltfExportOptions* options)
    {
        ModelSession* model = sessionOf(session);
        return exportShape(model, [&]
        {
            validateGltfOptions(options);
            const TopoDS_Shape& shape = model->requireShape(shapeId);

            double deflection = options->deflection <= 0.0 ? 0.01 : options->deflection;
            BRepMesh_IncrementalMesh mesh(shape, deflection, Standard_False, 0.5, Standard_True);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("glTF meshing failed.");

            const auto path = requiredPath(utf8Path);
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());

            Handle(TDocStd_Document) doc;
            Handle(XCAFApp_Application) app = XCAFApp_Application::GetApplication();
            app->NewDocument("MDTV-XCAF", doc);
            Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(doc->Main());
            shapeTool->AddShape(shape);

            TColStd_IndexedDataMapOfStringString fileInfo;
            RWGltf_CafWriter writer(path.string().c_str(), options->writeBinary != 0);
            writer.SetTransformationFormat(RWGltf_WriterTrsfFormat_Compact);
            if (options->transformToGltfCs != 0)
            {
                RWMesh_CoordinateSystemConverter conv;
                conv.SetInputCoordinateSystem(RWMesh_CoordinateSystem_posYfwd_posZup);
                conv.SetOutputCoordinateSystem(RWMesh_CoordinateSystem_negZfwd_posYup);
                writer.SetCoordinateSystemConverter(conv);
            }
            if (!writer.Perform(doc, fileInfo, Message_ProgressRange()))
                throw std::runtime_error("glTF file could not be written.");
        });
    }

    OcctStatus occt_model_stl_export_multiple(
        OcctModelingSessionHandle session,
        const OcctObjectId* shapeIds,
        int count,
        const char* utf8Path,
        const OcctStlExportOptions* options)
    {
        ModelSession* model = sessionOf(session);
        return exportShape(model, [&]
        {
            if (options == nullptr) throw std::invalid_argument("STL export options are null.");
            if (shapeIds == nullptr && count > 0) throw std::invalid_argument("Shape IDs array is null.");
            
            BRep_Builder builder;
            TopoDS_Compound compound;
            builder.MakeCompound(compound);
            for (int i = 0; i < count; ++i)
            {
                builder.Add(compound, model->requireShape(shapeIds[i]));
            }

            BRepMesh_IncrementalMesh mesh(
                compound,
                options->linearDeflection,
                Standard_False,
                options->angularDeflection,
                Standard_True);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("STL meshing failed.");

            const auto path = requiredPath(utf8Path);
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
            StlAPI_Writer writer;
            writer.ASCIIMode() = options->ascii != 0;
            if (!writer.Write(compound, path.string().c_str()))
            {
                throw std::runtime_error(
                    "STL file could not be written.");
            }
        });
    }
}
