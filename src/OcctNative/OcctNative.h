#pragma once

#include <cstdint>
#include "OcctStatus.h"

#if defined(_WIN32)
#define OCCTBRIDGE_API __declspec(dllexport)
#elif defined(__GNUC__) || defined(__clang__)
#define OCCTBRIDGE_API __attribute__((visibility("default")))
#else
#define OCCTBRIDGE_API
#endif

extern "C"
{
    using OcctObjectId = std::int64_t;

    struct OcctPoint3d { double x; double y; double z; };
    struct OcctVector3d { double x; double y; double z; };
    struct OcctBounds { double minX; double minY; double minZ; double maxX; double maxY; double maxZ; };
    struct OcctMassProperties { double mass; double centerX; double centerY; double centerZ; };
    struct OcctDistanceResult { double distance; OcctPoint3d pointOnFirst; OcctPoint3d pointOnSecond; };
    struct OcctEdgeProjectionResult { OcctPoint3d point; OcctVector3d tangent; double normalizedParameter; double distance; };
    struct OcctEdgeTangentPoint { OcctPoint3d point; double normalizedParameter; };
    enum OcctIntersectionKind
    {
        OcctIntersection_Point = 0,
        OcctIntersection_Overlap = 1
    };
    struct OcctEdgeIntersection
    {
        int kind;
        OcctPoint3d startPoint;
        OcctPoint3d endPoint;
        double firstParameterStart;
        double firstParameterEnd;
        double secondParameterStart;
        double secondParameterEnd;
    };
    struct OcctFaceProjectionResult { OcctPoint3d point; OcctVector3d normal; double u; double v; double distance; };
    struct OcctCameraState { OcctPoint3d eye; OcctPoint3d center; OcctVector3d up; OcctVector3d direction; double scale; };
    struct OcctProjectionRay { OcctPoint3d origin; OcctVector3d direction; };
    struct OcctViewportState
    {
        int width;
        int height;
        int projectionType;
        int computedMode;
        int antialiasingEnabled;
        int msaaSamples;
        int renderingMethod;
        int shadowsEnabled;
        int frustumCullingEnabled;
        int faceBoundariesVisible;
        int selectionTolerance;
        int automaticHighlight;
        double perspectiveFov;
        double renderResolutionScale;
        double renderResolutionDpi;
    };
    struct OcctAutoZFitSettings { int enabled; double scaleFactor; };
    struct OcctPolygonOffsetSettings { int mode; double factor; double units; };
    struct OcctColorRgb { double r; double g; double b; };
    struct OcctSceneLightingSettings
    {
        OcctColorRgb ambientColor;
        double ambientIntensity;
        int cameraLightEnabled;
        OcctColorRgb cameraLightColor;
        double cameraLightIntensity;
        OcctVector3d cameraLightDirection;
        int sunLightEnabled;
        OcctColorRgb sunLightColor;
        double sunLightIntensity;
        OcctVector3d sunLightDirection;
        int fillLightEnabled;
        OcctColorRgb fillLightColor;
        double fillLightIntensity;
        OcctVector3d fillLightDirection;
    };
    struct OcctUvBounds { double uMin; double uMax; double vMin; double vMax; };
    struct OcctObjectDescriptor { OcctObjectId objectId; int kind; };

    enum OcctObjectKind
    {
        OcctObject_Unknown = 0,
        OcctObject_Shape = 1,
        OcctObject_Text = 2,
        OcctObject_Dimension = 3,
        OcctObject_Point = 4,
        OcctObject_Overlay = 5,
        OcctObject_Manipulator = 6
    };

    enum OcctShapeType
    {
        OcctShape_Compound = 0,
        OcctShape_CompSolid = 1,
        OcctShape_Solid = 2,
        OcctShape_Shell = 3,
        OcctShape_Face = 4,
        OcctShape_Wire = 5,
        OcctShape_Edge = 6,
        OcctShape_Vertex = 7,
        OcctShape_Shape = 8
    };

    enum OcctViewOrientation
    {
        OcctView_Isometric = 0,
        OcctView_Front = 1,
        OcctView_Back = 2,
        OcctView_Left = 3,
        OcctView_Right = 4,
        OcctView_Top = 5,
        OcctView_Bottom = 6
    };

    enum OcctProjectionType { OcctProjection_Orthographic = 0, OcctProjection_Perspective = 1 };
    enum OcctDisplayMode { OcctDisplay_Wireframe = 0, OcctDisplay_Shaded = 1 };
    enum OcctRenderingMethod { OcctRendering_Rasterization = 0, OcctRendering_RayTracing = 1 };

    enum OcctZUpViewOrientation
    {
        OcctZUp_Front = 0,
        OcctZUp_Back = 1,
        OcctZUp_Left = 2,
        OcctZUp_Right = 3,
        OcctZUp_Top = 4,
        OcctZUp_Bottom = 5,
        OcctZUp_XNegativeYNegative = 6,
        OcctZUp_XPositiveYNegative = 7,
        OcctZUp_XNegativeYPositive = 8,
        OcctZUp_XPositiveYPositive = 9
    };

    enum OcctSelectionMode
    {
        OcctSelection_Object = 0,
        OcctSelection_Vertex = 1,
        OcctSelection_Edge = 2,
        OcctSelection_Wire = 3,
        OcctSelection_Face = 4,
        OcctSelection_Shell = 5,
        OcctSelection_Solid = 6
    };

    enum OcctBooleanOperation
    {
        OcctBoolean_Fuse = 0,
        OcctBoolean_Cut = 1,
        OcctBoolean_Common = 2,
        OcctBoolean_Section = 3
    };

    enum OcctCurveType
    {
        OcctCurve_Line = 0,
        OcctCurve_Circle = 1,
        OcctCurve_Ellipse = 2,
        OcctCurve_Hyperbola = 3,
        OcctCurve_Parabola = 4,
        OcctCurve_Bezier = 5,
        OcctCurve_BSpline = 6,
        OcctCurve_Offset = 7,
        OcctCurve_Other = 8
    };

    enum OcctSurfaceType
    {
        OcctSurface_Plane = 0,
        OcctSurface_Cylinder = 1,
        OcctSurface_Cone = 2,
        OcctSurface_Sphere = 3,
        OcctSurface_Torus = 4,
        OcctSurface_Bezier = 5,
        OcctSurface_BSpline = 6,
        OcctSurface_Revolution = 7,
        OcctSurface_Extrusion = 8,
        OcctSurface_Offset = 9,
        OcctSurface_Other = 10
    };

    enum OcctMaterial
    {
        OcctMaterial_Brass = 0,
        OcctMaterial_Bronze = 1,
        OcctMaterial_Copper = 2,
        OcctMaterial_Gold = 3,
        OcctMaterial_Pewter = 4,
        OcctMaterial_Plastered = 5,
        OcctMaterial_Plastified = 6,
        OcctMaterial_Silver = 7,
        OcctMaterial_Steel = 8,
        OcctMaterial_Stone = 9,
        OcctMaterial_ShinyPlastified = 10,
        OcctMaterial_Satin = 11,
        OcctMaterial_Metalized = 12,
        OcctMaterial_Ionized = 13,
        OcctMaterial_Chrome = 14,
        OcctMaterial_Aluminum = 15,
        OcctMaterial_Obsidian = 16,
        OcctMaterial_Neon = 17,
        OcctMaterial_Jade = 18,
        OcctMaterial_Charcoal = 19,
        OcctMaterial_Water = 20,
        OcctMaterial_Glass = 21,
        OcctMaterial_Diamond = 22,
        OcctMaterial_Transparent = 23,
        OcctMaterial_Default = 24
    };

    enum OcctSelectionOperation
    {
        OcctSelection_Replace = 0,
        OcctSelection_Add = 1,
        OcctSelection_Remove = 2,
        OcctSelection_Toggle = 3,
        OcctSelection_Clear = 4
    };

    enum OcctShapeUpdateOptions
    {
        OcctShapeUpdate_None = 0,
        OcctShapeUpdate_PreserveAppearance = 1 << 0,
        OcctShapeUpdate_PreserveTransformation = 1 << 1,
        OcctShapeUpdate_PreserveSelection = 1 << 2,
        OcctShapeUpdate_PreserveSelectability = 1 << 3,
        OcctShapeUpdate_RecomputePresentation = 1 << 4,
        OcctShapeUpdate_RecomputeSelection = 1 << 5,
        OcctShapeUpdate_PreserveAll =
            OcctShapeUpdate_PreserveAppearance |
            OcctShapeUpdate_PreserveTransformation |
            OcctShapeUpdate_PreserveSelection |
            OcctShapeUpdate_PreserveSelectability |
            OcctShapeUpdate_RecomputePresentation |
            OcctShapeUpdate_RecomputeSelection
    };

    enum OcctViewCubeLanguage
    {
        OcctViewCubeLanguage_English = 0,
        OcctViewCubeLanguage_ChineseSimplified = 1
    };

    struct OcctTransform3d
    {
        double m00; double m01; double m02; double m03;
        double m10; double m11; double m12; double m13;
        double m20; double m21; double m22; double m23;
    };

    // ABI5 core lifecycle and diagnostics. Domain APIs live in their own headers.
    OCCTBRIDGE_API OcctEngineHandle occt_engine_create();
    OCCTBRIDGE_API void occt_engine_destroy(OcctEngineHandle handle);
    OCCTBRIDGE_API OcctStatus occt_engine_last_error_code(OcctEngineHandle handle);
    OCCTBRIDGE_API OcctStatus occt_engine_last_error_message(
        OcctEngineHandle handle,
        char* buffer,
        int capacity,
        int* required);

    OCCTBRIDGE_API const char* occt_version();
    OCCTBRIDGE_API int occt_bridge_current_abi_version();
    OCCTBRIDGE_API const char* occt_bridge_version();
    OCCTBRIDGE_API const char* occt_bridge_build_info();
}
