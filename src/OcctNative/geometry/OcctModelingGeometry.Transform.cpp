#include "OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_Transform.hxx>
#include <gp_Ax1.hxx>
#include <gp_Trsf.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{
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
