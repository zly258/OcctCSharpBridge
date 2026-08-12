# OcctNative C ABI 完整参考

Author: **zly258**。Bridge **2.7.0** · Native ABI **4** · OCCT **7.9.0** · .NET SDK **10.0.302** · C# **14.0** · C++17 · Avalonia **12.1.0** · `net10.0-windows` · Windows x64。

本页从 `bridge-contract.json` 声明的全部 Native 公开头文件生成，覆盖公开 ABI 类型和全部 `OCCTBRIDGE_API occt_*` 导出，用于 P/Invoke 对等核查、底层集成和 ABI 诊断。

- **Bridge:** `2.7.0`
- **Native ABI:** `4`
- **Exports:** `349`
- **Public headers:** `14`

## ABI 类型

### `OcctNative.h`

```cpp
using OcctHandle = void*;
    using OcctObjectId = std::int64_t;

    struct OcctPoint3d { double x; double y; double z; };
    struct OcctVector3d { double x; double y; double z; };
    struct OcctBounds { double minX; double minY; double minZ; double maxX; double maxY; double maxZ; };
    struct OcctMassProperties { double mass; double centerX; double centerY; double centerZ; };
    struct OcctDistanceResult { double distance; OcctPoint3d pointOnFirst; OcctPoint3d pointOnSecond; };
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

    enum OcctObjectKind { OcctObject_Unknown = 0, OcctObject_Shape = 1, OcctObject_Text = 2, OcctObject_Dimension = 3 };
    enum OcctShapeType { OcctShape_Compound = 0, OcctShape_CompSolid = 1, OcctShape_Solid = 2, OcctShape_Shell = 3, OcctShape_Face = 4, OcctShape_Wire = 5, OcctShape_Edge = 6, OcctShape_Vertex = 7, OcctShape_Shape = 8 };
    enum OcctViewOrientation { OcctView_Isometric = 0, OcctView_Front = 1, OcctView_Back = 2, OcctView_Left = 3, OcctView_Right = 4, OcctView_Top = 5, OcctView_Bottom = 6 };
    enum OcctProjectionType { OcctProjection_Orthographic = 0, OcctProjection_Perspective = 1 };
    enum OcctDisplayMode { OcctDisplay_Wireframe = 0, OcctDisplay_Shaded = 1 };
    enum OcctRenderingMethod { OcctRendering_Rasterization = 0, OcctRendering_RayTracing = 1 };
    enum OcctZUpViewOrientation
    {
        OcctZUp_Front = 0, OcctZUp_Back = 1, OcctZUp_Left = 2, OcctZUp_Right = 3,
        OcctZUp_Top = 4, OcctZUp_Bottom = 5,
        OcctZUp_XNegativeYNegative = 6, OcctZUp_XPositiveYNegative = 7,
        OcctZUp_XNegativeYPositive = 8, OcctZUp_XPositiveYPositive = 9
    };
    enum OcctSelectionMode { OcctSelection_Object = 0, OcctSelection_Vertex = 1, OcctSelection_Edge = 2, OcctSelection_Wire = 3, OcctSelection_Face = 4, OcctSelection_Shell = 5, OcctSelection_Solid = 6 };
    enum OcctBooleanOperation { OcctBoolean_Fuse = 0, OcctBoolean_Cut = 1, OcctBoolean_Common = 2, OcctBoolean_Section = 3 };
    enum OcctCurveType { OcctCurve_Line = 0, OcctCurve_Circle = 1, OcctCurve_Ellipse = 2, OcctCurve_Hyperbola = 3, OcctCurve_Parabola = 4, OcctCurve_Bezier = 5, OcctCurve_BSpline = 6, OcctCurve_Offset = 7, OcctCurve_Other = 8 };
    enum OcctSurfaceType { OcctSurface_Plane = 0, OcctSurface_Cylinder = 1, OcctSurface_Cone = 2, OcctSurface_Sphere = 3, OcctSurface_Torus = 4, OcctSurface_Bezier = 5, OcctSurface_BSpline = 6, OcctSurface_Revolution = 7, OcctSurface_Extrusion = 8, OcctSurface_Offset = 9, OcctSurface_Other = 10 };
    enum OcctMaterial
    {
        OcctMaterial_Brass = 0, OcctMaterial_Bronze = 1, OcctMaterial_Copper = 2, OcctMaterial_Gold = 3,
        OcctMaterial_Pewter = 4, OcctMaterial_Plastered = 5, OcctMaterial_Plastified = 6, OcctMaterial_Silver = 7,
        OcctMaterial_Steel = 8, OcctMaterial_Stone = 9, OcctMaterial_ShinyPlastified = 10, OcctMaterial_Satin = 11,
        OcctMaterial_Metalized = 12, OcctMaterial_Ionized = 13, OcctMaterial_Chrome = 14, OcctMaterial_Aluminum = 15,
        OcctMaterial_Obsidian = 16, OcctMaterial_Neon = 17, OcctMaterial_Jade = 18, OcctMaterial_Charcoal = 19,
        OcctMaterial_Water = 20, OcctMaterial_Glass = 21, OcctMaterial_Diamond = 22, OcctMaterial_Transparent = 23,
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
```

### `OcctRenderSurface.h`

```cpp
// Resize the native OCCT render surface without presenting a frame.
```

### `OcctStepDocument.h`

```cpp
// Return a UTF-8 JSON snapshot of the most recently imported STEP/XDE document.
    // The pointer remains valid until the engine scratch buffer is reused.
```

### `OcctPoints.h`

```cpp
enum OcctPointMarker
    {
        OcctPointMarker_Point = 0,
        OcctPointMarker_Plus = 1,
        OcctPointMarker_Star = 2,
        OcctPointMarker_X = 3,
        OcctPointMarker_Circle = 4,
        OcctPointMarker_CirclePoint = 5,
        OcctPointMarker_CirclePlus = 6,
        OcctPointMarker_CircleStar = 7,
        OcctPointMarker_CircleX = 8,
        OcctPointMarker_Ring1 = 9,
        OcctPointMarker_Ring2 = 10,
        OcctPointMarker_Ring3 = 11,
        OcctPointMarker_Ball = 12
    };
```

### `OcctSelectionOverlay.h`

```cpp
// Displays or updates an OCCT-native 2D rubber-band rectangle in the top overlay layer.
    // Coordinates use the host window client coordinate system (origin at left/top).
```

### `OcctSelectionState.h`

```cpp
struct OcctSelectionHit
    {
        OcctObjectId ownerObjectId;
        int subshapeType;
        int subshapeIndex;
    };

    // Returns the current registered AIS selection as structured object/subshape identities.
    // Call with items=nullptr/capacity=0 to query count, then call again with a large enough buffer.
    // subshapeIndex follows the same TopExp_Explorer ordering as occt_get_subshape;
    // whole-object selection uses OcctShape_Shape and index -1.
```

### `OcctModeling.h`

