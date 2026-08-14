#include "OcctModelingMeshInternal.hxx"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepLib_ToolTriangulatedShape.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <IMeshTools_Parameters.hxx>
#include <gp_Pnt2d.hxx>

#include <algorithm>

using namespace OcctModelingInternal;

extern "C"
{
    int occt_model_mesh(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelMeshParameters* parameters)
    {
        ModelSession* model = modelOf(handle);
        if (parameters == nullptr) return 0;
        return execute(model, [&]
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

    int occt_model_clear_mesh(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&] { BRepTools::Clean(model->requireShape(shapeId)); });
    }

    int occt_model_face_mesh_nodes_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshNode* results, int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Mesh-node buffer capacity must not be negative.");
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int count = triangulation->NbNodes();
            if (results == nullptr)
            {
                if (capacity != 0) throw std::invalid_argument("Null mesh-node buffer requires zero capacity.");
                copied = count;
                return;
            }
            if (capacity < count) throw std::invalid_argument("Mesh-node buffer capacity is smaller than the result count.");

            if (!triangulation->HasNormals()) BRepLib_ToolTriangulatedShape::ComputeNormals(face, triangulation);
            const bool hasUv = triangulation->HasUVNodes();
            const bool hasNormal = triangulation->HasNormals();
            for (int oneBased = 1; oneBased <= count; ++oneBased)
            {
                OcctModelMeshNode& result = results[oneBased - 1];
                const gp_Pnt point = triangulation->Node(oneBased).Transformed(location.Transformation());
                result.point = {point.X(), point.Y(), point.Z()};
                result.hasUv = hasUv ? 1 : 0;
                result.hasNormal = hasNormal ? 1 : 0;

                if (hasUv)
                {
                    const gp_Pnt2d uv = triangulation->UVNode(oneBased);
                    result.u = uv.X();
                    result.v = uv.Y();
                }
                else
                {
                    result.u = 0.0;
                    result.v = 0.0;
                }

                if (hasNormal)
                {
                    gp_Dir normal = triangulation->Normal(oneBased);
                    normal.Transform(location.Transformation());
                    result.normal = {normal.X(), normal.Y(), normal.Z()};
                }
                else
                {
                    result.normal = {0.0, 0.0, 0.0};
                }
            }
            copied = count;
        }) == 0)
            return -1;
        return copied;
    }

    int occt_model_face_mesh_triangles_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshTriangle* results, int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Mesh-triangle buffer capacity must not be negative.");
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int count = triangulation->NbTriangles();
            if (results == nullptr)
            {
                if (capacity != 0) throw std::invalid_argument("Null mesh-triangle buffer requires zero capacity.");
                copied = count;
                return;
            }
            if (capacity < count) throw std::invalid_argument("Mesh-triangle buffer capacity is smaller than the result count.");

            for (int oneBased = 1; oneBased <= count; ++oneBased)
            {
                int node1 = 0;
                int node2 = 0;
                int node3 = 0;
                triangulation->Triangle(oneBased).Get(node1, node2, node3);
                if (face.Orientation() == TopAbs_REVERSED) std::swap(node2, node3);
                OcctModelMeshTriangle& result = results[oneBased - 1];
                result.node1 = node1 - 1;
                result.node2 = node2 - 1;
                result.node3 = node3 - 1;
            }
            copied = count;
        }) == 0)
            return -1;
        return copied;
    }
}
