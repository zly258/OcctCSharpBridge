#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_exchange_import_step(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_import_iges(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_import_brep(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_import_stl(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_import_file(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_export_step(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_export_all_step(
        OcctEngineHandle handle,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_export_iges(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_export_all_iges(
        OcctEngineHandle handle,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_export_brep(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path);

    OCCTBRIDGE_API OcctStatus occt_engine_exchange_export_stl(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path,
        double linearDeflection,
        double angularDeflection,
        OcctBool asciiMode);
}
