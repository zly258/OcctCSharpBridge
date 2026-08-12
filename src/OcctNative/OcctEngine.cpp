#include "OcctInternal.hxx"

#include <AIS_SelectionScheme.hxx>
#include <Aspect_PolygonOffsetMode.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <BRepBndLib.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <BRepTools.hxx>
#include <Bnd_Box.hxx>
#include <GProp_GProps.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_AspectFillArea3d.hxx>
#include <Graphic3d_MaterialAspect.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_NameOfMaterial.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Precision.hxx>
#include <Standard_Version.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <V3d_ListOfLight.hxx>
#include <V3d_TypeOfOrientation.hxx>

#include <fstream>

namespace OcctBridge
{
    bool Engine::isInitialized() const { return !view.IsNull() && !context.IsNull(); }
    void Engine::clearError() { lastError.clear(); }
    void Engine::setError(const std::string& message) { lastError = message; }

    bool Engine::isUpdating() const { return updateDepth > 0; }

    void Engine::beginUpdate()
    {
        ++updateDepth;
    }

    void Engine::requestRedraw()
    {
        if (isUpdating())
        {
            redrawPending = true;
            return;
        }
        view->Redraw();
    }

    void Engine::requestFitAll()
    {
        if (isUpdating())
        {
            fitAllPending = true;
            redrawPending = true;
            return;
        }
        view->FitAll(0.01, Standard_False);
        view->ZFitAll();
        view->Redraw();
    }

    void Engine::endUpdate(bool fitAll)
    {
        if (updateDepth <= 0) throw std::logic_error("No OCCT display batch is active.");
        if (fitAll)
        {
            fitAllPending = true;
            redrawPending = true;
        }
        --updateDepth;
        if (updateDepth > 0) return;

        if (fitAllPending)
        {
            view->FitAll(0.01, Standard_False);
            view->ZFitAll();
        }
        if (fitAllPending || redrawPending) view->Redraw();
        fitAllPending = false;
        redrawPending = false;
    }

    ObjectEntry* Engine::findObject(OcctObjectId id)
    {
        const auto iterator = objects.find(id);
        return iterator == objects.end() ? nullptr : &iterator->second;
    }

    const ObjectEntry* Engine::findObject(OcctObjectId id) const
    {
        const auto iterator = objects.find(id);
        return iterator == objects.end() ? nullptr : &iterator->second;
    }

    ObjectEntry* Engine::findShape(OcctObjectId id)
    {
        ObjectEntry* entry = findObject(id);
        return entry != nullptr && entry->kind == OcctObject_Shape && !entry->shape.IsNull() ? entry : nullptr;
    }

    const ObjectEntry* Engine::findShape(OcctObjectId id) const
    {
        const ObjectEntry* entry = findObject(id);
        return entry != nullptr && entry->kind == OcctObject_Shape && !entry->shape.IsNull() ? entry : nullptr;
    }

    OcctObjectId Engine::findPresentation(const Handle(AIS_InteractiveObject)& presentation) const
    {
        if (presentation.IsNull()) return 0;
        for (const auto& pair : objects)
        {
            if (pair.second.presentation == presentation) return pair.first;
        }
        return 0;
    }

    void Engine::applySelectionMode(const Handle(AIS_InteractiveObject)& presentation)
    {
        if (presentation.IsNull()) return;
        context->Deactivate(presentation);
        int mode = 0;
        const Handle(AIS_Shape) aisShape = Handle(AIS_Shape)::DownCast(presentation);
        if (!aisShape.IsNull() && selectionMode != OcctSelection_Object)
        {
            TopAbs_ShapeEnum type = TopAbs_SHAPE;
            switch (selectionMode)
            {
                case OcctSelection_Vertex: type = TopAbs_VERTEX; break;
                case OcctSelection_Edge: type = TopAbs_EDGE; break;
                case OcctSelection_Wire: type = TopAbs_WIRE; break;
                case OcctSelection_Face: type = TopAbs_FACE; break;
                case OcctSelection_Shell: type = TopAbs_SHELL; break;
                case OcctSelection_Solid: type = TopAbs_SOLID; break;
                default: break;
            }
            mode = AIS_Shape::SelectionMode(type);
        }
        context->Activate(presentation, mode, Standard_False);
    }

