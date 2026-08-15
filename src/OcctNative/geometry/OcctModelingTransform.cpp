#include "geometry/OcctModelingTransform.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_Transform.hxx>
#include <gp_Ax1.hxx>
#include <gp_Trsf.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_transform_translate(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctVector3d vectorValue,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            gp_Trsf transform;
            transform.SetTranslation(toVector(vectorValue));
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Translation failed.");
            return algorithm.Shape();
        });
    }

    OcctStatus occt_model_transform_rotate(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (std::abs(angleDegrees) <= Precision::Angular())
                throw std::invalid_argument("Rotation angle must not be zero.");

            gp_Trsf transform;
            transform.SetRotation(
                gp_Ax1(toPoint(axisPoint), toDirection(axisDirection)),
                angleDegrees * 3.14159265358979323846 / 180.0);
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Rotation failed.");
            return algorithm.Shape();
        });
    }

    OcctStatus occt_model_transform_scale(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d center,
        double factor,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (std::abs(factor) <= Precision::Confusion())
                throw std::invalid_argument("Scale factor must not be zero.");

            gp_Trsf transform;
            transform.SetScale(toPoint(center), factor);
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Scaling failed.");
            return algorithm.Shape();
        });
    }

    OcctStatus occt_model_transform_mirror_plane(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            gp_Trsf transform;
            transform.SetMirror(gp_Ax2(toPoint(planePoint), toDirection(planeNormal)));
            BRepBuilderAPI_Transform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Mirror operation failed.");
            return algorithm.Shape();
        });
    }
}
