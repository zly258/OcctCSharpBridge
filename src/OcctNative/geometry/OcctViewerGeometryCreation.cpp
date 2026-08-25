#include "geometry/OcctViewerGeometryCreation.h"
#include "core/OcctInternal.hxx"

#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeSolid.hxx>
#include <BRepBuilderAPI_MakeVertex.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_Sewing.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepPrimAPI_MakeWedge.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_Circle.hxx>
#include <Geom_Ellipse.hxx>
#include <Precision.hxx>
#include <TColgp_Array1OfPnt.hxx>
#include <TColgp_HArray1OfPnt.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Shell.hxx>
#include <TopoDS_Wire.hxx>
#include <gp_Circ.hxx>

#include <cmath>
#include <stdexcept>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus createViewerShape(Engine* engine, OcctObjectId* result, Function&& function)
    {
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = 0;
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        const int ok = execute(engine, [&]
        {
            *result = function();
            if (*result <= 0) throw std::runtime_error("Geometry operation did not create a viewer object.");
        });
        return ok != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr || entry->shape.IsNull())
            throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }

    std::vector<gp_Pnt> pointsFrom(const OcctPoint3d* points, int count)
    {
        if (points == nullptr) throw std::invalid_argument("Point array is null.");
        std::vector<gp_Pnt> result;
        result.reserve(static_cast<std::size_t>(count));
        for (int index = 0; index < count; ++index) result.push_back(point(points[index]));
        return result;
    }

    void hideInputs(Engine* engine, const OcctObjectId* ids, int count)
    {
        for (int index = 0; index < count; ++index) engine->hide(ids[index]);
    }

    TopoDS_Wire rectangleWire(
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height)
    {
        requirePositive(width, "Width");
        requirePositive(height, "Height");
        const gp_Ax2 plane = axis2(origin, normal, xDirection);
        const gp_Pnt p0 = plane.Location();
        const gp_Vec xVector(plane.XDirection());
        const gp_Vec yVector(plane.YDirection());
        const gp_Pnt p1 = p0.Translated(xVector * width);
        const gp_Pnt p2 = p1.Translated(yVector * height);
        const gp_Pnt p3 = p0.Translated(yVector * height);
        BRepBuilderAPI_MakePolygon polygon;
        polygon.Add(p0);
        polygon.Add(p1);
        polygon.Add(p2);
        polygon.Add(p3);
        polygon.Close();
        if (!polygon.IsDone()) throw std::runtime_error("Rectangle wire creation failed.");
        return polygon.Wire();
    }
}

