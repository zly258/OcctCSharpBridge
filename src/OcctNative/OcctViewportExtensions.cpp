#include "OcctInternal.hxx"
#include "OcctViewerInteraction.h"

#include <Aspect_GradientFillMethod.hxx>
#include <Aspect_TypeOfLine.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <Graphic3d_RenderingParams.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Precision.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_LineAspect.hxx>
#include <V3d_TypeOfOrientation.hxx>

using namespace OcctBridge;

namespace
{
    V3d_TypeOfOrientation zUpOrientation(int value)
    {
        switch (value)
        {
            case OcctZUp_Front: return V3d_TypeOfOrientation_Zup_Front;
            case OcctZUp_Back: return V3d_TypeOfOrientation_Zup_Back;
            case OcctZUp_Left: return V3d_TypeOfOrientation_Zup_Left;
            case OcctZUp_Right: return V3d_TypeOfOrientation_Zup_Right;
            case OcctZUp_Top: return V3d_TypeOfOrientation_Zup_Top;
            case OcctZUp_Bottom: return V3d_TypeOfOrientation_Zup_Bottom;
            case OcctZUp_XNegativeYNegative: return V3d_XnegYnegZpos;
            case OcctZUp_XPositiveYNegative: return V3d_XposYnegZpos;
            case OcctZUp_XNegativeYPositive: return V3d_XnegYposZpos;
            case OcctZUp_XPositiveYPositive: return V3d_XposYposZpos;
            default: throw std::invalid_argument("Z-up view orientation is out of range.");
        }
    }

    void validateMargin(double margin)
    {
        if (margin < 0.0 || margin >= 1.0)
            throw std::invalid_argument("Fit margin must be in the range [0, 1).");
    }

}

