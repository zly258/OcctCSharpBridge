#include "OcctModelingInternal.hxx"

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

    OcctObjectId occt_model_make_regular_polygon(
        OcctModelHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        int sideCount,
        int makeFace)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(radius, "Radius");
            requireCount(sideCount, 3, "Polygon");
            const gp_Ax2 axis(toPoint(center), toDirection(normal), toDirection(xDirection));
            BRepBuilderAPI_MakePolygon polygon;
            for (int index = 0; index < sideCount; ++index)
            {
                const double angle = 2.0 * 3.14159265358979323846 * static_cast<double>(index) / static_cast<double>(sideCount);
                const gp_Vec radial = gp_Vec(axis.XDirection()) * (radius * std::cos(angle))
                    + gp_Vec(axis.YDirection()) * (radius * std::sin(angle));
                polygon.Add(axis.Location().Translated(radial));
            }
            polygon.Close();
            if (!polygon.IsDone()) throw std::runtime_error("Regular polygon creation failed.");
            if (makeFace == 0) return TopoDS_Shape(polygon.Wire());
            BRepBuilderAPI_MakeFace faceMaker(polygon.Wire(), Standard_True);
            if (!faceMaker.IsDone()) throw std::runtime_error("Regular polygon face creation failed.");
            return TopoDS_Shape(faceMaker.Face());
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

    OcctObjectId occt_model_make_rectangle_wire(
        OcctModelHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&] { return TopoDS_Shape(modelRectangleWire(origin, xDirection, normal, width, height)); });
    }

    OcctObjectId occt_model_make_plane_face(
        OcctModelHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            BRepBuilderAPI_MakeFace maker(modelRectangleWire(origin, xDirection, normal, width, height), Standard_True);
            if (!maker.IsDone()) throw std::runtime_error("Planar face creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_face_from_wire(OcctModelHandle handle, OcctObjectId wireId, int onlyPlane)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(wireId);
            if (shape.ShapeType() != TopAbs_WIRE) throw std::invalid_argument("Input must be a wire.");
            BRepBuilderAPI_MakeFace maker(TopoDS::Wire(shape), onlyPlane != 0);
            if (!maker.IsDone()) throw std::runtime_error("Face creation failed.");
            return maker.Face();
        });
    }

    OcctObjectId occt_model_make_box(OcctModelHandle handle, double x, double y, double z, double dx, double dy, double dz)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(dx, "Box X size");
            requirePositive(dy, "Box Y size");
            requirePositive(dz, "Box Z size");
            BRepPrimAPI_MakeBox maker(gp_Pnt(x, y, z), dx, dy, dz);
            if (!maker.IsDone()) throw std::runtime_error("Box creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_cylinder(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(radius, "Radius");
            requirePositive(height, "Height");
            BRepPrimAPI_MakeCylinder maker(toAxis2(origin, axis), radius, height);
            if (!maker.IsDone()) throw std::runtime_error("Cylinder creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_cone(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (radius1 < 0.0 || radius2 < 0.0 || radius1 + radius2 <= 0.0)
                throw std::invalid_argument("Cone radii are invalid.");
            requirePositive(height, "Height");
            BRepPrimAPI_MakeCone maker(toAxis2(origin, axis), radius1, radius2, height);
            if (!maker.IsDone()) throw std::runtime_error("Cone creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_sphere(OcctModelHandle handle, OcctPoint3d center, double radius)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(radius, "Radius");
            BRepPrimAPI_MakeSphere maker(toPoint(center), radius);
            if (!maker.IsDone()) throw std::runtime_error("Sphere creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_torus(OcctModelHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(majorRadius, "Major radius");
            requirePositive(minorRadius, "Minor radius");
            if (minorRadius >= majorRadius) throw std::invalid_argument("Minor radius must be less than major radius.");
            BRepPrimAPI_MakeTorus maker(toAxis2(center, axis), majorRadius, minorRadius);
            if (!maker.IsDone()) throw std::runtime_error("Torus creation failed.");
            return maker.Shape();
        });
    }

    OcctObjectId occt_model_make_wedge(OcctModelHandle handle, double dx, double dy, double dz, double ltx)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(dx, "Wedge X size");
            requirePositive(dy, "Wedge Y size");
            requirePositive(dz, "Wedge Z size");
            return BRepPrimAPI_MakeWedge(dx, dy, dz, ltx).Shape();
        });
    }

    OcctObjectId occt_model_make_compound(OcctModelHandle handle, const OcctObjectId* shapeIds, int count)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requireCount(count, 1, "Compound");
            if (shapeIds == nullptr) throw std::invalid_argument("Shape ID array is null.");
            BRep_Builder builder;
            TopoDS_Compound compound;
            builder.MakeCompound(compound);
            for (int index = 0; index < count; ++index) builder.Add(compound, model->requireShape(shapeIds[index]));
            return compound;
        });
    }

    OcctObjectId occt_model_make_wire(OcctModelHandle handle, const OcctObjectId* edgeIds, int count)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requireCount(count, 1, "Wire");
            if (edgeIds == nullptr) throw std::invalid_argument("Edge ID array is null.");
            BRepBuilderAPI_MakeWire maker;
            for (int index = 0; index < count; ++index)
            {
                const TopoDS_Shape& edge = model->requireShape(edgeIds[index]);
                if (edge.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Wire inputs must be edges.");
                maker.Add(TopoDS::Edge(edge));
            }
            if (!maker.IsDone()) throw std::runtime_error("Wire creation failed.");
            return maker.Wire();
        });
    }

    OcctObjectId occt_model_make_solid_from_shell(OcctModelHandle handle, OcctObjectId shellId)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const TopoDS_Shape& shell = model->requireShape(shellId);
            if (shell.ShapeType() != TopAbs_SHELL) throw std::invalid_argument("Input must be a shell.");
            BRepBuilderAPI_MakeSolid maker(TopoDS::Shell(shell));
            if (!maker.IsDone()) throw std::runtime_error("Solid creation failed.");
            return maker.Solid();
        });
    }

    OcctObjectId occt_model_translate(OcctModelHandle handle, OcctObjectId shapeId, OcctVector3d vectorValue)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            gp_Trsf transform;
            transform.SetTranslation(toVector(vectorValue));
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Translation failed.");
            return algorithm.Shape();
        });
    }

    OcctObjectId occt_model_rotate(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (std::abs(angleDegrees) <= Precision::Angular()) throw std::invalid_argument("Rotation angle must not be zero.");
            gp_Trsf transform;
            transform.SetRotation(gp_Ax1(toPoint(axisPoint), toDirection(axisDirection)), angleDegrees * 3.14159265358979323846 / 180.0);
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Rotation failed.");
            return algorithm.Shape();
        });
    }

    OcctObjectId occt_model_scale(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (std::abs(factor) <= Precision::Confusion()) throw std::invalid_argument("Scale factor must not be zero.");
            gp_Trsf transform;
            transform.SetScale(toPoint(center), factor);
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Scaling failed.");
            return algorithm.Shape();
        });
    }

    OcctObjectId occt_model_mirror_plane(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            gp_Trsf transform;
            transform.SetMirror(gp_Ax2(toPoint(planePoint), toDirection(planeNormal)));
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Mirror operation failed.");
            return algorithm.Shape();
        });
    }
}
