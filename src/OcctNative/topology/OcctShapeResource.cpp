#include "OcctShapeResource.h"
#include "modeling/OcctModelingSessionInternal.hxx"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <TopoDS_Shape.hxx>

struct OcctShapeHandle_t
{
    explicit OcctShapeHandle_t(const TopoDS_Shape& value)
        : shape(value)
    {
    }

    TopoDS_Shape shape;
};

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_shape_acquire(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        OcctShapeHandle* result)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        model->errors.clear();
        if (result == nullptr)
        {
            model->errors.set(OcctStatus_ErrorInvalidArgument, "Shape handle output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }

        *result = nullptr;
        if (!execute(model, [&]
        {
            *result = new OcctShapeHandle_t(model->requireShape(shapeId));
        }))
        {
            return model->errors.code;
        }
        return OcctStatus_Ok;
    }

    void occt_shape_release(OcctShapeHandle handle)
    {
        delete handle;
    }

    OcctStatus occt_shape_get_type(OcctShapeHandle handle, int* result)
    {
        if (handle == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = toOcctShapeType(handle->shape.ShapeType());
        return OcctStatus_Ok;
    }
}
