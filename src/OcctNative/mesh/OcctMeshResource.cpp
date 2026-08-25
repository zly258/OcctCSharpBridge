#include "OcctMeshResource.h"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRepBuilderAPI_Copy.hxx>
#include <BRepLib_ToolTriangulatedShape.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRep_Tool.hxx>
#include <IMeshTools_Parameters.hxx>
#include <Poly_Triangulation.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Face.hxx>

#include <algorithm>
#include <limits>
#include <vector>

struct OcctMeshHandle_t
{
    std::vector<OcctModelMeshNode> nodes;
    std::vector<OcctModelMeshTriangle> triangles;
};

using namespace OcctModelingInternal;

namespace
{
    constexpr std::uint32_t MeshOptionsApiVersion = 1;

    OcctStatus validateCopy(
        OcctMeshHandle handle,
        const void* results,
        int capacity,
        int count,
        int* written)
    {
        if (handle == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (written == nullptr || capacity < 0) return OcctStatus_ErrorInvalidArgument;
        *written = count;
        if (count == 0) return OcctStatus_Ok;
        if (results == nullptr)
            return capacity == 0 ? OcctStatus_ErrorBufferTooSmall : OcctStatus_ErrorInvalidArgument;
        return capacity < count ? OcctStatus_ErrorBufferTooSmall : OcctStatus_Ok;
    }
}

extern "C"
{
    OcctStatus occt_model_mesh_create(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const OcctMeshBuildOptions* options,
        OcctMeshHandle* result)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeStatus(model, [&]
        {
            if (options == nullptr || result == nullptr)
                throw std::invalid_argument("Mesh options or handle output is null.");
            *result = nullptr;
            if (options->structSize < sizeof(OcctMeshBuildOptions) ||
                options->apiVersion != MeshOptionsApiVersion)
                throw std::invalid_argument("Mesh options layout or API version is unsupported.");
            if (options->linearDeflection <= 0.0 || options->angularDeflection <= 0.0)
                throw std::invalid_argument("Mesh deflections must be positive.");
            if (options->minSize < 0.0)
                throw std::invalid_argument("Mesh minimum size must not be negative.");

            BRepBuilderAPI_Copy copier(model->requireShape(shapeId), true, true);
            TopoDS_Shape shape = copier.Shape();

            IMeshTools_Parameters parameters;
            parameters.Deflection = options->linearDeflection;
            parameters.DeflectionInterior = options->linearDeflection;
            parameters.Angle = options->angularDeflection;
            parameters.AngleInterior = options->angularDeflection;
            parameters.MinSize = options->minSize > 0.0
                ? options->minSize
                : options->linearDeflection * IMeshTools_Parameters::RelMinSize();
            parameters.Relative = options->relative != 0;
            parameters.InParallel = options->parallel != 0;
            parameters.InternalVerticesMode = options->internalVertices != 0;
            parameters.ControlSurfaceDeflection = options->controlSurfaceDeflection != 0;

            BRepMesh_IncrementalMesh mesher(shape, parameters);
            mesher.Perform();
            if (!mesher.IsDone()) throw std::runtime_error("Shape meshing failed.");

            auto mesh = new OcctMeshHandle_t();
            try
            {
                for (TopExp_Explorer explorer(shape, TopAbs_FACE); explorer.More(); explorer.Next())
                {
                    TopoDS_Face face = TopoDS::Face(explorer.Current());
                    TopLoc_Location location;
                    Handle(Poly_Triangulation) triangulation = BRep_Tool::Triangulation(face, location);
                    if (triangulation.IsNull()) continue;
                    if (!triangulation->HasNormals())
                        BRepLib_ToolTriangulatedShape::ComputeNormals(face, triangulation);

                    const auto nodeOffset = mesh->nodes.size();
                    const auto nodeCount = static_cast<std::size_t>(triangulation->NbNodes());
                    const auto triangleCount = static_cast<std::size_t>(triangulation->NbTriangles());
                    if (nodeOffset > static_cast<std::size_t>(std::numeric_limits<int>::max()) ||
                        nodeCount > static_cast<std::size_t>(std::numeric_limits<int>::max()) - nodeOffset)
                        throw std::overflow_error("Mesh node count exceeds the ABI limit.");

                    mesh->nodes.reserve(nodeOffset + nodeCount);
                    mesh->triangles.reserve(mesh->triangles.size() + triangleCount);
                    const bool hasUv = triangulation->HasUVNodes();
                    const bool hasNormal = triangulation->HasNormals();

                    for (int oneBased = 1; oneBased <= triangulation->NbNodes(); ++oneBased)
                    {
                        const gp_Pnt point = triangulation->Node(oneBased).Transformed(location.Transformation());
                        OcctModelMeshNode node{};
                        node.point = {point.X(), point.Y(), point.Z()};
                        node.hasUv = hasUv ? 1 : 0;
                        node.hasNormal = hasNormal ? 1 : 0;
                        if (hasUv)
                        {
                            const gp_Pnt2d uv = triangulation->UVNode(oneBased);
                            node.u = uv.X();
                            node.v = uv.Y();
                        }
                        if (hasNormal)
                        {
                            gp_Dir normal = triangulation->Normal(oneBased);
                            normal.Transform(location.Transformation());
                            node.normal = {normal.X(), normal.Y(), normal.Z()};
                        }
                        mesh->nodes.push_back(node);
                    }

                    for (int oneBased = 1; oneBased <= triangulation->NbTriangles(); ++oneBased)
                    {
                        int node1 = 0;
                        int node2 = 0;
                        int node3 = 0;
                        triangulation->Triangle(oneBased).Get(node1, node2, node3);
                        if (face.Orientation() == TopAbs_REVERSED) std::swap(node2, node3);
                        mesh->triangles.push_back({
                            static_cast<int>(nodeOffset) + node1 - 1,
                            static_cast<int>(nodeOffset) + node2 - 1,
                            static_cast<int>(nodeOffset) + node3 - 1});
                    }
                }
                *result = mesh;
            }
            catch (...)
            {
                delete mesh;
                throw;
            }
        });
    }

