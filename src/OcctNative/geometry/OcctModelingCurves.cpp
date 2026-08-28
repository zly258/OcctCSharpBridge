#include "geometry/OcctModelingCurves.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepLib.hxx>
#include <BRepBuilderAPI_MakeVertex.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <Geom2d_Line.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_Circle.hxx>
#include <Geom_Ellipse.hxx>
#include <Geom_CylindricalSurface.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <TColgp_Array1OfPnt.hxx>
#include <TColgp_HArray1OfPnt.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{

    OcctStatus occt_model_curve_helix_create(OcctModelingSessionHandle handle, OcctPoint3d origin, OcctVector3d axis, OcctVector3d xDirection, double radius, double pitch, double turns, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(radius, "Radius"); requirePositive(turns, "Turns");
            if (!std::isfinite(pitch) || std::abs(pitch) <= Precision::Confusion())
                throw std::invalid_argument("Pitch must be finite and non-zero.");
            const gp_Ax3 placement(toPoint(origin), toDirection(axis), toDirection(xDirection));
            Handle(Geom_CylindricalSurface) surface = new Geom_CylindricalSurface(placement, radius);
            const double angle = 2.0 * 3.14159265358979323846 * turns;
            const double height = pitch * turns;
            const double length2d = std::hypot(angle, height);
            Handle(Geom2d_Line) pcurve = new Geom2d_Line(gp_Pnt2d(0.0, 0.0), gp_Dir2d(angle / length2d, height / length2d));
            BRepBuilderAPI_MakeEdge maker(pcurve, surface, 0.0, length2d);
            if (!maker.IsDone()) throw std::runtime_error("Helix edge creation failed.");
            TopoDS_Edge edge = maker.Edge();
            if (!BRepLib::BuildCurve3d(edge)) throw std::runtime_error("Helix 3D curve construction failed.");
            return TopoDS_Shape(edge);
        });
    }

    OcctStatus occt_model_curve_vertex_create(OcctModelingSessionHandle handle, OcctPoint3d pointValue, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            BRepBuilderAPI_MakeVertex maker(toPoint(pointValue));
            if (!maker.IsDone()) throw std::runtime_error("Vertex creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_curve_line_create(OcctModelingSessionHandle handle, OcctPoint3d start, OcctPoint3d end, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (toPoint(start).Distance(toPoint(end)) <= Precision::Confusion())
                throw std::invalid_argument("Line endpoints must be different.");
            BRepBuilderAPI_MakeEdge maker(toPoint(start), toPoint(end));
            if (!maker.IsDone()) throw std::runtime_error("Line creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_curve_polyline_create(OcctModelingSessionHandle handle, const OcctPoint3d* points, int count, OcctBool closed, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requireCount(count, closed != 0 ? 3 : 2, "Polyline");
            if (points == nullptr) throw std::invalid_argument("Point array is null.");
            BRepBuilderAPI_MakePolygon maker;
            for (int index = 0; index < count; ++index) maker.Add(toPoint(points[index]));
            if (closed != 0) maker.Close();
            if (!maker.IsDone()) throw std::runtime_error("Polyline creation failed.");
            return maker.Wire();
        });
    }

    OcctStatus occt_model_curve_circle_create(OcctModelingSessionHandle handle, OcctPoint3d center, OcctVector3d normal, double radius, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(radius, "Radius");
            Handle(Geom_Circle) curve = new Geom_Circle(toAxis2(center, normal), radius);
            BRepBuilderAPI_MakeEdge maker(curve);
            if (!maker.IsDone()) throw std::runtime_error("Circle creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_curve_arc_three_points_create(OcctModelingSessionHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            GC_MakeArcOfCircle arc(toPoint(start), toPoint(middle), toPoint(end));
            if (!arc.IsDone()) throw std::runtime_error("Arc construction failed.");
            BRepBuilderAPI_MakeEdge maker(arc.Value());
            if (!maker.IsDone()) throw std::runtime_error("Arc edge creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_curve_arc_center_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(radius, "Radius");
            const gp_Ax2 axis(toPoint(center), toDirection(normal), toDirection(xDirection));
            Handle(Geom_Circle) circle = new Geom_Circle(axis, radius);
            const double start = startAngleDegrees * 3.14159265358979323846 / 180.0;
            const double end = endAngleDegrees * 3.14159265358979323846 / 180.0;
            Handle(Geom_TrimmedCurve) arc = new Geom_TrimmedCurve(circle, start, end, Standard_True);
            BRepBuilderAPI_MakeEdge maker(arc);
            if (!maker.IsDone()) throw std::runtime_error("Arc edge creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_curve_ellipse_create(OcctModelingSessionHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(majorRadius, "Major radius");
            requirePositive(minorRadius, "Minor radius");
            if (majorRadius < minorRadius) throw std::invalid_argument("Major radius must be greater than or equal to minor radius.");
            Handle(Geom_Ellipse) curve = new Geom_Ellipse(toAxis2(center, normal), majorRadius, minorRadius);
            BRepBuilderAPI_MakeEdge maker(curve);
            if (!maker.IsDone()) throw std::runtime_error("Ellipse creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_curve_bezier_create(OcctModelingSessionHandle handle, const OcctPoint3d* poles, int count, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requireCount(count, 2, "Bezier curve");
            if (poles == nullptr) throw std::invalid_argument("Pole array is null.");
            TColgp_Array1OfPnt array(1, count);
            for (int index = 0; index < count; ++index) array.SetValue(index + 1, toPoint(poles[index]));
            Handle(Geom_BezierCurve) curve = new Geom_BezierCurve(array);
            BRepBuilderAPI_MakeEdge maker(curve);
            if (!maker.IsDone()) throw std::runtime_error("Bezier creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_curve_bspline_interpolated_create(OcctModelingSessionHandle handle, const OcctPoint3d* points, int count, OcctBool periodic, double tolerance, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requireCount(count, periodic != 0 ? 3 : 2, "B-spline");
            requirePositive(tolerance, "Tolerance");
            if (points == nullptr) throw std::invalid_argument("Point array is null.");
            Handle(TColgp_HArray1OfPnt) array = new TColgp_HArray1OfPnt(1, count);
            for (int index = 0; index < count; ++index) array->SetValue(index + 1, toPoint(points[index]));
            GeomAPI_Interpolate interpolation(array, periodic != 0, tolerance);
            interpolation.Perform();
            if (!interpolation.IsDone()) throw std::runtime_error("B-spline interpolation failed.");
            BRepBuilderAPI_MakeEdge maker(interpolation.Curve());
            if (!maker.IsDone()) throw std::runtime_error("B-spline edge creation failed.");
            return maker.Shape();
        });
    }
}
