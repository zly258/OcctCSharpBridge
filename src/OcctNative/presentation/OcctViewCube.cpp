#include "presentation/OcctViewCube.h"
#include "core/OcctInternal.hxx"

#include <AIS_ViewCube.hxx>
#include <TCollection_AsciiString.hxx>

#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeViewCubeStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    void setCubeLabels(const Handle(AIS_ViewCube)& cube, int language)
    {
        if (cube.IsNull()) throw std::runtime_error("The view cube has not been initialized.");
        if (language == OcctViewCubeLanguage_English)
        {
            cube->SetFont("Arial");
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Front, "FRONT");
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Back, "BACK");
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Left, "LEFT");
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Right, "RIGHT");
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Top, "TOP");
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Bottom, "BOTTOM");
        }
        else if (language == OcctViewCubeLanguage_ChineseSimplified)
        {
            cube->SetFont("Microsoft YaHei UI");
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Front, TCollection_AsciiString(u8"前"));
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Back, TCollection_AsciiString(u8"后"));
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Left, TCollection_AsciiString(u8"左"));
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Right, TCollection_AsciiString(u8"右"));
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Top, TCollection_AsciiString(u8"上"));
            cube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Bottom, TCollection_AsciiString(u8"下"));
        }
        else
        {
            throw std::invalid_argument("View cube language is out of range.");
        }
    }
}

extern "C"
{
    OcctStatus occt_engine_view_cube_language_set(OcctEngineHandle handle, int language)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewCubeStatus(engine, [&]
        {
            setCubeLabels(engine->viewerContext.viewCube, language);
            engine->viewerContext.context->Redisplay(
                engine->viewerContext.viewCube,
                Standard_False,
                Standard_True);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_view_cube_try_click(
        OcctEngineHandle handle,
        int x,
        int y,
        int* handled)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (handled == nullptr) return OcctStatus_ErrorInvalidArgument;
        *handled = 0;
        return executeViewCubeStatus(engine, [&]
        {
            ViewerContext& viewerContext = engine->viewerContext;
            if (viewerContext.viewCube.IsNull()) return;

            viewerContext.context->MoveTo(x, y, viewerContext.view, Standard_False);
            const Handle(SelectMgr_EntityOwner) detected = viewerContext.context->DetectedOwner();
            const Handle(AIS_ViewCubeOwner) cubeOwner = Handle(AIS_ViewCubeOwner)::DownCast(detected);
            if (cubeOwner.IsNull() || cubeOwner->Selectable() != viewerContext.viewCube) return;

            viewerContext.viewCube->HandleClick(cubeOwner);
            viewerContext.context->ClearDetected(Standard_False);
            engine->requestRedraw();
            *handled = 1;
        });
    }
}
