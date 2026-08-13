#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerZLayer
    {
        OcctViewerZLayer_Bottom = 0,
        OcctViewerZLayer_Default = 1,
        OcctViewerZLayer_Top = 2,
        OcctViewerZLayer_Topmost = 3
    };

    enum OcctCornerPosition
    {
        OcctCorner_LeftLower = 0,
        OcctCorner_LeftUpper = 1,
        OcctCorner_RightLower = 2,
        OcctCorner_RightUpper = 3
    };

    struct OcctTriedronOptions
    {
        int visible;
        int position;
        double scale;
        double r;
        double g;
        double b;
    };

    struct OcctViewCubeOptions
    {
        int visible;
        int position;
        int sizePixels;
        int offsetX;
        int offsetY;
    };

    struct OcctObjectTransformUpdate
    {
        OcctObjectId objectId;
        OcctTransform3d transformation;
    };

    OCCTBRIDGE_API int occt_set_object_z_layer(
        OcctHandle handle,
        OcctObjectId objectId,
        int layer);

    OCCTBRIDGE_API int occt_set_objects_z_layer(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int layer);

    OCCTBRIDGE_API int occt_get_object_z_layer(
        OcctHandle handle,
        OcctObjectId objectId,
        int* layer);

    OCCTBRIDGE_API int occt_set_triedron_options(
        OcctHandle handle,
        const OcctTriedronOptions* options);

    OCCTBRIDGE_API int occt_set_view_cube_options(
        OcctHandle handle,
        const OcctViewCubeOptions* options);

    OCCTBRIDGE_API int occt_set_face_boundary_style(
        OcctHandle handle,
        OcctObjectId shapeId,
        int visible,
        double r,
        double g,
        double b,
        double width);

    OCCTBRIDGE_API int occt_set_face_boundary_styles(
        OcctHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        int visible,
        double r,
        double g,
        double b,
        double width);

    OCCTBRIDGE_API int occt_set_default_face_boundary_style(
        OcctHandle handle,
        int visible,
        double r,
        double g,
        double b,
        double width,
        int applyExisting);

    OCCTBRIDGE_API int occt_indexed_vertex_point(
        OcctHandle handle,
        OcctObjectId ownerId,
        int vertexIndex,
        OcctPoint3d* result);

    OCCTBRIDGE_API int occt_indexed_edge_endpoints(
        OcctHandle handle,
        OcctObjectId ownerId,
        int edgeIndex,
        OcctPoint3d* start,
        OcctPoint3d* end);

    OCCTBRIDGE_API int occt_indexed_edge_point_at(
        OcctHandle handle,
        OcctObjectId ownerId,
        int edgeIndex,
        double normalizedParameter,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultTangent);

    OCCTBRIDGE_API int occt_indexed_face_point_normal(
        OcctHandle handle,
        OcctObjectId ownerId,
        int faceIndex,
        double u,
        double v,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultNormal);

    OCCTBRIDGE_API int occt_indexed_face_center(
        OcctHandle handle,
        OcctObjectId ownerId,
        int faceIndex,
        OcctPoint3d* result);

    OCCTBRIDGE_API int occt_set_object_transforms(
        OcctHandle handle,
        const OcctObjectTransformUpdate* updates,
        int count);
}
