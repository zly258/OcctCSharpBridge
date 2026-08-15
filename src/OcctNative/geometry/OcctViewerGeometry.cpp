#include "geometry/OcctViewerGeometry.h"
#include "core/OcctInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepGProp.hxx>
#include <BRep_Tool.hxx>
#include <GProp_GProps.hxx>
#include <Precision.hxx>
#include <TopAbs_Orientation.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>

#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t GeometryApiVersion = 1;
    constexpr std::uint32_t AllEdgeQueryBits =
        OcctViewerIndexedEdgeQuery_Endpoints |
        OcctViewerIndexedEdgeQuery_Evaluation;
    constexpr std::uint32_t AllFaceQueryBits =
        OcctViewerIndexedFaceQuery_Evaluation |
        OcctViewerIndexedFaceQuery_Center;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeGeometryStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    TopoDS_Shape requiredOwnerShape(Engine* engine, OcctObjectId ownerId)
    {
        const ObjectEntry* entry = engine->findShape(ownerId);
        if (entry == nullptr || entry->shape.IsNull())
            throw std::invalid_argument("Owner shape ID does not exist.");
        const TopoDS_Shape shape = shapeWithPresentationTransformation(*entry);
        if (shape.IsNull()) throw std::runtime_error("Owner shape is null after transformation.");
        return shape;
    }

    TopoDS_Shape indexedSubshape(
        const TopoDS_Shape& owner,
        TopAbs_ShapeEnum type,
        int index,
        const char* label)
    {
        if (index < 0) throw std::invalid_argument(std::string(label) + " index must not be negative.");
        int current = 0;
        for (TopExp_Explorer explorer(owner, type); explorer.More(); explorer.Next(), ++current)
        {
            if (current == index) return explorer.Current();
        }
        throw std::out_of_range(std::string(label) + " index is out of range.");
    }

    OcctPoint3d pointValue(const gp_Pnt& point)
    {
        return { point.X(), point.Y(), point.Z() };
    }

    OcctVector3d vectorValue(const gp_Vec& vector)
    {
        return { vector.X(), vector.Y(), vector.Z() };
    }

    void validateEdgeOptions(const OcctViewerIndexedEdgeQueryOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Indexed edge query options are null.");
        if (options->structSize < sizeof(OcctViewerIndexedEdgeQueryOptions) ||
            options->apiVersion != GeometryApiVersion)
        {
            throw std::invalid_argument("Unsupported indexed edge query options size or version.");
        }
        if (options->queryMask == 0 || (options->queryMask & ~AllEdgeQueryBits) != 0)
            throw std::invalid_argument("Indexed edge query mask is invalid.");
        if (options->edgeIndex < 0)
            throw std::invalid_argument("Edge index must not be negative.");
        if ((options->queryMask & OcctViewerIndexedEdgeQuery_Evaluation) != 0 &&
            (!std::isfinite(options->normalizedParameter) ||
             options->normalizedParameter < 0.0 || options->normalizedParameter > 1.0))
        {
            throw std::invalid_argument("Normalized edge parameter must be between 0 and 1.");
        }
    }

    void validateFaceOptions(const OcctViewerIndexedFaceQueryOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Indexed face query options are null.");
        if (options->structSize < sizeof(OcctViewerIndexedFaceQueryOptions) ||
            options->apiVersion != GeometryApiVersion)
        {
            throw std::invalid_argument("Unsupported indexed face query options size or version.");
        }
        if (options->queryMask == 0 || (options->queryMask & ~AllFaceQueryBits) != 0)
            throw std::invalid_argument("Indexed face query mask is invalid.");
        if (options->faceIndex < 0)
            throw std::invalid_argument("Face index must not be negative.");
        if ((options->queryMask & OcctViewerIndexedFaceQuery_Evaluation) != 0 &&
            (!std::isfinite(options->u) || !std::isfinite(options->v)))
        {
            throw std::invalid_argument("Face parameters must be finite.");
        }
    }
}

