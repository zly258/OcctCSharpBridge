#include "OcctOcafInternal.hxx"

using namespace OcctOcafInternal;

namespace
{
    template<typename Attribute>
    Handle(Attribute) requireAttribute(const TDF_Label& label, const Standard_GUID& id, const char* name)
    {
        Handle(Attribute) attribute;
        if (!label.FindAttribute(id, attribute) || attribute.IsNull())
            throw std::runtime_error(std::string("Label has no ") + name + " attribute.");
        return attribute;
    }

    template<typename Attribute, typename Value>
    void snapshotNumeric(OcafSession* session, const Handle(Attribute)& attribute, std::vector<Value>& target)
    {
        session->arrayLower = attribute->Lower();
        target.clear();
        target.reserve(static_cast<std::size_t>(attribute->Length()));
        for (int index = attribute->Lower(); index <= attribute->Upper(); ++index)
            target.push_back(static_cast<Value>(attribute->Value(index)));
    }

    template<typename Attribute>
    Handle(Attribute) prepareArray(const TDF_Label& label, int lower, int count)
    {
        if (count <= 0) throw std::invalid_argument("Array must contain at least one value.");
        const int upper = lower + count - 1;
        Handle(Attribute) attribute = Attribute::Set(label, lower, upper);
        if (attribute->Lower() != lower || attribute->Upper() != upper) attribute->Init(lower, upper);
        return attribute;
    }
}

