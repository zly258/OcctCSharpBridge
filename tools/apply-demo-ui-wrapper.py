from __future__ import annotations

import re
import textwrap
from collections import OrderedDict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8-sig")


def write(path: str, content: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content, encoding="utf-8-sig", newline="\n")


def replace_once(path: str, old: str, new: str) -> None:
    text = read(path)
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"Expected exactly one match in {path}, found {count}: {old[:120]!r}")
    write(path, text.replace(old, new, 1))


def regex_replace_once(path: str, pattern: str, replacement: str) -> None:
    text = read(path)
    result, count = re.subn(pattern, replacement, text, count=1, flags=re.DOTALL)
    if count != 1:
        raise RuntimeError(f"Expected exactly one regex match in {path}, found {count}: {pattern}")
    write(path, result)


# -----------------------------------------------------------------------------
# Native OCCT bridge: selection appearance, advanced lighting, solid background
# and Ctrl-click XOR selection.
# -----------------------------------------------------------------------------
replace_once(
    "src/OcctNative/CMakeLists.txt",
    "    OcctEngine.cpp\n    OcctSelectionOverlay.cpp",
    "    OcctEngine.cpp\n    OcctAppearance.cpp\n    OcctSelectionOverlay.cpp",
)

replace_once(
    "src/OcctNative/OcctNative.h",
    "    struct OcctPolygonOffsetSettings { int mode; double factor; double units; };\n",
    "    struct OcctPolygonOffsetSettings { int mode; double factor; double units; };\n"
    "    struct OcctColorRgb { double r; double g; double b; };\n"
    "    struct OcctSceneLightingSettings\n"
    "    {\n"
    "        OcctColorRgb ambientColor;\n"
    "        double ambientIntensity;\n"
    "        int cameraLightEnabled;\n"
    "        OcctColorRgb cameraLightColor;\n"
    "        double cameraLightIntensity;\n"
    "        OcctVector3d cameraLightDirection;\n"
    "        int sunLightEnabled;\n"
    "        OcctColorRgb sunLightColor;\n"
    "        double sunLightIntensity;\n"
    "        OcctVector3d sunLightDirection;\n"
    "        int fillLightEnabled;\n"
    "        OcctColorRgb fillLightColor;\n"
    "        double fillLightIntensity;\n"
    "        OcctVector3d fillLightDirection;\n"
    "    };\n",
)

replace_once(
    "src/OcctNative/OcctNative.h",
    "    OCCTBRIDGE_API int occt_set_scene_lighting(OcctHandle handle, double ambientIntensity, double directionalIntensity, OcctVector3d direction, int headlight);\n",
    "    OCCTBRIDGE_API int occt_set_scene_lighting(OcctHandle handle, double ambientIntensity, double directionalIntensity, OcctVector3d direction, int headlight);\n"
    "    OCCTBRIDGE_API int occt_set_scene_lighting_ex(OcctHandle handle, const OcctSceneLightingSettings* settings);\n"
    "    OCCTBRIDGE_API int occt_set_selection_highlight_color(OcctHandle handle, double r, double g, double b);\n"
    "    OCCTBRIDGE_API int occt_set_hover_highlight_color(OcctHandle handle, double r, double g, double b);\n",
)

replace_once(
    "src/OcctNative/OcctInternal.hxx",
    "        Handle(V3d_DirectionalLight) customDirectionalLight;\n",
    "        Handle(V3d_DirectionalLight) customDirectionalLight;\n"
    "        Handle(V3d_DirectionalLight) customSunLight;\n"
    "        Handle(V3d_DirectionalLight) customFillLight;\n",
)

replace_once(
    "src/OcctNative/OcctEngine.cpp",
    "#include <AIS_SelectionScheme.hxx>\n#include <Aspect_PolygonOffsetMode.hxx>",
    "#include <AIS_SelectionScheme.hxx>\n#include <Aspect_GradientFillMethod.hxx>\n#include <Aspect_PolygonOffsetMode.hxx>",
)

replace_once(
    "src/OcctNative/OcctEngine.cpp",
    "    int occt_set_background(OcctHandle h, double r, double g, double b) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->SetBackgroundColor(color(r,g,b)); e->view->Redraw(); }); }",
    "    int occt_set_background(OcctHandle h, double r, double g, double b)\n"
    "    {\n"
    "        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;\n"
    "        return execute(e, [&]\n"
    "        {\n"
    "            // A previously enabled gradient otherwise remains the active background.\n"
    "            e->view->SetBgGradientStyle(Aspect_GradientFillMethod_None, Standard_False);\n"
    "            e->view->SetBackgroundColor(color(r, g, b));\n"
    "            e->view->Redraw();\n"
    "        });\n"
    "    }",
)