extern "C"
{
    int occt_fit_objects(OcctHandle h, const OcctObjectId* objectIds, int count, double margin)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");
            requireCount(count, 1, "Object ID array");
            validateMargin(margin);

            Bnd_Box bounds;
            for (int index = 0; index < count; ++index)
            {
                const ObjectEntry* entry = e->findShape(objectIds[index]);
                if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
                BRepBndLib::Add(shapeWithPresentationTransformation(*entry), bounds);
            }
            if (bounds.IsVoid()) throw std::runtime_error("Selected shapes have no finite bounds.");
            e->viewerContext.view->FitAll(bounds, margin, Standard_False);
            e->viewerContext.view->ZFitAll();
            e->requestRedraw();
        });
    }

    int occt_set_zup_view(OcctHandle h, int orientation, int fitAll)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->viewerContext.view->SetProj(zUpOrientation(orientation));
            if (fitAll != 0)
            {
                e->viewerContext.view->FitAll(0.01, Standard_False);
                e->viewerContext.view->ZFitAll();
            }
            e->requestRedraw();
        });
    }

    int occt_screen_to_ray(OcctHandle h, int x, int y, OcctProjectionRay* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            Standard_Real px = 0.0, py = 0.0, pz = 0.0;
            Standard_Real vx = 0.0, vy = 0.0, vz = 0.0;
            e->viewerContext.view->ConvertWithProj(x, y, px, py, pz, vx, vy, vz);
            const gp_Dir rayDirection(gp_Vec(vx, vy, vz));
            result->origin = {px, py, pz};
            result->direction = {rayDirection.X(), rayDirection.Y(), rayDirection.Z()};
        });
    }

    int occt_zoom_at_point(OcctHandle h, int x, int y, double delta)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(delta) || std::abs(delta) <= Precision::Confusion())
                throw std::invalid_argument("Zoom delta must be finite and non-zero.");

            const double clampedDelta = std::clamp(delta, -10000.0, 10000.0);
            Standard_Integer zoomDelta = static_cast<Standard_Integer>(std::lround(clampedDelta));
            if (zoomDelta == 0)
                zoomDelta = delta > 0.0 ? 1 : -1;

            e->viewerContext.view->StartZoomAtPoint(x, y);
            e->viewerContext.view->ZoomAtPoint(0, 0, zoomDelta, 0);
        });
    }

    int occt_select_all_visible(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->viewerContext.context->ClearSelected(Standard_False);
            for (const auto& pair : e->scene.objects)
            {
                if (!pair.second.presentation.IsNull()
                    && pair.second.selectable
                    && e->viewerContext.context->IsDisplayed(pair.second.presentation))
                    e->viewerContext.context->AddSelect(pair.second.presentation);
            }
            e->viewerContext.context->HilightSelected(Standard_False);
            e->requestRedraw();
        });
    }

    int occt_invert_selection(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            for (const auto& pair : e->scene.objects)
            {
                if (!pair.second.presentation.IsNull()
                    && pair.second.selectable
                    && e->viewerContext.context->IsDisplayed(pair.second.presentation))
                    e->viewerContext.context->AddOrRemoveSelected(pair.second.presentation, Standard_False);
            }
            e->viewerContext.context->HilightSelected(Standard_False);
            e->requestRedraw();
        });
    }

    int occt_hide_selected(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            std::vector<Handle(AIS_InteractiveObject)> selected;
            for (e->viewerContext.context->InitSelected(); e->viewerContext.context->MoreSelected(); e->viewerContext.context->NextSelected())
            {
                const Handle(AIS_InteractiveObject) value = e->viewerContext.context->SelectedInteractive();
                if (!value.IsNull()) selected.push_back(value);
            }
            for (const auto& value : selected) e->viewerContext.context->Erase(value, Standard_False);
            e->viewerContext.context->ClearSelected(Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_automatic_highlight(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->viewerContext.context->SetAutomaticHilight(enabled != 0); });
    }

    int occt_set_msaa_samples(OcctHandle h, int samples)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (samples < 0 || samples > 16)
                throw std::invalid_argument("MSAA sample count must be between 0 and 16.");
            e->viewerContext.view->ChangeRenderingParams().NbMsaaSamples = samples;
            e->requestRedraw();
        });
    }

    int occt_set_render_resolution_scale(OcctHandle h, double scale)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(scale) || scale < 0.25 || scale > 4.0)
                throw std::invalid_argument("Render resolution scale must be between 0.25 and 4.0.");
            e->viewerContext.view->ChangeRenderingParams().RenderResolutionScale = static_cast<Standard_ShortReal>(scale);
            e->requestRedraw();
        });
    }

    int occt_set_render_resolution(OcctHandle h, double dpi)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(dpi) || dpi < 36.0 || dpi > 600.0)
                throw std::invalid_argument("Render resolution must be between 36 and 600 DPI.");
            e->viewerContext.view->ChangeRenderingParams().Resolution = static_cast<unsigned int>(std::lround(dpi));
            e->requestRedraw();
        });
    }

    int occt_set_rendering_method(OcctHandle h, int method)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (method != OcctRendering_Rasterization && method != OcctRendering_RayTracing)
                throw std::invalid_argument("Rendering method is out of range.");
            e->viewerContext.view->ChangeRenderingParams().Method = method == OcctRendering_RayTracing
                ? Graphic3d_RM_RAYTRACING
                : Graphic3d_RM_RASTERIZATION;
            e->requestRedraw();
        });
    }

    int occt_set_shadows_enabled(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->viewerContext.view->ChangeRenderingParams().IsShadowEnabled = enabled != 0;
            e->requestRedraw();
        });
    }

    int occt_set_immediate_update(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->viewerContext.view->SetImmediateUpdate(enabled != 0); });
    }

    int occt_set_frustum_culling(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->viewerContext.view->SetFrustumCulling(enabled != 0);
            e->requestRedraw();
        });
    }

    int occt_set_face_boundaries_visible(OcctHandle h, int visible, int applyExisting)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->viewerContext.context->DefaultDrawer()->SetFaceBoundaryDraw(visible != 0);
            if (applyExisting != 0)
            {
                for (auto& pair : e->scene.objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    pair.second.presentation->Attributes()->SetFaceBoundaryDraw(visible != 0);
                    e->viewerContext.context->Redisplay(pair.second.presentation, Standard_False, Standard_True);
                }
            }
            e->requestRedraw();
        });
    }

}
