#include "OcctModelingInternal.hxx"
#include "OcctModelingTopologyAnalysis.h"

#include <ShapeAnalysis_FreeBounds.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctObjectId occt_model_shape_free_bounds(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        double tolerance,
        int boundaryKind,
        int splitClosed,
        int splitOpen)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (!std::isfinite(tolerance) || tolerance <= 0.0)
                throw std::invalid_argument("Free-boundary tolerance must be finite and greater than zero.");
            if (boundaryKind != OcctModelFreeBoundary_Closed && boundaryKind != OcctModelFreeBoundary_Open)
                throw std::invalid_argument("Free-boundary kind is invalid.");

            ShapeAnalysis_FreeBounds analysis(
                model->requireShape(shapeId),
                tolerance,
                splitClosed != 0 ? Standard_True : Standard_False,
                splitOpen != 0 ? Standard_True : Standard_False);

            if (boundaryKind == OcctModelFreeBoundary_Closed)
                return TopoDS_Shape(analysis.GetClosedWires());
            return TopoDS_Shape(analysis.GetOpenWires());
        });
    }
}