engine_text = read("src/OcctNative/OcctEngine.cpp")
old_nullify = "            e->customDirectionalLight.Nullify();\n"
if engine_text.count(old_nullify) != 2:
    raise RuntimeError(f"Expected two lighting reset blocks, found {engine_text.count(old_nullify)}")
engine_text = engine_text.replace(
    old_nullify,
    old_nullify
    + "            e->customSunLight.Nullify();\n"
    + "            e->customFillLight.Nullify();\n",
)
write("src/OcctNative/OcctEngine.cpp", engine_text)

replace_once(
    "src/OcctNative/OcctEngine.cpp",
    "e->context->SelectDetected(append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);",
    "e->context->SelectDetected(append ? AIS_SelectionScheme_XOR : AIS_SelectionScheme_Replace);",
)

write(
    "src/OcctNative/OcctAppearance.cpp",
    textwrap.dedent(
        r'''\
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
                    e->view->Redraw();
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
                    e->view->Redraw();
                });
            }

            int occt_set_scene_lighting_ex(OcctHandle h, const OcctSceneLightingSettings* settings)
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
                        e->customAmbientLight->SetIntensity(static_cast<Standard_ShortReal>(settings->ambientIntensity));
                        e->viewer->AddLight(e->customAmbientLight);
                        e->viewer->SetLightOn(e->customAmbientLight);
                    }

                    if (settings->cameraLightEnabled != 0 && settings->cameraLightIntensity > 0.0)
                    {
                        e->customDirectionalLight = new V3d_DirectionalLight(
                            direction(settings->cameraLightDirection),
                            lightColor(settings->cameraLightColor),
                            Standard_True);
                        e->customDirectionalLight->SetIntensity(static_cast<Standard_ShortReal>(settings->cameraLightIntensity));
                        e->viewer->AddLight(e->customDirectionalLight);
                        e->viewer->SetLightOn(e->customDirectionalLight);
                    }

                    if (settings->sunLightEnabled != 0 && settings->sunLightIntensity > 0.0)
                    {
                        e->customSunLight = new V3d_DirectionalLight(
                            direction(settings->sunLightDirection),
                            lightColor(settings->sunLightColor),
                            Standard_False);
                        e->customSunLight->SetIntensity(static_cast<Standard_ShortReal>(settings->sunLightIntensity));
                        e->viewer->AddLight(e->customSunLight);
                        e->viewer->SetLightOn(e->customSunLight);
                    }

                    if (settings->fillLightEnabled != 0 && settings->fillLightIntensity > 0.0)
                    {
                        e->customFillLight = new V3d_DirectionalLight(
                            direction(settings->fillLightDirection),
                            lightColor(settings->fillLightColor),
                            Standard_False);
                        e->customFillLight->SetIntensity(static_cast<Standard_ShortReal>(settings->fillLightIntensity));
                        e->viewer->AddLight(e->customFillLight);
                        e->viewer->SetLightOn(e->customFillLight);
                    }

                    e->viewer->UpdateLights();
                    e->view->Redraw();
                });
            }
        }
        '''
    ),
)

# -----------------------------------------------------------------------------
# Managed wrapper additions.
# -----------------------------------------------------------------------------
write(
    "src/OcctNet/OcctAppearanceTypes.cs",
    textwrap.dedent(
        r'''\
        using System.Drawing;

        namespace OcctNet;

        public enum OcctLightingPreset
        {
            Neutral = 0,
            Studio = 1,
            Sunlight = 2,
            Flat = 3
        }

        public readonly record struct OcctDirectionalLightSettings(
            bool Enabled,
            Color Color,
            double Intensity,
            OcctVector3d Direction,
            bool Headlight = false);

        public readonly record struct OcctSceneLightingSettings(
            Color AmbientColor,
            double AmbientIntensity,
            OcctDirectionalLightSettings CameraLight,
            OcctDirectionalLightSettings SunLight,
            OcctDirectionalLightSettings FillLight);

        public static class OcctLightingPresets
        {
            public static OcctSceneLightingSettings Create(OcctLightingPreset preset)
            {
                return preset switch
                {
                    OcctLightingPreset.Neutral => new(
                        Color.White,
                        0.45,
                        new(true, Color.White, 0.90, new OcctVector3d(0, 0, -1), true),
                        new(false, Color.White, 0.0, new OcctVector3d(-1, -1, -2)),
                        new(false, Color.White, 0.0, new OcctVector3d(1, 1, -1))),
                    OcctLightingPreset.Sunlight => new(
                        Color.FromArgb(245, 248, 255),
                        0.25,
                        new(true, Color.White, 0.25, new OcctVector3d(0, 0, -1), true),
                        new(true, Color.FromArgb(255, 242, 210), 1.40, new OcctVector3d(-1, -0.6, -1.8)),
                        new(true, Color.FromArgb(190, 215, 255), 0.20, new OcctVector3d(1, 0.5, -1))),
                    OcctLightingPreset.Flat => new(
                        Color.White,
                        0.85,
                        new(true, Color.White, 0.25, new OcctVector3d(0, 0, -1), true),
                        new(false, Color.White, 0.0, new OcctVector3d(-1, -1, -2)),
                        new(false, Color.White, 0.0, new OcctVector3d(1, 1, -1))),
                    _ => new(
                        Color.FromArgb(248, 250, 255),
                        0.30,
                        new(true, Color.White, 0.85, new OcctVector3d(0, 0, -1), true),
                        new(true, Color.FromArgb(255, 244, 220), 0.75, new OcctVector3d(-1, -1, -2)),
                        new(true, Color.FromArgb(195, 220, 255), 0.35, new OcctVector3d(1, 0.5, -1)))
                };
            }
        }
        '''
    ),
)

