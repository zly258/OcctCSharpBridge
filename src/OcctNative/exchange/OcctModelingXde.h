#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_step_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document);

    OCCTBRIDGE_API OcctStatus occt_model_iges_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document);

    OCCTBRIDGE_API OcctStatus occt_model_obj_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document);

    OCCTBRIDGE_API OcctStatus occt_model_gltf_document_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* primaryShapeId,
        OcctXdeDocumentHandle* document);

    OCCTBRIDGE_API void occt_xde_document_release(OcctXdeDocumentHandle document);

    OCCTBRIDGE_API OcctStatus occt_model_step_document_export(
        OcctModelingSessionHandle session,
        OcctXdeDocumentHandle document,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_model_iges_document_export(
        OcctModelingSessionHandle session,
        OcctXdeDocumentHandle document,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_model_xde_document_json_get(
        OcctModelingSessionHandle session,
        OcctXdeDocumentHandle document,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes);
}
