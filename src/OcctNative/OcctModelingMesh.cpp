#include "OcctModelingMeshInternal.hxx"
#include "OcctModelingShapeInternal.hxx"

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

    int occt_model_face_mesh_counts(OcctModelHandle handle, OcctObjectId faceId, int* nodeCount, int* triangleCount)
    {
        ModelSession* model = modelOf(handle);
        if (nodeCount == nullptr || triangleCount == nullptr) return 0;
        return execute(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            *nodeCount = triangulation->NbNodes();
            *triangleCount = triangulation->NbTriangles();
        });
    }

    int occt_model_face_mesh_node(OcctModelHandle handle, OcctObjectId faceId, int index, OcctModelMeshNode* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int oneBased = index + 1;
            if (index < 0 || oneBased > triangulation->NbNodes()) throw std::out_of_range("Mesh node index is out of range.");
            if (!triangulation->HasNormals()) BRepLib_ToolTriangulatedShape::ComputeNormals(face, triangulation);
            const gp_Pnt point = triangulation->Node(oneBased).Transformed(location.Transformation());
            result->point = {point.X(), point.Y(), point.Z()};
            result->hasUv = triangulation->HasUVNodes() ? 1 : 0;
            result->hasNormal = triangulation->HasNormals() ? 1 : 0;
            if (result->hasUv != 0)
            {
                const gp_Pnt2d uv = triangulation->UVNode(oneBased);
                result->u = uv.X();
                result->v = uv.Y();
            }
            else
            {
                result->u = 0.0;
                result->v = 0.0;
            }
            if (result->hasNormal != 0)
            {
                gp_Dir normal = triangulation->Normal(oneBased);
                normal.Transform(location.Transformation());
                result->normal = {normal.X(), normal.Y(), normal.Z()};
            }
            else result->normal = {0.0, 0.0, 0.0};
        });
    }

    int occt_model_face_mesh_triangle(OcctModelHandle handle, OcctObjectId faceId, int index, OcctModelMeshTriangle* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int oneBased = index + 1;
            if (index < 0 || oneBased > triangulation->NbTriangles()) throw std::out_of_range("Mesh triangle index is out of range.");
            int node1 = 0;
            int node2 = 0;
            int node3 = 0;
            triangulation->Triangle(oneBased).Get(node1, node2, node3);
            if (face.Orientation() == TopAbs_REVERSED) std::swap(node2, node3);
            result->node1 = node1 - 1;
            result->node2 = node2 - 1;
            result->node3 = node3 - 1;
        });
    }
}
