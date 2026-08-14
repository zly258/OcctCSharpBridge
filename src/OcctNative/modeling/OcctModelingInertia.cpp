#include "modeling/OcctModelingInertia.h"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <GProp_PrincipalProps.hxx>
#include <gp_Mat.hxx>

using namespace OcctModelingInternal;

namespace
{
    void fillInertiaProperties(const GProp_GProps& properties, OcctModelInertiaProperties* result)
    {
        const gp_Pnt center = properties.CentreOfMass();
        const gp_Mat inertia = properties.MatrixOfInertia();
        const GProp_PrincipalProps principal = properties.PrincipalProperties();

        Standard_Real moment1 = 0.0;
        Standard_Real moment2 = 0.0;
        Standard_Real moment3 = 0.0;
        principal.Moments(moment1, moment2, moment3);

        Standard_Real radius1 = 0.0;
        Standard_Real radius2 = 0.0;
        Standard_Real radius3 = 0.0;
        principal.RadiusOfGyration(radius1, radius2, radius3);

        const gp_Vec axis1(principal.FirstAxisOfInertia());
        const gp_Vec axis2(principal.SecondAxisOfInertia());
        const gp_Vec axis3(principal.ThirdAxisOfInertia());

        result->mass = properties.Mass();
        result->centerOfMass = {center.X(), center.Y(), center.Z()};
        result->ixx = inertia.Value(1, 1);
        result->iyy = inertia.Value(2, 2);
        result->izz = inertia.Value(3, 3);
        result->ixy = inertia.Value(1, 2);
        result->ixz = inertia.Value(1, 3);
        result->iyz = inertia.Value(2, 3);
        result->principalMoment1 = moment1;
        result->principalMoment2 = moment2;
        result->principalMoment3 = moment3;
        result->principalAxis1 = {axis1.X(), axis1.Y(), axis1.Z()};
        result->principalAxis2 = {axis2.X(), axis2.Y(), axis2.Z()};
        result->principalAxis3 = {axis3.X(), axis3.Y(), axis3.Z()};
        result->radiusOfGyration1 = radius1;
        result->radiusOfGyration2 = radius2;
        result->radiusOfGyration3 = radius3;
        result->hasSymmetryAxis = principal.HasSymmetryAxis() ? 1 : 0;
        result->hasSymmetryPoint = principal.HasSymmetryPoint() ? 1 : 0;
    }

    int computeInertia(
        ModelSession* model,
        OcctObjectId shapeId,
        OcctModelInertiaProperties* result,
        void (*compute)(const TopoDS_Shape&, GProp_GProps&))
    {
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            GProp_GProps properties;
            compute(model->requireShape(shapeId), properties);
            fillInertiaProperties(properties, result);
        });
    }

    void computeLinear(const TopoDS_Shape& shape, GProp_GProps& properties)
    {
        BRepGProp::LinearProperties(shape, properties);
    }

    void computeSurface(const TopoDS_Shape& shape, GProp_GProps& properties)
    {
        BRepGProp::SurfaceProperties(shape, properties);
    }

    void computeVolume(const TopoDS_Shape& shape, GProp_GProps& properties)
    {
        BRepGProp::VolumeProperties(shape, properties);
    }
}

extern "C"
{
    int occt_model_shape_linear_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result)
    {
        return computeInertia(modelOf(handle), shapeId, result, computeLinear);
    }

    int occt_model_shape_surface_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result)
    {
        return computeInertia(modelOf(handle), shapeId, result, computeSurface);
    }

    int occt_model_shape_volume_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result)
    {
        return computeInertia(modelOf(handle), shapeId, result, computeVolume);
    }
}