write(
    "src/OcctNet/AppearanceNativeMethods.cs",
    textwrap.dedent(
        r'''\
        using System.Runtime.InteropServices;

        namespace OcctNet;

        internal static class AppearanceNativeMethods
        {
            private const string LibraryName = "OcctNative";

            [StructLayout(LayoutKind.Sequential)]
            internal struct NativeColorRgb
            {
                internal double R;
                internal double G;
                internal double B;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct NativeSceneLightingSettings
            {
                internal NativeColorRgb AmbientColor;
                internal double AmbientIntensity;
                internal int CameraLightEnabled;
                internal NativeColorRgb CameraLightColor;
                internal double CameraLightIntensity;
                internal OcctVector3d CameraLightDirection;
                internal int SunLightEnabled;
                internal NativeColorRgb SunLightColor;
                internal double SunLightIntensity;
                internal OcctVector3d SunLightDirection;
                internal int FillLightEnabled;
                internal NativeColorRgb FillLightColor;
                internal double FillLightIntensity;
                internal OcctVector3d FillLightDirection;
            }

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int occt_set_scene_lighting_ex(
                IntPtr handle,
                in NativeSceneLightingSettings settings);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int occt_set_selection_highlight_color(
                IntPtr handle,
                double r,
                double g,
                double b);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int occt_set_hover_highlight_color(
                IntPtr handle,
                double r,
                double g,
                double b);
        }
        '''
    ),
)

write(
    "src/OcctNet/OcctEngine.Appearance.cs",
    textwrap.dedent(
        r'''\
        using System.Drawing;

        namespace OcctNet;

        public sealed partial class OcctEngine
        {
            public void SetSelectionHighlightColor(Color color) => CheckInitialized(() =>
                AppearanceNativeMethods.occt_set_selection_highlight_color(
                    _handle,
                    color.R / 255.0,
                    color.G / 255.0,
                    color.B / 255.0));

            public void SetHoverHighlightColor(Color color) => CheckInitialized(() =>
                AppearanceNativeMethods.occt_set_hover_highlight_color(
                    _handle,
                    color.R / 255.0,
                    color.G / 255.0,
                    color.B / 255.0));

            public void SetSceneLighting(OcctSceneLightingSettings settings)
            {
                ValidateIntensity(settings.AmbientIntensity, nameof(settings.AmbientIntensity));
                ValidateDirectionalLight(settings.CameraLight, nameof(settings.CameraLight));
                ValidateDirectionalLight(settings.SunLight, nameof(settings.SunLight));
                ValidateDirectionalLight(settings.FillLight, nameof(settings.FillLight));

                var native = new AppearanceNativeMethods.NativeSceneLightingSettings
                {
                    AmbientColor = ToNativeColor(settings.AmbientColor),
                    AmbientIntensity = settings.AmbientIntensity,
                    CameraLightEnabled = settings.CameraLight.Enabled ? 1 : 0,
                    CameraLightColor = ToNativeColor(settings.CameraLight.Color),
                    CameraLightIntensity = settings.CameraLight.Intensity,
                    CameraLightDirection = settings.CameraLight.Direction,
                    SunLightEnabled = settings.SunLight.Enabled ? 1 : 0,
                    SunLightColor = ToNativeColor(settings.SunLight.Color),
                    SunLightIntensity = settings.SunLight.Intensity,
                    SunLightDirection = settings.SunLight.Direction,
                    FillLightEnabled = settings.FillLight.Enabled ? 1 : 0,
                    FillLightColor = ToNativeColor(settings.FillLight.Color),
                    FillLightIntensity = settings.FillLight.Intensity,
                    FillLightDirection = settings.FillLight.Direction
                };

                CheckInitialized(() => AppearanceNativeMethods.occt_set_scene_lighting_ex(_handle, in native));
            }

            public void ApplyLightingPreset(OcctLightingPreset preset) =>
                SetSceneLighting(OcctLightingPresets.Create(preset));

            private static AppearanceNativeMethods.NativeColorRgb ToNativeColor(Color color) => new()
            {
                R = color.R / 255.0,
                G = color.G / 255.0,
                B = color.B / 255.0
            };

            private static void ValidateDirectionalLight(OcctDirectionalLightSettings light, string name)
            {
                ValidateIntensity(light.Intensity, $"{name}.{nameof(light.Intensity)}");
                if (light.Enabled && light.Direction.Length <= 1e-12)
                {
                    throw new ArgumentException("Enabled directional lights require a non-zero direction.", name);
                }
            }

            private static void ValidateIntensity(double value, string name)
            {
                if (!double.IsFinite(value) || value < 0.0 || value > 10.0)
                {
                    throw new ArgumentOutOfRangeException(name, value, "Light intensity must be between 0 and 10.");
                }
            }
        }
        '''
    ),
)

