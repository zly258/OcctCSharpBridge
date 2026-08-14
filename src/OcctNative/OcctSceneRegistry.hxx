#pragma once

#include "OcctNative.h"

#include <AIS_InteractiveObject.hxx>
#include <TopoDS_Shape.hxx>

#include <string>
#include <unordered_map>
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
