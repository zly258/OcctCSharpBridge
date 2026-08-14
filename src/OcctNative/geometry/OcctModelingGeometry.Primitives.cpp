#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepPrimAPI_MakeWedge.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctObjectId occt_model_make_box(OcctModelHandle handle, double x, double y, double z, double dx, double dy, double dz)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requirePositive(dx, "Box X size");
            requirePositive(dy, "Box Y size");
            requirePositive(dz, "Box Z size");
            BRepPrimAPI_MakeBox maker(gp_Pnt(x, y, z), dx, dy, dz);
            maker.Build();
            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Box creation failed.");
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
            maker.Build();
            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Cylinder creation failed.");
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
            maker.Build();
            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Cone creation failed.");
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
            maker.Build();
            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Sphere creation failed.");
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
            maker.Build();
            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Torus creation failed.");
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
}