    OcctObjectId Engine::addShape(const TopoDS_Shape& shape, bool /*fit*/, const std::string& name)
    {
        if (shape.IsNull()) throw std::runtime_error("OCCT returned a null shape.");
        if (!isInitialized()) throw std::runtime_error("The OCCT viewer has not been initialized.");
        const OcctObjectId id = nextId++;
        Handle(AIS_Shape) ais = new AIS_Shape(shape);
        ais->SetDisplayMode(displayMode);
        context->Display(ais, Standard_False);
        applySelectionMode(ais);
        objects.emplace(id, ObjectEntry{OcctObject_Shape, shape, ais, name});
        // Shape creation changes the scene but must not change the user's camera.
        // Fit/FitAll remain explicit public view operations.
        requestRedraw();
        return id;
    }

    OcctObjectId Engine::addPresentation(const Handle(AIS_InteractiveObject)& presentation, int kind, const std::string& name)
    {
        if (presentation.IsNull()) throw std::runtime_error("OCCT returned a null presentation.");
        const OcctObjectId id = nextId++;
        context->Display(presentation, Standard_False);
        applySelectionMode(presentation);
        objects.emplace(id, ObjectEntry{kind, TopoDS_Shape(), presentation, name});
        requestRedraw();
        return id;
    }

    void Engine::hide(OcctObjectId id)
    {
        ObjectEntry* entry = findObject(id);
        if (entry != nullptr && !entry->presentation.IsNull()) context->Erase(entry->presentation, Standard_False);
    }

    void Engine::erase(OcctObjectId id)
    {
        auto iterator = objects.find(id);
        if (iterator == objects.end()) return;
        if (!iterator->second.presentation.IsNull()) context->Remove(iterator->second.presentation, Standard_False);
        objects.erase(iterator);
    }

    Engine* engineOf(OcctHandle handle) { return static_cast<Engine*>(handle); }

    bool validateInitialized(Engine* engine)
    {
        if (engine == nullptr) return false;
        engine->clearError();
        if (!engine->isInitialized())
        {
            engine->setError("The OCCT viewer has not been initialized.");
            return false;
        }
        return true;
    }

    std::string failureMessage(const Standard_Failure& failure)
    {
        const char* message = failure.GetMessageString();
        return message == nullptr ? "Open CASCADE operation failed." : std::string(message);
    }

    std::filesystem::path pathFromUtf8(const char* utf8Path)
    {
        if (utf8Path == nullptr || *utf8Path == '\0') return {};
#if defined(_WIN32)
        return std::filesystem::u8path(utf8Path);
#else
        return std::filesystem::path(utf8Path);
#endif
    }

    std::string lowerExtension(const std::filesystem::path& path)
    {
        std::string value = path.extension().u8string();
        std::transform(value.begin(), value.end(), value.begin(), [](unsigned char ch) { return static_cast<char>(std::tolower(ch)); });
        return value;
    }

    Quantity_Color color(double r, double g, double b)
    {
        return Quantity_Color(std::clamp(r, 0.0, 1.0), std::clamp(g, 0.0, 1.0), std::clamp(b, 0.0, 1.0), Quantity_TOC_RGB);
    }

    gp_Pnt point(OcctPoint3d value) { return gp_Pnt(value.x, value.y, value.z); }
    gp_Vec vector(OcctVector3d value) { return gp_Vec(value.x, value.y, value.z); }

    gp_Dir direction(OcctVector3d value)
    {
        gp_Vec v = vector(value);
        if (v.SquareMagnitude() <= Precision::SquareConfusion()) throw std::invalid_argument("Direction vector must not be zero.");
        return gp_Dir(v);
    }