# -----------------------------------------------------------------------------
# WinForms demo cleanup and appearance controls.
# -----------------------------------------------------------------------------
replace_once(
    "src/CadWinForms/MainForm.cs",
    "using CadCommon;\nusing OcctNet;",
    "using System.Globalization;\nusing CadCommon;\nusing OcctNet;",
)
replace_once("src/CadWinForms/MainForm.cs", "    private OcctObject? _treeHighlighted;\n", "")
replace_once(
    "src/CadWinForms/MainForm.cs",
    "    private bool _initialPanelLayoutScheduled;\n",
    "    private bool _initialPanelLayoutScheduled;\n"
    "    private Color _selectionHighlightColor = Color.FromArgb(255, 155, 0);\n"
    "    private Color _hoverHighlightColor = Color.FromArgb(0, 185, 255);\n"
    "    private OcctSceneLightingSettings _lightingSettings = OcctLightingPresets.Create(OcctLightingPreset.Studio);\n",
)
regex_replace_once(
    "src/CadWinForms/MainForm.cs",
    r"\n        var samples = new ToolStripMenuItem\(CadLocalization\.Text\(\"Menu\.Samples\"\)\);\n        AddCommands\(samples,.*?\);\n",
    "\n",
)
replace_once(
    "src/CadWinForms/MainForm.cs",
    "_menu.Items.AddRange(new ToolStripItem[] { file, edit, draw, solid, annotate, BuildViewMenu(), tools, samples, language, help });",
    "_menu.Items.AddRange(new ToolStripItem[] { file, edit, draw, solid, annotate, BuildViewMenu(), tools, language, help });",
)
replace_once(
    "src/CadWinForms/MainForm.cs",
    "        view.DropDownItems.Add(MenuItem(CadLocalization.Text(\"Menu.Lighting\"), (_, _) => SetLighting()));\n        view.DropDownItems.Add(MenuItem(CadLocalization.Text(\"Menu.ResetLighting\"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));",
    "        view.DropDownItems.Add(BuildLightingMenu());",
)
replace_once(
    "src/CadWinForms/MainForm.cs",
    "        view.DropDownItems.Add(BuildSelectionMenu());\n",
    "        view.DropDownItems.Add(BuildSelectionMenu());\n        view.DropDownItems.Add(BuildSelectionAppearanceMenu());\n",
)
replace_once(
    "src/CadWinForms/MainForm.cs",
    "        _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);\n",
    "        _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);\n"
    "        _session.Engine.SetSelectionHighlightColor(_selectionHighlightColor);\n"
    "        _session.Engine.SetHoverHighlightColor(_hoverHighlightColor);\n"
    "        _session.Engine.SetSceneLighting(_lightingSettings);\n",
)
replace_once(
    "src/CadWinForms/MainForm.cs",
    "        if (_treeHighlighted is { } previous && Session.Engine.Exists(previous)) Session.Engine.Unhighlight(previous);\n        Session.Engine.Highlight(value);\n        _treeHighlighted = value;\n",
    "",
)

