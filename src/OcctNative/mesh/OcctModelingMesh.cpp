#include "mesh/OcctModelingMesh.h"
#include "mesh/OcctModelingMeshInternal.hxx"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepLib_ToolTriangulatedShape.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <IMeshTools_Parameters.hxx>
#include <Poly_Triangulation.hxx>
#include <gp_Pnt2d.hxx>

#include <algorithm>
#include <stdexcept>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_mesh(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const OcctModelMeshParameters* parameters)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (parameters == nullptr) return OcctStatus_ErrorInvalidArgument;

        return executeStatus(model, [&]
        {
            requirePositive(parameters->linearDeflection, "Linear deflection");
            requirePositive(parameters->angularDeflection, "Angular deflection");

            IMeshTools_Parameters nativeParameters;
            nativeParameters.Deflection = parameters->linearDeflection;
            nativeParameters.DeflectionInterior = parameters->linearDeflection;
            nativeParameters.Angle = parameters->angularDeflection;
            nativeParameters.AngleInterior = parameters->angularDeflection;
            nativeParameters.MinSize = parameters->minSize > 0.0
                ? parameters->minSize
                : parameters->linearDeflection * IMeshTools_Parameters::RelMinSize();
            nativeParameters.Relative = parameters->relative != 0;
            nativeParameters.InParallel = parameters->parallel != 0;
            nativeParameters.InternalVerticesMode = parameters->internalVertices != 0;
            nativeParameters.ControlSurfaceDeflection = parameters->controlSurfaceDeflection != 0;

            BRepMesh_IncrementalMesh mesh(model->requireShape(shapeId), nativeParameters);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("Shape meshing failed.");
        });
    }

    OcctStatus occt_model_clear_mesh(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId)
    {
        ModelSession* model = sessionOf(handle);
        return executeStatus(model, [&]
        {
            BRepTools::Clean(model->requireShape(shapeId));
        });
    }

    OcctStatus occt_model_face_mesh_nodes_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelMeshNode* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int count = triangulation->NbNodes();
            *required = count;

            if (results == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null mesh-node buffer requires zero capacity.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Mesh-node buffer capacity is smaller than the result count.");

            if (!triangulation->HasNormals())
                BRepLib_ToolTriangulatedShape::ComputeNormals(face, triangulation);
            const bool hasUv = triangulation->HasUVNodes();
            const bool hasNormal = triangulation->HasNormals();

            for (int oneBased = 1; oneBased <= count; ++oneBased)
            {
                OcctModelMeshNode& node = results[oneBased - 1];
                const gp_Pnt point = triangulation->Node(oneBased).Transformed(location.Transformation());
                node.point = {point.X(), point.Y(), point.Z()};
                node.hasUv = hasUv ? 1 : 0;
                node.hasNormal = hasNormal ? 1 : 0;

                if (hasUv)
                {
                    const gp_Pnt2d uv = triangulation->UVNode(oneBased);
                    node.u = uv.X();
                    node.v = uv.Y();
                }
                else
                {
                    node.u = 0.0;
                    node.v = 0.0;
                }

                if (hasNormal)
                {
                    gp_Dir normal = triangulation->Normal(oneBased);
                    normal.Transform(location.Transformation());
                    node.normal = {normal.X(), normal.Y(), normal.Z()};
                }
                else
                {
                    node.normal = {0.0, 0.0, 0.0};
                }
            }
        });
    }

    OcctStatus occt_model_face_mesh_triangles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelMeshTriangle* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int count = triangulation->NbTriangles();
            *required = count;

            if (results == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null mesh-triangle buffer requires zero capacity.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Mesh-triangle buffer capacity is smaller than the result count.");

            for (int oneBased = 1; oneBased <= count; ++oneBased)
            {
                int node1 = 0;
                int node2 = 0;
                int node3 = 0;
                triangulation->Triangle(oneBased).Get(node1, node2, node3);
                if (face.Orientation() == TopAbs_REVERSED) std::swap(node2, node3);

                OcctModelMeshTriangle& triangle = results[oneBased - 1];
                triangle.node1 = node1 - 1;
                triangle.node2 = node2 - 1;
                triangle.node3 = node3 - 1;
            }
        });
    }
}
