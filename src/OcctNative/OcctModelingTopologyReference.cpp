#include "OcctModelingTopologyReference.h"
#include "OcctModelingSessionInternal.hxx"
#include "OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepBndLib.hxx>
#include <BRepGProp.hxx>
#include <Bnd_Box.hxx>
#include <GProp_GProps.hxx>
#include <TopExp.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <TopTools_ListIteratorOfListOfShape.hxx>
#include <TopoDS.hxx>

#include <algorithm>
#include <cmath>
#include <limits>
#include <vector>

using namespace OcctModelingInternal;

namespace
{
    constexpr int ReferenceVersion = 1;
    constexpr double MinimumResolvedScore = 0.78;
    constexpr double AmbiguityMargin = 0.025;

    struct Candidate
    {
        TopoDS_Shape shape;
        int index = -1;
        double score = 0.0;
    };

    OcctPoint3d toNativePoint(const gp_Pnt& point)
    {
        return {point.X(), point.Y(), point.Z()};
    }

    int countSubshapes(const TopoDS_Shape& shape, TopAbs_ShapeEnum type)
    {
        TopTools_IndexedMapOfShape map;
        TopExp::MapShapes(shape, type, map);
        return map.Extent();
    }

    int countAncestors(
        const TopoDS_Shape& root,
        const TopoDS_Shape& child,
        TopAbs_ShapeEnum childType,
        TopAbs_ShapeEnum ancestorType)
    {
        TopTools_IndexedDataMapOfShapeListOfShape map;
        TopExp::MapShapesAndAncestors(root, childType, ancestorType, map);
        if (!map.Contains(child)) return 0;
        return map.FindFromKey(child).Size();
    }

    void fillBounds(const TopoDS_Shape& shape, OcctBounds& result)
    {
        Bnd_Box box;
        BRepBndLib::AddOptimal(shape, box, Standard_True, Standard_False);
        if (box.IsVoid())
        {
            result = {};
            return;
        }
        box.Get(result.minX, result.minY, result.minZ, result.maxX, result.maxY, result.maxZ);
    }

    gp_Pnt centerAndMeasure(const TopoDS_Shape& shape, double& measure)
    {
        switch (shape.ShapeType())
        {
            case TopAbs_VERTEX:
                measure = 0.0;
                return BRep_Tool::Pnt(TopoDS::Vertex(shape));
            case TopAbs_EDGE:
            {
                GProp_GProps properties;
                BRepGProp::LinearProperties(shape, properties);
                measure = properties.Mass();
                return properties.CentreOfMass();
            }
            case TopAbs_FACE:
            {
                GProp_GProps properties;
                BRepGProp::SurfaceProperties(shape, properties);
                measure = properties.Mass();
                return properties.CentreOfMass();
            }
            default:
                throw std::invalid_argument("Persistent topology references support only Vertex, Edge, and Face shapes.");
        }
    }

    int findRuntimeIndex(const TopoDS_Shape& root, const TopoDS_Shape& shape)
    {
        TopTools_IndexedMapOfShape map;
        TopExp::MapShapes(root, shape.ShapeType(), map);
        const int oneBased = map.FindIndex(shape);
        return oneBased == 0 ? -1 : oneBased - 1;
    }

    bool isSupportedType(int type)
    {
        return type == OcctShape_Vertex || type == OcctShape_Edge || type == OcctShape_Face;
    }

