#include "OcctModelingExchangeInternal.hxx"
#include "OcctExchangePath.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <StlAPI_Writer.hxx>

#include <filesystem>
#include <string>

using namespace OcctModelingInternal;

extern "C"
{
    OcctObjectId occt_model_import_step(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelStep(path);
        });
    }

    OcctObjectId occt_model_import_iges(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelIges(path);
        });
    }

    OcctObjectId occt_model_import_brep(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelBrep(path);
        });
    }

    OcctObjectId occt_model_import_stl(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelStl(path);
        });
    }

    OcctObjectId occt_model_import_file(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        model->errors.clear();
        const auto path = OcctBridge::pathFromUtf8(utf8Path);
        const std::string extension = OcctBridge::lowerExtension(path);
        if (extension == ".step" || extension == ".stp") return occt_model_import_step(handle, utf8Path);
        if (extension == ".iges" || extension == ".igs") return occt_model_import_iges(handle, utf8Path);
        if (extension == ".brep" || extension == ".rle") return occt_model_import_brep(handle, utf8Path);
        if (extension == ".stl") return occt_model_import_stl(handle, utf8Path);
        model->errors.set(OcctStatus_ErrorFormat, "Unsupported file extension. Supported: STEP, IGES, BREP and STL.");
        return 0;
    }

    int occt_model_export_step(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            writeModelStep(model->requireShape(shapeId), OcctBridge::pathFromUtf8(utf8Path));
        });
    }

    int occt_model_export_iges(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            writeModelIges(model->requireShape(shapeId), OcctBridge::pathFromUtf8(utf8Path));
        });
    }

    int occt_model_export_brep(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            auto stream = modelOutputStream(OcctBridge::pathFromUtf8(utf8Path));
            BRepTools::Write(model->requireShape(shapeId), stream);
            if (!stream) throw std::runtime_error("BREP file could not be written.");
        });
    }

    int occt_model_export_stl(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            requirePositive(linearDeflection, "Linear deflection");
            requirePositive(angularDeflection, "Angular deflection");
            const TopoDS_Shape& shape = model->requireShape(shapeId);
            BRepMesh_IncrementalMesh mesh(shape, linearDeflection, Standard_False, angularDeflection, Standard_True);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("STL meshing failed.");
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
            StlAPI_Writer writer;
            writer.ASCIIMode() = asciiMode != 0;
            if (!writer.Write(shape, path.string().c_str()))
                throw std::runtime_error("STL file could not be written. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        });
    }
}
