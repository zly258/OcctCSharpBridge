#include "OcctInternal.hxx"

#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <Precision.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS_Wire.hxx>
#include <gp_Circ.hxx>

using namespace OcctBridge;

extern "C"
{
    OcctObjectId occt_make_arc_center(OcctHandle h, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            requirePositive(radius, "Radius");
            const double start = startAngleDegrees * 3.14159265358979323846 / 180.0;
            const double end = endAngleDegrees * 3.14159265358979323846 / 180.0;
            if (std::abs(end - start) <= Precision::Angular()) throw std::invalid_argument("Arc angle must not be zero.");
            const gp_Circ circle(axis2(center, normal, xDirection), radius);
            BRepBuilderAPI_MakeEdge maker(circle, start, end);
            if (!maker.IsDone()) throw std::runtime_error("Arc creation failed.");
            return e->addShape(maker.Edge(), false, "Arc");
        });
    }

    OcctObjectId occt_make_regular_polygon(OcctHandle h, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            requirePositive(radius, "Radius"); requireCount(sideCount, 3, "Polygon");
            const gp_Ax2 plane = axis2(center, normal, xDirection);
            const gp_Vec x(plane.XDirection()); const gp_Vec y(plane.YDirection()); const gp_Pnt c = point(center);
            BRepBuilderAPI_MakePolygon polygon;
            for (int index = 0; index < sideCount; ++index)
            {
                const double angle = 2.0 * 3.14159265358979323846 * static_cast<double>(index) / static_cast<double>(sideCount);
                polygon.Add(c.Translated(x * (radius * std::cos(angle)) + y * (radius * std::sin(angle))));
            }
            polygon.Close();
            if (!polygon.IsDone()) throw std::runtime_error("Regular polygon creation failed.");
            const TopoDS_Wire wire = polygon.Wire();
            if (!makeFace) return e->addShape(wire, false, "RegularPolygon");
            BRepBuilderAPI_MakeFace face(wire, Standard_True);
            if (!face.IsDone()) throw std::runtime_error("Regular polygon face creation failed.");
            return e->addShape(face.Face(), false, "RegularPolygonFace");
        });
    }
}
