#pragma once

#include "OcctNative.h"

#include <cstdint>

extern "C"
{
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

    OCCTBRIDGE_API OcctModelHandle occt_model_create();
    OCCTBRIDGE_API void occt_model_destroy(OcctModelHandle handle);
    OCCTBRIDGE_API const char* occt_model_last_error(OcctModelHandle handle);
    OCCTBRIDGE_API const char* occt_model_capabilities();
    OCCTBRIDGE_API int occt_model_shape_count(OcctModelHandle handle);
    OCCTBRIDGE_API OcctObjectId occt_model_shape_id_at(OcctModelHandle handle, int index);
    OCCTBRIDGE_API int occt_model_shape_exists(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_delete_shape(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_clear(OcctModelHandle handle);
    OCCTBRIDGE_API const char* occt_model_operation_report(OcctModelHandle handle, OcctOperationId operationId);

    OCCTBRIDGE_API OcctObjectId occt_model_copy_shape(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API std::int64_t occt_model_shape_hash(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_type(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_orientation(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_is_closed(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_is_valid(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API double occt_model_shape_tolerance(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_bounds(OcctModelHandle handle, OcctObjectId shapeId, OcctBounds* result);
    OCCTBRIDGE_API int occt_model_shape_linear_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
    OCCTBRIDGE_API int occt_model_shape_surface_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
    OCCTBRIDGE_API int occt_model_shape_volume_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
    OCCTBRIDGE_API int occt_model_shape_distance(OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
    OCCTBRIDGE_API const char* occt_model_check_report(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_get_location(OcctModelHandle handle, OcctObjectId shapeId, OcctModelLocation* result);
    OCCTBRIDGE_API OcctObjectId occt_model_set_location(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelLocation* location, int copyShape);

    OCCTBRIDGE_API int occt_model_topology_count(OcctModelHandle handle, OcctObjectId shapeId, int shapeType);
    OCCTBRIDGE_API OcctObjectId occt_model_get_subshape(OcctModelHandle handle, OcctObjectId shapeId, int shapeType, int index);
    OCCTBRIDGE_API OcctObjectId occt_model_outer_wire(OcctModelHandle handle, OcctObjectId faceId);
    OCCTBRIDGE_API int occt_model_inner_wire_count(OcctModelHandle handle, OcctObjectId faceId);
    OCCTBRIDGE_API OcctObjectId occt_model_inner_wire_at(OcctModelHandle handle, OcctObjectId faceId, int index);
    OCCTBRIDGE_API int occt_model_ancestor_count(OcctModelHandle handle, OcctObjectId rootId, OcctObjectId childId, int ancestorType);
    OCCTBRIDGE_API OcctObjectId occt_model_ancestor_at(OcctModelHandle handle, OcctObjectId rootId, OcctObjectId childId, int ancestorType, int index);

    OCCTBRIDGE_API int occt_model_vertex_point(OcctModelHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
    OCCTBRIDGE_API int occt_model_edge_endpoints(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
    OCCTBRIDGE_API int occt_model_edge_point_at(OcctModelHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
    OCCTBRIDGE_API int occt_model_edge_curve_type(OcctModelHandle handle, OcctObjectId edgeId);
    OCCTBRIDGE_API int occt_model_face_surface_type(OcctModelHandle handle, OcctObjectId faceId);
    OCCTBRIDGE_API int occt_model_face_uv_bounds(OcctModelHandle handle, OcctObjectId faceId, OcctUvBounds* result);
    OCCTBRIDGE_API int occt_model_face_point_normal(OcctModelHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);
    OCCTBRIDGE_API int occt_model_edge_line_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
    OCCTBRIDGE_API int occt_model_edge_circle_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
    OCCTBRIDGE_API int occt_model_edge_ellipse_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
    OCCTBRIDGE_API int occt_model_face_plane_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
    OCCTBRIDGE_API int occt_model_face_cylinder_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
    OCCTBRIDGE_API int occt_model_face_cone_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
    OCCTBRIDGE_API int occt_model_face_sphere_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
    OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);

    OCCTBRIDGE_API int occt_model_edge_parameter_range(OcctModelHandle handle, OcctObjectId edgeId, OcctModelParameterRange* result);
    OCCTBRIDGE_API int occt_model_edge_differential(OcctModelHandle handle, OcctObjectId edgeId, double parameter, OcctModelCurveDifferential* result);
    OCCTBRIDGE_API int occt_model_edge_curvature(OcctModelHandle handle, OcctObjectId edgeId, double parameter, double resolution, OcctModelCurveCurvature* result);
    OCCTBRIDGE_API int occt_model_face_periodicity(OcctModelHandle handle, OcctObjectId faceId, OcctModelSurfacePeriodicity* result);
    OCCTBRIDGE_API int occt_model_face_differential(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceDifferential* result);
    OCCTBRIDGE_API int occt_model_face_curvature(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceCurvature* result);

    OCCTBRIDGE_API OcctObjectId occt_model_make_vertex(OcctModelHandle handle, OcctPoint3d point);
    OCCTBRIDGE_API OcctObjectId occt_model_make_line(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d end);
    OCCTBRIDGE_API OcctObjectId occt_model_make_polyline(OcctModelHandle handle, const OcctPoint3d* points, int count, int closed);
    OCCTBRIDGE_API OcctObjectId occt_model_make_circle(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
    OCCTBRIDGE_API OcctObjectId occt_model_make_arc_three_points(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
    OCCTBRIDGE_API OcctObjectId occt_model_make_arc_center(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
    OCCTBRIDGE_API OcctObjectId occt_model_make_regular_polygon(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
    OCCTBRIDGE_API OcctObjectId occt_model_make_ellipse(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
    OCCTBRIDGE_API OcctObjectId occt_model_make_bezier(OcctModelHandle handle, const OcctPoint3d* poles, int count);
    OCCTBRIDGE_API OcctObjectId occt_model_make_bspline_interpolated(OcctModelHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
    OCCTBRIDGE_API OcctObjectId occt_model_make_rectangle_wire(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    OCCTBRIDGE_API OcctObjectId occt_model_make_plane_face(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    OCCTBRIDGE_API OcctObjectId occt_model_make_face_from_wire(OcctModelHandle handle, OcctObjectId wireId, int onlyPlane);
    OCCTBRIDGE_API OcctObjectId occt_model_make_box(OcctModelHandle handle, double x, double y, double z, double dx, double dy, double dz);
    OCCTBRIDGE_API OcctObjectId occt_model_make_cylinder(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
    OCCTBRIDGE_API OcctObjectId occt_model_make_cone(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
    OCCTBRIDGE_API OcctObjectId occt_model_make_sphere(OcctModelHandle handle, OcctPoint3d center, double radius);
    OCCTBRIDGE_API OcctObjectId occt_model_make_torus(OcctModelHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
    OCCTBRIDGE_API OcctObjectId occt_model_make_wedge(OcctModelHandle handle, double dx, double dy, double dz, double ltx);
    OCCTBRIDGE_API OcctObjectId occt_model_make_compound(OcctModelHandle handle, const OcctObjectId* shapeIds, int count);
    OCCTBRIDGE_API OcctObjectId occt_model_make_wire(OcctModelHandle handle, const OcctObjectId* edgeIds, int count);
    OCCTBRIDGE_API OcctObjectId occt_model_sew(OcctModelHandle handle, const OcctObjectId* shapeIds, int count, double tolerance);
    OCCTBRIDGE_API OcctObjectId occt_model_make_solid_from_shell(OcctModelHandle handle, OcctObjectId shellId);

    OCCTBRIDGE_API OcctObjectId occt_model_translate(OcctModelHandle handle, OcctObjectId shapeId, OcctVector3d vector);
    OCCTBRIDGE_API OcctObjectId occt_model_rotate(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
    OCCTBRIDGE_API OcctObjectId occt_model_scale(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor);
    OCCTBRIDGE_API OcctObjectId occt_model_mirror_plane(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_boolean(OcctModelHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, const OcctModelBooleanOptions* options);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_split(OcctModelHandle handle, const OcctObjectId* objectIds, int objectCount, const OcctObjectId* toolIds, int toolCount, const OcctModelBooleanOptions* options);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_extrude(OcctModelHandle handle, OcctObjectId profileId, OcctVector3d vector);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_revolve(OcctModelHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_sweep(OcctModelHandle handle, OcctObjectId spineWireId, OcctObjectId profileId);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_loft(OcctModelHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fillet_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_chamfer_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_offset(OcctModelHandle handle, OcctObjectId shapeId, double offset, double tolerance);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_thick_solid(OcctModelHandle handle, OcctObjectId solidId, const int* faceIndicesToRemove, int count, double thickness, double tolerance);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_unify_same_domain(OcctModelHandle handle, OcctObjectId shapeId, int unifyEdges, int unifyFaces, int concatBsplines);
    OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fix_shape(OcctModelHandle handle, OcctObjectId shapeId, double precision, double minTolerance, double maxTolerance);

    OCCTBRIDGE_API int occt_model_project_point_on_edge(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d point, OcctModelProjectionResult* result);
    OCCTBRIDGE_API int occt_model_project_point_on_face(OcctModelHandle handle, OcctObjectId faceId, OcctPoint3d point, OcctModelProjectionResult* result);
    OCCTBRIDGE_API int occt_model_ray_intersections(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance);
    OCCTBRIDGE_API int occt_model_ray_hit_count(OcctModelHandle handle);
    OCCTBRIDGE_API int occt_model_ray_hit_at(OcctModelHandle handle, int index, OcctModelRayHit* result);
    OCCTBRIDGE_API int occt_model_ray_hits_copy(OcctModelHandle handle, OcctModelRayHit* results, int capacity);
    OCCTBRIDGE_API int occt_model_classify_point(OcctModelHandle handle, OcctObjectId solidId, OcctPoint3d point, double tolerance);

    OCCTBRIDGE_API int occt_model_mesh(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelMeshParameters* parameters);
    OCCTBRIDGE_API int occt_model_clear_mesh(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_face_mesh_counts(OcctModelHandle handle, OcctObjectId faceId, int* nodeCount, int* triangleCount);
    OCCTBRIDGE_API int occt_model_face_mesh_node(OcctModelHandle handle, OcctObjectId faceId, int index, OcctModelMeshNode* result);
    OCCTBRIDGE_API int occt_model_face_mesh_triangle(OcctModelHandle handle, OcctObjectId faceId, int index, OcctModelMeshTriangle* result);

    OCCTBRIDGE_API OcctObjectId occt_model_import_file(OcctModelHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_model_import_step(OcctModelHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_model_import_iges(OcctModelHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_model_import_brep(OcctModelHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_model_import_stl(OcctModelHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_model_export_step(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
    OCCTBRIDGE_API int occt_model_export_iges(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
    OCCTBRIDGE_API int occt_model_export_brep(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
    OCCTBRIDGE_API int occt_model_export_stl(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);

    OCCTBRIDGE_API int occt_model_history_generated_count(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId);
    OCCTBRIDGE_API OcctObjectId occt_model_history_generated_at(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, int index);
    OCCTBRIDGE_API int occt_model_history_generated_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
    OCCTBRIDGE_API int occt_model_history_modified_count(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId);
    OCCTBRIDGE_API OcctObjectId occt_model_history_modified_at(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, int index);
    OCCTBRIDGE_API int occt_model_history_modified_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
    OCCTBRIDGE_API int occt_model_history_is_removed(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId);

    OCCTBRIDGE_API OcctObjectId occt_model_display_in_engine(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId shapeId, int fit);
    OCCTBRIDGE_API int occt_update_object_shape_from_model(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId viewerObjectId, OcctObjectId modelShapeId, unsigned int options);
}
