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
        return executeStatus(model, [&]
        {
            if (result == nullptr)
                throw std::invalid_argument("Shape handle output is null.");
            *result = nullptr;
            *result = new OcctShapeHandle_t(model->requireShape(shapeId));
        });
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
