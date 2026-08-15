#include "topology/OcctModelingShapeProperties.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBndLib.hxx>
#include <Bnd_OBB.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    int occt_model_shape_is_same(
        OcctModelHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId)
    {
        ModelSession* model = modelOf(handle);
        return executeValue(model, 0, [&]
        {
            return model->requireShape(firstId).IsSame(model->requireShape(secondId)) ? 1 : 0;
        });
    }

    int occt_model_shape_is_partner(
        OcctModelHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId)
    {
        ModelSession* model = modelOf(handle);
        return executeValue(model, 0, [&]
        {
            return model->requireShape(firstId).IsPartner(model->requireShape(secondId)) ? 1 : 0;
        });
    }

    int occt_model_shape_oriented_bounds(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        int optimal,
        OcctOrientedBounds* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            Bnd_OBB box;
            BRepBndLib::AddOBB(
                model->requireShape(shapeId),
                box,
                Standard_True,
                optimal != 0 ? Standard_True : Standard_False,
                Standard_True);
            if (box.IsVoid())
                throw std::runtime_error("Shape oriented bounding box is empty.");

            const gp_Pnt center = box.Center();
            const gp_Dir xDirection = box.XDirection();
            const gp_Dir yDirection = box.YDirection();
            const gp_Dir zDirection = box.ZDirection();
            result->center = {center.X(), center.Y(), center.Z()};
            result->xDirection = {xDirection.X(), xDirection.Y(), xDirection.Z()};
            result->yDirection = {yDirection.X(), yDirection.Y(), yDirection.Z()};
            result->zDirection = {zDirection.X(), zDirection.Y(), zDirection.Z()};
            result->halfSizeX = box.XHSize();
            result->halfSizeY = box.YHSize();
            result->halfSizeZ = box.ZHSize();
        });
    }
}
