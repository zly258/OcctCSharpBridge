#include "OcctInternal.hxx"
#include "OcctPoints.h"
#include "OcctViewerInteraction.h"
#include "OcctViewerInteractionExtensions.h"

#include <AIS_Point.hxx>
#include <AIS_SelectionModesConcurrency.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Geom_CartesianPoint.hxx>
#include <Graphic3d_ClipPlane.hxx>
#include <Graphic3d_DisplayPriority.hxx>
#include <Graphic3d_SequenceOfHClipPlane.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_Vec2.hxx>
#include <gp_Pln.hxx>

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    int selectionModeValue(const ObjectEntry& entry, int mode)
    {
        if (mode < OcctSelection_Object || mode > OcctSelection_Solid)
            throw std::invalid_argument("Selection mode is out of range.");
        if (mode == OcctSelection_Object) return 0;

        const Handle(AIS_Shape) shape = Handle(AIS_Shape)::DownCast(entry.presentation);
        if (shape.IsNull())
            throw std::invalid_argument("Subshape selection modes require an AIS_Shape object.");

        TopAbs_ShapeEnum type = TopAbs_SHAPE;
        switch (mode)
        {
            case OcctSelection_Vertex: type = TopAbs_VERTEX; break;
            case OcctSelection_Edge: type = TopAbs_EDGE; break;
            case OcctSelection_Wire: type = TopAbs_WIRE; break;
            case OcctSelection_Face: type = TopAbs_FACE; break;
            case OcctSelection_Shell: type = TopAbs_SHELL; break;
            case OcctSelection_Solid: type = TopAbs_SOLID; break;
            default: break;
        }
        return AIS_Shape::SelectionMode(type);
    }

    AIS_SelectionModesConcurrency selectionConcurrency(int value)
    {
        switch (value)
        {
            case OcctSelectionConcurrency_Single:
                return AIS_SelectionModesConcurrency_Single;
            case OcctSelectionConcurrency_GlobalOrLocal:
                return AIS_SelectionModesConcurrency_GlobalOrLocal;
            case OcctSelectionConcurrency_Multiple:
                return AIS_SelectionModesConcurrency_Multiple;
            default:
                throw std::invalid_argument("Selection mode concurrency is out of range.");
        }
    }

    Graphic3d_DisplayPriority displayPriority(int value)
    {
        if (value < 0 || value > 10)
            throw std::invalid_argument("Display priority must be between 0 and 10.");
        return static_cast<Graphic3d_DisplayPriority>(value);
    }

    Aspect_TypeOfTriedronPosition cornerPosition(int value)
    {
        switch (value)
        {
            case OcctCorner_LeftLower: return Aspect_TOTP_LEFT_LOWER;
            case OcctCorner_LeftUpper: return Aspect_TOTP_LEFT_UPPER;
            case OcctCorner_RightLower: return Aspect_TOTP_RIGHT_LOWER;
            case OcctCorner_RightUpper: return Aspect_TOTP_RIGHT_UPPER;
            default: throw std::invalid_argument("Corner position is out of range.");
        }
    }

    int cornerValue(Aspect_TypeOfTriedronPosition value)
    {
        switch (value)
        {
            case Aspect_TOTP_LEFT_LOWER: return OcctCorner_LeftLower;
            case Aspect_TOTP_LEFT_UPPER: return OcctCorner_LeftUpper;
            case Aspect_TOTP_RIGHT_LOWER: return OcctCorner_RightLower;
            case Aspect_TOTP_RIGHT_UPPER: return OcctCorner_RightUpper;
            default: throw std::runtime_error("Transform persistence uses an unsupported corner position.");
        }
    }

    Graphic3d_TransModeFlags persistenceMode3d(int value)
    {
        switch (value)
        {
            case OcctTransformPersistence_Zoom: return Graphic3d_TMF_ZoomPers;
            case OcctTransformPersistence_Rotate: return Graphic3d_TMF_RotatePers;
            case OcctTransformPersistence_ZoomRotate: return Graphic3d_TMF_ZoomRotatePers;
            default: throw std::invalid_argument("Transform persistence mode is not a 3D anchor mode.");
        }
    }

    Graphic3d_TransModeFlags persistenceMode2d(int value)
    {
        switch (value)
        {
            case OcctTransformPersistence_Screen2d: return Graphic3d_TMF_2d;
            case OcctTransformPersistence_Triedron: return Graphic3d_TMF_TriedronPers;
            default: throw std::invalid_argument("Transform persistence mode is not a screen anchor mode.");
        }
    }

    int persistenceModeValue(Graphic3d_TransModeFlags value)
    {
        if (value == Graphic3d_TMF_ZoomPers) return OcctTransformPersistence_Zoom;
        if (value == Graphic3d_TMF_RotatePers) return OcctTransformPersistence_Rotate;
        if (value == Graphic3d_TMF_ZoomRotatePers) return OcctTransformPersistence_ZoomRotate;
        if (value == Graphic3d_TMF_2d) return OcctTransformPersistence_Screen2d;
        if (value == Graphic3d_TMF_TriedronPers) return OcctTransformPersistence_Triedron;
        throw std::runtime_error("Object uses a transform persistence mode outside the managed set.");
    }

    struct PointUpdateTarget
    {
        ObjectEntry* entry;
        Handle(AIS_Point) presentation;
        const OcctPointStateUpdate* update;
    };

    PointUpdateTarget pointUpdateTarget(
        Engine* engine,
        const OcctPointStateUpdate& update)
    {
        ObjectEntry* entry = engine->findObject(update.pointId);
        if (entry == nullptr || entry->kind != OcctPointObjectKind)
            throw std::invalid_argument("Point ID does not exist.");

        Handle(AIS_Point) presentation = Handle(AIS_Point)::DownCast(entry->presentation);
        if (presentation.IsNull())
            throw std::runtime_error("Point presentation type is invalid.");

        const gp_Pnt position = point(update.position);
        (void)position;
        return {entry, presentation, &update};
    }
}

