#include "topology/OcctModelingTopologyAnalysis.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRep_Builder.hxx>
#include <ShapeAnalysis_FreeBounds.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>
#include <TopoDS_Compound.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_shape_free_bounds(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double tolerance,
        int boundaryKind,
        OcctBool splitClosed,
        OcctBool splitOpen,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (!std::isfinite(tolerance) || tolerance <= 0.0)
                throw std::invalid_argument("Free-boundary tolerance must be finite and greater than zero.");
            if (boundaryKind != OcctModelFreeBoundary_Closed && boundaryKind != OcctModelFreeBoundary_Open)
                throw std::invalid_argument("Free-boundary kind is invalid.");

            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRep_Builder builder;
            TopoDS_Compound faces;
            builder.MakeCompound(faces);

            int faceCount = 0;
            if (source.ShapeType() == TopAbs_FACE)
            {
                builder.Add(faces, source);
                faceCount = 1;
            }
            else
            {
                for (TopExp_Explorer explorer(source, TopAbs_FACE); explorer.More(); explorer.Next())
                {
                    builder.Add(faces, explorer.Current());
                    ++faceCount;
                }
            }

            if (faceCount == 0)
                throw std::invalid_argument("Free-boundary analysis requires a shape containing at least one face.");

            ShapeAnalysis_FreeBounds analysis(
                faces,
                tolerance,
                splitClosed != 0 ? Standard_True : Standard_False,
                splitOpen != 0 ? Standard_True : Standard_False);

            return boundaryKind == OcctModelFreeBoundary_Closed
                ? TopoDS_Shape(analysis.GetClosedWires())
                : TopoDS_Shape(analysis.GetOpenWires());
        });
    }

    OcctStatus occt_model_shape_edge_adjacency_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelEdgeAdjacency* items,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& root = model->requireShape(shapeId);
            TopTools_IndexedMapOfShape edges;
            TopExp::MapShapes(root, TopAbs_EDGE, edges);
            *required = edges.Extent();

            if (items == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null edge-adjacency buffer requires zero capacity.");
                return;
            }
            if (capacity < *required)
                throw std::out_of_range("Edge-adjacency buffer capacity is too small.");

            TopTools_IndexedDataMapOfShapeListOfShape edgeFaces;
            TopExp::MapShapesAndUniqueAncestors(root, TopAbs_EDGE, TopAbs_FACE, edgeFaces, Standard_False);

            for (int index = 1; index <= edges.Extent(); ++index)
            {
                const TopoDS_Shape& edge = edges(index);
                items[index - 1].edgeId = model->addShape(edge);
                items[index - 1].adjacentFaceCount = edgeFaces.Contains(edge)
                    ? edgeFaces.FindFromKey(edge).Size()
                    : 0;
            }
        });
    }
}
