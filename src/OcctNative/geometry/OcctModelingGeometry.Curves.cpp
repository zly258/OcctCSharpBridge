#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeVertex.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_Circle.hxx>
#include <Geom_Ellipse.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <TColgp_Array1OfPnt.hxx>
#include <TColgp_HArray1OfPnt.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctObjectId occt_model_make_vertex(OcctModelHandle handle, OcctPoint3d pointValue)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            BRepBuilderAPI_MakeVertex maker(toPoint(pointValue));
            if (!maker.IsDone()) throw std::runtime_error("Vertex creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_line(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d end)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (toPoint(start).Distance(toPoint(end)) <= Precision::Confusion())
                throw std::invalid_argument("Line endpoints must be different.");
            BRepBuilderAPI_MakeEdge maker(toPoint(start), toPoint(end));
            if (!maker.IsDone()) throw std::runtime_error("Line creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_polyline(OcctModelHandle handle, const OcctPoint3d* points, int count, int closed)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requireCount(count, 2, "Polyline");
            if (points == nullptr) throw std::invalid_argument("Point array is null.");
            BRepBuilderAPI_MakePolygon maker;
            for (int index = 0; index < count; ++index) maker.Add(toPoint(points[index]));
            if (closed != 0) maker.Close();
            if (!maker.IsDone()) throw std::runtime_error("Polyline creation failed.");
            return maker.Wire();
        });
    }

    OcctObjectId occt_model_make_circle(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double radius)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(radius, "Radius");
            Handle(Geom_Circle) curve = new Geom_Circle(toAxis2(center, normal), radius);
            BRepBuilderAPI_MakeEdge maker(curve);
            if (!maker.IsDone()) throw std::runtime_error("Circle creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_arc_three_points(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            GC_MakeArcOfCircle arc(toPoint(start), toPoint(middle), toPoint(end));
            if (!arc.IsDone()) throw std::runtime_error("Arc construction failed.");
            BRepBuilderAPI_MakeEdge maker(arc.Value());
            if (!maker.IsDone()) throw std::runtime_error("Arc edge creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_arc_center(
        OcctModelHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
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

    OcctObjectId occt_model_make_ellipse(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
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

    OcctObjectId occt_model_make_bezier(OcctModelHandle handle, const OcctPoint3d* poles, int count)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
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

    OcctObjectId occt_model_make_bspline_interpolated(OcctModelHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requireCount(count, 2, "B-spline");
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
