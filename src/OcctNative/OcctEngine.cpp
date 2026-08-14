#include "OcctInternal.hxx"
#include "platform/OcctNativeSurface.h"

#include <Aspect_PolygonOffsetMode.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <Graphic3d_AspectFillArea3d.hxx>
#include <Graphic3d_NameOfMaterial.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Precision.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>
#include <Standard_Version.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <V3d_TypeOfOrientation.hxx>
#include <cstring>


namespace OcctBridge
{
    bool Engine::isInitialized() const { return viewerContext.isInitialized(); }
    void Engine::clearError()
    {
        errors.clear();
    }
    void Engine::setError(const std::string& message)
    {
        setError(OcctStatus_ErrorUnknown, message);
    }
    void Engine::setError(OcctStatus code, const std::string& message)
    {
        errors.set(code, message);
    }

    bool Engine::isUpdating() const { return viewerContext.isUpdating(); }

    void Engine::beginUpdate()
    {
        viewerContext.beginUpdate();
    }

    void Engine::requestRedraw()
    {
        viewerContext.requestRedraw();
    }

    void Engine::requestFitAll()
    {
        viewerContext.requestFitAll();
    }

    void Engine::endUpdate(bool fitAll)
    {
        viewerContext.endUpdate(fitAll);
    }

    void Engine::invalidatePristineStepDocument()
    {
        documents.invalidatePristine();
    }

    ObjectEntry* Engine::findObject(OcctObjectId id)
    {
        return scene.findObject(id);
    }

    const ObjectEntry* Engine::findObject(OcctObjectId id) const
    {
        return scene.findObject(id);
    }

    ObjectEntry* Engine::findShape(OcctObjectId id)
    {
        return scene.findShape(id);
    }

    const ObjectEntry* Engine::findShape(OcctObjectId id) const
    {
        return scene.findShape(id);
    }

    OcctObjectId Engine::findPresentation(const Handle(AIS_InteractiveObject)& presentation) const
    {
        return scene.findPresentation(presentation);
    }

