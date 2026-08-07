#include "OcctInternal.hxx"

#include <TCollection_AsciiString.hxx>

using namespace OcctBridge;

namespace
{
    enum ViewCubeLanguage
    {
        ViewCubeLanguage_English = 0,
        ViewCubeLanguage_ChineseSimplified = 1
    };

    TCollection_AsciiString utf8Label(const char* value)
    {
        return TCollection_AsciiString(value);
    }

    void setEnglishLabels(Engine& engine)
    {
        engine.viewCube->SetFont("Arial");
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Front, "FRONT");
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Back, "BACK");
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Left, "LEFT");
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Right, "RIGHT");
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Top, "TOP");
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Bottom, "BOTTOM");
    }

    void setChineseLabels(Engine& engine)
    {
        engine.viewCube->SetFont("Microsoft YaHei UI");
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Front, utf8Label("\xE5\x89\x8D"));
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Back, utf8Label("\xE5\x90\x8E"));
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Left, utf8Label("\xE5\xB7\xA6"));
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Right, utf8Label("\xE5\x8F\xB3"));
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Top, utf8Label("\xE4\xB8\x8A"));
        engine.viewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Bottom, utf8Label("\xE4\xB8\x8B"));
    }
}

extern "C"
{
    int occt_set_view_cube_language(OcctHandle handle, int language)
    {
        Engine* engine = engineOf(handle);
        if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            if (engine->viewCube.IsNull())
                throw std::runtime_error("The view cube has not been initialized.");

            switch (language)
            {
                case ViewCubeLanguage_English:
                    setEnglishLabels(*engine);
                    break;
                case ViewCubeLanguage_ChineseSimplified:
                    setChineseLabels(*engine);
                    break;
                default:
                    throw std::invalid_argument("View cube language is out of range.");
            }

            engine->context->Redisplay(engine->viewCube, Standard_False, Standard_True);
            engine->requestRedraw();
        });
    }
}
