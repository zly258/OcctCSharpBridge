#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{
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
}
