#include "OcctInternal.hxx"

#include <Aspect_PolygonOffsetMode.hxx>
#include <Graphic3d_AspectFillArea3d.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>

#include <cmath>

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->presentation.IsNull())
        {
            throw std::invalid_argument("Object ID does not exist or has no presentation.");
        }
        return *entry;
    }

    void validatePolygonOffset(int mode, double factor, double units)
    {
        if (mode < static_cast<int>(Aspect_POM_Off)
            || (mode & ~static_cast<int>(Aspect_POM_All)) != 0)
        {
            throw std::invalid_argument("Polygon offset mode is out of range.");
        }
        if (!std::isfinite(factor) || !std::isfinite(units))
        {
            throw std::invalid_argument("Polygon offset factor and units must be finite.");
        }
    }

    void readDefaultPolygonOffset(
        Engine* engine,
        Standard_Integer& mode,
        Standard_ShortReal& factor,
        Standard_ShortReal& units)
    {
        const Handle(Prs3d_Drawer)& drawer = engine->context->DefaultDrawer();
        if (drawer.IsNull() || drawer->ShadingAspect().IsNull()
            || drawer->ShadingAspect()->Aspect().IsNull())
        {
            mode = Aspect_POM_Fill;
            factor = 1.0f;
            units = 1.0f;
            return;
        }
        drawer->ShadingAspect()->Aspect()->PolygonOffsets(mode, factor, units);
    }

    void writePolygonOffset(
        OcctPolygonOffsetSettings* result,
        Standard_Integer mode,
        Standard_ShortReal factor,
        Standard_ShortReal units)
    {
        if (result == nullptr) throw std::invalid_argument("Result pointer is null.");
        result->mode = mode;
        result->factor = static_cast<double>(factor);
        result->units = static_cast<double>(units);
    }
}

extern "C"
{
    int occt_set_auto_z_fit_mode(OcctHandle h, int enabled, double scaleFactor)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(scaleFactor) || scaleFactor <= 0.0)
                throw std::invalid_argument("Auto Z-fit scale factor must be finite and greater than zero.");
            e->view->SetAutoZFitMode(enabled != 0, scaleFactor);
            if (enabled != 0) e->view->AutoZFit();
            e->requestRedraw();
        });
    }

    int occt_get_auto_z_fit_mode(OcctHandle h, OcctAutoZFitSettings* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            result->enabled = e->view->AutoZFitMode() ? 1 : 0;
            result->scaleFactor = e->view->AutoZFitScaleFactor();
        });
    }

    int occt_auto_z_fit(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->view->AutoZFit();
            e->requestRedraw();
        });
    }

    int occt_set_default_polygon_offsets(
        OcctHandle h,
        int mode,
        double factor,
        double units,
        int applyExisting)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            validatePolygonOffset(mode, factor, units);
            const Standard_ShortReal nativeFactor = static_cast<Standard_ShortReal>(factor);
            const Standard_ShortReal nativeUnits = static_cast<Standard_ShortReal>(units);
            const Handle(Prs3d_Drawer)& drawer = e->context->DefaultDrawer();
            drawer->SetupOwnShadingAspect();
            drawer->ShadingAspect()->Aspect()->SetPolygonOffsets(mode, nativeFactor, nativeUnits);

            if (applyExisting != 0)
            {
                for (auto& pair : e->objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    e->context->SetPolygonOffsets(
                        pair.second.presentation,
                        mode,
                        nativeFactor,
                        nativeUnits,
                        Standard_False);
                }
            }
            e->requestRedraw();
        });
    }

    int occt_get_default_polygon_offsets(OcctHandle h, OcctPolygonOffsetSettings* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            Standard_Integer mode = Aspect_POM_Fill;
            Standard_ShortReal factor = 1.0f;
            Standard_ShortReal units = 1.0f;
            readDefaultPolygonOffset(e, mode, factor, units);
            writePolygonOffset(result, mode, factor, units);
        });
    }

    int occt_set_object_polygon_offsets(
        OcctHandle h,
        OcctObjectId id,
        int mode,
        double factor,
        double units)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            validatePolygonOffset(mode, factor, units);
            ObjectEntry& entry = requiredObject(e, id);
            e->context->SetPolygonOffsets(
                entry.presentation,
                mode,
                static_cast<Standard_ShortReal>(factor),
                static_cast<Standard_ShortReal>(units),
                Standard_False);
            e->requestRedraw();
        });
    }

    int occt_get_object_polygon_offsets(
        OcctHandle h,
        OcctObjectId id,
        OcctPolygonOffsetSettings* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            Standard_Integer mode = Aspect_POM_Fill;
            Standard_ShortReal factor = 1.0f;
            Standard_ShortReal units = 1.0f;
            if (e->context->HasPolygonOffsets(entry.presentation))
                e->context->PolygonOffsets(entry.presentation, mode, factor, units);
            else
                readDefaultPolygonOffset(e, mode, factor, units);
            writePolygonOffset(result, mode, factor, units);
        });
    }

    int occt_reset_object_polygon_offsets(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            Standard_Integer mode = Aspect_POM_Fill;
            Standard_ShortReal factor = 1.0f;
            Standard_ShortReal units = 1.0f;
            readDefaultPolygonOffset(e, mode, factor, units);
            e->context->SetPolygonOffsets(entry.presentation, mode, factor, units, Standard_False);
            e->requestRedraw();
        });
    }
}