extern "C"
{
    int occt_ocaf_set_name(OcctOcafHandle handle, const char* entry, const char* utf8Value)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDataStd_Name::Set(session->resolve(entry), extended(utf8Value)); });
    }

    int occt_ocaf_get_name(OcctOcafHandle handle, const char* entry, const char** utf8Value)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || utf8Value == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Handle(TDataStd_Name) attribute;
            found = session->resolve(entry).FindAttribute(TDataStd_Name::GetID(), attribute) && !attribute.IsNull() ? 1 : 0;
            if (found != 0) *utf8Value = session->setScratch(utf8(attribute->Get()));
        })) return 0;
        return found;
    }

    int occt_ocaf_set_comment(OcctOcafHandle handle, const char* entry, const char* utf8Value)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDataStd_Comment::Set(session->resolve(entry), extended(utf8Value)); });
    }

    int occt_ocaf_get_comment(OcctOcafHandle handle, const char* entry, const char** utf8Value)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || utf8Value == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Handle(TDataStd_Comment) attribute;
            found = session->resolve(entry).FindAttribute(TDataStd_Comment::GetID(), attribute) && !attribute.IsNull() ? 1 : 0;
            if (found != 0) *utf8Value = session->setScratch(utf8(attribute->Get()));
        })) return 0;
        return found;
    }

    int occt_ocaf_set_ascii_string(OcctOcafHandle handle, const char* entry, const char* utf8Value)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            if (utf8Value == nullptr) throw std::invalid_argument("ASCII string must not be null.");
            TDataStd_AsciiString::Set(session->resolve(entry), TCollection_AsciiString(utf8Value));
        });
    }

    int occt_ocaf_get_ascii_string(OcctOcafHandle handle, const char* entry, const char** utf8Value)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || utf8Value == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Handle(TDataStd_AsciiString) attribute;
            found = session->resolve(entry).FindAttribute(TDataStd_AsciiString::GetID(), attribute) && !attribute.IsNull() ? 1 : 0;
            if (found != 0) *utf8Value = session->setScratch(attribute->Get().ToCString());
        })) return 0;
        return found;
    }

    int occt_ocaf_set_integer(OcctOcafHandle handle, const char* entry, int value)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDataStd_Integer::Set(session->resolve(entry), value); });
    }

    int occt_ocaf_get_integer(OcctOcafHandle handle, const char* entry, int* value)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || value == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Handle(TDataStd_Integer) attribute;
            found = session->resolve(entry).FindAttribute(TDataStd_Integer::GetID(), attribute) && !attribute.IsNull() ? 1 : 0;
            if (found != 0) *value = attribute->Get();
        })) return 0;
        return found;
    }

    int occt_ocaf_set_real(OcctOcafHandle handle, const char* entry, double value)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDataStd_Real::Set(session->resolve(entry), value); });
    }

    int occt_ocaf_get_real(OcctOcafHandle handle, const char* entry, double* value)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || value == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Handle(TDataStd_Real) attribute;
            found = session->resolve(entry).FindAttribute(TDataStd_Real::GetID(), attribute) && !attribute.IsNull() ? 1 : 0;
            if (found != 0) *value = attribute->Get();
        })) return 0;
        return found;
    }

    int occt_ocaf_set_uattribute(OcctOcafHandle handle, const char* entry, const char* guid)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDataStd_UAttribute::Set(session->resolve(entry), parseGuid(guid)); });
    }

    int occt_ocaf_has_uattribute(OcctOcafHandle handle, const char* entry, const char* guid)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&]
        {
            Handle(TDataStd_UAttribute) attribute;
            result = session->resolve(entry).FindAttribute(parseGuid(guid), attribute) && !attribute.IsNull() ? 1 : 0;
        });
        return result;
    }

    int occt_ocaf_set_reference(OcctOcafHandle handle, const char* entry, const char* targetEntry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDF_Reference::Set(session->resolve(entry), session->resolve(targetEntry)); });
    }

    int occt_ocaf_get_reference(OcctOcafHandle handle, const char* entry, const char** targetEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || targetEntry == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Handle(TDF_Reference) attribute;
            found = session->resolve(entry).FindAttribute(TDF_Reference::GetID(), attribute) && !attribute.IsNull() ? 1 : 0;
            if (found != 0) *targetEntry = session->setScratch(session->entry(attribute->Get()));
        })) return 0;
        return found;
    }

    int occt_ocaf_set_integer_array(OcctOcafHandle handle, const char* entry, const int* values, int count, int lower)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            if (values == nullptr) throw std::invalid_argument("Integer array values must not be null.");
            auto attribute = prepareArray<TDataStd_IntegerArray>(session->resolve(entry), lower, count);
            for (int index = 0; index < count; ++index) attribute->SetValue(lower + index, values[index]);
        });
    }

    int occt_ocaf_get_integer_array(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            const auto attribute = requireAttribute<TDataStd_IntegerArray>(session->resolve(entry), TDataStd_IntegerArray::GetID(), "TDataStd_IntegerArray");
            snapshotNumeric(session, attribute, session->integerSnapshot);
            session->realSnapshot.clear(); session->arrayStringSnapshot.clear();
        })) return 0;
        return static_cast<int>(session->integerSnapshot.size());
    }

    int occt_ocaf_set_real_array(OcctOcafHandle handle, const char* entry, const double* values, int count, int lower)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            if (values == nullptr) throw std::invalid_argument("Real array values must not be null.");
            auto attribute = prepareArray<TDataStd_RealArray>(session->resolve(entry), lower, count);
            for (int index = 0; index < count; ++index) attribute->SetValue(lower + index, values[index]);
        });
    }

    int occt_ocaf_get_real_array(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            const auto attribute = requireAttribute<TDataStd_RealArray>(session->resolve(entry), TDataStd_RealArray::GetID(), "TDataStd_RealArray");
            snapshotNumeric(session, attribute, session->realSnapshot);
            session->integerSnapshot.clear(); session->arrayStringSnapshot.clear();
        })) return 0;
        return static_cast<int>(session->realSnapshot.size());
    }

    int occt_ocaf_set_boolean_array(OcctOcafHandle handle, const char* entry, const int* values, int count, int lower)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            if (values == nullptr) throw std::invalid_argument("Boolean array values must not be null.");
            auto attribute = prepareArray<TDataStd_BooleanArray>(session->resolve(entry), lower, count);
            for (int index = 0; index < count; ++index) attribute->SetValue(lower + index, values[index] != 0);
        });
    }

    int occt_ocaf_get_boolean_array(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            const auto attribute = requireAttribute<TDataStd_BooleanArray>(session->resolve(entry), TDataStd_BooleanArray::GetID(), "TDataStd_BooleanArray");
            session->arrayLower = attribute->Lower(); session->integerSnapshot.clear();
            for (int index = attribute->Lower(); index <= attribute->Upper(); ++index) session->integerSnapshot.push_back(attribute->Value(index) ? 1 : 0);
            session->realSnapshot.clear(); session->arrayStringSnapshot.clear();
        })) return 0;
        return static_cast<int>(session->integerSnapshot.size());
    }

    int occt_ocaf_set_byte_array(OcctOcafHandle handle, const char* entry, const unsigned char* values, int count, int lower)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            if (values == nullptr) throw std::invalid_argument("Byte array values must not be null.");
            auto attribute = prepareArray<TDataStd_ByteArray>(session->resolve(entry), lower, count);
            for (int index = 0; index < count; ++index) attribute->SetValue(lower + index, values[index]);
        });
    }

    int occt_ocaf_get_byte_array(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            const auto attribute = requireAttribute<TDataStd_ByteArray>(session->resolve(entry), TDataStd_ByteArray::GetID(), "TDataStd_ByteArray");
            session->arrayLower = attribute->Lower(); session->integerSnapshot.clear();
            for (int index = attribute->Lower(); index <= attribute->Upper(); ++index) session->integerSnapshot.push_back(attribute->Value(index));
            session->realSnapshot.clear(); session->arrayStringSnapshot.clear();
        })) return 0;
        return static_cast<int>(session->integerSnapshot.size());
    }

    int occt_ocaf_set_string_array(OcctOcafHandle handle, const char* entry, const char* const* utf8Values, int count, int lower)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            if (utf8Values == nullptr) throw std::invalid_argument("String array values must not be null.");
            auto attribute = prepareArray<TDataStd_ExtStringArray>(session->resolve(entry), lower, count);
            for (int index = 0; index < count; ++index) attribute->SetValue(lower + index, extended(utf8Values[index]));
        });
    }

    int occt_ocaf_get_string_array(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            const auto attribute = requireAttribute<TDataStd_ExtStringArray>(session->resolve(entry), TDataStd_ExtStringArray::GetID(), "TDataStd_ExtStringArray");
            session->arrayLower = attribute->Lower(); session->arrayStringSnapshot.clear();
            for (int index = attribute->Lower(); index <= attribute->Upper(); ++index) session->arrayStringSnapshot.push_back(utf8(attribute->Value(index)));
            session->integerSnapshot.clear(); session->realSnapshot.clear();
        })) return 0;
        return static_cast<int>(session->arrayStringSnapshot.size());
    }

    int occt_ocaf_array_lower(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session == nullptr ? 0 : session->arrayLower;
    }

    int occt_ocaf_array_count(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        return static_cast<int>(std::max({session->integerSnapshot.size(), session->realSnapshot.size(), session->arrayStringSnapshot.size()}));
    }

    int occt_ocaf_array_int_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->integerSnapshot.size())) return 0;
        return session->integerSnapshot[static_cast<std::size_t>(index)];
    }

    double occt_ocaf_array_real_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->realSnapshot.size())) return 0.0;
        return session->realSnapshot[static_cast<std::size_t>(index)];
    }

    const char* occt_ocaf_array_string_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->arrayStringSnapshot.size())) return "";
        return session->arrayStringSnapshot[static_cast<std::size_t>(index)].c_str();
    }

    int occt_ocaf_set_position(OcctOcafHandle handle, const char* entry, OcctPoint3d point)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDataXtd_Position::Set(session->resolve(entry), gp_Pnt(point.x, point.y, point.z)); });
    }

    int occt_ocaf_get_position(OcctOcafHandle handle, const char* entry, OcctPoint3d* point)
    {
        OcafSession* session = sessionOf(handle);
        if (point == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            gp_Pnt value;
            found = TDataXtd_Position::Get(session->resolve(entry), value) ? 1 : 0;
            if (found != 0) *point = {value.X(), value.Y(), value.Z()};
        })) return 0;
        return found;
    }

    int occt_ocaf_set_shape_attribute(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId shapeId)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { TDataXtd_Shape::Set(session->resolve(entry), modelShape(model, shapeId)); });
    }

    OcctObjectId occt_ocaf_get_shape_attribute(OcctOcafHandle handle, OcctModelHandle model, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        OcctObjectId result = 0;
        execute(session, [&]
        {
            const TopoDS_Shape shape = TDataXtd_Shape::Get(session->resolve(entry));
            if (shape.IsNull()) throw std::runtime_error("Label has no TDataXtd_Shape value.");
            result = addModelShape(model, shape);
        });
        return result;
    }
}