extern "C"
{
    OcctStatus occt_engine_shape_vertex_create(
        OcctEngineHandle handle,
        OcctPoint3d value,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            return engine->addShape(BRepBuilderAPI_MakeVertex(point(value)).Shape(), false, "Vertex");
        });
    }

    OcctStatus occt_engine_shape_line_create(
        OcctEngineHandle handle,
        OcctPoint3d start,
        OcctPoint3d end,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            if (point(start).Distance(point(end)) <= Precision::Confusion())
                throw std::invalid_argument("Line endpoints must be different.");
            BRepBuilderAPI_MakeEdge maker(point(start), point(end));
            if (!maker.IsDone()) throw std::runtime_error("Line creation failed.");
            return engine->addShape(maker.Shape(), false, "Line");
        });
    }

    OcctStatus occt_engine_shape_polyline_create(
        OcctEngineHandle handle,
        const OcctPoint3d* input,
        int count,
        OcctBool closed,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requireCount(count, closed != 0 ? 3 : 2, "Polyline");
            const auto points = pointsFrom(input, count);
            BRepBuilderAPI_MakePolygon maker;
            for (const gp_Pnt& value : points) maker.Add(value);
            if (closed != 0) maker.Close();
            if (!maker.IsDone()) throw std::runtime_error("Polyline creation failed.");
            return engine->addShape(maker.Wire(), false, closed != 0 ? "Polygon" : "Polyline");
        });
    }

    OcctStatus occt_engine_shape_circle_create(
        OcctEngineHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        double radius,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(radius, "Radius");
            Handle(Geom_Circle) curve = new Geom_Circle(axis2(center, normal), radius);
            BRepBuilderAPI_MakeEdge maker(curve);
            if (!maker.IsDone()) throw std::runtime_error("Circle creation failed.");
            return engine->addShape(maker.Shape(), false, "Circle");
        });
    }

    OcctStatus occt_engine_shape_arc_three_points_create(
        OcctEngineHandle handle,
        OcctPoint3d start,
        OcctPoint3d middle,
        OcctPoint3d end,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            GC_MakeArcOfCircle arc(point(start), point(middle), point(end));
            if (!arc.IsDone()) throw std::runtime_error("Arc creation failed.");
            BRepBuilderAPI_MakeEdge maker(arc.Value());
            if (!maker.IsDone()) throw std::runtime_error("Arc edge creation failed.");
            return engine->addShape(maker.Shape(), false, "Arc");
        });
    }

    OcctStatus occt_engine_shape_arc_center_create(
        OcctEngineHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(radius, "Radius");
            if (!std::isfinite(startAngleDegrees) || !std::isfinite(endAngleDegrees))
                throw std::invalid_argument("Arc angles must be finite.");
            const double start = startAngleDegrees * 3.14159265358979323846 / 180.0;
            const double end = endAngleDegrees * 3.14159265358979323846 / 180.0;
            if (std::abs(end - start) <= Precision::Angular())
                throw std::invalid_argument("Arc angle must not be zero.");
            BRepBuilderAPI_MakeEdge maker(gp_Circ(axis2(center, normal, xDirection), radius), start, end);
            if (!maker.IsDone()) throw std::runtime_error("Arc creation failed.");
            return engine->addShape(maker.Edge(), false, "Arc");
        });
    }

    OcctStatus occt_engine_shape_ellipse_create(
        OcctEngineHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        double majorRadius,
        double minorRadius,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(majorRadius, "Major radius");
            requirePositive(minorRadius, "Minor radius");
            if (majorRadius < minorRadius)
                throw std::invalid_argument("Major radius must be greater than or equal to minor radius.");
            Handle(Geom_Ellipse) curve = new Geom_Ellipse(axis2(center, normal), majorRadius, minorRadius);
            BRepBuilderAPI_MakeEdge maker(curve);
            if (!maker.IsDone()) throw std::runtime_error("Ellipse edge creation failed.");
            return engine->addShape(maker.Shape(), false, "Ellipse");
        });
    }

    OcctStatus occt_engine_shape_bezier_create(
        OcctEngineHandle handle,
        const OcctPoint3d* input,
        int count,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requireCount(count, 2, "Bezier curve");
            if (input == nullptr) throw std::invalid_argument("Pole array is null.");
            TColgp_Array1OfPnt poles(1, count);
            for (int index = 0; index < count; ++index) poles.SetValue(index + 1, point(input[index]));
            Handle(Geom_BezierCurve) curve = new Geom_BezierCurve(poles);
            BRepBuilderAPI_MakeEdge maker(curve);
            if (!maker.IsDone()) throw std::runtime_error("Bezier edge creation failed.");
            return engine->addShape(maker.Shape(), false, "Bezier");
        });
    }

    OcctStatus occt_engine_shape_bspline_interpolated_create(
        OcctEngineHandle handle,
        const OcctPoint3d* input,
        int count,
        OcctBool periodic,
        double tolerance,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requireCount(count, periodic != 0 ? 3 : 2, "B-spline");
            requirePositive(tolerance, "Tolerance");
            if (input == nullptr) throw std::invalid_argument("Point array is null.");
            Handle(TColgp_HArray1OfPnt) points = new TColgp_HArray1OfPnt(1, count);
            for (int index = 0; index < count; ++index) points->SetValue(index + 1, point(input[index]));
            GeomAPI_Interpolate interpolation(points, periodic != 0, tolerance);
            interpolation.Perform();
            if (!interpolation.IsDone()) throw std::runtime_error("B-spline interpolation failed.");
            BRepBuilderAPI_MakeEdge maker(interpolation.Curve());
            if (!maker.IsDone()) throw std::runtime_error("B-spline edge creation failed.");
            return engine->addShape(maker.Shape(), false, "BSpline");
        });
    }

    OcctStatus occt_engine_shape_regular_polygon_create(
        OcctEngineHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        int sideCount,
        OcctBool makeFace,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(radius, "Radius");
            requireCount(sideCount, 3, "Polygon");
            const gp_Ax2 plane = axis2(center, normal, xDirection);
            const gp_Vec x(plane.XDirection());
            const gp_Vec y(plane.YDirection());
            const gp_Pnt c = point(center);
            BRepBuilderAPI_MakePolygon polygon;
            for (int index = 0; index < sideCount; ++index)
            {
                const double angle = 2.0 * 3.14159265358979323846 * static_cast<double>(index) / static_cast<double>(sideCount);
                polygon.Add(c.Translated(x * (radius * std::cos(angle)) + y * (radius * std::sin(angle))));
            }
            polygon.Close();
            if (!polygon.IsDone()) throw std::runtime_error("Regular polygon creation failed.");
            const TopoDS_Wire wire = polygon.Wire();
            if (makeFace == 0) return engine->addShape(wire, false, "RegularPolygon");
            BRepBuilderAPI_MakeFace face(wire, Standard_True);
            if (!face.IsDone()) throw std::runtime_error("Regular polygon face creation failed.");
            return engine->addShape(face.Face(), false, "RegularPolygonFace");
        });
    }

    OcctStatus occt_engine_shape_rectangle_wire_create(
        OcctEngineHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            return engine->addShape(rectangleWire(origin, xDirection, normal, width, height), false, "Rectangle");
        });
    }

    OcctStatus occt_engine_shape_face_from_wire_create(
        OcctEngineHandle handle,
        OcctObjectId wireId,
        OcctBool onlyPlane,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            const TopoDS_Shape wire = shapeWithPresentationTransformation(requiredShape(engine, wireId));
            if (wire.ShapeType() != TopAbs_WIRE) throw std::invalid_argument("Input must be a wire.");
            BRepBuilderAPI_MakeFace maker(TopoDS::Wire(wire), onlyPlane != 0);
            if (!maker.IsDone()) throw std::runtime_error("Face creation failed.");
            return engine->addShape(maker.Shape(), false, "Face");
        });
    }

    OcctStatus occt_engine_shape_plane_face_create(
        OcctEngineHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            BRepBuilderAPI_MakeFace maker(rectangleWire(origin, xDirection, normal, width, height), Standard_True);
            if (!maker.IsDone()) throw std::runtime_error("Planar face creation failed.");
            return engine->addShape(maker.Shape(), false, "PlaneFace");
        });
    }

    OcctStatus occt_engine_shape_box_create(
        OcctEngineHandle handle,
        double x,
        double y,
        double z,
        double dx,
        double dy,
        double dz,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(dx, "Box X size");
            requirePositive(dy, "Box Y size");
            requirePositive(dz, "Box Z size");
            return engine->addShape(BRepPrimAPI_MakeBox(gp_Pnt(x, y, z), dx, dy, dz).Shape(), true, "Box");
        });
    }

    OcctStatus occt_engine_shape_cylinder_create(
        OcctEngineHandle handle,
        OcctPoint3d origin,
        OcctVector3d axis,
        double radius,
        double height,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(radius, "Radius");
            requirePositive(height, "Height");
            return engine->addShape(BRepPrimAPI_MakeCylinder(axis2(origin, axis), radius, height).Shape(), true, "Cylinder");
        });
    }

    OcctStatus occt_engine_shape_sphere_create(
        OcctEngineHandle handle,
        OcctPoint3d center,
        double radius,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(radius, "Radius");
            return engine->addShape(BRepPrimAPI_MakeSphere(point(center), radius).Shape(), true, "Sphere");
        });
    }

    OcctStatus occt_engine_shape_cone_create(
        OcctEngineHandle handle,
        OcctPoint3d origin,
        OcctVector3d axis,
        double radius1,
        double radius2,
        double height,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            if (radius1 < 0.0 || radius2 < 0.0 || radius1 + radius2 <= 0.0)
                throw std::invalid_argument("Cone radii are invalid.");
            requirePositive(height, "Height");
            return engine->addShape(BRepPrimAPI_MakeCone(axis2(origin, axis), radius1, radius2, height).Shape(), true, "Cone");
        });
    }

    OcctStatus occt_engine_shape_torus_create(
        OcctEngineHandle handle,
        OcctPoint3d center,
        OcctVector3d axis,
        double majorRadius,
        double minorRadius,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(majorRadius, "Major radius");
            requirePositive(minorRadius, "Minor radius");
            if (minorRadius >= majorRadius)
                throw std::invalid_argument("Minor radius must be less than major radius.");
            return engine->addShape(BRepPrimAPI_MakeTorus(axis2(center, axis), majorRadius, minorRadius).Shape(), true, "Torus");
        });
    }

    OcctStatus occt_engine_shape_wedge_create(
        OcctEngineHandle handle,
        double dx,
        double dy,
        double dz,
        double ltx,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requirePositive(dx, "Wedge X size");
            requirePositive(dy, "Wedge Y size");
            requirePositive(dz, "Wedge Z size");
            if (!std::isfinite(ltx)) throw std::invalid_argument("Wedge ltx must be finite.");
            return engine->addShape(BRepPrimAPI_MakeWedge(dx, dy, dz, ltx).Shape(), true, "Wedge");
        });
    }

    OcctStatus occt_engine_shape_compound_create(
        OcctEngineHandle handle,
        const OcctObjectId* ids,
        int count,
        OcctBool hideInputsValue,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requireCount(count, 1, "Compound");
            if (ids == nullptr) throw std::invalid_argument("Shape ID array is null.");
            BRep_Builder builder;
            TopoDS_Compound compound;
            builder.MakeCompound(compound);
            for (int index = 0; index < count; ++index)
                builder.Add(compound, shapeWithPresentationTransformation(requiredShape(engine, ids[index])));
            const OcctObjectId created = engine->addShape(compound, false, "Compound");
            if (hideInputsValue != 0) hideInputs(engine, ids, count);
            return created;
        });
    }

    OcctStatus occt_engine_shape_wire_create(
        OcctEngineHandle handle,
        const OcctObjectId* ids,
        int count,
        OcctBool hideInputsValue,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requireCount(count, 1, "Wire");
            if (ids == nullptr) throw std::invalid_argument("Edge ID array is null.");
            BRepBuilderAPI_MakeWire maker;
            for (int index = 0; index < count; ++index)
            {
                const TopoDS_Shape edge = shapeWithPresentationTransformation(requiredShape(engine, ids[index]));
                if (edge.ShapeType() != TopAbs_EDGE)
                    throw std::invalid_argument("Wire inputs must be edges.");
                maker.Add(TopoDS::Edge(edge));
            }
            if (!maker.IsDone()) throw std::runtime_error("Wire assembly failed.");
            const OcctObjectId created = engine->addShape(maker.Wire(), false, "Wire");
            if (hideInputsValue != 0) hideInputs(engine, ids, count);
            return created;
        });
    }

    OcctStatus occt_engine_shape_sew(
        OcctEngineHandle handle,
        const OcctObjectId* ids,
        int count,
        double tolerance,
        OcctBool hideInputsValue,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            requireCount(count, 1, "Sewing");
            requirePositive(tolerance, "Tolerance");
            if (ids == nullptr) throw std::invalid_argument("Shape ID array is null.");
            BRepBuilderAPI_Sewing sewing(tolerance);
            for (int index = 0; index < count; ++index)
                sewing.Add(shapeWithPresentationTransformation(requiredShape(engine, ids[index])));
            sewing.Perform();
            const TopoDS_Shape sewn = sewing.SewedShape();
            if (sewn.IsNull()) throw std::runtime_error("Sewing failed.");
            const OcctObjectId created = engine->addShape(sewn, false, "SewnShape");
            if (hideInputsValue != 0) hideInputs(engine, ids, count);
            return created;
        });
    }

    OcctStatus occt_engine_shape_solid_from_shell_create(
        OcctEngineHandle handle,
        OcctObjectId shellId,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createViewerShape(engine, result, [&]
        {
            const TopoDS_Shape shell = shapeWithPresentationTransformation(requiredShape(engine, shellId));
            if (shell.ShapeType() != TopAbs_SHELL) throw std::invalid_argument("Input must be a shell.");
            BRepBuilderAPI_MakeSolid maker(TopoDS::Shell(shell));
            if (!maker.IsDone()) throw std::runtime_error("Solid creation failed.");
            const OcctObjectId created = engine->addShape(maker.Solid(), false, "Solid");
            if (hideInput != 0) engine->hide(shellId);
            return created;
        });
    }
}