winforms_lighting = r'''
    private ToolStripMenuItem BuildLightingMenu()
    {
        var menu = new ToolStripMenuItem(Local("Lighting", "灯光"));
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            menu.DropDownItems.Add(MenuItem(LightingPresetName(captured), (_, _) => ApplyLightingPreset(captured)));
        }
        menu.DropDownItems.Add(new ToolStripSeparator());
        menu.DropDownItems.Add(MenuItem(Local("Custom Lighting...", "自定义灯光..."), (_, _) => SetAdvancedLighting()));
        menu.DropDownItems.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));
        return menu;
    }

    private void ApplyLightingPreset(OcctLightingPreset preset)
    {
        ExecuteSafe(() =>
        {
            _lightingSettings = OcctLightingPresets.Create(preset);
            Session.Engine.SetSceneLighting(_lightingSettings);
            Log($"{Local("Lighting", "灯光")}: {LightingPresetName(preset)}");
        });
    }

    private void SetAdvancedLighting()
    {
        static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
        var parameters = new[]
        {
            new CadParameterDefinition("ambient", Local("Ambient Intensity", "环境光强度"), CadParameterKind.Number, Number(_lightingSettings.AmbientIntensity)),
            new CadParameterDefinition("cameraEnabled", Local("Camera Light", "相机直射光"), CadParameterKind.Boolean, _lightingSettings.CameraLight.Enabled.ToString()),
            new CadParameterDefinition("camera", Local("Camera Light Intensity", "相机直射光强度"), CadParameterKind.Number, Number(_lightingSettings.CameraLight.Intensity)),
            new CadParameterDefinition("sunEnabled", Local("Sun Light", "太阳光"), CadParameterKind.Boolean, _lightingSettings.SunLight.Enabled.ToString()),
            new CadParameterDefinition("sun", Local("Sun Intensity", "太阳光强度"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Intensity)),
            new CadParameterDefinition("sunX", Local("Sun Direction X", "太阳光方向 X"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.X)),
            new CadParameterDefinition("sunY", Local("Sun Direction Y", "太阳光方向 Y"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Y)),
            new CadParameterDefinition("sunZ", Local("Sun Direction Z", "太阳光方向 Z"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Z)),
            new CadParameterDefinition("fillEnabled", Local("Fill Light", "补光"), CadParameterKind.Boolean, _lightingSettings.FillLight.Enabled.ToString()),
            new CadParameterDefinition("fill", Local("Fill Intensity", "补光强度"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Intensity)),
            new CadParameterDefinition("fillX", Local("Fill Direction X", "补光方向 X"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.X)),
            new CadParameterDefinition("fillY", Local("Fill Direction Y", "补光方向 Y"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Y)),
            new CadParameterDefinition("fillZ", Local("Fill Direction Z", "补光方向 Z"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Z))
        };
        if (!ParameterDialog.TryGetValues(this, Local("Custom Lighting", "自定义灯光"), parameters, out var raw)) return;
        var values = new CadValues(raw);
        var settings = _lightingSettings with
        {
            AmbientIntensity = values.Number("ambient"),
            CameraLight = _lightingSettings.CameraLight with
            {
                Enabled = values.Boolean("cameraEnabled", true),
                Intensity = values.Number("camera")
            },
            SunLight = _lightingSettings.SunLight with
            {
                Enabled = values.Boolean("sunEnabled", true),
                Intensity = values.Number("sun"),
                Direction = values.Vector("sunX", "sunY", "sunZ")
            },
            FillLight = _lightingSettings.FillLight with
            {
                Enabled = values.Boolean("fillEnabled", true),
                Intensity = values.Number("fill"),
                Direction = values.Vector("fillX", "fillY", "fillZ")
            }
        };
        ExecuteSafe(() =>
        {
            Session.Engine.SetSceneLighting(settings);
            _lightingSettings = settings;
            Log(Local("Custom lighting applied.", "已应用自定义灯光。"));
        });
    }

    private ToolStripMenuItem BuildSelectionAppearanceMenu()
    {
        var menu = new ToolStripMenuItem(Local("Selection Appearance", "选择外观"));
        menu.DropDownItems.Add(MenuItem(Local("Selected Color...", "选中高亮颜色..."), (_, _) => SetSelectionHighlightColor()));
        menu.DropDownItems.Add(MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), (_, _) => SetHoverHighlightColor()));
        return menu;
    }

    private void SetSelectionHighlightColor()
    {
        using var dialog = new ColorDialog { Color = _selectionHighlightColor, FullOpen = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _selectionHighlightColor = dialog.Color;
            Session.Engine.SetSelectionHighlightColor(dialog.Color);
        });
    }

    private void SetHoverHighlightColor()
    {
        using var dialog = new ColorDialog { Color = _hoverHighlightColor, FullOpen = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _hoverHighlightColor = dialog.Color;
            Session.Engine.SetHoverHighlightColor(dialog.Color);
        });
    }
'''