```cpp
using OcctModelHandle = void*;
    using OcctOperationId = std::int64_t;

    enum OcctModelState
    {
        OcctModelState_Unknown = 0,
        OcctModelState_Inside = 1,
        OcctModelState_Outside = 2,
        OcctModelState_On = 3
    };

    enum OcctModelOrientation
    {
        OcctModelOrientation_Forward = 0,
        OcctModelOrientation_Reversed = 1,
        OcctModelOrientation_Internal = 2,
        OcctModelOrientation_External = 3
    };

    enum OcctModelBooleanOperation
    {
        OcctModelBoolean_Fuse = 0,
        OcctModelBoolean_Cut = 1,
        OcctModelBoolean_Common = 2,
        OcctModelBoolean_Section = 3
    };

    enum OcctModelBooleanGlue
    {
        OcctModelGlue_Off = 0,
        OcctModelGlue_Shift = 1,
        OcctModelGlue_Full = 2
    };

    struct OcctModelBooleanOptions
    {
        double fuzzyValue;
        double angularTolerance;
        int runParallel;
        int nonDestructive;
        int glue;
        int checkInverted;
        int simplifyEdges;
        int simplifyFaces;
    };

    struct OcctModelAlgorithmResult
    {
        OcctObjectId shapeId;
        OcctOperationId operationId;
        int succeeded;
        int hasWarnings;
        int hasErrors;
    };

    struct OcctModelProjectionResult
    {
        OcctPoint3d point;
        double distance;
        double parameter;
        double u;
        double v;
    };

    struct OcctModelRayHit
    {
        OcctPoint3d point;
        OcctObjectId faceId;
        double rayParameter;
        double u;
        double v;
        int state;
    };

    struct OcctModelMeshParameters
    {
        double linearDeflection;
        double angularDeflection;
        double minSize;
        int relative;
        int parallel;
        int internalVertices;
        int controlSurfaceDeflection;
    };

    struct OcctModelMeshNode
    {
        OcctPoint3d point;
        double u;
        double v;
        OcctVector3d normal;
        int hasUv;
        int hasNormal;
    };

    struct OcctModelMeshTriangle
    {
        int node1;
        int node2;
        int node3;
    };

    struct OcctModelLineGeometry
    {
        OcctPoint3d origin;
        OcctVector3d direction;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelCircleGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double radius;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelEllipseGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double majorRadius;
        double minorRadius;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelPlaneGeometry
    {
        OcctPoint3d origin;
        OcctVector3d normal;
        OcctVector3d xDirection;
    };

    struct OcctModelCylinderGeometry
    {
        OcctPoint3d origin;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double radius;
    };

    struct OcctModelConeGeometry
    {
        OcctPoint3d apex;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double referenceRadius;
        double semiAngleRadians;
    };

    struct OcctModelSphereGeometry
    {
        OcctPoint3d center;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double radius;
    };

    struct OcctModelTorusGeometry
    {
        OcctPoint3d center;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double majorRadius;
        double minorRadius;
    };

    struct OcctModelParameterRange
    {
        double firstParameter;
        double lastParameter;
        int isClosed;
        int isPeriodic;
        double period;
    };

    struct OcctModelCurveDifferential
    {
        double parameter;
        OcctPoint3d point;
        OcctVector3d firstDerivative;
        OcctVector3d secondDerivative;
    };

    struct OcctModelCurveCurvature
    {
        double parameter;
        OcctPoint3d point;
        OcctVector3d tangent;
        OcctVector3d normal;
        OcctPoint3d centerOfCurvature;
        double curvature;
        int hasTangent;
        int hasNormal;
        int hasCenterOfCurvature;
    };

    struct OcctModelSurfacePeriodicity
    {
        int isUClosed;
        int isVClosed;
        int isUPeriodic;
        int isVPeriodic;
        double uPeriod;
        double vPeriod;
    };

    struct OcctModelSurfaceDifferential
    {
        double u;
        double v;
        OcctPoint3d point;
        OcctVector3d normal;
        OcctVector3d uDerivative;
        OcctVector3d vDerivative;
        OcctVector3d uSecondDerivative;
        OcctVector3d vSecondDerivative;
        OcctVector3d uvDerivative;
        int hasNormal;
    };

    struct OcctModelSurfaceCurvature
    {
        double u;
        double v;
        OcctPoint3d point;
        OcctVector3d normal;
        OcctVector3d maximumDirection;
        OcctVector3d minimumDirection;
        double maximumCurvature;
        double minimumCurvature;
        double meanCurvature;
        double gaussianCurvature;
        int isUmbilic;
        int hasNormal;
        int hasCurvature;
    };

    struct OcctModelLocation
    {
        double m11; double m12; double m13; double m14;
        double m21; double m22; double m23; double m24;
        double m31; double m32; double m33; double m34;
        double m41; double m42; double m43; double m44;
    };
```

### `OcctModelingExtensions.h`

```cpp
enum OcctModelJoinType
    {
        OcctModelJoin_Arc = 0,
        OcctModelJoin_Tangent = 1,
        OcctModelJoin_Intersection = 2
    };

    struct OcctOrientedBounds
    {
        OcctPoint3d center;
        OcctVector3d xDirection;
        OcctVector3d yDirection;
        OcctVector3d zDirection;
        double halfSizeX;
        double halfSizeY;
        double halfSizeZ;
    };
```

### `OcctModelingBSpline.h`

```cpp
struct OcctModelBSplineCurveInfo
    {
        int degree;
        int poleCount;
        int knotCount;
        int rational;
        int periodic;
    };

    struct OcctModelBSplineSurfaceInfo
    {
        int uDegree;
        int vDegree;
        int uPoleCount;
        int vPoleCount;
        int uKnotCount;
        int vKnotCount;
        int uRational;
        int vRational;
        int uPeriodic;
        int vPeriodic;
    };
```

### `OcctModelingTopologyAnalysis.h`

```cpp
enum OcctModelFreeBoundaryKind
    {
        OcctModelFreeBoundary_Closed = 0,
        OcctModelFreeBoundary_Open = 1
    };

    struct OcctModelEdgeAdjacency
    {
        OcctObjectId edgeId;
        int adjacentFaceCount;
    };
```

### `OcctModelingFaceAnalysis.h`

```cpp
struct OcctModelFaceAnalysis
    {
        OcctObjectId faceId;
        int surfaceType;
        int orientation;
        int edgeCount;
        int wireCount;
        double area;
        double maximumTolerance;
        OcctUvBounds uvBounds;
        OcctBounds bounds;
    };
```

### `OcctModelingInertia.h`

```cpp
struct OcctModelInertiaProperties
    {
        double mass;
        OcctPoint3d centerOfMass;
        double ixx;
        double iyy;
        double izz;
        double ixy;
        double ixz;
        double iyz;
        double principalMoment1;
        double principalMoment2;
        double principalMoment3;
        OcctVector3d principalAxis1;
        OcctVector3d principalAxis2;
        OcctVector3d principalAxis3;
        double radiusOfGyration1;
        double radiusOfGyration2;
        double radiusOfGyration3;
        int hasSymmetryAxis;
        int hasSymmetryPoint;
    };
```

### `OcctModelingIntersection.h`

```cpp
enum OcctModelIntersectionKind
    {
        OcctModelIntersection_Point = 0,
        OcctModelIntersection_Overlap = 1
    };

    struct OcctModelEdgeIntersection
    {
        int kind;
        OcctPoint3d startPoint;
        OcctPoint3d endPoint;
        double firstParameterStart;
        double firstParameterEnd;
        double secondParameterStart;
        double secondParameterEnd;
    };
```

### `OcctModelingTopologyReference.h`

