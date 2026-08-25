#include "exchange/OcctModelingExchange.h"
#include "exchange/OcctModelingExchangeInternal.hxx"
#include "exchange/OcctExchangePath.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <StlAPI_Writer.hxx>

#include <cmath>
#include <filesystem>
#include <stdexcept>
#include <string>
#include <utility>

using OcctModelingInternal::ModelSession;
using OcctModelingInternal::executeStatus;
using OcctModelingInternal::modelOutputStream;
using OcctModelingInternal::readModelBrep;
using OcctModelingInternal::readModelIges;
using OcctModelingInternal::readModelStep;
using OcctModelingInternal::readModelStl;
using OcctModelingInternal::sessionOf;
using OcctModelingInternal::writeModelIges;
using OcctModelingInternal::writeModelStep;

namespace
{
    constexpr std::uint32_t StlExportOptionsApiVersion = 1;

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

    void validateStlOptions(const OcctStlExportOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("STL export options are null.");
        if (options->structSize < sizeof(OcctStlExportOptions) ||
            options->apiVersion != StlExportOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported STL export options size or version.");
        }
        if (!std::isfinite(options->linearDeflection) || options->linearDeflection <= 0.0)
            throw std::invalid_argument("Linear deflection must be finite and greater than zero.");
        if (!std::isfinite(options->angularDeflection) || options->angularDeflection <= 0.0)
            throw std::invalid_argument("Angular deflection must be finite and greater than zero.");
    }
}

extern "C"
{
    OcctStatus occt_model_step_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = sessionOf(session);
        return importShape(model, resultShapeId, [&]
        {
            return readModelStep(requiredPath(utf8Path));
        });
    }

    OcctStatus occt_model_iges_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = sessionOf(session);
        return importShape(model, resultShapeId, [&]
        {
            return readModelIges(requiredPath(utf8Path));
        });
    }

    OcctStatus occt_model_brep_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = sessionOf(session);
        return importShape(model, resultShapeId, [&]
        {
            return readModelBrep(requiredPath(utf8Path));
        });
    }

    OcctStatus occt_model_stl_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = sessionOf(session);
        return importShape(model, resultShapeId, [&]
        {
            return readModelStl(requiredPath(utf8Path));
        });
    }

    OcctStatus occt_model_file_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = sessionOf(session);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        const std::lock_guard<std::recursive_mutex> guard(model->mutex);
        model->errors.clear();
        if (resultShapeId == nullptr)
        {
            model->errors.set(OcctStatus_ErrorInvalidArgument, "Result shape ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }
        *resultShapeId = 0;

        const auto path = OcctBridge::pathFromUtf8(utf8Path);
        if (path.empty())
        {
            model->errors.set(OcctStatus_ErrorInvalidArgument, "Path is empty.");
            return OcctStatus_ErrorInvalidArgument;
        }

        const std::string extension = OcctBridge::lowerExtension(path);
        if (extension == ".step" || extension == ".stp")
            return occt_model_step_import(session, utf8Path, resultShapeId);
        if (extension == ".iges" || extension == ".igs")
            return occt_model_iges_import(session, utf8Path, resultShapeId);
        if (extension == ".brep" || extension == ".rle")
            return occt_model_brep_import(session, utf8Path, resultShapeId);
        if (extension == ".stl")
            return occt_model_stl_import(session, utf8Path, resultShapeId);

        model->errors.set(
            OcctStatus_ErrorFormat,
            "Unsupported file extension. Supported: STEP, IGES, BREP and STL.");
        return OcctStatus_ErrorFormat;
    }

    OcctStatus occt_model_step_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path)
    {
        ModelSession* model = sessionOf(session);
        return exportShape(model, [&]
        {
            writeModelStep(model->requireShape(shapeId), requiredPath(utf8Path));
        });
    }

    OcctStatus occt_model_iges_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path)
    {
        ModelSession* model = sessionOf(session);
        return exportShape(model, [&]
        {
            writeModelIges(model->requireShape(shapeId), requiredPath(utf8Path));
        });
    }

    OcctStatus occt_model_brep_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path)
    {
        ModelSession* model = sessionOf(session);
        return exportShape(model, [&]
        {
            auto stream = modelOutputStream(requiredPath(utf8Path));
            BRepTools::Write(model->requireShape(shapeId), stream);
            if (!stream) throw std::runtime_error("BREP file could not be written.");
        });
    }

    OcctStatus occt_model_stl_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path,
        const OcctStlExportOptions* options)
    {
        ModelSession* model = sessionOf(session);
        return exportShape(model, [&]
        {
            validateStlOptions(options);
            const TopoDS_Shape& shape = model->requireShape(shapeId);
            BRepMesh_IncrementalMesh mesh(
                shape,
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
            if (!writer.Write(shape, path.string().c_str()))
            {
                throw std::runtime_error(
                    "STL file could not be written. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
            }
        });
    }
}
