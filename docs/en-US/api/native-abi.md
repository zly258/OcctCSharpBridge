# OcctNative Complete C ABI Reference

Author: **zly258**. Bridge **2.6.0** · Native ABI **4** · OCCT **7.9.0** · .NET SDK **10.0.302** · C# **14.0** · C++17 · Avalonia **12.1.0** · `net10.0-windows` · Windows x64.

This page is generated from every public Native header declared by `bridge-contract.json` and covers the public ABI types plus every `OCCTBRIDGE_API occt_*` export for P/Invoke parity, low-level integration, and ABI diagnostics.

- **Bridge:** `2.6.0`
- **Native ABI:** `4`
- **Exports:** `344`
- **Public headers:** `11`

## ABI Types

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

## Exported Functions

### `OcctNative.h`

#### OcctNative

##### `occt_create`

- **Category:** OcctNative
- **Returns:** `OcctHandle`

```cpp
OCCTBRIDGE_API OcctHandle occt_create();
```

**Parameters:** None

##### `occt_destroy`

- **Category:** OcctNative
- **Returns:** `void`

```cpp
OCCTBRIDGE_API void occt_destroy(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_last_error`

- **Category:** OcctNative
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_last_error(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_version`

- **Category:** OcctNative
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_version();
```

**Parameters:** None

##### `occt_bridge_abi_version`

- **Category:** OcctNative
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_bridge_abi_version();
```

**Parameters:** None

##### `occt_bridge_version`

- **Category:** OcctNative
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_bridge_version();
```

**Parameters:** None

##### `occt_bridge_build_info`

- **Category:** OcctNative
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_bridge_build_info();
```

**Parameters:** None

#### Viewer and interaction

##### `occt_initialize`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_initialize(OcctHandle handle, void* windowHandle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `windowHandle` | `void*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_resize`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_resize(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_redraw`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_redraw(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_begin_update`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_begin_update(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_end_update`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_end_update(OcctHandle handle, int fitAll);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `fitAll` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_is_updating`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_is_updating(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_fit_all`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_all(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_fit_object`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_object(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_window_fit`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_window_fit(OcctHandle handle, int x1, int y1, int x2, int y2);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x1` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y1` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `x2` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y2` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_view`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view(OcctHandle handle, int orientation);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `orientation` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_projection`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_projection(OcctHandle handle, int projectionType);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `projectionType` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_perspective_fov`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_perspective_fov(OcctHandle handle, double degrees);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `degrees` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_background`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_background(OcctHandle handle, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_display_mode`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_display_mode(OcctHandle handle, int displayMode);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `displayMode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_triedron_visible`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_triedron_visible(OcctHandle handle, int visible);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `visible` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_view_cube_visible`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_cube_visible(OcctHandle handle, int visible);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `visible` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_computed_mode`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_computed_mode(OcctHandle handle, int enabled);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `enabled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_dump_view`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_dump_view(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_screen_to_world`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_screen_to_world(OcctHandle handle, int x, int y, OcctPoint3d* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `result` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_world_to_screen`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_world_to_screen(OcctHandle handle, OcctPoint3d point, int* x, int* y);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `point` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `x` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `y` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_move_to`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_move_to(OcctHandle handle, int x, int y);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_select`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select(OcctHandle handle, int x, int y, int appendSelection);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `appendSelection` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_select_rectangle_ex`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_rectangle_ex(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection, int allowOverlap);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x1` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y1` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `x2` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y2` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `appendSelection` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `allowOverlap` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_select_object`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_object(OcctHandle handle, OcctObjectId objectId, int appendSelection);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `appendSelection` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_selection_mode`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_mode(OcctHandle handle, int selectionMode);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `selectionMode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_clear_selection`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_clear_selection(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_start_rotation`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_start_rotation(OcctHandle handle, int x, int y);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_rotation`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_rotation(OcctHandle handle, int x, int y);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_pan`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_pan(OcctHandle handle, int deltaX, int deltaY);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `deltaX` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `deltaY` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_zoom`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_zoom(OcctHandle handle, double factor);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `factor` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_get_camera`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_camera(OcctHandle handle, OcctCameraState* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `result` | `OcctCameraState*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_set_camera`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_camera(OcctHandle handle, const OcctCameraState* state);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `state` | `const OcctCameraState*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_get_view_scale`

- **Category:** Viewer and interaction
- **Returns:** `double`

```cpp
OCCTBRIDGE_API double occt_get_view_scale(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_set_view_scale`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_scale(OcctHandle handle, double scale);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `scale` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_antialiasing`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_antialiasing(OcctHandle handle, int enabled);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `enabled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_gradient_background`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_gradient_background(OcctHandle handle, double r1, double g1, double b1, double r2, double g2, double b2, int fillMethod);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `r1` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g1` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b1` | `double` | Value | C ABI parameter passed according to the declaration. |
| `r2` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g2` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b2` | `double` | Value | C ABI parameter passed according to the declaration. |
| `fillMethod` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_display_precision`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_display_precision(OcctHandle handle, double deviationCoefficient, double deviationAngleDegrees, int applyExisting);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `deviationCoefficient` | `double` | Value | C ABI parameter passed according to the declaration. |
| `deviationAngleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |
| `applyExisting` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_default_material`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_default_material(OcctHandle handle, int material, int applyExisting);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `material` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `applyExisting` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_scene_lighting_ex`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_scene_lighting_ex(OcctHandle handle, const OcctSceneLightingSettings* settings);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `settings` | `const OcctSceneLightingSettings*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_set_selection_highlight_color`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_highlight_color(OcctHandle handle, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_hover_highlight_color`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_hover_highlight_color(OcctHandle handle, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_reset_scene_lighting`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_scene_lighting(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_set_selection_tolerance`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_tolerance(OcctHandle handle, int pixelTolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `pixelTolerance` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_auto_z_fit_mode`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_auto_z_fit_mode(OcctHandle handle, int enabled, double scaleFactor);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `enabled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `scaleFactor` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_get_auto_z_fit_mode`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_auto_z_fit_mode(OcctHandle handle, OcctAutoZFitSettings* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `result` | `OcctAutoZFitSettings*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_auto_z_fit`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_auto_z_fit(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_set_default_polygon_offsets`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_default_polygon_offsets(OcctHandle handle, int mode, double factor, double units, int applyExisting);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `mode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `factor` | `double` | Value | C ABI parameter passed according to the declaration. |
| `units` | `double` | Value | C ABI parameter passed according to the declaration. |
| `applyExisting` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_get_default_polygon_offsets`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_default_polygon_offsets(OcctHandle handle, OcctPolygonOffsetSettings* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `result` | `OcctPolygonOffsetSettings*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_set_object_polygon_offsets`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, int mode, double factor, double units);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `mode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `factor` | `double` | Value | C ABI parameter passed according to the declaration. |
| `units` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_get_object_polygon_offsets`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, OcctPolygonOffsetSettings* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctPolygonOffsetSettings*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_reset_object_polygon_offsets`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_fit_objects`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, double margin);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `margin` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_zup_view`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_zup_view(OcctHandle handle, int orientation, int fitAll);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `orientation` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `fitAll` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_screen_to_ray`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_screen_to_ray(OcctHandle handle, int x, int y, OcctProjectionRay* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `result` | `OcctProjectionRay*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_zoom_at_point`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_zoom_at_point(OcctHandle handle, int x, int y, double delta);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `delta` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_select_all_visible`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_all_visible(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_invert_selection`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_invert_selection(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_hide_selected`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_selected(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_set_automatic_highlight`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_automatic_highlight(OcctHandle handle, int enabled);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `enabled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_msaa_samples`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_msaa_samples(OcctHandle handle, int samples);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `samples` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_render_resolution_scale`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_render_resolution_scale(OcctHandle handle, double scale);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `scale` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_render_resolution`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_render_resolution(OcctHandle handle, double dpi);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `dpi` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_rendering_method`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_rendering_method(OcctHandle handle, int method);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `method` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_shadows_enabled`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_shadows_enabled(OcctHandle handle, int enabled);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `enabled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_immediate_update`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_immediate_update(OcctHandle handle, int enabled);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `enabled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_frustum_culling`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_frustum_culling(OcctHandle handle, int enabled);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `enabled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_face_boundaries_visible`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_face_boundaries_visible(OcctHandle handle, int visible, int applyExisting);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `visible` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `applyExisting` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_get_viewport_state`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_viewport_state(OcctHandle handle, OcctViewportState* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `result` | `OcctViewportState*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_reset_view`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_reset_view_orientation`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view_orientation(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_reset_view_mapping`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view_mapping(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_fit_selected`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_selected(OcctHandle handle, double margin);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `margin` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_get_scene_gravity_point`

- **Category:** Viewer and interaction
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_scene_gravity_point(OcctHandle handle, OcctPoint3d* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `result` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

#### Registry, AIS attributes and lifecycle

##### `occt_object_count`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_count(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_object_descriptors`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_descriptors(OcctHandle handle, OcctObjectDescriptor* items, int capacity, int* objectCount, int* shapeCount);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `items` | `OcctObjectDescriptor*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `objectCount` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `shapeCount` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_object_exists`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_exists(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_object_kind`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_kind(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_set_object_name`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_name(OcctHandle handle, OcctObjectId objectId, const char* utf8Name);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Name` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_get_object_name`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_object_name(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_set_object_application_tag`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_application_tag(OcctHandle handle, OcctObjectId objectId, const char* utf8Tag);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Tag` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_get_object_application_tag`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_object_application_tag(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_find_object_by_application_tag`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_find_object_by_application_tag(OcctHandle handle, const char* utf8Tag);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Tag` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_set_object_selectable`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_selectable(OcctHandle handle, OcctObjectId objectId, int selectable);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `selectable` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_get_object_selectable`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_selectable(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_set_objects_selectable`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_selectable(OcctHandle handle, const OcctObjectId* objectIds, int count, int selectable);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `selectable` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_selected_objects_ex`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selected_objects_ex(OcctHandle handle, const OcctObjectId* objectIds, int count, int operation);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `operation` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_object_transform`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transform(OcctHandle handle, OcctObjectId objectId, const double* matrix3x4);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `matrix3x4` | `const double*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_get_object_transform`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_transform(OcctHandle handle, OcctObjectId objectId, double* matrix3x4, int* hasTransform);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `matrix3x4` | `double*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `hasTransform` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_reset_object_transform`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_object_transform(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_set_view_cube_language`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_cube_language(OcctHandle handle, int language);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `language` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_object_color`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_color(OcctHandle handle, OcctObjectId objectId, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_object_transparency`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transparency(OcctHandle handle, OcctObjectId objectId, double transparency);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `transparency` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_object_visible`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_visible(OcctHandle handle, OcctObjectId objectId, int visible);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `visible` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_object_display_mode`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_display_mode(OcctHandle handle, OcctObjectId objectId, int displayMode);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `displayMode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_object_line_width`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_line_width(OcctHandle handle, OcctObjectId objectId, double width);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `width` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_object_material`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_material(OcctHandle handle, OcctObjectId objectId, int material);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `material` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_objects_color`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_color(OcctHandle handle, const OcctObjectId* objectIds, int count, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_objects_transparency`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_transparency(OcctHandle handle, const OcctObjectId* objectIds, int count, double transparency);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `transparency` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_objects_visible`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_visible(OcctHandle handle, const OcctObjectId* objectIds, int count, int visible);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `visible` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_objects_display_mode`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_display_mode(OcctHandle handle, const OcctObjectId* objectIds, int count, int displayMode);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `displayMode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_objects_line_width`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_line_width(OcctHandle handle, const OcctObjectId* objectIds, int count, double width);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `width` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_objects_material`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_material(OcctHandle handle, const OcctObjectId* objectIds, int count, int material);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `material` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_redisplay_objects`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_redisplay_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_select_objects`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, int appendSelection);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `appendSelection` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_object_is_visible`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_is_visible(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_object_is_selected`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_is_selected(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_delete_objects`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_delete_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_clear`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_clear(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_show_all`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_show_all(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_hide_all`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_all(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_redisplay_object`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_redisplay_object(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_highlight_object`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_highlight_object(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_unhighlight_object`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_unhighlight_object(OcctHandle handle, OcctObjectId objectId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_copy_selected_subshape_at`

- **Category:** Registry, AIS attributes and lifecycle
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_copy_selected_subshape_at(OcctHandle handle, int index);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `index` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

#### Shape query and analysis

##### `occt_shape_type`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_type(OcctHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_shape_is_valid`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_is_valid(OcctHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_shape_bounds`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_bounds(OcctHandle handle, OcctObjectId shapeId, OcctBounds* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctBounds*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_shape_linear_properties`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_linear_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctMassProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_shape_surface_properties`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_surface_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctMassProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_shape_volume_properties`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_volume_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctMassProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_shape_distance`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_distance(OcctHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `firstId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `secondId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctDistanceResult*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_topology_count`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_topology_count(OcctHandle handle, OcctObjectId shapeId, int shapeType);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `shapeType` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_get_subshape`

- **Category:** Shape query and analysis
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_get_subshape(OcctHandle handle, OcctObjectId shapeId, int shapeType, int index);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `shapeType` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `index` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_copy_shape`

- **Category:** Shape query and analysis
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_copy_shape(OcctHandle handle, OcctObjectId shapeId, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_shape_hash`

- **Category:** Shape query and analysis
- **Returns:** `std::int64_t`

```cpp
OCCTBRIDGE_API std::int64_t occt_shape_hash(OcctHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_vertex_point`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_vertex_point(OcctHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `vertexId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_edge_endpoints`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_endpoints(OcctHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `start` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `end` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_edge_point_at`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_point_at(OcctHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `normalizedParameter` | `double` | Value | C ABI parameter passed according to the declaration. |
| `point` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `tangent` | `OcctVector3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_edge_curve_type`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_curve_type(OcctHandle handle, OcctObjectId edgeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_face_surface_type`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_face_surface_type(OcctHandle handle, OcctObjectId faceId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_face_uv_bounds`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_face_uv_bounds(OcctHandle handle, OcctObjectId faceId, OcctUvBounds* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctUvBounds*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_face_point_normal`

- **Category:** Shape query and analysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_face_point_normal(OcctHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `u` | `double` | Value | C ABI parameter passed according to the declaration. |
| `v` | `double` | Value | C ABI parameter passed according to the declaration. |
| `point` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `normal` | `OcctVector3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

#### Shape transformations

##### `occt_translate`

- **Category:** Shape transformations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_translate(OcctHandle handle, OcctObjectId shapeId, OcctVector3d vector, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `vector` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_rotate`

- **Category:** Shape transformations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_rotate(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `axisPoint` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axisDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `angleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_scale`

- **Category:** Shape transformations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_scale(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `factor` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_mirror_plane`

- **Category:** Shape transformations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_mirror_plane(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `planePoint` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `planeNormal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

#### Basic points, 2D/3D curves and planar elements

##### `occt_make_vertex`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_vertex(OcctHandle handle, OcctPoint3d point);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `point` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_line`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_line(OcctHandle handle, OcctPoint3d start, OcctPoint3d end);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `start` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `end` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_polyline`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_polyline(OcctHandle handle, const OcctPoint3d* points, int count, int closed);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `points` | `const OcctPoint3d*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `closed` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_make_circle`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_circle(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_arc_three_points`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_arc_three_points(OcctHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `start` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `middle` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `end` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_arc_center`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_arc_center(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `startAngleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |
| `endAngleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_regular_polygon`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_regular_polygon(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `sideCount` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `makeFace` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_make_ellipse`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_ellipse(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `majorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `minorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_bezier`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_bezier(OcctHandle handle, const OcctPoint3d* poles, int count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `poles` | `const OcctPoint3d*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_make_bspline_interpolated`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_bspline_interpolated(OcctHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `points` | `const OcctPoint3d*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `periodic` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_rectangle_wire`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_rectangle_wire(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `width` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_face_from_wire`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_face_from_wire(OcctHandle handle, OcctObjectId wireId, int onlyPlane);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `wireId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `onlyPlane` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_make_plane_face`

- **Category:** Basic points, 2D/3D curves and planar elements
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_plane_face(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `width` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

#### Primitive solids

##### `occt_make_box`

- **Category:** Primitive solids
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_box(OcctHandle handle, double x, double y, double z, double dx, double dy, double dz);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `double` | Value | C ABI parameter passed according to the declaration. |
| `y` | `double` | Value | C ABI parameter passed according to the declaration. |
| `z` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dx` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dy` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dz` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_cylinder`

- **Category:** Primitive solids
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_cylinder(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axis` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_sphere`

- **Category:** Primitive solids
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_sphere(OcctHandle handle, OcctPoint3d center, double radius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_cone`

- **Category:** Primitive solids
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_cone(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axis` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius1` | `double` | Value | C ABI parameter passed according to the declaration. |
| `radius2` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_torus`

- **Category:** Primitive solids
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_torus(OcctHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axis` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `majorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `minorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_make_wedge`

- **Category:** Primitive solids
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_wedge(OcctHandle handle, double dx, double dy, double dz, double ltx);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `dx` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dy` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dz` | `double` | Value | C ABI parameter passed according to the declaration. |
| `ltx` | `double` | Value | C ABI parameter passed according to the declaration. |

#### Topology assembly

##### `occt_make_compound`

- **Category:** Topology assembly
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_compound(OcctHandle handle, const OcctObjectId* shapeIds, int count, int hideInputs);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `hideInputs` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_make_wire`

- **Category:** Topology assembly
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_wire(OcctHandle handle, const OcctObjectId* edgeIds, int count, int hideInputs);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `hideInputs` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_sew_shapes`

- **Category:** Topology assembly
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_sew_shapes(OcctHandle handle, const OcctObjectId* shapeIds, int count, double tolerance, int hideInputs);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInputs` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_make_solid_from_shell`

- **Category:** Topology assembly
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_solid_from_shell(OcctHandle handle, OcctObjectId shellId, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shellId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

#### Boolean and feature operations

##### `occt_boolean`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_boolean(OcctHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, int hideInputs);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `operation` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `leftId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `rightId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `hideInputs` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_extrude`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_extrude(OcctHandle handle, OcctObjectId profileId, OcctVector3d vector, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `profileId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `vector` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_revolve`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_revolve(OcctHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `profileId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `axisPoint` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axisDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `angleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_sweep`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_sweep(OcctHandle handle, OcctObjectId spineWireId, OcctObjectId profileId, int hideInputs);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `spineWireId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `profileId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `hideInputs` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_loft`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_loft(OcctHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance, int hideInputs);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `wireIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `makeSolid` | `int` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `ruled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInputs` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_fillet_all_edges`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_fillet_all_edges(OcctHandle handle, OcctObjectId shapeId, double radius, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_chamfer_all_edges`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_chamfer_all_edges(OcctHandle handle, OcctObjectId shapeId, double distance, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `distance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_fillet_edges`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_fillet_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `edgeIndices` | `const int*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_chamfer_edges`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_chamfer_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `edgeIndices` | `const int*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `distance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_offset_shape`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_offset_shape(OcctHandle handle, OcctObjectId shapeId, double offset, double tolerance, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `offset` | `double` | Value | C ABI parameter passed according to the declaration. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_thick_solid`

- **Category:** Boolean and feature operations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_thick_solid(OcctHandle handle, OcctObjectId solidId, int faceIndexToRemove, double thickness, double tolerance, int hideInput);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `solidId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `faceIndexToRemove` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `thickness` | `double` | Value | C ABI parameter passed according to the declaration. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `hideInput` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

#### Text and dimensional annotations

##### `occt_make_text_shape`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_text_shape(OcctHandle handle, const char* utf8Text, OcctPoint3d position, OcctVector3d normal, OcctVector3d xDirection, double height, double extrusionDepth, const char* utf8FontName, int bold, int italic);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Text` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |
| `position` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |
| `extrusionDepth` | `double` | Value | C ABI parameter passed according to the declaration. |
| `utf8FontName` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |
| `bold` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `italic` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_make_length_annotation_shape`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_length_annotation_shape(OcctHandle handle, OcctObjectId edgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |
| `textHeight` | `double` | Value | C ABI parameter passed according to the declaration. |
| `arrowSize` | `double` | Value | C ABI parameter passed according to the declaration. |
| `utf8FontName` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_make_angle_annotation_shape`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_angle_annotation_shape(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double radius, double textHeight, double arrowSize, const char* utf8FontName);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `firstEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `secondEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `textHeight` | `double` | Value | C ABI parameter passed according to the declaration. |
| `arrowSize` | `double` | Value | C ABI parameter passed according to the declaration. |
| `utf8FontName` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_make_radius_annotation_shape`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_radius_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `circularEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |
| `textHeight` | `double` | Value | C ABI parameter passed according to the declaration. |
| `arrowSize` | `double` | Value | C ABI parameter passed according to the declaration. |
| `utf8FontName` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_make_diameter_annotation_shape`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_diameter_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `circularEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |
| `textHeight` | `double` | Value | C ABI parameter passed according to the declaration. |
| `arrowSize` | `double` | Value | C ABI parameter passed according to the declaration. |
| `utf8FontName` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_add_text`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_text(OcctHandle handle, const char* utf8Text, OcctPoint3d position, double height, double r, double g, double b, int zoomable);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Text` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |
| `position` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |
| `zoomable` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_text`

- **Category:** Text and dimensional annotations
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text(OcctHandle handle, OcctObjectId textId, const char* utf8Text);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `textId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Text` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_set_text_position`

- **Category:** Text and dimensional annotations
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_position(OcctHandle handle, OcctObjectId textId, OcctPoint3d position);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `textId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `position` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_text_height`

- **Category:** Text and dimensional annotations
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_height(OcctHandle handle, OcctObjectId textId, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `textId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_text_font`

- **Category:** Text and dimensional annotations
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_font(OcctHandle handle, OcctObjectId textId, const char* utf8FontName);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `textId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8FontName` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_set_text_angle`

- **Category:** Text and dimensional annotations
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_angle(OcctHandle handle, OcctObjectId textId, double angleDegrees);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `textId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `angleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_set_text_zoomable`

- **Category:** Text and dimensional annotations
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_zoomable(OcctHandle handle, OcctObjectId textId, int zoomable);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `textId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `zoomable` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_set_dimension_flyout`

- **Category:** Text and dimensional annotations
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_dimension_flyout(OcctHandle handle, OcctObjectId dimensionId, double flyout);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `dimensionId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_add_length_dimension`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_length_dimension(OcctHandle handle, OcctObjectId edgeId, double flyout, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_add_angle_dimension`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_angle_dimension(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double flyout, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `firstEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `secondEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_add_radius_dimension`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_radius_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `circularShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_add_diameter_dimension`

- **Category:** Text and dimensional annotations
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_diameter_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `circularShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `flyout` | `double` | Value | C ABI parameter passed according to the declaration. |
| `r` | `double` | Value | C ABI parameter passed according to the declaration. |
| `g` | `double` | Value | C ABI parameter passed according to the declaration. |
| `b` | `double` | Value | C ABI parameter passed according to the declaration. |

#### BREP / STEP / IGES / STL IO

##### `occt_import_file`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_file(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_import_step`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_step(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_import_iges`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_iges(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_import_brep`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_brep(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_import_stl`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_stl(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_export_step`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_step(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_export_all_step`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_all_step(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_export_iges`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_iges(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_export_all_iges`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_all_iges(OcctHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_export_brep`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_brep(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_export_stl`

- **Category:** BREP / STEP / IGES / STL IO
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_stl(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |
| `linearDeflection` | `double` | Value | C ABI parameter passed according to the declaration. |
| `angularDeflection` | `double` | Value | C ABI parameter passed according to the declaration. |
| `asciiMode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

### `OcctSelectionOverlay.h`

#### Coordinates use the host window client coordinate system (origin at left/top)

##### `occt_show_selection_rectangle`

- **Category:** Coordinates use the host window client coordinate system (origin at left/top)
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_show_selection_rectangle( OcctHandle handle, int x1, int y1, int x2, int y2, double lineR, double lineG, double lineB, double fillR, double fillG, double fillB, double fillTransparency, double lineWidth);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x1` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y1` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `x2` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `y2` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `lineR` | `double` | Value | C ABI parameter passed according to the declaration. |
| `lineG` | `double` | Value | C ABI parameter passed according to the declaration. |
| `lineB` | `double` | Value | C ABI parameter passed according to the declaration. |
| `fillR` | `double` | Value | C ABI parameter passed according to the declaration. |
| `fillG` | `double` | Value | C ABI parameter passed according to the declaration. |
| `fillB` | `double` | Value | C ABI parameter passed according to the declaration. |
| `fillTransparency` | `double` | Value | C ABI parameter passed according to the declaration. |
| `lineWidth` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_hide_selection_rectangle`

- **Category:** Coordinates use the host window client coordinate system (origin at left/top)
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_selection_rectangle(OcctHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

### `OcctSelectionState.h`

#### whole-object selection uses OcctShape_Shape and index -1

##### `occt_selected_hits`

- **Category:** whole-object selection uses OcctShape_Shape and index -1
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_selected_hits( OcctHandle handle, OcctSelectionHit* items, int capacity, int* count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `items` | `OcctSelectionHit*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `count` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

#### a registered object is currently detected through hasHit

##### `occt_detected_hit`

- **Category:** a registered object is currently detected through hasHit
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_detected_hit( OcctHandle handle, OcctSelectionHit* result, int* hasHit);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `result` | `OcctSelectionHit*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `hasHit` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

### `OcctModeling.h`

#### OcctModeling

##### `occt_model_create`

- **Category:** OcctModeling
- **Returns:** `OcctModelHandle`

```cpp
OCCTBRIDGE_API OcctModelHandle occt_model_create();
```

**Parameters:** None

##### `occt_model_destroy`

- **Category:** OcctModeling
- **Returns:** `void`

```cpp
OCCTBRIDGE_API void occt_model_destroy(OcctModelHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_model_last_error`

- **Category:** OcctModeling
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_last_error(OcctModelHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_model_capabilities`

- **Category:** OcctModeling
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_capabilities();
```

**Parameters:** None

##### `occt_model_shape_ids_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_ids_copy(OcctModelHandle handle, OcctObjectId* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `results` | `OcctObjectId*` | Output | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_shape_exists`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_exists(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_delete_shape`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_delete_shape(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_clear`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_clear(OcctModelHandle handle);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |

##### `occt_model_operation_report`

- **Category:** OcctModeling
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_operation_report(OcctModelHandle handle, OcctOperationId operationId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `operationId` | `OcctOperationId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_copy_shape`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_copy_shape(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_hash`

- **Category:** OcctModeling
- **Returns:** `std::int64_t`

```cpp
OCCTBRIDGE_API std::int64_t occt_model_shape_hash(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_type`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_type(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_orientation`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_orientation(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_is_closed`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_closed(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_is_valid`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_valid(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_tolerance`

- **Category:** OcctModeling
- **Returns:** `double`

```cpp
OCCTBRIDGE_API double occt_model_shape_tolerance(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_bounds`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_bounds(OcctModelHandle handle, OcctObjectId shapeId, OcctBounds* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctBounds*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_shape_linear_properties`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_linear_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctMassProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_shape_surface_properties`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_surface_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctMassProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_shape_volume_properties`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_volume_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctMassProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_shape_distance`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_distance(OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `firstId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `secondId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctDistanceResult*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_check_report`

- **Category:** OcctModeling
- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_check_report(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_get_location`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_get_location(OcctModelHandle handle, OcctObjectId shapeId, OcctModelLocation* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelLocation*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_set_location`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_set_location(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelLocation* location, int copyShape);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `location` | `const OcctModelLocation*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `copyShape` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_subshapes_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_subshapes_copy(OcctModelHandle handle, OcctObjectId shapeId, int shapeType, OcctObjectId* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `shapeType` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `results` | `OcctObjectId*` | Output | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_outer_wire`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_outer_wire(OcctModelHandle handle, OcctObjectId faceId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_inner_wires_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_inner_wires_copy(OcctModelHandle handle, OcctObjectId faceId, OcctObjectId* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `results` | `OcctObjectId*` | Output | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_ancestors_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ancestors_copy(OcctModelHandle handle, OcctObjectId rootId, OcctObjectId childId, int ancestorType, OcctObjectId* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `rootId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `childId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `ancestorType` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `results` | `OcctObjectId*` | Output | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_vertex_point`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_vertex_point(OcctModelHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `vertexId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_endpoints`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_endpoints(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `start` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `end` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_point_at`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_point_at(OcctModelHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `normalizedParameter` | `double` | Value | C ABI parameter passed according to the declaration. |
| `point` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `tangent` | `OcctVector3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_curve_type`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_curve_type(OcctModelHandle handle, OcctObjectId edgeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_face_surface_type`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_surface_type(OcctModelHandle handle, OcctObjectId faceId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_face_uv_bounds`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_uv_bounds(OcctModelHandle handle, OcctObjectId faceId, OcctUvBounds* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctUvBounds*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_point_normal`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_point_normal(OcctModelHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `u` | `double` | Value | C ABI parameter passed according to the declaration. |
| `v` | `double` | Value | C ABI parameter passed according to the declaration. |
| `point` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `normal` | `OcctVector3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_line_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_line_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelLineGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_circle_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_circle_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelCircleGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_ellipse_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_ellipse_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelEllipseGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_plane_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_plane_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelPlaneGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_cylinder_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_cylinder_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelCylinderGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_cone_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_cone_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelConeGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_sphere_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_sphere_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelSphereGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_torus_geometry`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelTorusGeometry*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_parameter_range`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_parameter_range(OcctModelHandle handle, OcctObjectId edgeId, OcctModelParameterRange* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelParameterRange*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_differential`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_differential(OcctModelHandle handle, OcctObjectId edgeId, double parameter, OcctModelCurveDifferential* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `parameter` | `double` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelCurveDifferential*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_curvature`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_curvature(OcctModelHandle handle, OcctObjectId edgeId, double parameter, double resolution, OcctModelCurveCurvature* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `parameter` | `double` | Value | C ABI parameter passed according to the declaration. |
| `resolution` | `double` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelCurveCurvature*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_periodicity`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_periodicity(OcctModelHandle handle, OcctObjectId faceId, OcctModelSurfacePeriodicity* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelSurfacePeriodicity*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_differential`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_differential(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceDifferential* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `u` | `double` | Value | C ABI parameter passed according to the declaration. |
| `v` | `double` | Value | C ABI parameter passed according to the declaration. |
| `resolution` | `double` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelSurfaceDifferential*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_curvature`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_curvature(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceCurvature* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `u` | `double` | Value | C ABI parameter passed according to the declaration. |
| `v` | `double` | Value | C ABI parameter passed according to the declaration. |
| `resolution` | `double` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelSurfaceCurvature*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_make_vertex`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_vertex(OcctModelHandle handle, OcctPoint3d point);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `point` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_line`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_line(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d end);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `start` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `end` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_polyline`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_polyline(OcctModelHandle handle, const OcctPoint3d* points, int count, int closed);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `points` | `const OcctPoint3d*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `closed` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_make_circle`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_circle(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_arc_three_points`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_arc_three_points(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `start` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `middle` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `end` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_arc_center`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_arc_center(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `startAngleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |
| `endAngleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_regular_polygon`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_regular_polygon(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `sideCount` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `makeFace` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_make_ellipse`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_ellipse(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `majorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `minorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_bezier`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_bezier(OcctModelHandle handle, const OcctPoint3d* poles, int count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `poles` | `const OcctPoint3d*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_make_bspline_interpolated`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_bspline_interpolated(OcctModelHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `points` | `const OcctPoint3d*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `periodic` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_rectangle_wire`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_rectangle_wire(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `width` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_plane_face`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_plane_face(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `xDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `normal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `width` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_face_from_wire`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_face_from_wire(OcctModelHandle handle, OcctObjectId wireId, int onlyPlane);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `wireId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `onlyPlane` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_make_box`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_box(OcctModelHandle handle, double x, double y, double z, double dx, double dy, double dz);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `x` | `double` | Value | C ABI parameter passed according to the declaration. |
| `y` | `double` | Value | C ABI parameter passed according to the declaration. |
| `z` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dx` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dy` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dz` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_cylinder`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_cylinder(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axis` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_cone`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_cone(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axis` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `radius1` | `double` | Value | C ABI parameter passed according to the declaration. |
| `radius2` | `double` | Value | C ABI parameter passed according to the declaration. |
| `height` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_sphere`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_sphere(OcctModelHandle handle, OcctPoint3d center, double radius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_torus`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_torus(OcctModelHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axis` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `majorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |
| `minorRadius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_wedge`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_wedge(OcctModelHandle handle, double dx, double dy, double dz, double ltx);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `dx` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dy` | `double` | Value | C ABI parameter passed according to the declaration. |
| `dz` | `double` | Value | C ABI parameter passed according to the declaration. |
| `ltx` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_compound`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_compound(OcctModelHandle handle, const OcctObjectId* shapeIds, int count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_make_wire`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_wire(OcctModelHandle handle, const OcctObjectId* edgeIds, int count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_sew`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_sew(OcctModelHandle handle, const OcctObjectId* shapeIds, int count, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_make_solid_from_shell`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_solid_from_shell(OcctModelHandle handle, OcctObjectId shellId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shellId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_translate`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_translate(OcctModelHandle handle, OcctObjectId shapeId, OcctVector3d vector);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `vector` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_rotate`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_rotate(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `axisPoint` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axisDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `angleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_scale`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_scale(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `center` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `factor` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_mirror_plane`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_mirror_plane(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `planePoint` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `planeNormal` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_boolean`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_boolean(OcctModelHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, const OcctModelBooleanOptions* options);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `operation` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `leftId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `rightId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `options` | `const OcctModelBooleanOptions*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_split`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_split(OcctModelHandle handle, const OcctObjectId* objectIds, int objectCount, const OcctObjectId* toolIds, int toolCount, const OcctModelBooleanOptions* options);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `objectIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `objectCount` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `toolIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `toolCount` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `options` | `const OcctModelBooleanOptions*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_extrude`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_extrude(OcctModelHandle handle, OcctObjectId profileId, OcctVector3d vector);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `profileId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `vector` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_revolve`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_revolve(OcctModelHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `profileId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `axisPoint` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `axisDirection` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `angleDegrees` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_sweep`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_sweep(OcctModelHandle handle, OcctObjectId spineWireId, OcctObjectId profileId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `spineWireId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `profileId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_loft`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_loft(OcctModelHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `wireIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `makeSolid` | `int` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `ruled` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_fillet_edges`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fillet_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `edgeIndices` | `const int*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `radius` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_chamfer_edges`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_chamfer_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `edgeIndices` | `const int*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `distance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_offset`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_offset(OcctModelHandle handle, OcctObjectId shapeId, double offset, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `offset` | `double` | Value | C ABI parameter passed according to the declaration. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_thick_solid`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_thick_solid(OcctModelHandle handle, OcctObjectId solidId, const int* faceIndicesToRemove, int count, double thickness, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `solidId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `faceIndicesToRemove` | `const int*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `count` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `thickness` | `double` | Value | C ABI parameter passed according to the declaration. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_unify_same_domain`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_unify_same_domain(OcctModelHandle handle, OcctObjectId shapeId, int unifyEdges, int unifyFaces, int concatBsplines);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `unifyEdges` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `unifyFaces` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `concatBsplines` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_fix_shape`

- **Category:** OcctModeling
- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fix_shape(OcctModelHandle handle, OcctObjectId shapeId, double precision, double minTolerance, double maxTolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `precision` | `double` | Value | C ABI parameter passed according to the declaration. |
| `minTolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `maxTolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_project_point_on_edge`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_project_point_on_edge(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d point, OcctModelProjectionResult* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `point` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelProjectionResult*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_project_point_on_face`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_project_point_on_face(OcctModelHandle handle, OcctObjectId faceId, OcctPoint3d point, OcctModelProjectionResult* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `point` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelProjectionResult*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_ray_intersections`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ray_intersections(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `origin` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `direction` | `OcctVector3d` | Value | C ABI parameter passed according to the declaration. |
| `minimumParameter` | `double` | Value | C ABI parameter passed according to the declaration. |
| `maximumParameter` | `double` | Value | C ABI parameter passed according to the declaration. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_ray_hits_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ray_hits_copy(OcctModelHandle handle, OcctModelRayHit* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `results` | `OcctModelRayHit*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_classify_point`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_classify_point(OcctModelHandle handle, OcctObjectId solidId, OcctPoint3d point, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `solidId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `point` | `OcctPoint3d` | Value | C ABI parameter passed according to the declaration. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_mesh`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_mesh(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelMeshParameters* parameters);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `parameters` | `const OcctModelMeshParameters*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_clear_mesh`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_clear_mesh(OcctModelHandle handle, OcctObjectId shapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_face_mesh_nodes_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_mesh_nodes_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshNode* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `results` | `OcctModelMeshNode*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_face_mesh_triangles_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_mesh_triangles_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshTriangle* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `results` | `OcctModelMeshTriangle*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_import_file`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_file(OcctModelHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_import_step`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_step(OcctModelHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_import_iges`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_iges(OcctModelHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_import_brep`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_brep(OcctModelHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_import_stl`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_stl(OcctModelHandle handle, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_export_step`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_step(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_export_iges`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_iges(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_export_brep`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_brep(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |

##### `occt_model_export_stl`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_stl(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `utf8Path` | `const char*` | Input | UTF-8 string pointer. `const char*` is normally an input string. |
| `linearDeflection` | `double` | Value | C ABI parameter passed according to the declaration. |
| `angularDeflection` | `double` | Value | C ABI parameter passed according to the declaration. |
| `asciiMode` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_history_generated_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_generated_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `operationId` | `OcctOperationId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `sourceShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `results` | `OcctObjectId*` | Output | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_history_modified_copy`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_modified_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `operationId` | `OcctOperationId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `sourceShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `results` | `OcctObjectId*` | Output | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_history_is_removed`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_is_removed(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `operationId` | `OcctOperationId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `sourceShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_display_in_engine`

- **Category:** OcctModeling
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_display_in_engine(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId shapeId, int fit);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `engineHandle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `modelHandle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `fit` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_update_object_shape_from_model`

- **Category:** OcctModeling
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_object_shape_from_model(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId viewerObjectId, OcctObjectId modelShapeId, unsigned int options);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `engineHandle` | `OcctHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `modelHandle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `viewerObjectId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `modelShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `options` | `unsigned int` | Value | C ABI parameter passed according to the declaration. |

### `OcctModelingExtensions.h`

#### OcctModelingExtensions

##### `occt_model_shape_is_same`

- **Category:** OcctModelingExtensions
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_same( OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `firstId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `secondId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_is_partner`

- **Category:** OcctModelingExtensions
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_partner( OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `firstId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `secondId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |

##### `occt_model_shape_oriented_bounds`

- **Category:** OcctModelingExtensions
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_oriented_bounds( OcctModelHandle handle, OcctObjectId shapeId, int optimal, OcctOrientedBounds* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `optimal` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `result` | `OcctOrientedBounds*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_make_face_with_holes`

- **Category:** OcctModelingExtensions
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_face_with_holes( OcctModelHandle handle, OcctObjectId outerWireId, const OcctObjectId* innerWireIds, int innerWireCount);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `outerWireId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `innerWireIds` | `const OcctObjectId*` | Input | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `innerWireCount` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_trim_edge`

- **Category:** OcctModelingExtensions
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_trim_edge( OcctModelHandle handle, OcctObjectId edgeId, double firstParameter, double lastParameter);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `firstParameter` | `double` | Value | C ABI parameter passed according to the declaration. |
| `lastParameter` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_offset_wire`

- **Category:** OcctModelingExtensions
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_offset_wire( OcctModelHandle handle, OcctObjectId wireId, double offset, double altitude, int joinType, int openResult);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `wireId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `offset` | `double` | Value | C ABI parameter passed according to the declaration. |
| `altitude` | `double` | Value | C ABI parameter passed according to the declaration. |
| `joinType` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `openResult` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

### `OcctModelingBSpline.h`

#### OcctModelingBSpline

##### `occt_model_edge_bspline_info`

- **Category:** OcctModelingBSpline
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_info( OcctModelHandle handle, OcctObjectId edgeId, OcctModelBSplineCurveInfo* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelBSplineCurveInfo*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_bspline_pole_at`

- **Category:** OcctModelingBSpline
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_pole_at( OcctModelHandle handle, OcctObjectId edgeId, int index, OcctPoint3d* pole, double* weight);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `index` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `pole` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `weight` | `double*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_edge_bspline_knot_at`

- **Category:** OcctModelingBSpline
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_knot_at( OcctModelHandle handle, OcctObjectId edgeId, int index, double* knot, int* multiplicity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `edgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `index` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `knot` | `double*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `multiplicity` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_bspline_info`

- **Category:** OcctModelingBSpline
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_info( OcctModelHandle handle, OcctObjectId faceId, OcctModelBSplineSurfaceInfo* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelBSplineSurfaceInfo*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_bspline_pole_at`

- **Category:** OcctModelingBSpline
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_pole_at( OcctModelHandle handle, OcctObjectId faceId, int uIndex, int vIndex, OcctPoint3d* pole, double* weight);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `uIndex` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `vIndex` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `pole` | `OcctPoint3d*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `weight` | `double*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_bspline_u_knot_at`

- **Category:** OcctModelingBSpline
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_u_knot_at( OcctModelHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `index` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `knot` | `double*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `multiplicity` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_face_bspline_v_knot_at`

- **Category:** OcctModelingBSpline
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_v_knot_at( OcctModelHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `faceId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `index` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `knot` | `double*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `multiplicity` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

### `OcctModelingTopologyAnalysis.h`

#### OcctModelingTopologyAnalysis

##### `occt_model_shape_free_bounds`

- **Category:** OcctModelingTopologyAnalysis
- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_shape_free_bounds( OcctModelHandle handle, OcctObjectId shapeId, double tolerance, int boundaryKind, int splitClosed, int splitOpen);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `boundaryKind` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `splitClosed` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `splitOpen` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

##### `occt_model_shape_edge_adjacency`

- **Category:** OcctModelingTopologyAnalysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_edge_adjacency( OcctModelHandle handle, OcctObjectId shapeId, OcctModelEdgeAdjacency* items, int capacity, int* count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `items` | `OcctModelEdgeAdjacency*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `count` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

### `OcctModelingFaceAnalysis.h`

#### OcctModelingFaceAnalysis

##### `occt_model_shape_face_analysis`

- **Category:** OcctModelingFaceAnalysis
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_face_analysis( OcctModelHandle handle, OcctObjectId shapeId, OcctModelFaceAnalysis* items, int capacity, int* count);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `items` | `OcctModelFaceAnalysis*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |
| `count` | `int*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

### `OcctModelingInertia.h`

#### OcctModelingInertia

##### `occt_model_shape_linear_inertia`

- **Category:** OcctModelingInertia
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_linear_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelInertiaProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_shape_surface_inertia`

- **Category:** OcctModelingInertia
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_surface_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelInertiaProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_shape_volume_inertia`

- **Category:** OcctModelingInertia
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_volume_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `shapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelInertiaProperties*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

### `OcctModelingIntersection.h`

#### OcctModelingIntersection

##### `occt_model_intersect_edges`

- **Category:** OcctModelingIntersection
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_intersect_edges( OcctModelHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double tolerance);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `firstEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `secondEdgeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `tolerance` | `double` | Value | C ABI parameter passed according to the declaration. |

##### `occt_model_edge_intersections_copy`

- **Category:** OcctModelingIntersection
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_intersections_copy( OcctModelHandle handle, OcctModelEdgeIntersection* results, int capacity);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `results` | `OcctModelEdgeIntersection*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `capacity` | `int` | Value | Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose. |

### `OcctModelingTopologyReference.h`

#### OcctModelingTopologyReference

##### `occt_model_create_topology_reference`

- **Category:** OcctModelingTopologyReference
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_create_topology_reference( OcctModelHandle handle, OcctObjectId rootShapeId, OcctObjectId subshapeId, OcctModelTopologyReference* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `rootShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `subshapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `result` | `OcctModelTopologyReference*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_resolve_topology_reference`

- **Category:** OcctModelingTopologyReference
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_resolve_topology_reference( OcctModelHandle handle, OcctObjectId rootShapeId, const OcctModelTopologyReference* reference, double matchingTolerance, OcctModelTopologyReferenceResult* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `rootShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `reference` | `const OcctModelTopologyReference*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `matchingTolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelTopologyReferenceResult*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

##### `occt_model_resolve_topology_reference_with_history`

- **Category:** OcctModelingTopologyReference
- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_resolve_topology_reference_with_history( OcctModelHandle handle, OcctObjectId rootShapeId, OcctOperationId operationId, OcctObjectId sourceShapeId, const OcctModelTopologyReference* reference, double matchingTolerance, OcctModelTopologyReferenceResult* result);
```

| Name | C type | Direction | Meaning |
|---|---|---|---|
| `handle` | `OcctModelHandle` | Value | Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it. |
| `rootShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `operationId` | `OcctOperationId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `sourceShapeId` | `OcctObjectId` | Value | Bridge object ID scoped to the object registry owned by the associated native handle. |
| `reference` | `const OcctModelTopologyReference*` | Input | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |
| `matchingTolerance` | `double` | Value | C ABI parameter passed according to the declaration. |
| `result` | `OcctModelTopologyReferenceResult*` | Output | Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output. |