    void buildReference(
        const TopoDS_Shape& root,
        const TopoDS_Shape& shape,
        OcctModelTopologyReference& result)
    {
        if (shape.IsNull()) throw std::invalid_argument("Referenced topology is null.");
        const int runtimeIndex = findRuntimeIndex(root, shape);
        if (runtimeIndex < 0)
            throw std::invalid_argument("Referenced topology is not contained in the supplied root shape.");

        const int shapeType = static_cast<int>(shape.ShapeType());
        if (!isSupportedType(shapeType))
            throw std::invalid_argument("Persistent topology references support only Vertex, Edge, and Face shapes.");

        result = {};
        result.version = ReferenceVersion;
        result.shapeType = shapeType;
        result.runtimeIndexHint = runtimeIndex;
        result.curveType = OcctCurve_Other;
        result.surfaceType = OcctSurface_Other;
        result.orientation = static_cast<int>(shape.Orientation());
        result.tolerance = maximumTolerance(shape);

        double measure = 0.0;
        const gp_Pnt center = centerAndMeasure(shape, measure);
        result.measure = measure;
        result.center = toNativePoint(center);
        fillBounds(shape, result.bounds);

        if (shape.ShapeType() == TopAbs_EDGE)
            result.curveType = static_cast<int>(BRepAdaptor_Curve(TopoDS::Edge(shape)).GetType());
        else if (shape.ShapeType() == TopAbs_FACE)
            result.surfaceType = static_cast<int>(BRepAdaptor_Surface(TopoDS::Face(shape), Standard_True).GetType());

        result.vertexCount = countSubshapes(shape, TopAbs_VERTEX);
        result.edgeCount = countSubshapes(shape, TopAbs_EDGE);
        result.faceCount = countSubshapes(shape, TopAbs_FACE);

        if (shape.ShapeType() == TopAbs_VERTEX)
        {
            result.edgeCount = countAncestors(root, shape, TopAbs_VERTEX, TopAbs_EDGE);
            result.faceCount = countAncestors(root, shape, TopAbs_VERTEX, TopAbs_FACE);
        }
        else if (shape.ShapeType() == TopAbs_EDGE)
        {
            result.faceCount = countAncestors(root, shape, TopAbs_EDGE, TopAbs_FACE);
        }
    }

    double distance(OcctPoint3d left, OcctPoint3d right)
    {
        const double dx = left.x - right.x;
        const double dy = left.y - right.y;
        const double dz = left.z - right.z;
        return std::sqrt(dx * dx + dy * dy + dz * dz);
    }

    double diagonal(const OcctBounds& bounds)
    {
        const double dx = bounds.maxX - bounds.minX;
        const double dy = bounds.maxY - bounds.minY;
        const double dz = bounds.maxZ - bounds.minZ;
        return std::sqrt(dx * dx + dy * dy + dz * dz);
    }

    double boundsDelta(const OcctBounds& left, const OcctBounds& right)
    {
        const double lx = left.maxX - left.minX;
        const double ly = left.maxY - left.minY;
        const double lz = left.maxZ - left.minZ;
        const double rx = right.maxX - right.minX;
        const double ry = right.maxY - right.minY;
        const double rz = right.maxZ - right.minZ;
        return std::abs(lx - rx) + std::abs(ly - ry) + std::abs(lz - rz);
    }

    double similarity(double difference, double scale, double tolerance)
    {
        const double normalizedScale = std::max(std::abs(scale), 1.0);
        const double allowed = std::max(normalizedScale * std::max(tolerance, 1e-12), Precision::Confusion());
        return 1.0 / (1.0 + difference / allowed);
    }

    double adjacencySimilarity(
        const OcctModelTopologyReference& reference,
        const OcctModelTopologyReference& candidate)
    {
        int matches = 0;
        matches += reference.vertexCount == candidate.vertexCount ? 1 : 0;
        matches += reference.edgeCount == candidate.edgeCount ? 1 : 0;
        matches += reference.faceCount == candidate.faceCount ? 1 : 0;
        return static_cast<double>(matches) / 3.0;
    }

