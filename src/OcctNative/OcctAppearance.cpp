#include "OcctInternal.hxx"

#include <Prs3d_TypeOfHighlight.hxx>

using namespace OcctBridge;

namespace
{
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
        V3d_ListOfLight lights = engine->viewer->DefinedLights();
        for (V3d_ListOfLight::Iterator iterator(lights); iterator.More(); iterator.Next())
        {
            engine->viewer->DelLight(iterator.Value());
        }

        engine->customAmbientLight.Nullify();
        engine->customDirectionalLight.Nullify();
        engine->customSunLight.Nullify();
        engine->customFillLight.Nullify();
    }
}

extern "C"
{
    int occt_set_selection_highlight_color(OcctHandle h, double r, double g, double b)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            const Quantity_Color value = color(r, g, b);
            e->context->HighlightStyle(Prs3d_TypeOfHighlight_Selected)->SetColor(value);
            e->context->HighlightStyle(Prs3d_TypeOfHighlight_LocalSelected)->SetColor(value);
            e->context->UpdateSelected(Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_hover_highlight_color(OcctHandle h, double r, double g, double b)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            const Quantity_Color value = color(r, g, b);
            e->context->HighlightStyle(Prs3d_TypeOfHighlight_Dynamic)->SetColor(value);
            e->context->HighlightStyle(Prs3d_TypeOfHighlight_LocalDynamic)->SetColor(value);
            e->requestRedraw();
        });
    }

    int occt_set_scene_lighting_ex(
        OcctHandle h,
        const OcctSceneLightingSettings* settings)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || settings == nullptr) return 0;
        return execute(e, [&]
        {
            requireIntensity(settings->ambientIntensity, "Ambient intensity");
            requireIntensity(settings->cameraLightIntensity, "Camera light intensity");
            requireIntensity(settings->sunLightIntensity, "Sun light intensity");
            requireIntensity(settings->fillLightIntensity, "Fill light intensity");

            removeAllLights(e);

            if (settings->ambientIntensity > 0.0)
            {
                e->customAmbientLight = new V3d_AmbientLight(lightColor(settings->ambientColor));
                e->customAmbientLight->SetIntensity(
                    static_cast<Standard_ShortReal>(settings->ambientIntensity));
                e->viewer->AddLight(e->customAmbientLight);
                e->viewer->SetLightOn(e->customAmbientLight);
            }

            if (settings->cameraLightEnabled != 0 && settings->cameraLightIntensity > 0.0)
            {
                e->customDirectionalLight = new V3d_DirectionalLight(
                    direction(settings->cameraLightDirection),
                    lightColor(settings->cameraLightColor),
                    Standard_True);
                e->customDirectionalLight->SetIntensity(
                    static_cast<Standard_ShortReal>(settings->cameraLightIntensity));
                e->viewer->AddLight(e->customDirectionalLight);
                e->viewer->SetLightOn(e->customDirectionalLight);
            }

            if (settings->sunLightEnabled != 0 && settings->sunLightIntensity > 0.0)
            {
                e->customSunLight = new V3d_DirectionalLight(
                    direction(settings->sunLightDirection),
                    lightColor(settings->sunLightColor),
                    Standard_False);
                e->customSunLight->SetIntensity(
                    static_cast<Standard_ShortReal>(settings->sunLightIntensity));
                e->viewer->AddLight(e->customSunLight);
                e->viewer->SetLightOn(e->customSunLight);
            }

            if (settings->fillLightEnabled != 0 && settings->fillLightIntensity > 0.0)
            {
                e->customFillLight = new V3d_DirectionalLight(
                    direction(settings->fillLightDirection),
                    lightColor(settings->fillLightColor),
                    Standard_False);
                e->customFillLight->SetIntensity(
                    static_cast<Standard_ShortReal>(settings->fillLightIntensity));
                e->viewer->AddLight(e->customFillLight);
                e->viewer->SetLightOn(e->customFillLight);
            }

            e->viewer->UpdateLights();
            e->requestRedraw();
        });
    }
}
