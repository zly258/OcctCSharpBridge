#include "presentation/OcctAppearance.h"
#include "core/OcctInternal.hxx"

#include <Prs3d_TypeOfHighlight.hxx>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t LightingOptionsApiVersion = 1;
    constexpr std::uint32_t HighlightOptionsApiVersion = 1;
    constexpr std::uint32_t AllHighlightUpdateBits =
        OcctViewerHighlightUpdate_Selection |
        OcctViewerHighlightUpdate_Hover;

    void requireIntensity(double value, const char* name)
    {
        if (!std::isfinite(value) || value < 0.0 || value > 10.0)
        {
            throw std::invalid_argument(std::string(name) + " must be between 0 and 10.");
        }
    }

    Quantity_Color lightColor(OcctColorRgb value)
    {
        return color(value.r, value.g, value.b);
    }

    void removeAllLights(Engine* engine)
    {
        V3d_ListOfLight lights = engine->viewerContext.viewer->DefinedLights();
        for (V3d_ListOfLight::Iterator iterator(lights); iterator.More(); iterator.Next())
        {
            engine->viewerContext.viewer->DelLight(iterator.Value());
        }

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

    OcctViewerLightingOptions lightingOptions(const OcctSceneLightingSettings& settings)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctViewerLightingOptions)),
            LightingOptionsApiVersion,
            settings };
    }

    OcctViewerHighlightOptions highlightOptions(
        std::uint32_t updateMask,
        OcctColorRgb selectionColor,
        OcctColorRgb hoverColor)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctViewerHighlightOptions)),
            HighlightOptionsApiVersion,
            updateMask,
            selectionColor,
            hoverColor };
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
            if (options == nullptr) throw std::invalid_argument("Viewer highlight options are null.");
            if (options->structSize < sizeof(OcctViewerHighlightOptions) ||
                options->apiVersion != HighlightOptionsApiVersion)
            {
                throw std::invalid_argument("Unsupported viewer highlight options size or version.");
            }
            if (options->updateMask == 0 || (options->updateMask & ~AllHighlightUpdateBits) != 0)
                throw std::invalid_argument("Viewer highlight update mask is invalid.");

            if ((options->updateMask & OcctViewerHighlightUpdate_Selection) != 0)
            {
                const Quantity_Color value = lightColor(options->selectionColor);
                engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_Selected)->SetColor(value);
                engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_LocalSelected)->SetColor(value);
                engine->viewerContext.context->UpdateSelected(Standard_False);
            }
            if ((options->updateMask & OcctViewerHighlightUpdate_Hover) != 0)
            {
                const Quantity_Color value = lightColor(options->hoverColor);
                engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_Dynamic)->SetColor(value);
                engine->viewerContext.context->HighlightStyle(Prs3d_TypeOfHighlight_LocalDynamic)->SetColor(value);
            }
            engine->requestRedraw();
        });
    }

    int occt_set_selection_highlight_color(OcctHandle h, double r, double g, double b)
    {
        const OcctViewerHighlightOptions options = highlightOptions(
            OcctViewerHighlightUpdate_Selection,
            { r, g, b },
            {});
        return occt_engine_highlight_colors_set(
                   reinterpret_cast<OcctEngineHandle>(h),
                   &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_set_hover_highlight_color(OcctHandle h, double r, double g, double b)
    {
        const OcctViewerHighlightOptions options = highlightOptions(
            OcctViewerHighlightUpdate_Hover,
            {},
            { r, g, b });
        return occt_engine_highlight_colors_set(
                   reinterpret_cast<OcctEngineHandle>(h),
                   &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_set_scene_lighting_ex(
        OcctHandle h,
        const OcctSceneLightingSettings* settings)
    {
        if (settings == nullptr)
        {
            Engine* engine = engineOf(h);
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Scene lighting settings are null.");
            return 0;
        }
        const OcctViewerLightingOptions options = lightingOptions(*settings);
        return occt_engine_scene_lighting_set(
                   reinterpret_cast<OcctEngineHandle>(h),
                   &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_reset_scene_lighting(OcctHandle h)
    {
        return occt_engine_scene_lighting_reset(
                   reinterpret_cast<OcctEngineHandle>(h)) == OcctStatus_Ok
            ? 1
            : 0;
    }
}