    double scoreReference(
        const OcctModelTopologyReference& reference,
        const OcctModelTopologyReference& candidate,
        double rootDiagonal,
        double matchingTolerance,
        bool runtimeIndexMatched)
    {
        if (reference.shapeType != candidate.shapeType) return -1.0;

        double score = 0.20;
        if (reference.shapeType == OcctShape_Edge)
            score += reference.curveType == candidate.curveType ? 0.16 : 0.0;
        else if (reference.shapeType == OcctShape_Face)
            score += reference.surfaceType == candidate.surfaceType ? 0.16 : 0.0;
        else
            score += 0.16;

        const double measureScale = std::max(std::abs(reference.measure), 1.0);
        score += 0.20 * similarity(std::abs(reference.measure - candidate.measure), measureScale, matchingTolerance);
        score += 0.20 * similarity(distance(reference.center, candidate.center), std::max(rootDiagonal, 1.0), matchingTolerance);
        score += 0.12 * similarity(
            boundsDelta(reference.bounds, candidate.bounds),
            std::max(diagonal(reference.bounds), 1.0),
            matchingTolerance);
        score += 0.08 * adjacencySimilarity(reference, candidate);
        score += reference.orientation == candidate.orientation ? 0.02 : 0.0;
        score += runtimeIndexMatched ? 0.02 : 0.0;
        return std::clamp(score, 0.0, 1.0);
    }

    bool containsSame(const std::vector<TopoDS_Shape>& values, const TopoDS_Shape& shape)
    {
        return std::any_of(values.begin(), values.end(), [&](const TopoDS_Shape& current) { return current.IsSame(shape); });
    }

    void appendHistoryShapes(
        std::vector<TopoDS_Shape>& target,
        const TopTools_ListOfShape& source,
        TopAbs_ShapeEnum requiredType)
    {
        for (TopTools_ListIteratorOfListOfShape iterator(source); iterator.More(); iterator.Next())
        {
            const TopoDS_Shape& shape = iterator.Value();
            if (shape.ShapeType() == requiredType && !containsSame(target, shape))
                target.push_back(shape);
        }
    }

    std::vector<TopoDS_Shape> rootCandidates(const TopoDS_Shape& root, TopAbs_ShapeEnum type)
    {
        TopTools_IndexedMapOfShape map;
        TopExp::MapShapes(root, type, map);
        std::vector<TopoDS_Shape> result;
        result.reserve(static_cast<std::size_t>(map.Extent()));
        for (int index = 1; index <= map.Extent(); ++index)
            result.push_back(map(index));
        return result;
    }

    void resolveCandidates(
        ModelSession* model,
        const TopoDS_Shape& root,
        const OcctModelTopologyReference& reference,
        double matchingTolerance,
        const std::vector<TopoDS_Shape>& shapes,
        bool usedHistory,
        OcctModelTopologyReferenceResult& result)
    {
        OcctModelTopologyReference rootReference{};
        buildReference(root, root.ShapeType() == toShapeEnum(reference.shapeType) ? root : shapes.front(), rootReference);
        OcctBounds rootBounds{};
        fillBounds(root, rootBounds);
        const double rootDiagonal = diagonal(rootBounds);

        std::vector<Candidate> candidates;
        candidates.reserve(shapes.size());
        for (const TopoDS_Shape& shape : shapes)
        {
            const int runtimeIndex = findRuntimeIndex(root, shape);
            if (runtimeIndex < 0) continue;

            OcctModelTopologyReference fingerprint{};
            buildReference(root, shape, fingerprint);
            const bool indexMatched = runtimeIndex == reference.runtimeIndexHint;
            const double score = scoreReference(reference, fingerprint, rootDiagonal, matchingTolerance, indexMatched);
            candidates.push_back({shape, runtimeIndex, score});
        }

        if (candidates.empty())
        {
            result = {OcctModelTopologyReference_NotFound, 0, 0.0, 0, usedHistory ? 1 : 0, 0};
            return;
        }

        std::sort(candidates.begin(), candidates.end(), [](const Candidate& left, const Candidate& right)
        {
            if (left.score != right.score) return left.score > right.score;
            return left.index < right.index;
        });

        const Candidate& best = candidates.front();
        if (best.score < MinimumResolvedScore)
        {
            result = {OcctModelTopologyReference_NotFound, 0, best.score, static_cast<int>(candidates.size()), usedHistory ? 1 : 0, best.index == reference.runtimeIndexHint ? 1 : 0};
            return;
        }

        if (candidates.size() > 1 && (best.score - candidates[1].score) < AmbiguityMargin)
        {
            result = {OcctModelTopologyReference_Ambiguous, 0, best.score, static_cast<int>(candidates.size()), usedHistory ? 1 : 0, best.index == reference.runtimeIndexHint ? 1 : 0};
            return;
        }

        result = {
            OcctModelTopologyReference_Resolved,
            model->addShape(best.shape),
            best.score,
            static_cast<int>(candidates.size()),
            usedHistory ? 1 : 0,
            best.index == reference.runtimeIndexHint ? 1 : 0};
    }

