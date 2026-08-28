#include "geometry/OcctModelingTransform.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_GTransform.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <gp_Ax1.hxx>
#include <gp_GTrsf.hxx>
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

    OcctStatus occt_model_transform_affine(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctTransform3d transformValue,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            const double values[] = {
                transformValue.m00, transformValue.m01, transformValue.m02, transformValue.m03,
                transformValue.m10, transformValue.m11, transformValue.m12, transformValue.m13,
                transformValue.m20, transformValue.m21, transformValue.m22, transformValue.m23
            };
            for (double value : values)
            {
                if (!std::isfinite(value))
                    throw std::invalid_argument("Affine transform values must be finite.");
            }

            gp_GTrsf transform;
            transform.SetValue(1, 1, transformValue.m00);
            transform.SetValue(1, 2, transformValue.m01);
            transform.SetValue(1, 3, transformValue.m02);
            transform.SetValue(1, 4, transformValue.m03);
            transform.SetValue(2, 1, transformValue.m10);
            transform.SetValue(2, 2, transformValue.m11);
            transform.SetValue(2, 3, transformValue.m12);
            transform.SetValue(2, 4, transformValue.m13);
            transform.SetValue(3, 1, transformValue.m20);
            transform.SetValue(3, 2, transformValue.m21);
            transform.SetValue(3, 3, transformValue.m22);
            transform.SetValue(3, 4, transformValue.m23);
            transform.SetForm();

            BRepBuilderAPI_GTransform algorithm(model->requireShape(shapeId), transform, Standard_True);
            if (!algorithm.IsDone()) throw std::runtime_error("Affine transformation failed.");
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
