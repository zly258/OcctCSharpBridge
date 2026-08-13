#include "OcctInternal.hxx"
#include "OcctManipulator.h"

#include <AIS_Manipulator.hxx>
#include <AIS_ManipulatorMode.hxx>
#include <TCollection_AsciiString.hxx>

using namespace OcctBridge;

namespace
{
    std::vector<ObjectEntry*> requireObjects(
        Engine* engine,
        const OcctObjectId* objectIds,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");
        std::vector<ObjectEntry*> result;
        std::vector<OcctObjectId> uniqueIds;
        for (int index = 0; index < count; ++index)
        {
            const OcctObjectId id = objectIds[index];
            if (std::find(uniqueIds.begin(), uniqueIds.end(), id) != uniqueIds.end()) continue;
            ObjectEntry* entry = engine->findObject(id);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            uniqueIds.push_back(id);
            result.push_back(entry);
        }
        return result;
    }

    void restoreManipulatorModes(Engine* engine, ObjectEntry& entry)
    {
        Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
        if (manipulator.IsNull() || !manipulator->IsAttached()) return;
        if ((entry.presentationSubtype & (1 << OcctManipulator_Translation)) != 0)
            manipulator->EnableMode(AIS_MM_Translation);
        if ((entry.presentationSubtype & (1 << OcctManipulator_Rotation)) != 0)
            manipulator->EnableMode(AIS_MM_Rotation);
        if ((entry.presentationSubtype & (1 << OcctManipulator_Scaling)) != 0)
            manipulator->EnableMode(AIS_MM_Scaling);
        if ((entry.presentationSubtype & (1 << OcctManipulator_TranslationPlane)) != 0)
            manipulator->EnableMode(AIS_MM_TranslationPlane);
    }

    void setSelectable(Engine* engine, ObjectEntry& entry, bool selectable)
    {
        if (entry.selectable == selectable) return;
        entry.selectable = selectable;
        if (!selectable)
        {
            if (engine->context->IsSelected(entry.presentation))
                engine->context->AddOrRemoveSelected(entry.presentation, Standard_False);
            if (entry.kind == OcctManipulatorObjectKind)
            {
                Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
                if (!manipulator.IsNull())
                {
                    if (manipulator->HasActiveTransformation()) manipulator->StopTransform(Standard_False);
                    if (manipulator->HasActiveMode()) manipulator->DeactivateCurrentMode();
                }
            }
            engine->context->Deactivate(entry.presentation);
        }
        else if (entry.kind == OcctManipulatorObjectKind)
        {
            restoreManipulatorModes(engine, entry);
        }
        else
        {
            engine->applySelectionMode(entry.presentation);
        }
    }

    void setViewCubeLabels(Engine* engine, int language)
    {
        if (engine->viewCube.IsNull()) throw std::runtime_error("The view cube has not been initialized.");
        if (language == OcctViewCubeLanguage_English)
        {
            engine->viewCube->SetFont("Arial");
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Front, "FRONT");
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Back, "BACK");
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Left, "LEFT");
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Right, "RIGHT");
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Top, "TOP");
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Bottom, "BOTTOM");
        }
        else if (language == OcctViewCubeLanguage_ChineseSimplified)
        {
            engine->viewCube->SetFont("Microsoft YaHei UI");
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Front, TCollection_AsciiString(u8"前"));
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Back, TCollection_AsciiString(u8"后"));
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Left, TCollection_AsciiString(u8"左"));
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Right, TCollection_AsciiString(u8"右"));
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Top, TCollection_AsciiString(u8"上"));
            engine->viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Bottom, TCollection_AsciiString(u8"下"));
        }
        else
        {
            throw std::invalid_argument("View cube language is out of range.");
        }
        engine->context->Redisplay(engine->viewCube, Standard_False, Standard_True);
        engine->requestRedraw();
    }
}

extern "C"
{
    int occt_set_object_selectable(OcctHandle handle, OcctObjectId objectId, int selectable)
    {
        return occt_set_objects_selectable(handle, &objectId, 1, selectable);
    }

    int occt_get_object_selectable(OcctHandle handle, OcctObjectId objectId)
    {
        Engine* engine = engineOf(handle);
        const ObjectEntry* entry = engine == nullptr ? nullptr : engine->findObject(objectId);
        return entry != nullptr && entry->selectable ? 1 : 0;
    }

    int occt_set_objects_selectable(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int selectable)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            for (ObjectEntry* entry : entries) setSelectable(engine, *entry, selectable != 0);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_set_view_cube_language(OcctHandle handle, int language)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&] { setViewCubeLabels(engine, language); });
    }
}