    bool validateReference(const OcctModelTopologyReference& reference, double matchingTolerance)
    {
        return reference.version == ReferenceVersion &&
            isSupportedType(reference.shapeType) &&
            matchingTolerance >= 0.0 &&
            std::isfinite(matchingTolerance);
    }
}

extern "C"
{
    int occt_model_create_topology_reference(
        OcctModelHandle handle,
        OcctObjectId rootShapeId,
        OcctObjectId subshapeId,
        OcctModelTopologyReference* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            buildReference(model->requireShape(rootShapeId), model->requireShape(subshapeId), *result);
        });
    }

    int occt_model_resolve_topology_reference(
        OcctModelHandle handle,
        OcctObjectId rootShapeId,
        const OcctModelTopologyReference* reference,
        double matchingTolerance,
        OcctModelTopologyReferenceResult* result)
    {
        ModelSession* model = modelOf(handle);
        if (reference == nullptr || result == nullptr) return 0;
        return execute(model, [&]
        {
            if (!validateReference(*reference, matchingTolerance))
            {
                *result = {OcctModelTopologyReference_Invalid, 0, 0.0, 0, 0, 0};
                return;
            }
            const TopoDS_Shape& root = model->requireShape(rootShapeId);
            const auto candidates = rootCandidates(root, toShapeEnum(reference->shapeType));
            resolveCandidates(model, root, *reference, matchingTolerance, candidates, false, *result);
        });
    }

    int occt_model_resolve_topology_reference_with_history(
        OcctModelHandle handle,
        OcctObjectId rootShapeId,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        const OcctModelTopologyReference* reference,
        double matchingTolerance,
        OcctModelTopologyReferenceResult* result)
    {
        ModelSession* model = modelOf(handle);
        if (reference == nullptr || result == nullptr) return 0;
        return execute(model, [&]
        {
            if (!validateReference(*reference, matchingTolerance))
            {
                *result = {OcctModelTopologyReference_Invalid, 0, 0.0, 0, 0, 0};
                return;
            }

            const TopoDS_Shape& root = model->requireShape(rootShapeId);
            const TopoDS_Shape& source = model->requireShape(sourceShapeId);
            const OperationRecord& operation = requireOperation(model, operationId);
            if (!operation.history.IsNull() && operation.history->IsRemoved(source))
            {
                *result = {OcctModelTopologyReference_Removed, 0, 1.0, 0, 1, 0};
                return;
            }

            std::vector<TopoDS_Shape> historyCandidates;
            if (!operation.history.IsNull())
            {
                const TopAbs_ShapeEnum requiredType = toShapeEnum(reference->shapeType);
                appendHistoryShapes(historyCandidates, operation.history->Generated(source), requiredType);
                appendHistoryShapes(historyCandidates, operation.history->Modified(source), requiredType);
            }

            if (!historyCandidates.empty())
            {
                resolveCandidates(model, root, *reference, matchingTolerance, historyCandidates, true, *result);
                return;
            }

            const auto candidates = rootCandidates(root, toShapeEnum(reference->shapeType));
            resolveCandidates(model, root, *reference, matchingTolerance, candidates, false, *result);
        });
    }
}
