#include "OcctInternal.hxx"

#include <Aspect_GradientFillMethod.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_MaterialAspect.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>
#include <V3d_ListOfLight.hxx>
#include <V3d_TypeOfOrientation.hxx>

using namespace OcctBridge;

namespace
{
    void removeAllLights(const Handle(V3d_Viewer)& viewer)
    {
        V3d_ListOfLight lights = viewer->DefinedLights();
        for (V3d_ListOfLight::Iterator iterator(lights); iterator.More(); iterator.Next())
        {
            viewer->DelLight(iterator.Value());
        }
    }
}

extern "C"
{
    int occt_fit_all(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->requestFitAll(); });
    }

    int occt_fit_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            Bnd_Box box;
            BRepBndLib::Add(shapeWithPresentationTransformation(*entry), box);
            e->view->FitAll(box, 0.05, Standard_True);
            e->view->ZFitAll();
        });
    }

    int occt_window_fit(OcctHandle h, int x1, int y1, int x2, int y2)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->view->WindowFit(x1, y1, x2, y2); });
    }

    int occt_set_view(OcctHandle h, int orientation)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            V3d_TypeOfOrientation value = V3d_XposYnegZpos;
            switch (orientation)
            {
                case OcctView_Front: value = V3d_Yneg; break;
                case OcctView_Back: value = V3d_Ypos; break;
                case OcctView_Left: value = V3d_Xneg; break;
                case OcctView_Right: value = V3d_Xpos; break;
                case OcctView_Top: value = V3d_Zpos; break;
                case OcctView_Bottom: value = V3d_Zneg; break;
                default: break;
            }
            e->view->SetProj(value);
            e->view->FitAll(0.01, Standard_True);
            e->view->ZFitAll();
        });
    }

    int occt_set_projection(OcctHandle h, int type)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->view->Camera()->SetProjectionType(
                type == OcctProjection_Perspective
                    ? Graphic3d_Camera::Projection_Perspective
                    : Graphic3d_Camera::Projection_Orthographic);
            e->view->Redraw();
        });
    }

    int occt_set_perspective_fov(OcctHandle h, double degrees)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (degrees <= 1.0 || degrees >= 179.0)
                throw std::invalid_argument("FOV must be between 1 and 179 degrees.");
            e->view->Camera()->SetFOVy(degrees);
            e->view->Redraw();
        });
    }

    int occt_set_background(OcctHandle h, double r, double g, double b)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->view->SetBgGradientStyle(Aspect_GradientFillMethod_None, Standard_False);
            e->view->SetBackgroundColor(color(r, g, b));
            e->view->Redraw();
        });
    }

    int occt_set_display_mode(OcctHandle h, int mode)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->displayMode = mode == OcctDisplay_Wireframe ? AIS_WireFrame : AIS_Shaded;
            for (auto& pair : e->objects)
            {
                if (pair.second.kind == OcctObject_Shape)
                    e->context->SetDisplayMode(pair.second.presentation, e->displayMode, Standard_False);
            }
            e->view->Redraw();
        });
    }

    int occt_set_triedron_visible(OcctHandle h, int visible)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (visible)
                e->view->TriedronDisplay(Aspect_TOTP_RIGHT_LOWER, Quantity_NOC_GRAY40, 0.08, V3d_ZBUFFER);
            else
                e->view->TriedronErase();
            e->view->Redraw();
        });
    }

    int occt_set_view_cube_visible(OcctHandle h, int visible)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (e->viewCube.IsNull()) throw std::runtime_error("The view cube has not been initialized.");
            if (visible != 0)
                e->context->Display(e->viewCube, Standard_False);
            else
                e->context->Erase(e->viewCube, Standard_False);
            e->view->Redraw();
        });
    }

    int occt_set_computed_mode(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->view->SetComputedMode(enabled != 0);
            e->view->Redraw();
        });
    }

    int occt_set_display_precision(OcctHandle h, double deviationCoefficient, double deviationAngleDegrees, int applyExisting)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(deviationCoefficient, "Deviation coefficient");
            if (deviationAngleDegrees <= 0.0 || deviationAngleDegrees >= 90.0)
                throw std::invalid_argument("Deviation angle must be between 0 and 90 degrees.");

            const double angleRadians = deviationAngleDegrees * 3.14159265358979323846 / 180.0;
            const Handle(Prs3d_Drawer)& drawer = e->context->DefaultDrawer();
            drawer->SetDeviationCoefficient(deviationCoefficient);
            drawer->SetDeviationAngle(angleRadians);
            if (applyExisting != 0)
            {
                for (auto& pair : e->objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    e->context->SetDeviationCoefficient(pair.second.presentation, deviationCoefficient, Standard_False);
                    e->context->SetDeviationAngle(pair.second.presentation, angleRadians, Standard_False);
                    e->context->Redisplay(pair.second.presentation, Standard_False, Standard_True);
                }
            }
            e->view->Redraw();
        });
    }

    int occt_set_default_material(OcctHandle h, int material, int applyExisting)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            const Graphic3d_MaterialAspect aspect(materialName(material));
            const Handle(Prs3d_Drawer)& drawer = e->context->DefaultDrawer();
            drawer->SetupOwnShadingAspect();
            drawer->ShadingAspect()->SetMaterial(aspect);
            if (applyExisting != 0)
            {
                for (auto& pair : e->objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    e->context->SetMaterial(pair.second.presentation, aspect, Standard_False);
                }
            }
            e->view->Redraw();
        });
    }

    int occt_reset_scene_lighting(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            removeAllLights(e->viewer);
            e->customAmbientLight.Nullify();
            e->customDirectionalLight.Nullify();
            e->customSunLight.Nullify();
            e->customFillLight.Nullify();
            e->viewer->SetDefaultLights();
            e->viewer->SetLightOn();
            e->viewer->UpdateLights();
            e->view->Redraw();
        });
    }

    int occt_dump_view(OcctHandle h, const char* path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            const auto p = pathFromUtf8(path);
            if (p.empty()) throw std::invalid_argument("Path is empty.");
            if (!e->view->Dump(p.string().c_str())) throw std::runtime_error("View image export failed.");
        });
    }

    int occt_screen_to_world(OcctHandle h, int x, int y, OcctPoint3d* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&] { e->view->Convert(x, y, result->x, result->y, result->z); });
    }

    int occt_world_to_screen(OcctHandle h, OcctPoint3d p, int* x, int* y)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || x == nullptr || y == nullptr) return 0;
        return execute(e, [&] { e->view->Convert(p.x, p.y, p.z, *x, *y); });
    }

    int occt_start_rotation(OcctHandle h, int x, int y)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->view->StartRotation(x, y, 0.4); });
    }

    int occt_rotation(OcctHandle h, int x, int y)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->view->Rotation(x, y); });
    }

    int occt_pan(OcctHandle h, int dx, int dy)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->view->Pan(dx, dy); });
    }

    int occt_zoom(OcctHandle h, double factor)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(factor, "Zoom factor");
            e->view->SetZoom(factor, Standard_True);
        });
    }
}