```cpp
enum OcctModelTopologyReferenceStatus
    {
        OcctModelTopologyReference_Resolved = 0,
        OcctModelTopologyReference_Ambiguous = 1,
        OcctModelTopologyReference_Removed = 2,
        OcctModelTopologyReference_NotFound = 3,
        OcctModelTopologyReference_Invalid = 4
    };

    struct OcctModelTopologyReference
    {
        int version;
        int shapeType;
        int runtimeIndexHint;
        int curveType;
        int surfaceType;
        double measure;
        OcctPoint3d center;
        OcctBounds bounds;
        double tolerance;
        int orientation;
        int vertexCount;
        int edgeCount;
        int faceCount;
    };

    struct OcctModelTopologyReferenceResult
    {
        int status;
        OcctObjectId shapeId;
        double score;
        int candidateCount;
        int usedOperationHistory;
        int runtimeIndexMatched;
    };
```

## 导出函数

### `OcctNative.h`

#### OcctNative

##### `occt_create`

- **分类:** OcctNative
- **返回值:** `OcctHandle`

```cpp
OCCTBRIDGE_API OcctHandle occt_create();
```

**参数:** 无

##### `occt_destroy`

- **分类:** OcctNative
- **返回值:** `void`

```cpp
OCCTBRIDGE_API void occt_destroy(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_last_error`

