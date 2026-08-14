#include "presentation/OcctView.h"
#include "core/OcctInternal.hxx"

#include <stdexcept>

using namespace OcctBridge;

namespace
{
    void setCubeLabels(const Handle(AIS_ViewCube)& cube, int language)
    {
        if (cube.IsNull()) throw std::runtime_error("View cube is not initialized.");
        switch (language)
        {
            case 0:
                cube->SetBoxSideLabel(V3d_Xpos, TCollection_AsciiString("Right"));
                cube->SetBoxSideLabel(V3d_Xneg, TCollection_AsciiString("Left"));
                cube->SetBoxSideLabel(V3d_Ypos, TCollection_AsciiString("Back"));
                cube->SetBoxSideLabel(V3d_Yneg, TCollection_AsciiString("Front"));
                cube->SetBoxSideLabel(V3d_Zpos, TCollection_AsciiString("Top"));
                cube->SetBoxSideLabel(V3d_Zneg, TCollection_AsciiString("Bottom"));
                break;
            case 1:
                cube->SetBoxSideLabel(V3d_Xpos, TCollection_AsciiString("You"));
                cube->SetBoxSideLabel(V3d_Xneg, TCollection_AsciiString("Zuo"));
                cube->SetBoxSideLabel(V3d_Ypos, TCollection_AsciiString("Hou"));
                cube->SetBoxSideLabel(V3d_Yneg, TCollection_AsciiString("Qian"));
                cube->SetBoxSideLabel(V3d_Zpos, TCollection_AsciiString("Shang"));
                cube->SetBoxSideLabel(V3d_Zneg, TCollection_AsciiString("Xia"));
                break;
            default:
                throw std::invalid_argument("View cube language is out of range.");
        }
    }
}

extern "C"
{
    OcctStatus occt_engine_view_cube_language_set(OcctEngineHandle handle, int language)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return execute(engine, [&]
        {
            setCubeLabels(engine->viewerContext.viewCube, language);
            engine->viewerContext.context->Redisplay(engine->viewerContext.viewCube, Standard_False);
            engine->requestRedraw();
        }) != 0 ? OcctStatus_Ok : engine->errors.code;
    }
}