regex_replace_once(
    "src/CadWinForms/MainForm.cs",
    r"    private void SetLighting\(\)\n    \{.*?\n    \}\n\n    private void SetSelectionTolerance",
    winforms_lighting + "\n    private void SetSelectionTolerance",
)
replace_once(
    "src/CadWinForms/MainForm.cs",
    "    private static string SelectionModeName(OcctSelectionMode mode) => CadLocalization.SelectionMode(mode);\n",
    "    private static string SelectionModeName(OcctSelectionMode mode) => CadLocalization.SelectionMode(mode);\n"
    "    private static string LightingPresetName(OcctLightingPreset preset) => preset switch\n"
    "    {\n"
    "        OcctLightingPreset.Neutral => Local(\"Neutral\", \"中性\"),\n"
    "        OcctLightingPreset.Sunlight => Local(\"Sunlight\", \"日光\"),\n"
    "        OcctLightingPreset.Flat => Local(\"Flat\", \"平光\"),\n"
    "        _ => Local(\"Studio\", \"摄影棚\")\n"
    "    };\n"
    "    private static string Local(string english, string chinese) =>\n"
    "        CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? chinese : english;\n",
)

# -----------------------------------------------------------------------------
# WPF demo cleanup and matching appearance controls.
# -----------------------------------------------------------------------------
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "using CadCommon;\nusing OcctNet;",
    "using System.Globalization;\nusing CadCommon;\nusing OcctNet;",
)
replace_once("src/CadWpf/MainWindow.xaml.cs", "    private OcctObject? _treeHighlighted;\n", "")
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "    private bool _autoZFitEnabled = true;\n",
    "    private bool _autoZFitEnabled = true;\n"
    "    private DrawingColor _selectionHighlightColor = DrawingColor.FromArgb(255, 155, 0);\n"
    "    private DrawingColor _hoverHighlightColor = DrawingColor.FromArgb(0, 185, 255);\n"
    "    private OcctSceneLightingSettings _lightingSettings = OcctLightingPresets.Create(OcctLightingPreset.Studio);\n",
)
regex_replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    r"\n        var samples = Menu\(MenuHeader\(\"Menu\.Samples\"\)\);\n        AddCommands\(samples,.*?\);\n",
    "\n",
)
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "        MainMenu.Items.Add(samples);\n",
    "",
)
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "        view.Items.Add(MenuItem(CadLocalization.Text(\"Menu.Lighting\"), (_, _) => SetLighting()));\n        view.Items.Add(MenuItem(CadLocalization.Text(\"Menu.ResetLighting\"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));",
    "        view.Items.Add(BuildLightingMenu());",
)
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "        view.Items.Add(BuildSelectionMenu());\n",
    "        view.Items.Add(BuildSelectionMenu());\n        view.Items.Add(BuildSelectionAppearanceMenu());\n",
)
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "            _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);\n",
    "            _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);\n"
    "            _session.Engine.SetSelectionHighlightColor(_selectionHighlightColor);\n"
    "            _session.Engine.SetHoverHighlightColor(_hoverHighlightColor);\n"
    "            _session.Engine.SetSceneLighting(_lightingSettings);\n",
)
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "        if (_treeHighlighted is { } previous && Session.Engine.Exists(previous)) Session.Engine.Unhighlight(previous);\n        Session.Engine.Highlight(value);\n        _treeHighlighted = value;\n",
    "",
)