extern "C"
{
    int occt_set_object_selection_mode_active(
        OcctHandle h,
        OcctObjectId objectId,
        int mode,
        int active,
        int concurrency,
        int force)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            if (active != 0 && !entry.selectable)
                throw std::invalid_argument("A non-selectable object cannot activate a selection mode.");

            e->viewerContext.context->SetSelectionModeActive(
                entry.presentation,
                selectionModeValue(entry, mode),
                active != 0,
                selectionConcurrency(concurrency),
                force != 0);
            e->requestRedraw();
        });
    }

    int occt_set_object_selection_sensitivity(
        OcctHandle h,
        OcctObjectId objectId,
        int mode,
        int sensitivity)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (sensitivity <= 0)
                throw std::invalid_argument("Selection sensitivity must be greater than zero.");
            ObjectEntry& entry = requiredObject(e, objectId);
            e->viewerContext.context->SetSelectionSensitivity(
                entry.presentation,
                selectionModeValue(entry, mode),
                sensitivity);
        });
    }

    int occt_set_object_display_priority(
        OcctHandle h,
        OcctObjectId objectId,
        int priority)
    {
        return occt_set_objects_display_priority(h, &objectId, 1, priority);
    }

    int occt_set_objects_display_priority(
        OcctHandle h,
        const OcctObjectId* objectIds,
        int count,
        int priority)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count < 0) throw std::invalid_argument("Object count must not be negative.");
            if (count > 0 && objectIds == nullptr)
                throw std::invalid_argument("Object ID array is null.");

            const Graphic3d_DisplayPriority nativePriority = displayPriority(priority);
            std::vector<ObjectEntry*> entries;
            entries.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
                entries.push_back(&requiredObject(e, objectIds[index]));

            for (ObjectEntry* entry : entries)
                e->viewerContext.context->SetDisplayPriority(entry->presentation, nativePriority);
            if (!entries.empty()) e->requestRedraw();
        });
    }

    int occt_get_object_display_priority(
        OcctHandle h,
        OcctObjectId objectId,
        int* priority)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || priority == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            *priority = static_cast<int>(e->viewerContext.context->DisplayPriority(entry.presentation));
        });
    }

    int occt_set_object_transform_persistence_3d(
        OcctHandle h,
        OcctObjectId objectId,
        int mode,
        OcctPoint3d anchor)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            const Handle(Graphic3d_TransformPers) persistence =
                new Graphic3d_TransformPers(persistenceMode3d(mode), point(anchor));
            e->viewerContext.context->SetTransformPersistence(entry.presentation, persistence);
            e->requestRedraw();
        });
    }

    int occt_set_object_transform_persistence_2d(
        OcctHandle h,
        OcctObjectId objectId,
        int mode,
        int position,
        int offsetX,
        int offsetY)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (offsetX < 0 || offsetY < 0)
                throw std::invalid_argument("Transform persistence offsets must not be negative.");
            ObjectEntry& entry = requiredObject(e, objectId);
            const Handle(Graphic3d_TransformPers) persistence =
                new Graphic3d_TransformPers(
                    persistenceMode2d(mode),
                    cornerPosition(position),
                    Graphic3d_Vec2i(offsetX, offsetY));
            e->viewerContext.context->SetTransformPersistence(entry.presentation, persistence);
            e->requestRedraw();
        });
    }

    int occt_clear_object_transform_persistence(
        OcctHandle h,
        OcctObjectId objectId)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            Handle(Graphic3d_TransformPers) persistence;
            e->viewerContext.context->SetTransformPersistence(entry.presentation, persistence);
            e->requestRedraw();
        });
    }

    int occt_get_object_transform_persistence(
        OcctHandle h,
        OcctObjectId objectId,
        OcctTransformPersistenceState* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            *result = {};
            result->mode = OcctTransformPersistence_None;
            result->position = OcctCorner_LeftLower;

            const Handle(Graphic3d_TransformPers)& persistence =
                entry.presentation->TransformPersistence();
            if (persistence.IsNull()) return;

            result->mode = persistenceModeValue(persistence->Mode());
            if (persistence->IsZoomOrRotate())
            {
                const gp_Pnt anchor = persistence->AnchorPoint();
                result->anchor = {anchor.X(), anchor.Y(), anchor.Z()};
            }
            else if (persistence->IsTrihedronOr2d())
            {
                result->position = cornerValue(persistence->Corner2d());
                const Graphic3d_Vec2i offset = persistence->Offset2d();
                result->offsetX = offset.x();
                result->offsetY = offset.y();
            }
        });
    }

    int occt_set_view_clip_planes(
        OcctHandle h,
        const OcctViewClipPlane* planes,
        int count)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count < 0) throw std::invalid_argument("Clip plane count must not be negative.");
            if (count > 0 && planes == nullptr)
                throw std::invalid_argument("Clip plane array is null.");
            if (count > e->viewerContext.view->PlaneLimit())
                throw std::invalid_argument("Clip plane count exceeds the view plane limit.");

            Handle(Graphic3d_SequenceOfHClipPlane) sequence =
                new Graphic3d_SequenceOfHClipPlane();
            for (int index = 0; index < count; ++index)
            {
                const OcctViewClipPlane& source = planes[index];
                Handle(Graphic3d_ClipPlane) plane =
                    new Graphic3d_ClipPlane(gp_Pln(point(source.point), direction(source.normal)));
                plane->SetOn(source.enabled != 0);
                plane->SetCapping(source.capping != 0);
                plane->SetCappingColor(color(source.cappingR, source.cappingG, source.cappingB));
                sequence->Append(plane);
            }
            e->viewerContext.view->SetClipPlanes(sequence);
            e->requestRedraw();
        });
    }

    int occt_get_view_clip_plane_limit(
        OcctHandle h,
        int* limit)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || limit == nullptr) return 0;
        return execute(e, [&] { *limit = e->viewerContext.view->PlaneLimit(); });
    }

    int occt_update_points(
        OcctHandle h,
        const OcctPointStateUpdate* updates,
        int count)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count < 0) throw std::invalid_argument("Point update count must not be negative.");
            if (count > 0 && updates == nullptr)
                throw std::invalid_argument("Point update array is null.");

            std::vector<PointUpdateTarget> targets;
            targets.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
                targets.push_back(pointUpdateTarget(e, updates[index]));

            for (PointUpdateTarget& target : targets)
            {
                Handle(Geom_CartesianPoint) component =
                    Handle(Geom_CartesianPoint)::DownCast(target.presentation->Component());
                if (component.IsNull())
                    target.presentation->SetComponent(
                        new Geom_CartesianPoint(point(target.update->position)));
                else
                    component->SetPnt(point(target.update->position));

                e->viewerContext.context->Redisplay(target.presentation, Standard_False);
                e->viewerContext.context->RecomputeSelectionOnly(target.presentation);
                target.entry->storedVisible = target.update->visible != 0;
                target.entry->hasStoredVisibility = true;
                if (target.update->visible != 0)
                    e->viewerContext.context->Display(target.presentation, Standard_False);
                else
                    e->viewerContext.context->Erase(target.presentation, Standard_False);
            }
            if (!targets.empty()) e->requestRedraw();
        });
    }
}