    gp_Ax2 axis2(OcctPoint3d origin, OcctVector3d normal) { return gp_Ax2(point(origin), direction(normal)); }
    gp_Ax2 axis2(OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection) { return gp_Ax2(point(origin), direction(normal), direction(xDirection)); }

    TopAbs_ShapeEnum shapeEnum(int shapeType)
    {
        switch (shapeType)
        {
            case OcctShape_Compound: return TopAbs_COMPOUND;
            case OcctShape_CompSolid: return TopAbs_COMPSOLID;
            case OcctShape_Solid: return TopAbs_SOLID;
            case OcctShape_Shell: return TopAbs_SHELL;
            case OcctShape_Face: return TopAbs_FACE;
            case OcctShape_Wire: return TopAbs_WIRE;
            case OcctShape_Edge: return TopAbs_EDGE;
            case OcctShape_Vertex: return TopAbs_VERTEX;
            default: return TopAbs_SHAPE;
        }
    }

    int shapeTypeValue(const TopoDS_Shape& shape) { return static_cast<int>(shape.ShapeType()); }
    void requirePositive(double value, const char* name) { if (value <= 0.0) throw std::invalid_argument(std::string(name) + " must be greater than zero."); }
    void requireCount(int count, int minimum, const char* name) { if (count < minimum) throw std::invalid_argument(std::string(name) + " has too few items."); }

    TopoDS_Shape transformed(const TopoDS_Shape& source, const gp_Trsf& transform)
    {
        BRepBuilderAPI_Transform algorithm(source, transform, Standard_True);
        if (!algorithm.IsDone()) throw std::runtime_error("Shape transformation failed.");
        return algorithm.Shape();
    }

    void fillMassProperties(const GProp_GProps& properties, OcctMassProperties* result)
    {
        if (result == nullptr) throw std::invalid_argument("Result pointer is null.");
        const gp_Pnt center = properties.CentreOfMass();
        result->mass = properties.Mass();
        result->centerX = center.X();
        result->centerY = center.Y();
        result->centerZ = center.Z();
    }

    Graphic3d_NameOfMaterial materialName(int value)
    {
        if (value < static_cast<int>(Graphic3d_NameOfMaterial_Brass) || value > static_cast<int>(Graphic3d_NameOfMaterial_DEFAULT))
        {
            throw std::invalid_argument("Material value is out of range.");
        }
        return static_cast<Graphic3d_NameOfMaterial>(value);
    }

    void removeAllLights(const Handle(V3d_Viewer)& viewer)
    {
        V3d_ListOfLight lights = viewer->DefinedLights();
        for (V3d_ListOfLight::Iterator iterator(lights); iterator.More(); iterator.Next())
        {
            viewer->DelLight(iterator.Value());
        }
    }
}

using namespace OcctBridge;

