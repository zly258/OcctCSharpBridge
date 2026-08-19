#include "presentation/OcctAppearance.h"
#include "core/OcctInternal.hxx"

#include <AIS_DisplayMode.hxx>
#include <Aspect_TypeOfHighlightMethod.hxx>
#include <Aspect_TypeOfLine.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_LineAspect.hxx>
#include <Prs3d_TypeOfHighlight.hxx>

#include <cmath>
#include <stdexcept>
#include <string>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t LightingOptionsApiVersion = 1;
    constexpr std::uint32_t HighlightOptionsApiVersion = 1;
    constexpr std::uint32_t HighlightStyleOptionsApiVersion = 1;
    constexpr std::uint32_t AllHighlightUpdateBits =
        OcctViewerHighlightUpdate_Selection |
        OcctViewerHighlightUpdate_Hover;
    constexpr std::uint32_t AllHighlightStyleUpdateBits =
        OcctViewerHighlightStyleUpdate_SelectionColor |
        OcctViewerHighlightStyleUpdate_HoverColor |
        OcctViewerHighlightStyleUpdate_SelectionMode |
        OcctViewerHighlightStyleUpdate_HoverMode;

    void requireIntensity(double value, const char* name)
    {
        if (!std::isfinite(value) || value < 0.0 || value > 10.0)
            throw std::invalid_argument(std::string(name) + " must be between 0 and 10.");
    }

    Quantity_Color lightColor(OcctColorRgb value)
    {
        return color(value.r, value.g, value.b);
    }

    void removeAllLights(Engine* engine)
    {
        V3d_ListOfLight lights = engine->viewerContext.viewer->DefinedLights();
        for (V3d_ListOfLight::Iterator iterator(lights); iterator.More(); iterator.Next())
            engine->viewerContext.viewer->DelLight(iterator.Value());

        engine->viewerContext.customAmbientLight.Nullify();
        engine->viewerContext.customDirectionalLight.Nullify();
        engine->viewerContext.customSunLight.Nullify();
        engine->viewerContext.customFillLight.Nullify();
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeAppearanceStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    void validateLightingOptions(const OcctViewerLightingOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Viewer lighting options are null.");
        if (options->structSize < sizeof(OcctViewerLightingOptions) ||
            options->apiVersion != LightingOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported viewer lighting options size or version.");
        }

        const auto& settings = options->settings;
        requireIntensity(settings.ambientIntensity, "Ambient intensity");
        requireIntensity(settings.cameraLightIntensity, "Camera light intensity");
        requireIntensity(settings.sunLightIntensity, "Sun light intensity");
        requireIntensity(settings.fillLightIntensity, "Fill light intensity");
        (void)lightColor(settings.ambientColor);
        (void)lightColor(settings.cameraLightColor);
        (void)lightColor(settings.sunLightColor);
        (void)lightColor(settings.fillLightColor);
        if (settings.cameraLightEnabled != 0) (void)direction(settings.cameraLightDirection);
        if (settings.sunLightEnabled != 0) (void)direction(settings.sunLightDirection);
        if (settings.fillLightEnabled != 0) (void)direction(settings.fillLightDirection);
    }

    void validateHighlightOptions(const OcctViewerHighlightOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Viewer highlight options are null.");
        if (options->structSize < sizeof(OcctViewerHighlightOptions) ||
            options->apiVersion != HighlightOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported viewer highlight options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllHighlightUpdateBits) != 0)
            throw std::invalid_argument("Viewer highlight update mask is invalid.");
        if ((options->updateMask & OcctViewerHighlightUpdate_Selection) != 0)
            (void)lightColor(options->selectionColor);
        if ((options->updateMask & OcctViewerHighlightUpdate_Hover) != 0)
            (void)lightColor(options->hoverColor);
    }

    void validateHighlightMode(int mode)
    {
        if (mode < OcctHighlight_BoundingBox || mode > OcctHighlight_Shaded)
            throw std::invalid_argument("Highlight mode is out of range.");
    }

    void validateHighlightStyleOptions(const OcctViewerHighlightStyleOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Viewer highlight style options are null.");
        if (options->structSize < sizeof(OcctViewerHighlightStyleOptions) ||
            options->apiVersion != HighlightStyleOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported viewer highlight style options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllHighlightStyleUpdateBits) != 0)
            throw std::invalid_argument("Viewer highlight style update mask is invalid.");
        if ((options->updateMask & OcctViewerHighlightStyleUpdate_SelectionColor) != 0)
            (void)lightColor(options->selectionColor);
        if ((options->updateMask & OcctViewerHighlightStyleUpdate_HoverColor) != 0)
            (void)lightColor(options->hoverColor);
        if ((options->updateMask & OcctViewerHighlightStyleUpdate_SelectionMode) != 0)
            validateHighlightMode(options->selectionMode);
        if ((options->updateMask & OcctViewerHighlightStyleUpdate_HoverMode) != 0)
            validateHighlightMode(options->hoverMode);
    }

    void applyLighting(Engine* engine, const OcctSceneLightingSettings& settings)
    {
        removeAllLights(engine);

        if (settings.ambientIntensity > 0.0)
        {
            engine->viewerContext.customAmbientLight = new V3d_AmbientLight(lightColor(settings.ambientColor));
            engine->viewerContext.customAmbientLight->SetIntensity(
                static_cast<Standard_ShortReal>(settings.ambientIntensity));
            engine->viewerContext.viewer->AddLight(engine->viewerContext.customAmbientLight);
            engine->viewerContext.viewer->SetLightOn(engine->viewerContext.customAmbientLight);
        }

        if (settings.cameraLightEnabled != 0 && settings.cameraLightIntensity > 0.0)
        {
            engine->viewerContext.customDirectionalLight = new V3d_DirectionalLight(
                direction(settings.cameraLightDirection),
                lightColor(settings.cameraLightColor),
                Standard_True);
            engine->viewerContext.customDirectionalLight->SetIntensity(
                static_cast<Standard_ShortReal>(settings.cameraLightIntensity));
            engine->viewerContext.viewer->AddLight(engine->viewerContext.customDirectionalLight);
            engine->viewerContext.viewer->SetLightOn(engine->viewerContext.customDirectionalLight);
        }

        if (settings.sunLightEnabled != 0 && settings.sunLightIntensity > 0.0)
        {
            engine->viewerContext.customSunLight = new V3d_DirectionalLight(
                direction(settings.sunLightDirection),
                lightColor(settings.sunLightColor),
                Standard_False);
            engine->viewerContext.customSunLight->SetIntensity(
                static_cast<Standard_ShortReal>(settings.sunLightIntensity));
            engine->viewerContext.viewer->AddLight(engine->viewerContext.customSunLight);
            engine->viewerContext.viewer->SetLightOn(engine->viewerContext.customSunLight);
        }

        if (settings.fillLightEnabled != 0 && settings.fillLightIntensity > 0.0)
        {
            engine->viewerContext.customFillLight = new V3d_DirectionalLight(
                direction(settings.fillLightDirection),
                lightColor(settings.fillLightColor),
                Standard_False);
            engine->viewerContext.customFillLight->SetIntensity(
                static_cast<Standard_ShortReal>(settings.fillLightIntensity));
            engine->viewerContext.viewer->AddLight(engine->viewerContext.customFillLight);
            engine->viewerContext.viewer->SetLightOn(engine->viewerContext.customFillLight);
        }

        engine->viewerContext.viewer->UpdateLights();
        engine->requestRedraw();
    }

    void resetLighting(Engine* engine)
    {
        removeAllLights(engine);
        engine->viewerContext.viewer->SetDefaultLights();
        engine->viewerContext.viewer->SetLightOn();
        engine->viewerContext.viewer->UpdateLights();
        engine->requestRedraw();
    }

    void setHighlightColor(const Handle(Prs3d_Drawer)& style, const Quantity_Color& value)
    {
        if (style.IsNull()) throw std::runtime_error("Viewer highlight style is not available.");
        style->SetColor(value);
    }

    void setHighlightMode(const Handle(Prs3d_Drawer)& style, int mode)
    {
        if (style.IsNull()) throw std::runtime_error("Viewer highlight style is not available.");
        switch (mode)
        {
            case OcctHighlight_BoundingBox:
                style->SetMethod(Aspect_TOHM_BOUNDBOX);
                style->SetDisplayMode(-1);
                break;
            case OcctHighlight_Wireframe:
                style->SetMethod(Aspect_TOHM_COLOR);
                style->SetDisplayMode(AIS_WireFrame);
                break;
            case OcctHighlight_Shaded:
                style->SetMethod(Aspect_TOHM_COLOR);
                style->SetDisplayMode(AIS_Shaded);
                break;
            default:
                throw std::invalid_argument("Highlight mode is out of range.");
        }
    }

    void setSelectionHighlightColor(Engine* engine, const Quantity_Color& value)
    {
        setHighlightColor(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_Selected), value);
        setHighlightColor(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_LocalSelected), value);
        engine->viewerContext.context->UpdateSelected(Standard_False);
    }

    void setHoverHighlightColor(Engine* engine, const Quantity_Color& value)
    {
        setHighlightColor(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_Dynamic), value);
        setHighlightColor(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_LocalDynamic), value);
    }

    void setSelectionHighlightMode(Engine* engine, int mode)
    {
        setHighlightMode(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_Selected), mode);
        setHighlightMode(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_LocalSelected), mode);
        engine->viewerContext.context->UpdateSelected(Standard_False);
    }

    void setHoverHighlightMode(Engine* engine, int mode)
    {
        setHighlightMode(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_Dynamic), mode);
        setHighlightMode(engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_LocalDynamic), mode);
    }

    Aspect_TypeOfLine standardLineType(int lineStyle)
    {
        switch (lineStyle)
        {
            case OcctLineStyle_Solid: return Aspect_TOL_SOLID;
            case OcctLineStyle_Dash: return Aspect_TOL_DASH;
            case OcctLineStyle_Dot: return Aspect_TOL_DOT;
            case OcctLineStyle_DotDash: return Aspect_TOL_DOTDASH;
            case OcctLineStyle_Center: return Aspect_TOL_USERDEFINED;
            default: throw std::invalid_argument("Line style is out of range.");
        }
    }

    void applyLineStyle(const Handle(Prs3d_LineAspect)& aspect, int lineStyle)
    {
        if (aspect.IsNull()) return;
        if (lineStyle == OcctLineStyle_Center)
        {
            // Long-short center pattern. SetLinePattern switches the aspect to USERDEFINED.
            aspect->Aspect()->SetLinePattern(0xF18F);
            aspect->Aspect()->SetLineStippleFactor(1);
            return;
        }
        aspect->SetTypeOfLine(standardLineType(lineStyle));
    }

    void setObjectLineStyle(Engine* engine, OcctObjectId objectId, int lineStyle)
    {
        ObjectEntry* entry = engine->findObject(objectId);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");

        Handle(Prs3d_Drawer) drawer = entry->presentation->Attributes();
        if (drawer.IsNull()) throw std::runtime_error("Object presentation has no drawer.");
        drawer->SetOwnLineAspects();
        applyLineStyle(drawer->LineAspect(), lineStyle);
        applyLineStyle(drawer->WireAspect(), lineStyle);
        applyLineStyle(drawer->SeenLineAspect(), lineStyle);
        applyLineStyle(drawer->FreeBoundaryAspect(), lineStyle);
        applyLineStyle(drawer->UnFreeBoundaryAspect(), lineStyle);
        applyLineStyle(drawer->FaceBoundaryAspect(), lineStyle);
        engine->viewerContext.context->Redisplay(entry->presentation, Standard_False);
        engine->requestRedraw();
    }
}