wpf_lighting = r'''
    private Controls.MenuItem BuildLightingMenu()
    {
        var menu = Menu(Local("Lighting", "灯光"));
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            menu.Items.Add(MenuItem(LightingPresetName(captured), (_, _) => ApplyLightingPreset(captured)));
        }
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(Local("Custom Lighting...", "自定义灯光..."), (_, _) => SetAdvancedLighting()));
        menu.Items.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));
        return menu;
    }

    private void ApplyLightingPreset(OcctLightingPreset preset)
    {
        ExecuteSafe(() =>
        {
            _lightingSettings = OcctLightingPresets.Create(preset);
            Session.Engine.SetSceneLighting(_lightingSettings);
            Log($"{Local("Lighting", "灯光")}: {LightingPresetName(preset)}");
        });
    }

    private void SetAdvancedLighting()
    {
        static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
        var parameters = new[]
        {
            new CadParameterDefinition("ambient", Local("Ambient Intensity", "环境光强度"), CadParameterKind.Number, Number(_lightingSettings.AmbientIntensity)),
            new CadParameterDefinition("cameraEnabled", Local("Camera Light", "相机直射光"), CadParameterKind.Boolean, _lightingSettings.CameraLight.Enabled.ToString()),
            new CadParameterDefinition("camera", Local("Camera Light Intensity", "相机直射光强度"), CadParameterKind.Number, Number(_lightingSettings.CameraLight.Intensity)),
            new CadParameterDefinition("sunEnabled", Local("Sun Light", "太阳光"), CadParameterKind.Boolean, _lightingSettings.SunLight.Enabled.ToString()),
            new CadParameterDefinition("sun", Local("Sun Intensity", "太阳光强度"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Intensity)),
            new CadParameterDefinition("sunX", Local("Sun Direction X", "太阳光方向 X"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.X)),
            new CadParameterDefinition("sunY", Local("Sun Direction Y", "太阳光方向 Y"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Y)),
            new CadParameterDefinition("sunZ", Local("Sun Direction Z", "太阳光方向 Z"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Z)),
            new CadParameterDefinition("fillEnabled", Local("Fill Light", "补光"), CadParameterKind.Boolean, _lightingSettings.FillLight.Enabled.ToString()),
            new CadParameterDefinition("fill", Local("Fill Intensity", "补光强度"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Intensity)),
            new CadParameterDefinition("fillX", Local("Fill Direction X", "补光方向 X"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.X)),
            new CadParameterDefinition("fillY", Local("Fill Direction Y", "补光方向 Y"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Y)),
            new CadParameterDefinition("fillZ", Local("Fill Direction Z", "补光方向 Z"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Z))
        };
        if (!ParameterDialog.TryGetValues(this, Local("Custom Lighting", "自定义灯光"), parameters, out var raw)) return;
        var values = new CadValues(raw);
        var settings = _lightingSettings with
        {
            AmbientIntensity = values.Number("ambient"),
            CameraLight = _lightingSettings.CameraLight with
            {
                Enabled = values.Boolean("cameraEnabled", true),
                Intensity = values.Number("camera")
            },
            SunLight = _lightingSettings.SunLight with
            {
                Enabled = values.Boolean("sunEnabled", true),
                Intensity = values.Number("sun"),
                Direction = values.Vector("sunX", "sunY", "sunZ")
            },
            FillLight = _lightingSettings.FillLight with
            {
                Enabled = values.Boolean("fillEnabled", true),
                Intensity = values.Number("fill"),
                Direction = values.Vector("fillX", "fillY", "fillZ")
            }
        };
        ExecuteSafe(() =>
        {
            Session.Engine.SetSceneLighting(settings);
            _lightingSettings = settings;
            Log(Local("Custom lighting applied.", "已应用自定义灯光。"));
        });
    }

    private Controls.MenuItem BuildSelectionAppearanceMenu()
    {
        var menu = Menu(Local("Selection Appearance", "选择外观"));
        menu.Items.Add(MenuItem(Local("Selected Color...", "选中高亮颜色..."), (_, _) => SetSelectionHighlightColor()));
        menu.Items.Add(MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), (_, _) => SetHoverHighlightColor()));
        return menu;
    }

    private void SetSelectionHighlightColor()
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = _selectionHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _selectionHighlightColor = dialog.Color;
            Session.Engine.SetSelectionHighlightColor(dialog.Color);
        });
    }

    private void SetHoverHighlightColor()
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = _hoverHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _hoverHighlightColor = dialog.Color;
            Session.Engine.SetHoverHighlightColor(dialog.Color);
        });
    }
'''

regex_replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    r"    private void SetLighting\(\)\n    \{.*?\n    \}\n\n    private void SetSelectionTolerance",
    wpf_lighting + "\n    private void SetSelectionTolerance",
)
replace_once(
    "src/CadWpf/MainWindow.xaml.cs",
    "    private static string SelectionModeName(OcctSelectionMode mode) => CadLocalization.SelectionMode(mode);\n",
    "    private static string SelectionModeName(OcctSelectionMode mode) => CadLocalization.SelectionMode(mode);\n"
    "    private static string LightingPresetName(OcctLightingPreset preset) => preset switch\n"
    "    {\n"
    "        OcctLightingPreset.Neutral => Local(\"Neutral\", \"中性\"),\n"
    "        OcctLightingPreset.Sunlight => Local(\"Sunlight\", \"日光\"),\n"
    "        OcctLightingPreset.Flat => Local(\"Flat\", \"平光\"),\n"
    "        _ => Local(\"Studio\", \"摄影棚\")\n"
    "    };\n",
)