    void Engine::applySelectionMode(const Handle(AIS_InteractiveObject)& presentation)
    {
        if (presentation.IsNull()) return;
        viewerContext.context->Deactivate(presentation);
        const OcctObjectId objectId = findPresentation(presentation);
        const ObjectEntry* entry = findObject(objectId);
        if (entry != nullptr && !entry->selectable) return;

        int mode = 0;
        const Handle(AIS_Shape) aisShape = Handle(AIS_Shape)::DownCast(presentation);
        if (!aisShape.IsNull() && viewerContext.selectionMode != OcctSelection_Object)
        {
            TopAbs_ShapeEnum type = TopAbs_SHAPE;
            switch (viewerContext.selectionMode)
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
        viewerContext.context->Activate(presentation, mode, Standard_False);
    }

    OcctObjectId Engine::addShapePresentation(
        const TopoDS_Shape& shape,
        const Handle(AIS_InteractiveObject)& presentation,
        bool /*fit*/,
        const std::string& name)
    {
        if (shape.IsNull()) throw std::runtime_error("OCCT returned a null shape.");
        if (presentation.IsNull()) throw std::runtime_error("OCCT returned a null shape presentation.");
        if (!isInitialized()) throw std::runtime_error("The OCCT viewer has not been initialized.");

        const OcctObjectId id = scene.allocateId();
        const Handle(AIS_Shape) aisShape = Handle(AIS_Shape)::DownCast(presentation);
        if (!aisShape.IsNull()) aisShape->SetDisplayMode(viewerContext.displayMode);
        viewerContext.context->Display(presentation, Standard_False);
        scene.objects.emplace(id, ObjectEntry{OcctObject_Shape, shape, presentation, name});
        applySelectionMode(presentation);
        requestRedraw();
        return id;
    }

    OcctObjectId Engine::addShape(const TopoDS_Shape& shape, bool fit, const std::string& name)
    {
        invalidatePristineStepDocument();
        Handle(AIS_Shape) ais = new AIS_Shape(shape);
        return addShapePresentation(shape, ais, fit, name);
    }

    OcctObjectId Engine::addPresentation(const Handle(AIS_InteractiveObject)& presentation, int kind, const std::string& name)
    {
        if (presentation.IsNull()) throw std::runtime_error("OCCT returned a null presentation.");
        const OcctObjectId id = scene.allocateId();
        viewerContext.context->Display(presentation, Standard_False);
        scene.objects.emplace(id, ObjectEntry{kind, TopoDS_Shape(), presentation, name});
        applySelectionMode(presentation);
        requestRedraw();
        return id;
    }

    void Engine::hide(OcctObjectId id)
    {
        ObjectEntry* entry = findObject(id);
        if (entry != nullptr && !entry->presentation.IsNull())
            viewerContext.context->Erase(entry->presentation, Standard_False);
    }

    void Engine::erase(OcctObjectId id)
    {
        auto iterator = scene.objects.find(id);
        if (iterator == scene.objects.end()) return;
        if (iterator->second.kind == OcctObject_Shape) invalidatePristineStepDocument();
        if (!iterator->second.presentation.IsNull())
            viewerContext.context->Remove(iterator->second.presentation, Standard_False);
        if (!iterator->second.applicationTag.empty())
            scene.objectIdByApplicationTag.erase(iterator->second.applicationTag);
        scene.objects.erase(iterator);
    }

    Engine* engineOf(OcctHandle handle) { return static_cast<Engine*>(handle); }

    bool validateInitialized(Engine* engine)
    {
        if (engine == nullptr) return false;
        engine->clearError();
        if (!engine->isInitialized())
        {
            engine->setError(OcctStatus_ErrorNotInitialized, "The OCCT viewer has not been initialized.");
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
        std::transform(value.begin(), value.end(), value.begin(), [](unsigned char ch)
        {
            return static_cast<char>(std::tolower(ch));
        });
        return value;
    }

    Quantity_Color color(double r, double g, double b)
    {
        return Quantity_Color(
            std::clamp(r, 0.0, 1.0),
            std::clamp(g, 0.0, 1.0),
            std::clamp(b, 0.0, 1.0),
            Quantity_TOC_RGB);
    }

    gp_Pnt point(OcctPoint3d value) { return gp_Pnt(value.x, value.y, value.z); }
    gp_Vec vector(OcctVector3d value) { return gp_Vec(value.x, value.y, value.z); }

    gp_Dir direction(OcctVector3d value)
    {
        gp_Vec valueVector = vector(value);
        if (valueVector.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::invalid_argument("Direction vector must not be zero.");
        return gp_Dir(valueVector);
    }

    gp_Ax2 axis2(OcctPoint3d origin, OcctVector3d normal)
    {
        return gp_Ax2(point(origin), direction(normal));
    }

    gp_Ax2 axis2(OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection)
    {
        return gp_Ax2(point(origin), direction(normal), direction(xDirection));
    }

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

    int shapeTypeValue(const TopoDS_Shape& shape)
    {
        switch (shape.ShapeType())
        {
            case TopAbs_COMPOUND: return OcctShape_Compound;
            case TopAbs_COMPSOLID: return OcctShape_CompSolid;
            case TopAbs_SOLID: return OcctShape_Solid;
            case TopAbs_SHELL: return OcctShape_Shell;
            case TopAbs_FACE: return OcctShape_Face;
            case TopAbs_WIRE: return OcctShape_Wire;
            case TopAbs_EDGE: return OcctShape_Edge;
            case TopAbs_VERTEX: return OcctShape_Vertex;
            default: return OcctShape_Shape;
        }
    }

    void requirePositive(double value, const char* name)
    {
        if (value <= 0.0) throw std::invalid_argument(std::string(name) + " must be greater than zero.");
    }

    void requireCount(int count, int minimum, const char* name)
    {
        if (count < minimum) throw std::invalid_argument(std::string(name) + " has too few items.");
    }

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
        if (value < static_cast<int>(Graphic3d_NameOfMaterial_Brass)
            || value > static_cast<int>(Graphic3d_NameOfMaterial_DEFAULT))
        {
            throw std::invalid_argument("Material value is out of range.");
        }
        return static_cast<Graphic3d_NameOfMaterial>(value);
    }
}

using namespace OcctBridge;

extern "C"
{
    OcctEngineHandle occt_engine_create()
    {
        try { return reinterpret_cast<OcctEngineHandle>(new Engine()); }
        catch (...) { return nullptr; }
    }

    void occt_engine_destroy(OcctEngineHandle handle)
    {
        delete reinterpret_cast<Engine*>(handle);
    }

    OcctHandle occt_create()
    {
        return reinterpret_cast<OcctHandle>(occt_engine_create());
    }

    void occt_destroy(OcctHandle handle)
    {
        occt_engine_destroy(reinterpret_cast<OcctEngineHandle>(handle));
    }

    const char* occt_last_error(OcctHandle handle)
    {
        Engine* engine = engineOf(handle);
        return engine == nullptr ? "Invalid OCCT engine handle." : engine->errors.message.c_str();
    }

    OcctStatus occt_engine_last_error_code(OcctEngineHandle handle)
    {
        const Engine* engine = reinterpret_cast<const Engine*>(handle);
        return engine == nullptr ? OcctStatus_ErrorInvalidHandle : engine->errors.code;
    }

    OcctStatus occt_engine_last_error_message(
        OcctEngineHandle handle,
        char* buffer,
        int capacity,
        int* required)
    {
        const Engine* engine = reinterpret_cast<const Engine*>(handle);
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        const int size = static_cast<int>(engine->errors.message.size()) + 1;
        *required = size;
        if (buffer == nullptr) return capacity == 0 ? OcctStatus_Ok : OcctStatus_ErrorInvalidArgument;
        if (capacity < size) return OcctStatus_ErrorBufferTooSmall;
        std::memcpy(buffer, engine->errors.message.c_str(), static_cast<std::size_t>(size));
        return OcctStatus_Ok;
    }


    const char* occt_version() { return OCC_VERSION_COMPLETE; }
    int occt_bridge_abi_version() { return 4; }
    int occt_bridge_current_abi_version() { return 5; }
    const char* occt_bridge_version() { return "3.0.0-preview.1"; }

    const char* occt_bridge_build_info()
    {
        static const std::string info = []
        {
            std::string value = std::string("OcctCSharpBridge/3.0.0-preview.1; ABI=5; OCCT=") + OCC_VERSION_COMPLETE;
#if defined(_M_X64) || defined(__x86_64__)
            value += "; Arch=x64";
#elif defined(_M_ARM64) || defined(__aarch64__)
            value += "; Arch=arm64";
#else
            value += "; Arch=unknown";
#endif
#if defined(_MSC_VER)
            value += "; Compiler=MSVC " + std::to_string(_MSC_VER);
#elif defined(__clang__)
            value += "; Compiler=Clang " + std::string(__clang_version__);
#elif defined(__GNUC__)
            value += "; Compiler=GCC " + std::to_string(__GNUC__) + "." + std::to_string(__GNUC_MINOR__);
#else
            value += "; Compiler=unknown";
#endif
#if defined(_WIN32)
            value += "; OS=Windows";
#elif defined(__linux__)
            value += "; OS=Linux";
#else
            value += "; OS=unknown";
#endif
            return value;
        }();
        return info.c_str();
    }

    int occt_initialize(OcctHandle handle, void* windowHandle)
    {
        const OcctNativeSurface surface{
            static_cast<std::uint32_t>(sizeof(OcctNativeSurface)),
            1,
            OcctNativeSurface_Auto,
            windowHandle,
            nullptr};
        return occt_engine_initialize_surface(reinterpret_cast<OcctEngineHandle>(handle), &surface) == OcctStatus_Ok ? 1 : 0;
    }

    int occt_resize(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->viewerContext.view->MustBeResized();
            e->viewerContext.view->Redraw();
        });
    }

    int occt_redraw(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->requestRedraw(); });
    }

    int occt_begin_update(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->beginUpdate(); });
    }

    int occt_end_update(OcctHandle h, int fitAll)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->endUpdate(fitAll != 0); });
    }

    int occt_is_updating(OcctHandle h)
    {
        Engine* e = engineOf(h);
        return e != nullptr && e->isUpdating() ? 1 : 0;
    }
}
