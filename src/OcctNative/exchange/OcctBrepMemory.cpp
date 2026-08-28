#include "exchange/OcctBrepMemory.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepTools.hxx>
#include <BRep_Builder.hxx>

#include <cstring>
#include <limits>
#include <sstream>
#include <stdexcept>
#include <string>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_brep_serialize(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        std::uint8_t* buffer,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            std::ostringstream stream(std::ios::out | std::ios::binary);
            BRepTools::Write(model->requireShape(shapeId), stream);
            if (!stream.good())
                throw std::runtime_error("BREP serialization failed.");

            const std::string data = stream.str();
            if (data.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
                throw std::length_error("Serialized BREP exceeds the ABI buffer size limit.");

            *required = static_cast<int>(data.size());
            if (buffer == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null BREP buffer requires zero capacity.");
                return;
            }
            if (capacity < *required)
                throw std::out_of_range("BREP buffer capacity is smaller than the serialized shape.");

            if (!data.empty())
                std::memcpy(buffer, data.data(), data.size());
        });
    }

    OcctStatus occt_model_brep_deserialize(
        OcctModelingSessionHandle handle,
        const std::uint8_t* buffer,
        int length,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (buffer == nullptr || length <= 0 || result == nullptr)
            return OcctStatus_ErrorInvalidArgument;

        *result = 0;
        return executeStatus(model, [&]
        {
            const std::string data(
                reinterpret_cast<const char*>(buffer),
                static_cast<std::size_t>(length));
            std::istringstream stream(data, std::ios::in | std::ios::binary);
            BRep_Builder builder;
            TopoDS_Shape shape;
            BRepTools::Read(shape, stream, builder);
            if (shape.IsNull())
                throw std::runtime_error("BREP deserialization produced an empty shape.");
            *result = model->addShape(shape);
        });
    }
}
