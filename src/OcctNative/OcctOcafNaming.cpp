#include "OcctOcafInternal.hxx"

using namespace OcctOcafInternal;

extern "C"
{
    int occt_ocaf_naming_generated(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId newShapeId)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            TNaming_Builder builder(session->resolve(entry, true));
            builder.Generated(modelShape(model, newShapeId));
        });
    }

    int occt_ocaf_naming_generated_from(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId oldShapeId, OcctObjectId newShapeId)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            TNaming_Builder builder(session->resolve(entry, true));
            builder.Generated(modelShape(model, oldShapeId), modelShape(model, newShapeId));
        });
    }

    int occt_ocaf_naming_modify(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId oldShapeId, OcctObjectId newShapeId)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            TNaming_Builder builder(session->resolve(entry, true));
            builder.Modify(modelShape(model, oldShapeId), modelShape(model, newShapeId));
        });
    }

    int occt_ocaf_naming_delete(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId oldShapeId)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            TNaming_Builder builder(session->resolve(entry, true));
            builder.Delete(modelShape(model, oldShapeId));
        });
    }

    int occt_ocaf_naming_select(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId selectedShapeId, OcctObjectId contextShapeId)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            TNaming_Builder builder(session->resolve(entry, true));
            builder.Select(modelShape(model, selectedShapeId), modelShape(model, contextShapeId));
        });
    }

    int occt_ocaf_named_shape_exists(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&]
        {
            Handle(TNaming_NamedShape) attribute;
            result = session->resolve(entry).FindAttribute(TNaming_NamedShape::GetID(), attribute) && !attribute.IsNull() ? 1 : 0;
        });
        return result;
    }

    int occt_ocaf_named_shape_is_empty(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = namedShape(session->resolve(entry))->IsEmpty() ? 1 : 0; });
        return result;
    }

    int occt_ocaf_named_shape_evolution(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return OcctOcafEvolution_Unknown;
        int result = OcctOcafEvolution_Unknown;
        execute(session, [&] { result = static_cast<int>(namedShape(session->resolve(entry))->Evolution()); });
        return result;
    }

    int occt_ocaf_named_shape_version(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = namedShape(session->resolve(entry))->Version(); });
        return result;
    }

    int occt_ocaf_set_named_shape_version(OcctOcafHandle handle, const char* entry, int version)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { namedShape(session->resolve(entry))->SetVersion(version); });
    }

    OcctObjectId occt_ocaf_named_shape_get(OcctOcafHandle handle, OcctModelHandle model, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        OcctObjectId result = 0;
        execute(session, [&]
        {
            const TopoDS_Shape shape = namedShape(session->resolve(entry))->Get();
            if (shape.IsNull()) throw std::runtime_error("Named shape has no current shape.");
            result = addModelShape(model, shape);
        });
        return result;
    }

    int occt_ocaf_named_shape_pair_snapshot(OcctOcafHandle handle, OcctModelHandle model, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->namedShapePairs.clear();
            const Handle(TNaming_NamedShape) attribute = namedShape(session->resolve(entry));
            for (TNaming_Iterator iterator(attribute); iterator.More(); iterator.Next())
            {
                NamedShapePair pair;
                if (!iterator.OldShape().IsNull()) pair.oldShapeId = addModelShape(model, iterator.OldShape());
                if (!iterator.NewShape().IsNull()) pair.newShapeId = addModelShape(model, iterator.NewShape());
                session->namedShapePairs.push_back(pair);
            }
        })) return 0;
        return static_cast<int>(session->namedShapePairs.size());
    }

    OcctObjectId occt_ocaf_named_shape_old_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->namedShapePairs.size())) return 0;
        return session->namedShapePairs[static_cast<std::size_t>(index)].oldShapeId;
    }

    OcctObjectId occt_ocaf_named_shape_new_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->namedShapePairs.size())) return 0;
        return session->namedShapePairs[static_cast<std::size_t>(index)].newShapeId;
    }

    int occt_ocaf_selector_select(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId selectedShapeId, OcctObjectId contextShapeId, int geometryMode)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int selected = 0;
        if (!execute(session, [&]
        {
            TNaming_Selector selector(session->resolve(entry, true));
            const TopoDS_Shape selection = modelShape(model, selectedShapeId);
            if (contextShapeId > 0)
                selected = selector.Select(selection, modelShape(model, contextShapeId), geometryMode != 0) ? 1 : 0;
            else
                selected = selector.Select(selection, geometryMode != 0) ? 1 : 0;
        })) return 0;
        return selected;
    }

    int occt_ocaf_selector_solve(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int solved = 0;
        if (!execute(session, [&]
        {
            TDF_LabelMap validLabels;
            TNaming_Selector selector(session->resolve(entry));
            solved = selector.Solve(validLabels) ? 1 : 0;
        })) return 0;
        return solved;
    }

    int occt_ocaf_selector_is_identified(OcctOcafHandle handle, const char* entry, OcctModelHandle model, OcctObjectId shapeId)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int identified = 0;
        if (!execute(session, [&]
        {
            Handle(TNaming_NamedShape) identity;
            identified = TNaming_Selector::IsIdentified(session->resolve(entry), modelShape(model, shapeId), identity, Standard_False) ? 1 : 0;
        })) return 0;
        return identified;
    }
}