extern "C"
{
    OcctStatus occt_engine_scene_lighting_set(
        OcctEngineHandle handle,
        const OcctViewerLightingOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeAppearanceStatus(engine, [&]
        {
            validateLightingOptions(options);
            applyLighting(engine, options->settings);
        });
    }

    OcctStatus occt_engine_scene_lighting_reset(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeAppearanceStatus(engine, [&] { resetLighting(engine); });
    }

    OcctStatus occt_engine_highlight_colors_set(
        OcctEngineHandle handle,
        const OcctViewerHighlightOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeAppearanceStatus(engine, [&]
        {
            validateHighlightOptions(options);
            if ((options->updateMask & OcctViewerHighlightUpdate_Selection) != 0)
                setSelectionHighlightColor(engine, lightColor(options->selectionColor));
            if ((options->updateMask & OcctViewerHighlightUpdate_Hover) != 0)
                setHoverHighlightColor(engine, lightColor(options->hoverColor));
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_highlight_style_set(
        OcctEngineHandle handle,
        const OcctViewerHighlightStyleOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeAppearanceStatus(engine, [&]
        {
            validateHighlightStyleOptions(options);
            if ((options->updateMask & OcctViewerHighlightStyleUpdate_SelectionColor) != 0)
                setSelectionHighlightColor(engine, lightColor(options->selectionColor));
            if ((options->updateMask & OcctViewerHighlightStyleUpdate_HoverColor) != 0)
                setHoverHighlightColor(engine, lightColor(options->hoverColor));
            if ((options->updateMask & OcctViewerHighlightStyleUpdate_SelectionMode) != 0)
                setSelectionHighlightMode(engine, options->selectionMode);
            if ((options->updateMask & OcctViewerHighlightStyleUpdate_HoverMode) != 0)
                setHoverHighlightMode(engine, options->hoverMode);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_line_style_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int lineStyle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeAppearanceStatus(engine, [&]
        {
            (void)standardLineType(lineStyle);
            setObjectLineStyle(engine, objectId, lineStyle);
        });
    }
}