- **分类:** OcctNative
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_last_error(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_version`

- **分类:** OcctNative
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_version();
```

**参数:** 无

##### `occt_bridge_abi_version`

- **分类:** OcctNative
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_bridge_abi_version();
```

**参数:** 无

##### `occt_bridge_version`

- **分类:** OcctNative
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_bridge_version();
```

**参数:** 无

##### `occt_bridge_build_info`

- **分类:** OcctNative
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_bridge_build_info();
```

**参数:** 无

#### Viewer and interaction

##### `occt_initialize`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_initialize(OcctHandle handle, void* windowHandle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `windowHandle` | `void*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_resize`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_resize(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_redraw`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_redraw(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_begin_update`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_begin_update(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_end_update`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_end_update(OcctHandle handle, int fitAll);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `fitAll` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_is_updating`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_is_updating(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_fit_all`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_all(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_fit_object`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_object(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_window_fit`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_window_fit(OcctHandle handle, int x1, int y1, int x2, int y2);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x1` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y1` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `x2` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y2` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_view`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view(OcctHandle handle, int orientation);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `orientation` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_projection`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_projection(OcctHandle handle, int projectionType);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `projectionType` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_perspective_fov`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_perspective_fov(OcctHandle handle, double degrees);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `degrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_background`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_background(OcctHandle handle, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_display_mode`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_display_mode(OcctHandle handle, int displayMode);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `displayMode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_triedron_visible`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_triedron_visible(OcctHandle handle, int visible);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `visible` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_view_cube_visible`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_cube_visible(OcctHandle handle, int visible);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `visible` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_computed_mode`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_computed_mode(OcctHandle handle, int enabled);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `enabled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_dump_view`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_dump_view(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_screen_to_world`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_screen_to_world(OcctHandle handle, int x, int y, OcctPoint3d* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `result` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_world_to_screen`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_world_to_screen(OcctHandle handle, OcctPoint3d point, int* x, int* y);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `point` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `x` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `y` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_move_to`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_move_to(OcctHandle handle, int x, int y);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_select`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_select(OcctHandle handle, int x, int y, int appendSelection);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `appendSelection` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_select_rectangle_ex`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_select_rectangle_ex(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection, int allowOverlap);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x1` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y1` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `x2` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y2` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `appendSelection` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `allowOverlap` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_select_object`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_select_object(OcctHandle handle, OcctObjectId objectId, int appendSelection);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `appendSelection` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_selection_mode`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_mode(OcctHandle handle, int selectionMode);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `selectionMode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_clear_selection`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_clear_selection(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_start_rotation`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_start_rotation(OcctHandle handle, int x, int y);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_rotation`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_rotation(OcctHandle handle, int x, int y);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_pan`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_pan(OcctHandle handle, int deltaX, int deltaY);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `deltaX` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `deltaY` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_zoom`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_zoom(OcctHandle handle, double factor);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `factor` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_get_camera`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_camera(OcctHandle handle, OcctCameraState* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `result` | `OcctCameraState*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_set_camera`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_camera(OcctHandle handle, const OcctCameraState* state);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `state` | `const OcctCameraState*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_get_view_scale`

- **分类:** Viewer and interaction
- **返回值:** `double`

```cpp
OCCTBRIDGE_API double occt_get_view_scale(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_set_view_scale`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_scale(OcctHandle handle, double scale);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `scale` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_antialiasing`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_antialiasing(OcctHandle handle, int enabled);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `enabled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_gradient_background`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_gradient_background(OcctHandle handle, double r1, double g1, double b1, double r2, double g2, double b2, int fillMethod);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `r1` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g1` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b1` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r2` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g2` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b2` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `fillMethod` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_display_precision`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_display_precision(OcctHandle handle, double deviationCoefficient, double deviationAngleDegrees, int applyExisting);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `deviationCoefficient` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `deviationAngleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `applyExisting` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_default_material`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_default_material(OcctHandle handle, int material, int applyExisting);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `material` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `applyExisting` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_scene_lighting_ex`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_scene_lighting_ex(OcctHandle handle, const OcctSceneLightingSettings* settings);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `settings` | `const OcctSceneLightingSettings*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_set_selection_highlight_color`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_highlight_color(OcctHandle handle, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_hover_highlight_color`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_hover_highlight_color(OcctHandle handle, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_reset_scene_lighting`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_scene_lighting(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_set_selection_tolerance`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_tolerance(OcctHandle handle, int pixelTolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `pixelTolerance` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_auto_z_fit_mode`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_auto_z_fit_mode(OcctHandle handle, int enabled, double scaleFactor);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `enabled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `scaleFactor` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_get_auto_z_fit_mode`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_auto_z_fit_mode(OcctHandle handle, OcctAutoZFitSettings* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `result` | `OcctAutoZFitSettings*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_auto_z_fit`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_auto_z_fit(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_set_default_polygon_offsets`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_default_polygon_offsets(OcctHandle handle, int mode, double factor, double units, int applyExisting);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `mode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `factor` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `units` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `applyExisting` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_get_default_polygon_offsets`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_default_polygon_offsets(OcctHandle handle, OcctPolygonOffsetSettings* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `result` | `OcctPolygonOffsetSettings*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_set_object_polygon_offsets`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, int mode, double factor, double units);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `mode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `factor` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `units` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_get_object_polygon_offsets`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, OcctPolygonOffsetSettings* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctPolygonOffsetSettings*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_reset_object_polygon_offsets`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_fit_objects`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, double margin);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `margin` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_zup_view`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_zup_view(OcctHandle handle, int orientation, int fitAll);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `orientation` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `fitAll` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_screen_to_ray`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_screen_to_ray(OcctHandle handle, int x, int y, OcctProjectionRay* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `result` | `OcctProjectionRay*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_zoom_at_point`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_zoom_at_point(OcctHandle handle, int x, int y, double delta);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `delta` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_select_all_visible`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_select_all_visible(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_invert_selection`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_invert_selection(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_hide_selected`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_selected(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_set_automatic_highlight`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_automatic_highlight(OcctHandle handle, int enabled);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `enabled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_msaa_samples`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_msaa_samples(OcctHandle handle, int samples);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `samples` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_render_resolution_scale`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_render_resolution_scale(OcctHandle handle, double scale);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `scale` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_render_resolution`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_render_resolution(OcctHandle handle, double dpi);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `dpi` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_rendering_method`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_rendering_method(OcctHandle handle, int method);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `method` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_shadows_enabled`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_shadows_enabled(OcctHandle handle, int enabled);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `enabled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_immediate_update`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_immediate_update(OcctHandle handle, int enabled);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `enabled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_frustum_culling`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_frustum_culling(OcctHandle handle, int enabled);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `enabled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_face_boundaries_visible`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_face_boundaries_visible(OcctHandle handle, int visible, int applyExisting);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `visible` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `applyExisting` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_get_viewport_state`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_viewport_state(OcctHandle handle, OcctViewportState* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `result` | `OcctViewportState*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_reset_view`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_reset_view_orientation`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view_orientation(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_reset_view_mapping`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view_mapping(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_fit_selected`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_selected(OcctHandle handle, double margin);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `margin` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_get_scene_gravity_point`

- **分类:** Viewer and interaction
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_scene_gravity_point(OcctHandle handle, OcctPoint3d* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `result` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

#### Registry, AIS attributes and lifecycle

##### `occt_object_count`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_object_count(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_object_descriptors`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_object_descriptors(OcctHandle handle, OcctObjectDescriptor* items, int capacity, int* objectCount, int* shapeCount);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `items` | `OcctObjectDescriptor*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `objectCount` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `shapeCount` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_object_exists`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_object_exists(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_object_kind`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_object_kind(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_set_object_name`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_name(OcctHandle handle, OcctObjectId objectId, const char* utf8Name);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Name` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_get_object_name`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_object_name(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_set_object_application_tag`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_application_tag(OcctHandle handle, OcctObjectId objectId, const char* utf8Tag);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Tag` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_get_object_application_tag`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_object_application_tag(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_find_object_by_application_tag`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_find_object_by_application_tag(OcctHandle handle, const char* utf8Tag);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Tag` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_set_object_selectable`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_selectable(OcctHandle handle, OcctObjectId objectId, int selectable);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `selectable` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_get_object_selectable`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_selectable(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_set_objects_selectable`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_selectable(OcctHandle handle, const OcctObjectId* objectIds, int count, int selectable);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `selectable` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_selected_objects_ex`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selected_objects_ex(OcctHandle handle, const OcctObjectId* objectIds, int count, int operation);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `operation` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_object_transform`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transform(OcctHandle handle, OcctObjectId objectId, const double* matrix3x4);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `matrix3x4` | `const double*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_get_object_transform`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_transform(OcctHandle handle, OcctObjectId objectId, double* matrix3x4, int* hasTransform);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `matrix3x4` | `double*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `hasTransform` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_reset_object_transform`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_object_transform(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_set_view_cube_language`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_cube_language(OcctHandle handle, int language);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `language` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_object_color`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_color(OcctHandle handle, OcctObjectId objectId, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_object_transparency`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transparency(OcctHandle handle, OcctObjectId objectId, double transparency);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `transparency` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_object_visible`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_visible(OcctHandle handle, OcctObjectId objectId, int visible);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `visible` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_object_display_mode`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_display_mode(OcctHandle handle, OcctObjectId objectId, int displayMode);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `displayMode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_object_line_width`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_line_width(OcctHandle handle, OcctObjectId objectId, double width);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `width` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_object_material`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_material(OcctHandle handle, OcctObjectId objectId, int material);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `material` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_objects_color`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_color(OcctHandle handle, const OcctObjectId* objectIds, int count, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_objects_transparency`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_transparency(OcctHandle handle, const OcctObjectId* objectIds, int count, double transparency);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `transparency` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_objects_visible`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_visible(OcctHandle handle, const OcctObjectId* objectIds, int count, int visible);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `visible` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_objects_display_mode`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_display_mode(OcctHandle handle, const OcctObjectId* objectIds, int count, int displayMode);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `displayMode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_objects_line_width`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_line_width(OcctHandle handle, const OcctObjectId* objectIds, int count, double width);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `width` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_objects_material`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_material(OcctHandle handle, const OcctObjectId* objectIds, int count, int material);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `material` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_redisplay_objects`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_redisplay_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_select_objects`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_select_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, int appendSelection);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `appendSelection` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_object_is_visible`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_object_is_visible(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_object_is_selected`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_object_is_selected(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_delete_objects`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_delete_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_clear`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_clear(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_show_all`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_show_all(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_hide_all`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_all(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_redisplay_object`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_redisplay_object(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_highlight_object`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_highlight_object(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_unhighlight_object`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_unhighlight_object(OcctHandle handle, OcctObjectId objectId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_copy_selected_subshape_at`

- **分类:** Registry, AIS attributes and lifecycle
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_copy_selected_subshape_at(OcctHandle handle, int index);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `index` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

#### Shape query and analysis

##### `occt_shape_type`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_type(OcctHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_shape_is_valid`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_is_valid(OcctHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_shape_bounds`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_bounds(OcctHandle handle, OcctObjectId shapeId, OcctBounds* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctBounds*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_shape_linear_properties`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_linear_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctMassProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_shape_surface_properties`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_surface_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctMassProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_shape_volume_properties`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_volume_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctMassProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_shape_distance`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_distance(OcctHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `firstId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `secondId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctDistanceResult*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_topology_count`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_topology_count(OcctHandle handle, OcctObjectId shapeId, int shapeType);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `shapeType` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_get_subshape`

- **分类:** Shape query and analysis
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_get_subshape(OcctHandle handle, OcctObjectId shapeId, int shapeType, int index);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `shapeType` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `index` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_copy_shape`

- **分类:** Shape query and analysis
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_copy_shape(OcctHandle handle, OcctObjectId shapeId, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_shape_hash`

- **分类:** Shape query and analysis
- **返回值:** `std::int64_t`

```cpp
OCCTBRIDGE_API std::int64_t occt_shape_hash(OcctHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_vertex_point`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_vertex_point(OcctHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `vertexId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_edge_endpoints`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_endpoints(OcctHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `start` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `end` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_edge_point_at`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_point_at(OcctHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `normalizedParameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `point` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `tangent` | `OcctVector3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_edge_curve_type`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_curve_type(OcctHandle handle, OcctObjectId edgeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_face_surface_type`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_face_surface_type(OcctHandle handle, OcctObjectId faceId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_face_uv_bounds`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_face_uv_bounds(OcctHandle handle, OcctObjectId faceId, OcctUvBounds* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctUvBounds*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_face_point_normal`

- **分类:** Shape query and analysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_face_point_normal(OcctHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `u` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `v` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `point` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `normal` | `OcctVector3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

#### Shape transformations

##### `occt_translate`

- **分类:** Shape transformations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_translate(OcctHandle handle, OcctObjectId shapeId, OcctVector3d vector, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `vector` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_rotate`

- **分类:** Shape transformations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_rotate(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `axisPoint` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axisDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `angleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_scale`

- **分类:** Shape transformations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_scale(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `factor` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_mirror_plane`

- **分类:** Shape transformations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_mirror_plane(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `planePoint` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `planeNormal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

#### Basic points, 2D/3D curves and planar elements

##### `occt_make_vertex`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_vertex(OcctHandle handle, OcctPoint3d point);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `point` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_line`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_line(OcctHandle handle, OcctPoint3d start, OcctPoint3d end);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `start` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `end` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_polyline`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_polyline(OcctHandle handle, const OcctPoint3d* points, int count, int closed);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `points` | `const OcctPoint3d*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `closed` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_make_circle`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_circle(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_arc_three_points`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_arc_three_points(OcctHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `start` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `middle` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `end` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_arc_center`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_arc_center(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `startAngleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `endAngleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_regular_polygon`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_regular_polygon(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `sideCount` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `makeFace` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_make_ellipse`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_ellipse(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `majorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `minorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_bezier`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_bezier(OcctHandle handle, const OcctPoint3d* poles, int count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `poles` | `const OcctPoint3d*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_make_bspline_interpolated`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_bspline_interpolated(OcctHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `points` | `const OcctPoint3d*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `periodic` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_rectangle_wire`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_rectangle_wire(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `width` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_face_from_wire`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_face_from_wire(OcctHandle handle, OcctObjectId wireId, int onlyPlane);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `wireId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `onlyPlane` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_make_plane_face`

- **分类:** Basic points, 2D/3D curves and planar elements
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_plane_face(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `width` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

#### Primitive solids

##### `occt_make_box`

- **分类:** Primitive solids
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_box(OcctHandle handle, double x, double y, double z, double dx, double dy, double dz);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `y` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `z` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dx` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dy` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dz` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_cylinder`

- **分类:** Primitive solids
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_cylinder(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axis` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_sphere`

- **分类:** Primitive solids
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_sphere(OcctHandle handle, OcctPoint3d center, double radius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_cone`

- **分类:** Primitive solids
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_cone(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axis` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius1` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `radius2` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_torus`

- **分类:** Primitive solids
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_torus(OcctHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axis` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `majorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `minorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_make_wedge`

- **分类:** Primitive solids
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_wedge(OcctHandle handle, double dx, double dy, double dz, double ltx);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `dx` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dy` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dz` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `ltx` | `double` | 值 | 按声明传递的 C ABI 参数。 |

#### Topology assembly

##### `occt_make_compound`

- **分类:** Topology assembly
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_compound(OcctHandle handle, const OcctObjectId* shapeIds, int count, int hideInputs);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `hideInputs` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_make_wire`

- **分类:** Topology assembly
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_wire(OcctHandle handle, const OcctObjectId* edgeIds, int count, int hideInputs);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `hideInputs` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_sew_shapes`

- **分类:** Topology assembly
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_sew_shapes(OcctHandle handle, const OcctObjectId* shapeIds, int count, double tolerance, int hideInputs);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInputs` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_make_solid_from_shell`

- **分类:** Topology assembly
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_solid_from_shell(OcctHandle handle, OcctObjectId shellId, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shellId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

#### Boolean and feature operations

##### `occt_boolean`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_boolean(OcctHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, int hideInputs);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `operation` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `leftId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `rightId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `hideInputs` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_extrude`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_extrude(OcctHandle handle, OcctObjectId profileId, OcctVector3d vector, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `profileId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `vector` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_revolve`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_revolve(OcctHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `profileId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `axisPoint` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axisDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `angleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_sweep`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_sweep(OcctHandle handle, OcctObjectId spineWireId, OcctObjectId profileId, int hideInputs);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `spineWireId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `profileId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `hideInputs` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_loft`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_loft(OcctHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance, int hideInputs);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `wireIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `makeSolid` | `int` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `ruled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInputs` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_fillet_all_edges`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_fillet_all_edges(OcctHandle handle, OcctObjectId shapeId, double radius, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_chamfer_all_edges`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_chamfer_all_edges(OcctHandle handle, OcctObjectId shapeId, double distance, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `distance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_fillet_edges`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_fillet_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `edgeIndices` | `const int*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_chamfer_edges`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_chamfer_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `edgeIndices` | `const int*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `distance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_offset_shape`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_offset_shape(OcctHandle handle, OcctObjectId shapeId, double offset, double tolerance, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `offset` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_thick_solid`

- **分类:** Boolean and feature operations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_thick_solid(OcctHandle handle, OcctObjectId solidId, int faceIndexToRemove, double thickness, double tolerance, int hideInput);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `solidId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `faceIndexToRemove` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `thickness` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `hideInput` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

#### Text and dimensional annotations

##### `occt_make_text_shape`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_text_shape(OcctHandle handle, const char* utf8Text, OcctPoint3d position, OcctVector3d normal, OcctVector3d xDirection, double height, double extrusionDepth, const char* utf8FontName, int bold, int italic);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Text` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |
| `position` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `extrusionDepth` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `utf8FontName` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |
| `bold` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `italic` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_make_length_annotation_shape`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_length_annotation_shape(OcctHandle handle, OcctObjectId edgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `textHeight` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `arrowSize` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `utf8FontName` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_make_angle_annotation_shape`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_angle_annotation_shape(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double radius, double textHeight, double arrowSize, const char* utf8FontName);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `firstEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `secondEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `textHeight` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `arrowSize` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `utf8FontName` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_make_radius_annotation_shape`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_radius_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `circularEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `textHeight` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `arrowSize` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `utf8FontName` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_make_diameter_annotation_shape`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_diameter_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `circularEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `textHeight` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `arrowSize` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `utf8FontName` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_add_text`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_text(OcctHandle handle, const char* utf8Text, OcctPoint3d position, double height, double r, double g, double b, int zoomable);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Text` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |
| `position` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `zoomable` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_text`

- **分类:** Text and dimensional annotations
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text(OcctHandle handle, OcctObjectId textId, const char* utf8Text);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `textId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Text` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_set_text_position`

- **分类:** Text and dimensional annotations
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_position(OcctHandle handle, OcctObjectId textId, OcctPoint3d position);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `textId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `position` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_text_height`

- **分类:** Text and dimensional annotations
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_height(OcctHandle handle, OcctObjectId textId, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `textId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_text_font`

- **分类:** Text and dimensional annotations
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_font(OcctHandle handle, OcctObjectId textId, const char* utf8FontName);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `textId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8FontName` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_set_text_angle`

- **分类:** Text and dimensional annotations
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_angle(OcctHandle handle, OcctObjectId textId, double angleDegrees);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `textId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `angleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_text_zoomable`

- **分类:** Text and dimensional annotations
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_zoomable(OcctHandle handle, OcctObjectId textId, int zoomable);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `textId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `zoomable` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_set_dimension_flyout`

- **分类:** Text and dimensional annotations
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_dimension_flyout(OcctHandle handle, OcctObjectId dimensionId, double flyout);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `dimensionId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_add_length_dimension`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_length_dimension(OcctHandle handle, OcctObjectId edgeId, double flyout, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_add_angle_dimension`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_angle_dimension(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double flyout, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `firstEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `secondEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_add_radius_dimension`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_radius_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `circularShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_add_diameter_dimension`

- **分类:** Text and dimensional annotations
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_diameter_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `circularShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `flyout` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

#### BREP / STEP / IGES / STL IO

##### `occt_import_file`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_file(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_import_step`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_step(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_import_iges`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_iges(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_import_brep`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_brep(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_import_stl`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_stl(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_export_step`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_export_step(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_export_all_step`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_export_all_step(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_export_iges`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_export_iges(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_export_all_iges`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_export_all_iges(OcctHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_export_brep`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_export_brep(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_export_stl`

- **分类:** BREP / STEP / IGES / STL IO
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_export_stl(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |
| `linearDeflection` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `angularDeflection` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `asciiMode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

### `OcctRenderSurface.h`

#### Resize the native OCCT render surface without presenting a frame

##### `occt_resize_surface`

- **分类:** Resize the native OCCT render surface without presenting a frame
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_resize_surface(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

### `OcctStepDocument.h`

#### The pointer remains valid until the engine scratch buffer is reused

##### `occt_get_last_step_document_json`

- **分类:** The pointer remains valid until the engine scratch buffer is reused
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_last_step_document_json(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

### `OcctPoints.h`

#### Appended object-kind value. Existing OcctObjectKind numeric values remain stable

##### `occt_add_point`

- **分类:** Appended object-kind value. Existing OcctObjectKind numeric values remain stable
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_point( OcctHandle handle, OcctPoint3d position, int marker, double scale, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `position` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `marker` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `scale` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_point_position`

- **分类:** Appended object-kind value. Existing OcctObjectKind numeric values remain stable
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_point_position( OcctHandle handle, OcctObjectId pointId, OcctPoint3d position);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `pointId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `position` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_set_point_style`

- **分类:** Appended object-kind value. Existing OcctObjectKind numeric values remain stable
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_set_point_style( OcctHandle handle, OcctObjectId pointId, int marker, double scale, double r, double g, double b);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `pointId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `marker` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `scale` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `r` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `g` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `b` | `double` | 值 | 按声明传递的 C ABI 参数。 |

### `OcctSelectionOverlay.h`

#### Coordinates use the host window client coordinate system (origin at left/top)

##### `occt_show_selection_rectangle`

- **分类:** Coordinates use the host window client coordinate system (origin at left/top)
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_show_selection_rectangle( OcctHandle handle, int x1, int y1, int x2, int y2, double lineR, double lineG, double lineB, double fillR, double fillG, double fillB, double fillTransparency, double lineWidth);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x1` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y1` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `x2` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `y2` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `lineR` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `lineG` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `lineB` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `fillR` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `fillG` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `fillB` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `fillTransparency` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `lineWidth` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_hide_selection_rectangle`

- **分类:** Coordinates use the host window client coordinate system (origin at left/top)
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_selection_rectangle(OcctHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

### `OcctSelectionState.h`

#### whole-object selection uses OcctShape_Shape and index -1

##### `occt_selected_hits`

- **分类:** whole-object selection uses OcctShape_Shape and index -1
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_selected_hits( OcctHandle handle, OcctSelectionHit* items, int capacity, int* count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `items` | `OcctSelectionHit*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `count` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

#### a registered object is currently detected through hasHit

##### `occt_detected_hit`

- **分类:** a registered object is currently detected through hasHit
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_detected_hit( OcctHandle handle, OcctSelectionHit* result, int* hasHit);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `result` | `OcctSelectionHit*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `hasHit` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

### `OcctModeling.h`

#### OcctModeling

##### `occt_model_create`

- **分类:** OcctModeling
- **返回值:** `OcctModelHandle`

```cpp
OCCTBRIDGE_API OcctModelHandle occt_model_create();
```

**参数:** 无

##### `occt_model_destroy`

- **分类:** OcctModeling
- **返回值:** `void`

```cpp
OCCTBRIDGE_API void occt_model_destroy(OcctModelHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_model_last_error`

- **分类:** OcctModeling
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_last_error(OcctModelHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_model_capabilities`

- **分类:** OcctModeling
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_capabilities();
```

**参数:** 无

##### `occt_model_shape_ids_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_ids_copy(OcctModelHandle handle, OcctObjectId* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `results` | `OcctObjectId*` | 输出 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_shape_exists`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_exists(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_delete_shape`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_delete_shape(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_clear`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_clear(OcctModelHandle handle);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |

##### `occt_model_operation_report`

- **分类:** OcctModeling
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_operation_report(OcctModelHandle handle, OcctOperationId operationId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `operationId` | `OcctOperationId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_copy_shape`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_copy_shape(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_hash`

- **分类:** OcctModeling
- **返回值:** `std::int64_t`

```cpp
OCCTBRIDGE_API std::int64_t occt_model_shape_hash(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_type`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_type(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_orientation`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_orientation(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_is_closed`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_closed(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_is_valid`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_valid(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_tolerance`

- **分类:** OcctModeling
- **返回值:** `double`

```cpp
OCCTBRIDGE_API double occt_model_shape_tolerance(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_bounds`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_bounds(OcctModelHandle handle, OcctObjectId shapeId, OcctBounds* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctBounds*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_shape_linear_properties`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_linear_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctMassProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_shape_surface_properties`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_surface_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctMassProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_shape_volume_properties`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_volume_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctMassProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_shape_distance`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_distance(OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `firstId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `secondId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctDistanceResult*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_check_report`

- **分类:** OcctModeling
- **返回值:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_check_report(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_get_location`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_get_location(OcctModelHandle handle, OcctObjectId shapeId, OcctModelLocation* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelLocation*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_set_location`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_set_location(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelLocation* location, int copyShape);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `location` | `const OcctModelLocation*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `copyShape` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_subshapes_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_subshapes_copy(OcctModelHandle handle, OcctObjectId shapeId, int shapeType, OcctObjectId* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `shapeType` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `results` | `OcctObjectId*` | 输出 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_outer_wire`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_outer_wire(OcctModelHandle handle, OcctObjectId faceId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_inner_wires_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_inner_wires_copy(OcctModelHandle handle, OcctObjectId faceId, OcctObjectId* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `results` | `OcctObjectId*` | 输出 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_ancestors_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ancestors_copy(OcctModelHandle handle, OcctObjectId rootId, OcctObjectId childId, int ancestorType, OcctObjectId* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `rootId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `childId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `ancestorType` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `results` | `OcctObjectId*` | 输出 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_vertex_point`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_vertex_point(OcctModelHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `vertexId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_endpoints`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_endpoints(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `start` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `end` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_point_at`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_point_at(OcctModelHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `normalizedParameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `point` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `tangent` | `OcctVector3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_curve_type`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_curve_type(OcctModelHandle handle, OcctObjectId edgeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_face_surface_type`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_surface_type(OcctModelHandle handle, OcctObjectId faceId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_face_uv_bounds`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_uv_bounds(OcctModelHandle handle, OcctObjectId faceId, OcctUvBounds* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctUvBounds*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_point_normal`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_point_normal(OcctModelHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `u` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `v` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `point` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `normal` | `OcctVector3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_line_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_line_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelLineGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_circle_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_circle_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelCircleGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_ellipse_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_ellipse_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelEllipseGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_plane_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_plane_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelPlaneGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_cylinder_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_cylinder_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelCylinderGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_cone_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_cone_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelConeGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_sphere_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_sphere_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelSphereGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_torus_geometry`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelTorusGeometry*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_parameter_range`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_parameter_range(OcctModelHandle handle, OcctObjectId edgeId, OcctModelParameterRange* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelParameterRange*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_differential`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_differential(OcctModelHandle handle, OcctObjectId edgeId, double parameter, OcctModelCurveDifferential* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `parameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelCurveDifferential*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_curvature`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_curvature(OcctModelHandle handle, OcctObjectId edgeId, double parameter, double resolution, OcctModelCurveCurvature* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `parameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `resolution` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelCurveCurvature*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_periodicity`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_periodicity(OcctModelHandle handle, OcctObjectId faceId, OcctModelSurfacePeriodicity* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelSurfacePeriodicity*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_differential`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_differential(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceDifferential* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `u` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `v` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `resolution` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelSurfaceDifferential*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_curvature`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_curvature(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceCurvature* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `u` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `v` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `resolution` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelSurfaceCurvature*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_make_vertex`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_vertex(OcctModelHandle handle, OcctPoint3d point);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `point` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_line`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_line(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d end);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `start` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `end` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_polyline`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_polyline(OcctModelHandle handle, const OcctPoint3d* points, int count, int closed);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `points` | `const OcctPoint3d*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `closed` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_make_circle`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_circle(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_arc_three_points`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_arc_three_points(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `start` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `middle` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `end` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_arc_center`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_arc_center(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `startAngleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `endAngleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_regular_polygon`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_regular_polygon(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `sideCount` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `makeFace` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_make_ellipse`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_ellipse(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `majorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `minorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_bezier`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_bezier(OcctModelHandle handle, const OcctPoint3d* poles, int count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `poles` | `const OcctPoint3d*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_make_bspline_interpolated`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_bspline_interpolated(OcctModelHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `points` | `const OcctPoint3d*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `periodic` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_rectangle_wire`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_rectangle_wire(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `width` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_plane_face`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_plane_face(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `xDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `normal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `width` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_face_from_wire`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_face_from_wire(OcctModelHandle handle, OcctObjectId wireId, int onlyPlane);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `wireId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `onlyPlane` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_make_box`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_box(OcctModelHandle handle, double x, double y, double z, double dx, double dy, double dz);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `x` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `y` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `z` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dx` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dy` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dz` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_cylinder`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_cylinder(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axis` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_cone`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_cone(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axis` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius1` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `radius2` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `height` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_sphere`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_sphere(OcctModelHandle handle, OcctPoint3d center, double radius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_torus`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_torus(OcctModelHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axis` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `majorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `minorRadius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_wedge`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_wedge(OcctModelHandle handle, double dx, double dy, double dz, double ltx);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `dx` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dy` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `dz` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `ltx` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_compound`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_compound(OcctModelHandle handle, const OcctObjectId* shapeIds, int count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_make_wire`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_wire(OcctModelHandle handle, const OcctObjectId* edgeIds, int count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_sew`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_sew(OcctModelHandle handle, const OcctObjectId* shapeIds, int count, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_make_solid_from_shell`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_solid_from_shell(OcctModelHandle handle, OcctObjectId shellId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shellId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_translate`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_translate(OcctModelHandle handle, OcctObjectId shapeId, OcctVector3d vector);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `vector` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_rotate`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_rotate(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `axisPoint` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axisDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `angleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_scale`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_scale(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `center` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `factor` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_mirror_plane`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_mirror_plane(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `planePoint` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `planeNormal` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_boolean`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_boolean(OcctModelHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, const OcctModelBooleanOptions* options);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `operation` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `leftId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `rightId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `options` | `const OcctModelBooleanOptions*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_split`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_split(OcctModelHandle handle, const OcctObjectId* objectIds, int objectCount, const OcctObjectId* toolIds, int toolCount, const OcctModelBooleanOptions* options);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `objectIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `objectCount` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `toolIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `toolCount` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `options` | `const OcctModelBooleanOptions*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_extrude`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_extrude(OcctModelHandle handle, OcctObjectId profileId, OcctVector3d vector);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `profileId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `vector` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_revolve`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_revolve(OcctModelHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `profileId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `axisPoint` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `axisDirection` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `angleDegrees` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_sweep`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_sweep(OcctModelHandle handle, OcctObjectId spineWireId, OcctObjectId profileId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `spineWireId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `profileId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_loft`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_loft(OcctModelHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `wireIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `makeSolid` | `int` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `ruled` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_fillet_edges`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fillet_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `edgeIndices` | `const int*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `radius` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_chamfer_edges`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_chamfer_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `edgeIndices` | `const int*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `distance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_offset`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_offset(OcctModelHandle handle, OcctObjectId shapeId, double offset, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `offset` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_thick_solid`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_thick_solid(OcctModelHandle handle, OcctObjectId solidId, const int* faceIndicesToRemove, int count, double thickness, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `solidId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `faceIndicesToRemove` | `const int*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `count` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `thickness` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_unify_same_domain`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_unify_same_domain(OcctModelHandle handle, OcctObjectId shapeId, int unifyEdges, int unifyFaces, int concatBsplines);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `unifyEdges` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `unifyFaces` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `concatBsplines` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_fix_shape`

- **分类:** OcctModeling
- **返回值:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fix_shape(OcctModelHandle handle, OcctObjectId shapeId, double precision, double minTolerance, double maxTolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `precision` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `minTolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `maxTolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_project_point_on_edge`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_project_point_on_edge(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d point, OcctModelProjectionResult* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `point` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelProjectionResult*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_project_point_on_face`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_project_point_on_face(OcctModelHandle handle, OcctObjectId faceId, OcctPoint3d point, OcctModelProjectionResult* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `point` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelProjectionResult*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_ray_intersections`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ray_intersections(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `origin` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `direction` | `OcctVector3d` | 值 | 按声明传递的 C ABI 参数。 |
| `minimumParameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `maximumParameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_ray_hits_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ray_hits_copy(OcctModelHandle handle, OcctModelRayHit* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `results` | `OcctModelRayHit*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_classify_point`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_classify_point(OcctModelHandle handle, OcctObjectId solidId, OcctPoint3d point, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `solidId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `point` | `OcctPoint3d` | 值 | 按声明传递的 C ABI 参数。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_mesh`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_mesh(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelMeshParameters* parameters);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `parameters` | `const OcctModelMeshParameters*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_clear_mesh`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_clear_mesh(OcctModelHandle handle, OcctObjectId shapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_face_mesh_nodes_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_mesh_nodes_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshNode* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `results` | `OcctModelMeshNode*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_face_mesh_triangles_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_mesh_triangles_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshTriangle* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `results` | `OcctModelMeshTriangle*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_import_file`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_file(OcctModelHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_import_step`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_step(OcctModelHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_import_iges`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_iges(OcctModelHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_import_brep`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_brep(OcctModelHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_import_stl`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_stl(OcctModelHandle handle, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_export_step`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_step(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_export_iges`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_iges(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_export_brep`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_brep(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |

##### `occt_model_export_stl`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_stl(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `utf8Path` | `const char*` | 输入 | UTF-8 字符串指针。`const char*` 通常为输入字符串。 |
| `linearDeflection` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `angularDeflection` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `asciiMode` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_history_generated_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_generated_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `operationId` | `OcctOperationId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `sourceShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `results` | `OcctObjectId*` | 输出 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_history_modified_copy`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_modified_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `operationId` | `OcctOperationId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `sourceShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `results` | `OcctObjectId*` | 输出 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_history_is_removed`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_is_removed(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `operationId` | `OcctOperationId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `sourceShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_display_in_engine`

- **分类:** OcctModeling
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_display_in_engine(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId shapeId, int fit);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `engineHandle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `modelHandle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `fit` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_update_object_shape_from_model`

- **分类:** OcctModeling
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_update_object_shape_from_model(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId viewerObjectId, OcctObjectId modelShapeId, unsigned int options);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `engineHandle` | `OcctHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `modelHandle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `viewerObjectId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `modelShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `options` | `unsigned int` | 值 | 按声明传递的 C ABI 参数。 |

### `OcctModelingExtensions.h`

#### OcctModelingExtensions

##### `occt_model_shape_is_same`

- **分类:** OcctModelingExtensions
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_same( OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `firstId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `secondId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_is_partner`

- **分类:** OcctModelingExtensions
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_partner( OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `firstId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `secondId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |

##### `occt_model_shape_oriented_bounds`

- **分类:** OcctModelingExtensions
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_oriented_bounds( OcctModelHandle handle, OcctObjectId shapeId, int optimal, OcctOrientedBounds* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `optimal` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `result` | `OcctOrientedBounds*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_make_face_with_holes`

- **分类:** OcctModelingExtensions
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_face_with_holes( OcctModelHandle handle, OcctObjectId outerWireId, const OcctObjectId* innerWireIds, int innerWireCount);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `outerWireId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `innerWireIds` | `const OcctObjectId*` | 输入 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `innerWireCount` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_trim_edge`

- **分类:** OcctModelingExtensions
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_trim_edge( OcctModelHandle handle, OcctObjectId edgeId, double firstParameter, double lastParameter);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `firstParameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `lastParameter` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_offset_wire`

- **分类:** OcctModelingExtensions
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_offset_wire( OcctModelHandle handle, OcctObjectId wireId, double offset, double altitude, int joinType, int openResult);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `wireId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `offset` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `altitude` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `joinType` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `openResult` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

### `OcctModelingBSpline.h`

#### OcctModelingBSpline

##### `occt_model_edge_bspline_info`

- **分类:** OcctModelingBSpline
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_info( OcctModelHandle handle, OcctObjectId edgeId, OcctModelBSplineCurveInfo* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelBSplineCurveInfo*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_bspline_pole_at`

- **分类:** OcctModelingBSpline
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_pole_at( OcctModelHandle handle, OcctObjectId edgeId, int index, OcctPoint3d* pole, double* weight);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `index` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `pole` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `weight` | `double*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_edge_bspline_knot_at`

- **分类:** OcctModelingBSpline
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_knot_at( OcctModelHandle handle, OcctObjectId edgeId, int index, double* knot, int* multiplicity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `edgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `index` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `knot` | `double*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `multiplicity` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_bspline_info`

- **分类:** OcctModelingBSpline
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_info( OcctModelHandle handle, OcctObjectId faceId, OcctModelBSplineSurfaceInfo* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelBSplineSurfaceInfo*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_bspline_pole_at`

- **分类:** OcctModelingBSpline
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_pole_at( OcctModelHandle handle, OcctObjectId faceId, int uIndex, int vIndex, OcctPoint3d* pole, double* weight);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `uIndex` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `vIndex` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `pole` | `OcctPoint3d*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `weight` | `double*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_bspline_u_knot_at`

- **分类:** OcctModelingBSpline
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_u_knot_at( OcctModelHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `index` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `knot` | `double*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `multiplicity` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_face_bspline_v_knot_at`

- **分类:** OcctModelingBSpline
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_v_knot_at( OcctModelHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `faceId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `index` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `knot` | `double*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `multiplicity` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

### `OcctModelingTopologyAnalysis.h`

#### OcctModelingTopologyAnalysis

##### `occt_model_shape_free_bounds`

- **分类:** OcctModelingTopologyAnalysis
- **返回值:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_shape_free_bounds( OcctModelHandle handle, OcctObjectId shapeId, double tolerance, int boundaryKind, int splitClosed, int splitOpen);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `boundaryKind` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `splitClosed` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `splitOpen` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

##### `occt_model_shape_edge_adjacency`

- **分类:** OcctModelingTopologyAnalysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_edge_adjacency( OcctModelHandle handle, OcctObjectId shapeId, OcctModelEdgeAdjacency* items, int capacity, int* count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `items` | `OcctModelEdgeAdjacency*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `count` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

### `OcctModelingFaceAnalysis.h`

#### OcctModelingFaceAnalysis

##### `occt_model_shape_face_analysis`

- **分类:** OcctModelingFaceAnalysis
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_face_analysis( OcctModelHandle handle, OcctObjectId shapeId, OcctModelFaceAnalysis* items, int capacity, int* count);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `items` | `OcctModelFaceAnalysis*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |
| `count` | `int*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

### `OcctModelingInertia.h`

#### OcctModelingInertia

##### `occt_model_shape_linear_inertia`

- **分类:** OcctModelingInertia
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_linear_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelInertiaProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_shape_surface_inertia`

- **分类:** OcctModelingInertia
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_surface_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelInertiaProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_shape_volume_inertia`

- **分类:** OcctModelingInertia
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_volume_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `shapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelInertiaProperties*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

### `OcctModelingIntersection.h`

#### OcctModelingIntersection

##### `occt_model_intersect_edges`

- **分类:** OcctModelingIntersection
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_intersect_edges( OcctModelHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double tolerance);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `firstEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `secondEdgeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `tolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |

##### `occt_model_edge_intersections_copy`

- **分类:** OcctModelingIntersection
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_intersections_copy( OcctModelHandle handle, OcctModelEdgeIntersection* results, int capacity);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `results` | `OcctModelEdgeIntersection*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `capacity` | `int` | 值 | 数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。 |

### `OcctModelingTopologyReference.h`

#### OcctModelingTopologyReference

##### `occt_model_create_topology_reference`

- **分类:** OcctModelingTopologyReference
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_create_topology_reference( OcctModelHandle handle, OcctObjectId rootShapeId, OcctObjectId subshapeId, OcctModelTopologyReference* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `rootShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `subshapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `result` | `OcctModelTopologyReference*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_resolve_topology_reference`

- **分类:** OcctModelingTopologyReference
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_resolve_topology_reference( OcctModelHandle handle, OcctObjectId rootShapeId, const OcctModelTopologyReference* reference, double matchingTolerance, OcctModelTopologyReferenceResult* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `rootShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `reference` | `const OcctModelTopologyReference*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `matchingTolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelTopologyReferenceResult*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

##### `occt_model_resolve_topology_reference_with_history`

- **分类:** OcctModelingTopologyReference
- **返回值:** `int`

```cpp
OCCTBRIDGE_API int occt_model_resolve_topology_reference_with_history( OcctModelHandle handle, OcctObjectId rootShapeId, OcctOperationId operationId, OcctObjectId sourceShapeId, const OcctModelTopologyReference* reference, double matchingTolerance, OcctModelTopologyReferenceResult* result);
```

| 名称 | C 类型 | 方向 | 含义 |
|---|---|---|---|
| `handle` | `OcctModelHandle` | 值 | Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。 |
| `rootShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `operationId` | `OcctOperationId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `sourceShapeId` | `OcctObjectId` | 值 | Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。 |
| `reference` | `const OcctModelTopologyReference*` | 输入 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |
| `matchingTolerance` | `double` | 值 | 按声明传递的 C ABI 参数。 |
| `result` | `OcctModelTopologyReferenceResult*` | 输出 | 连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。 |

