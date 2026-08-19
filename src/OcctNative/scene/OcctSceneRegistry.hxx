#pragma once

#include "OcctNative.h"

#include <AIS_InteractiveObject.hxx>
#include <TopoDS_Shape.hxx>

#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

namespace OcctBridge
{
    struct ObjectEntry
    {
        int kind = OcctObject_Unknown;
        TopoDS_Shape shape;
        Handle(AIS_InteractiveObject) presentation;
        std::string name;
        std::string applicationTag;
        std::vector<std::string> stepHierarchyPath;
        bool hasStoredColor = false;
        double storedColorR = 0.0;
        double storedColorG = 0.0;
        double storedColorB = 0.0;
        bool selectable = true;
        double storedColorA = 1.0;
        bool hasStoredAlpha = false;
        bool storedVisible = true;
        bool hasStoredVisibility = false;
        int stepDocumentIndex = -1;
        std::string stepNodeId;
        int presentationSubtype = 0;

        ObjectEntry() = default;

        ObjectEntry(
            int objectKind,
            const TopoDS_Shape& objectShape,
            const Handle(AIS_InteractiveObject)& objectPresentation,
            std::string objectName,
            std::string objectApplicationTag = {},
            std::vector<std::string> objectStepHierarchyPath = {},
            bool objectHasStoredColor = false,
            double objectStoredColorR = 0.0,
            double objectStoredColorG = 0.0,
            double objectStoredColorB = 0.0,
            bool objectSelectable = true,
            double objectStoredColorA = 1.0,
            bool objectHasStoredAlpha = false,
            bool objectStoredVisible = true,
            bool objectHasStoredVisibility = false,
            int objectStepDocumentIndex = -1,
            std::string objectStepNodeId = {},
            int objectPresentationSubtype = 0)
            : kind(objectKind),
              shape(objectShape),
              presentation(objectPresentation),
              name(std::move(objectName)),
              applicationTag(std::move(objectApplicationTag)),
              stepHierarchyPath(std::move(objectStepHierarchyPath)),
              hasStoredColor(objectHasStoredColor),
              storedColorR(objectStoredColorR),
              storedColorG(objectStoredColorG),
              storedColorB(objectStoredColorB),
              selectable(objectSelectable),
              storedColorA(objectStoredColorA),
              hasStoredAlpha(objectHasStoredAlpha),
              storedVisible(objectStoredVisible),
              hasStoredVisibility(objectHasStoredVisibility),
              stepDocumentIndex(objectStepDocumentIndex),
              stepNodeId(std::move(objectStepNodeId)),
              presentationSubtype(objectPresentationSubtype)
        {
        }
    };

    class SceneRegistry
    {
    public:
        std::unordered_map<OcctObjectId, ObjectEntry> objects;
        std::unordered_map<std::string, OcctObjectId> objectIdByApplicationTag;

        ObjectEntry* findObject(OcctObjectId id);
        const ObjectEntry* findObject(OcctObjectId id) const;
        ObjectEntry* findShape(OcctObjectId id);
        const ObjectEntry* findShape(OcctObjectId id) const;
        OcctObjectId findPresentation(const Handle(AIS_InteractiveObject)& presentation) const;
        OcctObjectId allocateId();
        void clear();

    private:
        OcctObjectId nextId = 1;
    };
}