extern "C"
{
    OcctStatus occt_engine_indexed_vertex_get(
        OcctEngineHandle handle,
        OcctObjectId ownerId,
        int vertexIndex,
        OcctPoint3d* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeGeometryStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Vertex point output is null.");
            const TopoDS_Shape owner = requiredOwnerShape(engine, ownerId);
            const TopoDS_Vertex vertex = TopoDS::Vertex(
                indexedSubshape(owner, TopAbs_VERTEX, vertexIndex, "Vertex"));
            *result = pointValue(BRep_Tool::Pnt(vertex));
        });
    }

    OcctStatus occt_engine_indexed_edge_query(
        OcctEngineHandle handle,
        OcctObjectId ownerId,
        const OcctViewerIndexedEdgeQueryOptions* options,
        OcctViewerIndexedEdgeQueryResult* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeGeometryStatus(engine, [&]
        {
            validateEdgeOptions(options);
            if (result == nullptr) throw std::invalid_argument("Indexed edge query output is null.");

            *result = {};
            result->structSize = static_cast<std::uint32_t>(sizeof(OcctViewerIndexedEdgeQueryResult));
            result->apiVersion = GeometryApiVersion;

            const TopoDS_Shape owner = requiredOwnerShape(engine, ownerId);
            const TopoDS_Edge edge = TopoDS::Edge(
                indexedSubshape(owner, TopAbs_EDGE, options->edgeIndex, "Edge"));

            if ((options->queryMask & OcctViewerIndexedEdgeQuery_Endpoints) != 0)
            {
                TopoDS_Vertex firstVertex;
                TopoDS_Vertex lastVertex;
                TopExp::Vertices(edge, firstVertex, lastVertex, Standard_True);
                if (firstVertex.IsNull() || lastVertex.IsNull())
                    throw std::runtime_error("Edge does not contain two finite endpoint vertices.");
                result->start = pointValue(BRep_Tool::Pnt(firstVertex));
                result->end = pointValue(BRep_Tool::Pnt(lastVertex));
            }

            if ((options->queryMask & OcctViewerIndexedEdgeQuery_Evaluation) != 0)
            {
                BRepAdaptor_Curve curve(edge);
                const double first = curve.FirstParameter();
                const double last = curve.LastParameter();
                if (!std::isfinite(first) || !std::isfinite(last))
                    throw std::runtime_error("Edge parameter range is not finite.");
                const double parameter = first + (last - first) * options->normalizedParameter;
                gp_Pnt point;
                gp_Vec derivative;
                curve.D1(parameter, point, derivative);
                if (derivative.SquareMagnitude() <= Precision::SquareConfusion())
                    throw std::runtime_error("Edge tangent is undefined at the requested parameter.");
                derivative.Normalize();
                result->point = pointValue(point);
                result->tangent = vectorValue(derivative);
            }
        });
    }

    OcctStatus occt_engine_indexed_face_query(
        OcctEngineHandle handle,
        OcctObjectId ownerId,
        const OcctViewerIndexedFaceQueryOptions* options,
        OcctViewerIndexedFaceQueryResult* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeGeometryStatus(engine, [&]
        {
            validateFaceOptions(options);
            if (result == nullptr) throw std::invalid_argument("Indexed face query output is null.");

            *result = {};
            result->structSize = static_cast<std::uint32_t>(sizeof(OcctViewerIndexedFaceQueryResult));
            result->apiVersion = GeometryApiVersion;

            const TopoDS_Shape owner = requiredOwnerShape(engine, ownerId);
            const TopoDS_Face face = TopoDS::Face(
                indexedSubshape(owner, TopAbs_FACE, options->faceIndex, "Face"));

            if ((options->queryMask & OcctViewerIndexedFaceQuery_Evaluation) != 0)
            {
                BRepAdaptor_Surface surface(face, Standard_True);
                const double uMin = surface.FirstUParameter();
                const double uMax = surface.LastUParameter();
                const double vMin = surface.FirstVParameter();
                const double vMax = surface.LastVParameter();
                const double tolerance = Precision::PConfusion();
                if (options->u < uMin - tolerance || options->u > uMax + tolerance ||
                    options->v < vMin - tolerance || options->v > vMax + tolerance)
                {
                    throw std::out_of_range("Face parameters are outside the surface parameter bounds.");
                }

                gp_Pnt point;
                gp_Vec du;
                gp_Vec dv;
                surface.D1(options->u, options->v, point, du, dv);
                gp_Vec normal = du.Crossed(dv);
                if (normal.SquareMagnitude() <= Precision::SquareConfusion())
                    throw std::runtime_error("Face normal is undefined at the requested parameters.");
                if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
                normal.Normalize();
                result->point = pointValue(point);
                result->normal = vectorValue(normal);
            }

            if ((options->queryMask & OcctViewerIndexedFaceQuery_Center) != 0)
            {
                GProp_GProps properties;
                BRepGProp::SurfaceProperties(face, properties);
                result->center = pointValue(properties.CentreOfMass());
            }
        });
    }
}