extern "C"
{
    OcctHandle occt_create() { try { return new Engine(); } catch (...) { return nullptr; } }
    void occt_destroy(OcctHandle handle) { delete engineOf(handle); }
    const char* occt_last_error(OcctHandle handle) { Engine* engine = engineOf(handle); return engine == nullptr ? "Invalid OCCT engine handle." : engine->lastError.c_str(); }
    const char* occt_version() { return OCC_VERSION_COMPLETE; }

    int occt_initialize(OcctHandle handle, void* windowHandle)
    {
        Engine* engine = engineOf(handle);
        return execute(engine, [&]()
        {
            if (windowHandle == nullptr) throw std::invalid_argument("The target HWND is null.");
            engine->displayConnection = new Aspect_DisplayConnection();
            engine->graphicDriver = new OpenGl_GraphicDriver(engine->displayConnection);
            engine->viewer = new V3d_Viewer(engine->graphicDriver);
            engine->viewer->SetDefaultLights();
            engine->viewer->SetLightOn();
            engine->viewer->SetDefaultTypeOfView(V3d_ORTHOGRAPHIC);
            engine->context = new AIS_InteractiveContext(engine->viewer);
            engine->view = engine->viewer->CreateView();
            engine->view->SetAutoZFitMode(Standard_True, 1.0);
            const Handle(Prs3d_Drawer)& defaultDrawer = engine->context->DefaultDrawer();
            defaultDrawer->SetupOwnShadingAspect();
            defaultDrawer->ShadingAspect()->Aspect()->SetPolygonOffsets(
                Aspect_POM_Fill, 1.0f, 1.0f);
            engine->window = new WNT_Window(reinterpret_cast<Aspect_Handle>(windowHandle));
            engine->view->SetWindow(engine->window);
            if (!engine->window->IsMapped()) engine->window->Map();
            engine->view->SetBackgroundColor(color(0.94, 0.96, 0.98));
            engine->view->TriedronDisplay(Aspect_TOTP_RIGHT_LOWER, Quantity_NOC_GRAY40, 0.08, V3d_ZBUFFER);

            engine->viewCube = new AIS_ViewCube();
            engine->viewCube->SetSize(55.0);
            engine->viewCube->SetBoxFacetExtension(6.0);
            engine->viewCube->SetFontHeight(14.0);
            engine->viewCube->SetAutoStartAnimation(true);
            engine->viewCube->SetResetCamera(true);
            engine->viewCube->SetFitSelected(false);
            engine->viewCube->SetTransformPersistence(
                new Graphic3d_TransformPers(
                    Graphic3d_TMF_TriedronPers,
                    Aspect_TOTP_RIGHT_UPPER,
                    Graphic3d_Vec2i(85, 85)));
            engine->context->Display(engine->viewCube, Standard_False);

            engine->view->SetProj(V3d_XposYnegZpos);
            engine->view->MustBeResized();
            engine->view->Redraw();
        });
    }

    int occt_resize(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->MustBeResized(); e->view->Redraw(); }); }
    int occt_redraw(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->requestRedraw(); }); }
    int occt_begin_update(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->beginUpdate(); }); }
    int occt_end_update(OcctHandle h, int fitAll) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->endUpdate(fitAll != 0); }); }
    int occt_is_updating(OcctHandle h) { Engine* e = engineOf(h); return e != nullptr && e->isUpdating() ? 1 : 0; }
    int occt_fit_all(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->requestFitAll(); }); }

    int occt_fit_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findShape(id); if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            Bnd_Box box; BRepBndLib::Add(entry->shape, box); e->view->FitAll(box, 0.05, Standard_True); e->view->ZFitAll();
        });
    }

    int occt_window_fit(OcctHandle h, int x1, int y1, int x2, int y2) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->WindowFit(x1, y1, x2, y2); }); }

    int occt_set_view(OcctHandle h, int orientation)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            V3d_TypeOfOrientation value = V3d_XposYnegZpos;
            switch (orientation)
            {
                case OcctView_Front: value = V3d_Yneg; break; case OcctView_Back: value = V3d_Ypos; break;
                case OcctView_Left: value = V3d_Xneg; break; case OcctView_Right: value = V3d_Xpos; break;
                case OcctView_Top: value = V3d_Zpos; break; case OcctView_Bottom: value = V3d_Zneg; break; default: break;
            }
            e->view->SetProj(value); e->view->FitAll(0.01, Standard_True); e->view->ZFitAll();
        });
    }

    int occt_set_projection(OcctHandle h, int type)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->view->Camera()->SetProjectionType(type == OcctProjection_Perspective ? Graphic3d_Camera::Projection_Perspective : Graphic3d_Camera::Projection_Orthographic);
            e->view->Redraw();
        });
    }

    int occt_set_perspective_fov(OcctHandle h, double degrees)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { if (degrees <= 1.0 || degrees >= 179.0) throw std::invalid_argument("FOV must be between 1 and 179 degrees."); e->view->Camera()->SetFOVy(degrees); e->view->Redraw(); });
    }

    int occt_set_background(OcctHandle h, double r, double g, double b) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->SetBackgroundColor(color(r,g,b)); e->view->Redraw(); }); }

    int occt_set_display_mode(OcctHandle h, int mode)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->displayMode = mode == OcctDisplay_Wireframe ? AIS_WireFrame : AIS_Shaded;
            for (auto& pair : e->objects) if (pair.second.kind == OcctObject_Shape) e->context->SetDisplayMode(pair.second.presentation, e->displayMode, Standard_False);
            e->view->Redraw();
        });
    }

    int occt_set_triedron_visible(OcctHandle h, int visible)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { if (visible) e->view->TriedronDisplay(Aspect_TOTP_RIGHT_LOWER, Quantity_NOC_GRAY40, 0.08, V3d_ZBUFFER); else e->view->TriedronErase(); e->view->Redraw(); });
    }

    int occt_set_view_cube_visible(OcctHandle h, int visible)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (e->viewCube.IsNull()) throw std::runtime_error("The view cube has not been initialized.");
            if (visible != 0)
            {
                e->context->Display(e->viewCube, Standard_False);
            }
            else
            {
                e->context->Erase(e->viewCube, Standard_False);
            }
            e->view->Redraw();
        });
    }

    int occt_set_computed_mode(OcctHandle h, int enabled) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->SetComputedMode(enabled != 0); e->view->Redraw(); }); }

    int occt_set_display_precision(OcctHandle h, double deviationCoefficient, double deviationAngleDegrees, int applyExisting)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(deviationCoefficient, "Deviation coefficient");
            if (deviationAngleDegrees <= 0.0 || deviationAngleDegrees >= 90.0) throw std::invalid_argument("Deviation angle must be between 0 and 90 degrees.");
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
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
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

    int occt_set_scene_lighting(OcctHandle h, double ambientIntensity, double directionalIntensity, OcctVector3d lightDirection, int headlight)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (ambientIntensity < 0.0 || ambientIntensity > 10.0 || directionalIntensity <= 0.0 || directionalIntensity > 10.0)
                throw std::invalid_argument("Light intensity is out of range.");

            removeAllLights(e->viewer);
            e->customAmbientLight.Nullify();
            e->customDirectionalLight.Nullify();

            e->customAmbientLight = new V3d_AmbientLight(Quantity_NOC_WHITE);
            e->customAmbientLight->SetIntensity(static_cast<Standard_ShortReal>(ambientIntensity));
            e->customDirectionalLight = new V3d_DirectionalLight(direction(lightDirection), Quantity_NOC_WHITE, headlight != 0);
            e->customDirectionalLight->SetIntensity(static_cast<Standard_ShortReal>(directionalIntensity));
            e->viewer->AddLight(e->customAmbientLight);
            e->viewer->AddLight(e->customDirectionalLight);
            e->viewer->SetLightOn(e->customAmbientLight);
            e->viewer->SetLightOn(e->customDirectionalLight);
            e->viewer->UpdateLights();
            e->view->Redraw();
        });
    }

    int occt_reset_scene_lighting(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            removeAllLights(e->viewer);
            e->customAmbientLight.Nullify();
            e->customDirectionalLight.Nullify();
            e->viewer->SetDefaultLights();
            e->viewer->SetLightOn();
            e->viewer->UpdateLights();
            e->view->Redraw();
        });
    }

    int occt_set_selection_tolerance(OcctHandle h, int pixelTolerance)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (pixelTolerance < 0 || pixelTolerance > 100) throw std::invalid_argument("Selection tolerance must be between 0 and 100 pixels.");
            e->context->SetPixelTolerance(pixelTolerance);
        });
    }

    int occt_dump_view(OcctHandle h, const char* path)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { const auto p = pathFromUtf8(path); if (p.empty()) throw std::invalid_argument("Path is empty."); if (!e->view->Dump(p.string().c_str())) throw std::runtime_error("View image export failed."); });
    }

    int occt_screen_to_world(OcctHandle h, int x, int y, OcctPoint3d* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&] { e->view->Convert(x, y, result->x, result->y, result->z); });
    }

    int occt_world_to_screen(OcctHandle h, OcctPoint3d p, int* x, int* y)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || x == nullptr || y == nullptr) return 0;
        return execute(e, [&] { e->view->Convert(p.x, p.y, p.z, *x, *y); });
    }

    int occt_move_to(OcctHandle h, int x, int y) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->context->MoveTo(x,y,e->view,Standard_True); }); }

    int occt_select(OcctHandle h, int x, int y, int append)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->context->MoveTo(x,y,e->view,Standard_False); e->context->SelectDetected(append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace); e->view->Redraw(); });
    }

    int occt_select_rectangle(OcctHandle h, int x1, int y1, int x2, int y2, int append)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Graphic3d_Vec2i minPoint(std::min(x1,x2), std::min(y1,y2)); Graphic3d_Vec2i maxPoint(std::max(x1,x2), std::max(y1,y2));
            e->context->SelectRectangle(minPoint, maxPoint, e->view, append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace); e->view->Redraw();
        });
    }

    int occt_select_object(OcctHandle h, OcctObjectId objectId, int append)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(objectId);
            if (entry == nullptr || entry->presentation.IsNull()) throw std::invalid_argument("Object ID does not exist.");
            if (!append) e->context->ClearSelected(Standard_False);
            e->context->SetSelected(entry->presentation, Standard_False);
            e->view->Redraw();
        });
    }

    int occt_set_selection_mode(OcctHandle h, int mode)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->selectionMode = mode; for (auto& pair : e->objects) e->applySelectionMode(pair.second.presentation); e->view->Redraw(); });
    }

    int occt_selected_count(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        int count = 0; for (e->context->InitSelected(); e->context->MoreSelected(); e->context->NextSelected()) ++count; return count;
    }

    OcctObjectId occt_selected_at(OcctHandle h, int index)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || index < 0) return 0;
        int current = 0; for (e->context->InitSelected(); e->context->MoreSelected(); e->context->NextSelected(), ++current) if (current == index) return e->findPresentation(e->context->SelectedInteractive()); return 0;
    }

    OcctObjectId occt_first_selected(OcctHandle h) { return occt_selected_at(h, 0); }
    int occt_clear_selection(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->context->ClearSelected(Standard_True); }); }
    int occt_start_rotation(OcctHandle h, int x, int y) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->StartRotation(x,y,0.4); }); }
    int occt_rotation(OcctHandle h, int x, int y) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->Rotation(x,y); }); }
    int occt_pan(OcctHandle h, int dx, int dy) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->Pan(dx,dy); }); }
    int occt_zoom(OcctHandle h, double factor) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { requirePositive(factor,"Zoom factor"); e->view->SetZoom(factor,Standard_True); }); }

    int occt_object_count(OcctHandle h) { Engine* e = engineOf(h); return e == nullptr ? 0 : static_cast<int>(e->objects.size()); }
    OcctObjectId occt_object_id_at(OcctHandle h, int index)
    {
        Engine* e = engineOf(h); if (e == nullptr || index < 0) return 0;
        std::vector<OcctObjectId> ids; ids.reserve(e->objects.size());
        for (const auto& pair : e->objects) ids.push_back(pair.first);
        std::sort(ids.begin(), ids.end());
        return index < static_cast<int>(ids.size()) ? ids[static_cast<std::size_t>(index)] : 0;
    }

    OcctObjectId occt_shape_id_at(OcctHandle h, int index)
    {
        Engine* e = engineOf(h); if (e == nullptr || index < 0) return 0;
        std::vector<OcctObjectId> ids;
        for (const auto& pair : e->objects) if (pair.second.kind == OcctObject_Shape) ids.push_back(pair.first);
        std::sort(ids.begin(), ids.end());
        return index < static_cast<int>(ids.size()) ? ids[static_cast<std::size_t>(index)] : 0;
    }
    int occt_object_exists(OcctHandle h, OcctObjectId id) { Engine* e = engineOf(h); return e != nullptr && e->findObject(id) != nullptr ? 1 : 0; }
    int occt_object_kind(OcctHandle h, OcctObjectId id) { Engine* e = engineOf(h); const ObjectEntry* entry = e == nullptr ? nullptr : e->findObject(id); return entry == nullptr ? OcctObject_Unknown : entry->kind; }

    int occt_set_object_name(OcctHandle h, OcctObjectId id, const char* name)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { ObjectEntry* entry=e->findObject(id); if (!entry) throw std::invalid_argument("Object ID does not exist."); entry->name=name?name:""; });
    }

    const char* occt_get_object_name(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (e == nullptr) return ""; const ObjectEntry* entry=e->findObject(id); e->scratchString=entry?entry->name:""; return e->scratchString.c_str();
    }

    int occt_set_object_color(OcctHandle h, OcctObjectId id, double r, double g, double b) { Engine* e=engineOf(h); if(!validateInitialized(e))return 0; return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetColor(o->presentation,color(r,g,b),Standard_False);e->requestRedraw();}); }
    int occt_set_object_transparency(OcctHandle h, OcctObjectId id, double value) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetTransparency(o->presentation,std::clamp(value,0.0,1.0),Standard_False);e->requestRedraw();}); }
    int occt_set_object_visible(OcctHandle h, OcctObjectId id, int visible) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");if(visible)e->context->Display(o->presentation,Standard_False);else e->context->Erase(o->presentation,Standard_False);e->requestRedraw();}); }
    int occt_set_object_display_mode(OcctHandle h, OcctObjectId id, int mode) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetDisplayMode(o->presentation,mode==OcctDisplay_Wireframe?AIS_WireFrame:AIS_Shaded,Standard_False);e->requestRedraw();}); }
    int occt_set_object_line_width(OcctHandle h, OcctObjectId id, double width) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{requirePositive(width,"Line width");ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetWidth(o->presentation,width,Standard_False);e->requestRedraw();}); }
    int occt_set_object_material(OcctHandle h, OcctObjectId id, int material)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]
        {
            ObjectEntry* entry=e->findObject(id);
            if(!entry || entry->presentation.IsNull()) throw std::invalid_argument("Object ID does not exist.");
            e->context->SetMaterial(entry->presentation, Graphic3d_MaterialAspect(materialName(material)), Standard_False);
            e->requestRedraw();
        });
    }
    int occt_delete_object(OcctHandle h, OcctObjectId id) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{e->erase(id);e->requestRedraw();}); }
    int occt_clear(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            for (auto& pair : e->objects)
            {
                if (!pair.second.presentation.IsNull())
                {
                    e->context->Remove(pair.second.presentation, Standard_False);
                }
            }
            e->objects.clear();
            e->nextId = 1;
            e->context->ClearSelected(Standard_False);
            e->requestRedraw();
        });
    }

    int occt_shape_type(OcctHandle h, OcctObjectId id) { Engine* e=engineOf(h);const ObjectEntry* o=e?e->findShape(id):nullptr;return o?shapeTypeValue(o->shape):OcctShape_Shape; }
    int occt_shape_is_valid(OcctHandle h, OcctObjectId id) { Engine* e=engineOf(h);const ObjectEntry* o=e?e->findShape(id):nullptr;return o && BRepCheck_Analyzer(o->shape,Standard_True).IsValid()?1:0; }

    int occt_shape_bounds(OcctHandle h, OcctObjectId id, OcctBounds* result)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e)||!result)return 0;return execute(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");Bnd_Box box;BRepBndLib::Add(o->shape,box);box.Get(result->minX,result->minY,result->minZ,result->maxX,result->maxY,result->maxZ);});
    }

    int occt_shape_linear_properties(OcctHandle h, OcctObjectId id, OcctMassProperties* result) { Engine* e=engineOf(h);if(!validateInitialized(e)||!result)return 0;return execute(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");GProp_GProps p;BRepGProp::LinearProperties(o->shape,p);fillMassProperties(p,result);}); }
    int occt_shape_surface_properties(OcctHandle h, OcctObjectId id, OcctMassProperties* result) { Engine* e=engineOf(h);if(!validateInitialized(e)||!result)return 0;return execute(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");GProp_GProps p;BRepGProp::SurfaceProperties(o->shape,p);fillMassProperties(p,result);}); }
    int occt_shape_volume_properties(OcctHandle h, OcctObjectId id, OcctMassProperties* result) { Engine* e=engineOf(h);if(!validateInitialized(e)||!result)return 0;return execute(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");GProp_GProps p;BRepGProp::VolumeProperties(o->shape,p);fillMassProperties(p,result);}); }

    int occt_topology_count(OcctHandle h, OcctObjectId id, int type)
    {
        Engine* e=engineOf(h);const ObjectEntry* o=e?e->findShape(id):nullptr;if(!o)return 0;int count=0;for(TopExp_Explorer ex(o->shape,shapeEnum(type));ex.More();ex.Next())++count;return count;
    }

    OcctObjectId occt_get_subshape(OcctHandle h, OcctObjectId id, int type, int index)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e)||index<0)return 0;return executeObject(e,[&]()->OcctObjectId{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");int i=0;for(TopExp_Explorer ex(o->shape,shapeEnum(type));ex.More();ex.Next(),++i)if(i==index)return e->addShape(ex.Current(),false,"Subshape");throw std::out_of_range("Subshape index is out of range.");});
    }

    OcctObjectId occt_copy_shape(OcctHandle h, OcctObjectId id, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");BRepBuilderAPI_Copy copy(o->shape);if(!copy.IsDone())throw std::runtime_error("Shape copy failed.");const OcctObjectId result=e->addShape(copy.Shape(),false,"Copy");if(hideInput)e->hide(id);return result;});
    }

    OcctObjectId occt_translate(OcctHandle h, OcctObjectId id, OcctVector3d v, int hideInput) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");gp_Trsf t;t.SetTranslation(vector(v));auto result=e->addShape(transformed(o->shape,t),false,"Translated");if(hideInput)e->hide(id);return result;}); }
    OcctObjectId occt_rotate(OcctHandle h, OcctObjectId id, OcctPoint3d p, OcctVector3d d, double degrees, int hideInput) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");gp_Trsf t;t.SetRotation(gp_Ax1(point(p),direction(d)),degrees*3.14159265358979323846/180.0);auto result=e->addShape(transformed(o->shape,t),false,"Rotated");if(hideInput)e->hide(id);return result;}); }
    OcctObjectId occt_scale(OcctHandle h, OcctObjectId id, OcctPoint3d center, double factor, int hideInput) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{requirePositive(factor,"Scale factor");ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");gp_Trsf t;t.SetScale(point(center),factor);auto result=e->addShape(transformed(o->shape,t),false,"Scaled");if(hideInput)e->hide(id);return result;}); }
    OcctObjectId occt_mirror_plane(OcctHandle h, OcctObjectId id, OcctPoint3d p, OcctVector3d normal, int hideInput) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{ObjectEntry* o=e->findShape(id);if(!o)throw std::invalid_argument("Shape ID does not exist.");gp_Trsf t;t.SetMirror(gp_Ax2(point(p),direction(normal)));auto result=e->addShape(transformed(o->shape,t),false,"Mirrored");if(hideInput)e->hide(id);return result;}); }

    int occt_set_shape_color(OcctHandle h, OcctObjectId id, double r, double g, double b) { return occt_set_object_color(h,id,r,g,b); }
    int occt_set_shape_transparency(OcctHandle h, OcctObjectId id, double value) { return occt_set_object_transparency(h,id,value); }
    int occt_set_shape_visible(OcctHandle h, OcctObjectId id, int visible) { return occt_set_object_visible(h,id,visible); }
    int occt_delete_shape(OcctHandle h, OcctObjectId id) { return occt_delete_object(h,id); }
    int occt_shape_count(OcctHandle h) { Engine* e=engineOf(h);if(!e)return 0;int count=0;for(const auto& p:e->objects)if(p.second.kind==OcctObject_Shape)++count;return count; }
}
