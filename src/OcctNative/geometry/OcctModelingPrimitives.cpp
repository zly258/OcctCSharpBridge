#include "geometry/OcctModelingPrimitives.h"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepPrimAPI_MakeWedge.hxx>
#include <Precision.hxx>
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>

#include <cmath>
#include <stdexcept>

using namespace OcctModelingInternal;

namespace
{
    gp_Pnt point(OcctPoint3d value)
    {
        if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
            throw std::invalid_argument("Point coordinates must be finite.");
        return gp_Pnt(value.x, value.y, value.z);
    }

    gp_Dir direction(OcctVector3d value)
    {
        if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
            throw std::invalid_argument("Direction coordinates must be finite.");
        const gp_Vec vector(value.x, value.y, value.z);
        if (vector.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::invalid_argument("Direction must not be zero.");
        return gp_Dir(vector);
    }

    void requirePositive(double value, const char* name)
    {
        if (!std::isfinite(value) || value <= 0.0)
            throw std::invalid_argument(std::string(name) + " must be finite and greater than zero.");
    }
}

extern "C"
{
    OcctStatus occt_model_primitive_box_create(OcctModelingSessionHandle handle, double x, double y, double z, double dx, double dy, double dz, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(dx, "Box X size");
            requirePositive(dy, "Box Y size");
            requirePositive(dz, "Box Z size");
            BRepPrimAPI_MakeBox maker(point({x, y, z}), dx, dy, dz);
            if (!maker.IsDone()) throw std::runtime_error("Box creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_primitive_cylinder_create(OcctModelingSessionHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(radius, "Cylinder radius");
            requirePositive(height, "Cylinder height");
            BRepPrimAPI_MakeCylinder maker(gp_Ax2(point(origin), direction(axis)), radius, height);
            if (!maker.IsDone()) throw std::runtime_error("Cylinder creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_primitive_cone_create(OcctModelingSessionHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (!std::isfinite(radius1) || !std::isfinite(radius2) || radius1 < 0.0 || radius2 < 0.0)
                throw std::invalid_argument("Cone radii must be finite and non-negative.");
            if (radius1 + radius2 <= Precision::Confusion())
                throw std::invalid_argument("At least one cone radius must be greater than zero.");
            requirePositive(height, "Cone height");
            BRepPrimAPI_MakeCone maker(gp_Ax2(point(origin), direction(axis)), radius1, radius2, height);
            if (!maker.IsDone()) throw std::runtime_error("Cone creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_primitive_sphere_create(OcctModelingSessionHandle handle, OcctPoint3d center, double radius, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(radius, "Sphere radius");
            BRepPrimAPI_MakeSphere maker(point(center), radius);
            if (!maker.IsDone()) throw std::runtime_error("Sphere creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_primitive_torus_create(OcctModelingSessionHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(majorRadius, "Torus major radius");
            requirePositive(minorRadius, "Torus minor radius");
            if (minorRadius >= majorRadius)
                throw std::invalid_argument("Torus minor radius must be less than major radius.");
            BRepPrimAPI_MakeTorus maker(gp_Ax2(point(center), direction(axis)), majorRadius, minorRadius);
            if (!maker.IsDone()) throw std::runtime_error("Torus creation failed.");
            return maker.Shape();
        });
    }

    OcctStatus occt_model_primitive_wedge_create(OcctModelingSessionHandle handle, double dx, double dy, double dz, double ltx, OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requirePositive(dx, "Wedge X size");
            requirePositive(dy, "Wedge Y size");
            requirePositive(dz, "Wedge Z size");
            if (!std::isfinite(ltx)) throw std::invalid_argument("Wedge ltx must be finite.");
            BRepPrimAPI_MakeWedge maker(dx, dy, dz, ltx);
            if (!maker.IsDone()) throw std::runtime_error("Wedge creation failed.");
            return maker.Shape();
        });
    }
}
