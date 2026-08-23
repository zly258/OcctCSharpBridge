#include "geometry/OcctModelingCurveFit.h"
#include "modeling/OcctModelingShapeInternal.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRepBuilderAPI_MakeEdge.hxx>
#include <GeomAPI_PointsToBSpline.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Precision.hxx>
#include <TColgp_Array1OfPnt.hxx>

#include <cmath>

using namespace OcctModelingInternal;

namespace {
    Approx_ParametrizationType defaultParametrization() {
        return Approx_ChordLength;
    }

    GeomAbs_Shape continuityValue(int v) {
        switch (v) {
            case 0: return GeomAbs_C0;
            case 2: return GeomAbs_C2;
            case 3: return GeomAbs_G1;
            case 4: return GeomAbs_G2;
            default: return GeomAbs_C1;
        }
    }
}

extern "C" {
    OcctStatus occt_model_curve_fit_bspline(
        OcctModelingSessionHandle handle,
        const OcctPoint3d* points,
        int count,
        const OcctFitBSplineOptions* options,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]() -> TopoDS_Shape
        {
            constexpr uint32_t kApiVersion = 1;
            if (points == nullptr) throw std::invalid_argument("Point array is null.");
            if (count < 2) throw std::invalid_argument("At least 2 points are required.");
            if (options == nullptr) throw std::invalid_argument("Fit options are null.");
            if (options->structSize < sizeof(OcctFitBSplineOptions))
                throw std::invalid_argument("Unsupported fit options size.");
            if (options->apiVersion != kApiVersion)
                throw std::invalid_argument("Unsupported fit options API version.");

            int degMin = options->degMin > 0 ? options->degMin : 3;
            int degMax = options->degMax > 0 ? options->degMax : 8;
            if (degMin > degMax) throw std::invalid_argument("degMin must not exceed degMax.");
            double tol = options->tolerance > 0.0 ? options->tolerance : Precision::Confusion();

            TColgp_Array1OfPnt pts(1, count);
            for (int i = 0; i < count; ++i)
                pts.SetValue(i + 1, gp_Pnt(points[i].x, points[i].y, points[i].z));

            GeomAPI_PointsToBSpline fitter(pts, degMin, degMax, continuityValue(options->continuity), tol);
            if (!fitter.IsDone()) throw std::runtime_error("BSpline fitting failed.");

            BRepBuilderAPI_MakeEdge edgeMaker(fitter.Curve());
            if (!edgeMaker.IsDone()) throw std::runtime_error("Failed to create fitted BSpline edge.");
            return edgeMaker.Edge();
        });
    }
}