# -----------------------------------------------------------------------------
# Keep docs to one exact English inventory and one exact Chinese inventory.
# -----------------------------------------------------------------------------
def collect_native_inventory() -> tuple[OrderedDict[str, list[str]], list[str], list[str]]:
    headers = [
        "src/OcctNative/OcctNative.h",
        "src/OcctNative/OcctSelectionOverlay.h",
        "src/OcctNative/OcctModeling.h",
        "src/OcctNative/OcctOcaf.h",
        "src/OcctNative/OcctOcafExtended.h",
    ]
    groups: OrderedDict[str, list[str]] = OrderedDict()
    types: list[str] = []
    for header in headers:
        current = Path(header).name
        statement = ""
        for raw_line in read(header).splitlines():
            line = raw_line.strip()
            comment = re.match(r"//\s*(.+?)[.]?$", line)
            if comment and not line.startswith("///"):
                current = f"{Path(header).name} — {comment.group(1)}"
            type_match = re.match(r"(?:struct|enum)\s+(Occt[A-Za-z0-9_]+)", line)
            if type_match:
                types.append(type_match.group(1))
            statement += " " + line
            if ";" not in line:
                continue
            function = re.search(r"OCCTBRIDGE_API\s+.*?\b(occt_[a-z0-9_]+)\s*\(", statement)
            if function:
                groups.setdefault(current, []).append(function.group(1))
            statement = ""
    pinvoke_names: list[str] = []
    for source in sorted((ROOT / "src/OcctNet").glob("*NativeMethods*.cs")):
        pinvoke_names.extend(re.findall(r"extern\s+[A-Za-z0-9_<>,\[\]?]+\s+(occt_[a-z0-9_]+)\s*\(", source.read_text(encoding="utf-8-sig")))
    return groups, sorted(set(types)), sorted(set(pinvoke_names))


def collect_managed_types() -> list[str]:
    names: set[str] = set()
    pattern = re.compile(r"public\s+(?:sealed\s+)?(?:static\s+)?(?:partial\s+)?(?:class|struct|enum|interface|readonly\s+record\s+struct|record\s+struct)\s+([A-Za-z0-9_]+)")
    for source in sorted((ROOT / "src/OcctNet").glob("*.cs")):
        names.update(pattern.findall(source.read_text(encoding="utf-8-sig")))
    return sorted(names)


def render_inventory(chinese: bool) -> str:
    groups, types, pinvokes = collect_native_inventory()
    exports = [name for values in groups.values() for name in values]
    managed_types = collect_managed_types()
    title = "OCCT 封装接口详细清单" if chinese else "OCCT Bridge API Inventory"
    intro = (
        "本文件由源码接口声明整理，列出当前原生 C ABI、C# P/Invoke 映射及公开 .NET 类型。"
        if chinese
        else "This source-derived inventory lists the current native C ABI, C# P/Invoke mapping, and public .NET types."
    )
    lines = [f"# {title}", "", intro, "", f"- OCCT: `7.9.0`", f"- Native exports: `{len(exports)}`", f"- Managed P/Invoke declarations: `{len(pinvokes)}`", f"- Public .NET types: `{len(managed_types)}`", ""]
    if sorted(set(exports)) != pinvokes:
        lines.append("> WARNING: Native exports and managed P/Invoke declarations are not identical." if not chinese else "> 警告：原生导出与 C# P/Invoke 声明不一致。")
        lines.append("")
    lines.append("## 原生 C ABI" if chinese else "## Native C ABI")
    lines.append("")
    for section, functions in groups.items():
        lines.append(f"### {section} ({len(functions)})")
        lines.append("")
        lines.extend(f"- `{name}`" for name in sorted(set(functions)))
        lines.append("")
    lines.append("## 原生数据类型" if chinese else "## Native data types")
    lines.append("")
    lines.extend(f"- `{name}`" for name in types)
    lines.append("")
    lines.append("## 公开 .NET 类型" if chinese else "## Public .NET types")
    lines.append("")
    lines.extend(f"- `{name}`" for name in managed_types)
    lines.append("")
    lines.append("## 一致性规则" if chinese else "## Consistency rule")
    lines.append("")
    lines.append(
        "`tests/check-api-surface.ps1` 校验每个原生声明均存在 C++ 定义和 C# P/Invoke 声明。"
        if chinese
        else "`tests/check-api-surface.ps1` verifies that every native declaration has both a C++ definition and a C# P/Invoke declaration."
    )
    lines.append("")
    return "\n".join(lines)


docs = ROOT / "docs"
docs.mkdir(exist_ok=True)
for item in docs.iterdir():
    if item.is_file():
        item.unlink()
write("docs/API_COVERAGE.md", render_inventory(False))
write("docs/API_COVERAGE.zh-CN.md", render_inventory(True))

print("Demo UI, wrapper, selection, background, lighting, and docs enhancements applied.")
