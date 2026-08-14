#include "OcctInternal.hxx"

#include <Aspect_GradientFillMethod.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_RenderingParams.hxx>

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }
}

extern "C"
{
    int occt_get_camera(OcctHandle h, OcctCameraState* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            const Handle(Graphic3d_Camera)& camera = e->viewerContext.view->Camera();
            const gp_Pnt eye = camera->Eye();
            const gp_Pnt center = camera->Center();
            const gp_Dir up = camera->Up();
            const gp_Dir directionValue = camera->Direction();
            result->eye = {eye.X(), eye.Y(), eye.Z()};
            result->center = {center.X(), center.Y(), center.Z()};
            result->up = {up.X(), up.Y(), up.Z()};
            result->direction = {directionValue.X(), directionValue.Y(), directionValue.Z()};
            result->scale = camera->Scale();
        });
    }

    int occt_set_camera(OcctHandle h, const OcctCameraState* state)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || state == nullptr) return 0;
        return execute(e, [&]
        {
            requirePositive(state->scale, "Camera scale");
            const Handle(Graphic3d_Camera)& camera = e->viewerContext.view->Camera();
            camera->SetEyeAndCenter(point(state->eye), point(state->center));
            camera->SetUp(direction(state->up));
            camera->OrthogonalizeUp();
            camera->SetScale(state->scale);
            e->requestRedraw();
        });
    }

    double occt_get_view_scale(OcctHandle h)
    {
        Engine* e = engineOf(h);
        return e != nullptr && e->isInitialized() ? e->viewerContext.view->Camera()->Scale() : 0.0;
    }

    int occt_set_view_scale(OcctHandle h, double scale)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(scale, "View scale");
            e->viewerContext.view->Camera()->SetScale(scale);
            e->requestRedraw();
        });
    }

    int occt_set_antialiasing(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->viewerContext.view->ChangeRenderingParams().IsAntialiasingEnabled = enabled != 0;
            e->requestRedraw();
        });
    }

    int occt_set_gradient_background(
        OcctHandle h,
        double r1,
        double g1,
        double b1,
        double r2,
        double g2,
        double b2,
        int fillMethod)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (fillMethod < static_cast<int>(Aspect_GradientFillMethod_None)
                || fillMethod > static_cast<int>(Aspect_GradientFillMethod_Elliptical))
                throw std::invalid_argument("Gradient fill method is out of range.");
            e->viewerContext.view->SetBgGradientColors(
                color(r1, g1, b1),
                color(r2, g2, b2),
                static_cast<Aspect_GradientFillMethod>(fillMethod),
                Standard_False);
            e->requestRedraw();
        });
    }

    int occt_show_all(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            for (auto& pair : e->scene.objects)
                if (!pair.second.presentation.IsNull())
                    e->viewerContext.context->Display(pair.second.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_hide_all(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            for (auto& pair : e->scene.objects)
                if (!pair.second.presentation.IsNull())
                    e->viewerContext.context->Erase(pair.second.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_redisplay_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            e->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
            e->requestRedraw();
        });
    }

    int occt_highlight_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            e->viewerContext.context->HilightWithColor(
                entry.presentation,
                e->viewerContext.context->HighlightStyle(),
                Standard_False);
            e->requestRedraw();
        });
    }

    int occt_unhighlight_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            e->viewerContext.context->Unhilight(entry.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    OcctObjectId occt_copy_selected_subshape_at(OcctHandle h, int index)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || index < 0) return 0;
        return executeObject(e, [&]() -> OcctObjectId
        {
            int current = 0;
            for (e->viewerContext.context->InitSelected();
                 e->viewerContext.context->MoreSelected();
                 e->viewerContext.context->NextSelected(), ++current)
            {
                if (current != index) continue;
                if (!e->viewerContext.context->HasSelectedShape())
                    throw std::runtime_error("The selected item has no topological shape.");
                const TopoDS_Shape selected = e->viewerContext.context->SelectedShape();
                if (selected.IsNull())
                    throw std::runtime_error("The selected topological subshape is null.");
                return e->addShape(selected, false, "SelectedSubshape");
            }
            throw std::out_of_range("Selected subshape index is out of range.");
        });
    }
}
