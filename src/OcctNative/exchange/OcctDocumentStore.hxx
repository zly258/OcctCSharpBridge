#pragma once

#include "OcctNative.h"

#include <TDocStd_Document.hxx>

#include <vector>

namespace OcctBridge
{
    struct DocumentStore
    {
        std::vector<Handle(TDocStd_Document)> stepDocuments;
        std::vector<OcctObjectId> lastStepImportObjectIds;
        Handle(TDocStd_Document) pristineStepDocument;
        bool pristineStepDocumentMatchesScene = false;

        void invalidatePristine()
        {
            pristineStepDocumentMatchesScene = false;
        }

        void clear()
        {
            stepDocuments.clear();
            lastStepImportObjectIds.clear();
            pristineStepDocument.Nullify();
            pristineStepDocumentMatchesScene = false;
        }
    };
}