    void occt_mesh_release(OcctMeshHandle handle)
    {
        delete handle;
    }

    OcctStatus occt_mesh_get_counts(OcctMeshHandle handle, int* nodeCount, int* triangleCount)
    {
        if (handle == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (nodeCount == nullptr || triangleCount == nullptr) return OcctStatus_ErrorInvalidArgument;
        if (handle->nodes.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()) ||
            handle->triangles.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
            return OcctStatus_ErrorInvalidState;
        *nodeCount = static_cast<int>(handle->nodes.size());
        *triangleCount = static_cast<int>(handle->triangles.size());
        return OcctStatus_Ok;
    }

    OcctStatus occt_mesh_nodes_copy(
        OcctMeshHandle handle,
        OcctModelMeshNode* results,
        int capacity,
        int* written)
    {
        if (handle != nullptr && handle->nodes.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
            return OcctStatus_ErrorInvalidState;
        const int count = handle == nullptr ? 0 : static_cast<int>(handle->nodes.size());
        const OcctStatus status = validateCopy(handle, results, capacity, count, written);
        if (status != OcctStatus_Ok) return status;
        if (count > 0) std::copy(handle->nodes.begin(), handle->nodes.end(), results);
        return OcctStatus_Ok;
    }

    OcctStatus occt_mesh_triangles_copy(
        OcctMeshHandle handle,
        OcctModelMeshTriangle* results,
        int capacity,
        int* written)
    {
        if (handle != nullptr && handle->triangles.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
            return OcctStatus_ErrorInvalidState;
        const int count = handle == nullptr ? 0 : static_cast<int>(handle->triangles.size());
        const OcctStatus status = validateCopy(handle, results, capacity, count, written);
        if (status != OcctStatus_Ok) return status;
        if (count > 0) std::copy(handle->triangles.begin(), handle->triangles.end(), results);
        return OcctStatus_Ok;
    }
}
