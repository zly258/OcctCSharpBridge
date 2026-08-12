#include "OcctInternal.hxx"

#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_RenderingParams.hxx>
#include <Prs3d_Drawer.hxx>

using namespace OcctBridge;

namespace
{
    void validateMargin(double margin)
    {
        if (!std::isfinite(margin) || margin < 0.0 || margin >= 1.0)
            throw std::invalid_argument("Fit margin must be in the range [0, 1).");
    }
}

extern "C"
{
    int occt_get_viewport_state(OcctHandle handle, OcctViewportState* result)
    {
        Engine* engine = engineOf(handle);
        if (!validateInitialized(engine) || result == nullptr) return 0;
        return execute(engine, [&]
        {
            Standard_Integer width = 0;
            Standard_Integer height = 0;
            engine->window->Size(width, height);

            const Handle(Graphic3d_Camera)& camera = engine->view->Camera();
            const Graphic3d_RenderingParams& rendering = engine->view->RenderingParams();
            result->width = width;
            result->height = height;
            result->projectionType = camera->ProjectionType() == Graphic3d_Camera::Projection_Perspective
                ? OcctProjection_Perspective
                : OcctProjection_Orthographic;
            result->computedMode = engine->view->ComputedMode() ? 1 : 0;
            result->antialiasingEnabled = rendering.IsAntialiasingEnabled ? 1 : 0;
            result->msaaSamples = rendering.NbMsaaSamples;
            result->renderingMethod = rendering.Method == Graphic3d_RM_RAYTRACING
                ? OcctRendering_RayTracing
                : OcctRendering_Rasterization;
            result->shadowsEnabled = rendering.IsShadowEnabled ? 1 : 0;
            result->frustumCullingEnabled = engine->view->IsCullingEnabled() ? 1 : 0;
            result->faceBoundariesVisible = engine->context->DefaultDrawer()->FaceBoundaryDraw() ? 1 : 0;
            result->selectionTolerance = engine->context->PixelTolerance();
            result->automaticHighlight = engine->context->AutomaticHilight() ? 1 : 0;
            result->perspectiveFov = camera->FOVy();
            result->renderResolutionScale = rendering.RenderResolutionScale;
            result->renderResolutionDpi = rendering.Resolution;
        });
    }

    int occt_reset_view(OcctHandle handle)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&] { engine->view->Reset(Standard_True); });
    }

    int occt_reset_view_orientation(OcctHandle handle)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            engine->view->ResetViewOrientation();
            engine->view->Redraw();
        });
    }

    int occt_reset_view_mapping(OcctHandle handle)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            engine->view->ResetViewMapping();
            engine->view->Redraw();
        });
    }

    int occt_fit_selected(OcctHandle handle, double margin)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            validateMargin(margin);
            Bnd_Box bounds;
            int shapeCount = 0;
            for (engine->context->InitSelected(); engine->context->MoreSelected(); engine->context->NextSelected())
            {
                const OcctObjectId id = engine->findPresentation(engine->context->SelectedInteractive());
                const ObjectEntry* entry = engine->findShape(id);
                if (entry == nullptr) continue;
                BRepBndLib::Add(entry->shape, bounds);
                ++shapeCount;
            }
            if (shapeCount == 0 || bounds.IsVoid())
                throw std::runtime_error("The current selection has no finite shape bounds.");
            engine->view->FitAll(bounds, margin, Standard_False);
            engine->view->ZFitAll();
            engine->view->Redraw();
        });
    }

    int occt_get_scene_gravity_point(OcctHandle handle, OcctPoint3d* result)
    {
        Engine* engine = engineOf(handle);
        if (!validateInitialized(engine) || result == nullptr) return 0;
        return execute(engine, [&]
        {
            const gp_Pnt point = engine->view->GravityPoint();
            *result = {point.X(), point.Y(), point.Z()};
        });
    }
}
