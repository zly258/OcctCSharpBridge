#pragma once

#include "OcctNative.h"

extern "C"
{
    struct OcctStlExportOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        double linearDeflection;
        double angularDeflection;
        OcctBool ascii;
    };

    OCCTBRIDGE_API OcctStatus occt_model_file_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_step_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_iges_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_brep_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_stl_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_step_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_model_iges_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_model_brep_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_model_stl_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path,
        const OcctStlExportOptions* options);
}
