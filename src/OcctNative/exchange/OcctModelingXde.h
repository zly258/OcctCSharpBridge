#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_step_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_iges_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_obj_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_gltf_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_step_document_export(
        OcctModelingSessionHandle session,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_model_iges_document_export(
        OcctModelingSessionHandle session,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_model_xde_document_json_get(
        OcctModelingSessionHandle session,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes);
}
