#include "geometry/OcctModelingPlanar.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_planar_regular_polygon_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        int sideCount,
        OcctBool makeFace,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
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

    OcctStatus occt_model_planar_rectangle_wire_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            return TopoDS_Shape(modelRectangleWire(origin, xDirection, normal, width, height));
        });
    }

    OcctStatus occt_model_planar_face_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            BRepBuilderAPI_MakeFace maker(modelRectangleWire(origin, xDirection, normal, width, height), Standard_True);
            if (!maker.IsDone()) throw std::runtime_error("Planar face creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_planar_face_from_wire_create(
        OcctModelingSessionHandle handle,
        OcctObjectId wireId,
        OcctBool onlyPlane,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(wireId);
            if (shape.ShapeType() != TopAbs_WIRE) throw std::invalid_argument("Input must be a wire.");
            BRepBuilderAPI_MakeFace maker(TopoDS::Wire(shape), onlyPlane != 0);
            if (!maker.IsDone()) throw std::runtime_error("Face creation failed.");
            return TopoDS_Shape(maker.Face());
        });
    }
}
