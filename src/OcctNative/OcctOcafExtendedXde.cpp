#include "OcctOcafExtended.h"
#include "OcctOcafInternal.hxx"

using namespace OcctOcafInternal;

namespace
{
    int classify(OcafSession* session, const char* entry, const std::function<bool(const TDF_Label&)>& predicate)
    {
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = predicate(session->resolve(entry)) ? 1 : 0; });
        return result;
    }

    const char* snapshotAt(OcafSession* session, int index)
    {
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->stringSnapshot.size())) return "";
        return session->stringSnapshot[static_cast<std::size_t>(index)].c_str();
    }

    Handle(TCollection_HAsciiString) asciiHandle(const char* value)
    {
        return new TCollection_HAsciiString(value == nullptr ? "" : value);
    }
}

extern "C"
{
    const char* occt_ocaf_xde_new_shape(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            const TDF_Label label = session->shapeTool()->NewShape();
            if (label.IsNull()) throw std::runtime_error("Unable to create an empty XDE shape label.");
            return session->entry(label);
        });
    }

    int occt_ocaf_xde_is_top_level(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        return classify(session, entry, [session](const TDF_Label& label)
        {
            return session->shapeTool()->IsTopLevel(label);
        });
    }

    int occt_ocaf_xde_is_compound(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label)
        {
            return XCAFDoc_ShapeTool::IsCompound(label);
        });
    }

    int occt_ocaf_xde_component_count(OcctOcafHandle handle, const char* entry, int recursive)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&]
        {
            result = XCAFDoc_ShapeTool::NbComponents(session->resolve(entry), recursive != 0);
        });
        return result;
    }

    int occt_ocaf_xde_user_snapshot(OcctOcafHandle handle, const char* entry, int recursive)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            XCAFDoc_ShapeTool::GetUsers(session->resolve(entry), labels, recursive != 0);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_user_at(OcctOcafHandle handle, int index)
    {
        return snapshotAt(sessionOf(handle), index);
    }

    const char* occt_ocaf_xde_search_shape(OcctOcafHandle handle, OcctModelHandle model,
                                           OcctObjectId shapeId, int findInstance,
                                           int findComponent, int findSubshape)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            TDF_Label label;
            if (!session->shapeTool()->Search(modelShape(model, shapeId), label,
                                               findInstance != 0, findComponent != 0, findSubshape != 0))
                return std::string();
            return session->entry(label);
        });
    }

    const char* occt_ocaf_xde_find_subshape(OcctOcafHandle handle, OcctModelHandle model,
                                            const char* shapeEntry, OcctObjectId subshapeId)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            TDF_Label label;
            if (!session->shapeTool()->FindSubShape(session->resolve(shapeEntry), modelShape(model, subshapeId), label))
                return std::string();
            return session->entry(label);
        });
    }

    const char* occt_ocaf_xde_add_subshape(OcctOcafHandle handle, OcctModelHandle model,
                                           const char* shapeEntry, OcctObjectId subshapeId)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            const TDF_Label label = session->shapeTool()->AddSubShape(
                session->resolve(shapeEntry), modelShape(model, subshapeId));
            if (label.IsNull()) throw std::runtime_error("The supplied shape is not a subshape of the XDE shape.");
            return session->entry(label);
        });
    }

    int occt_ocaf_xde_subshape_snapshot(OcctOcafHandle handle, const char* shapeEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            XCAFDoc_ShapeTool::GetSubShapes(session->resolve(shapeEntry), labels);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_subshape_at(OcctOcafHandle handle, int index)
    {
        return snapshotAt(sessionOf(handle), index);
    }

    const char* occt_ocaf_xde_add_color(OcctOcafHandle handle, OcctOcafColor color)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            return session->entry(session->colorTool()->AddColor(toColor(color)));
        });
    }

    const char* occt_ocaf_xde_find_color(OcctOcafHandle handle, OcctOcafColor color)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            return session->entry(session->colorTool()->FindColor(toColor(color)));
        });
    }

    int occt_ocaf_xde_is_color(OcctOcafHandle handle, const char* colorEntry)
    {
        OcafSession* session = sessionOf(handle);
        return classify(session, colorEntry, [session](const TDF_Label& label)
        {
            return session->colorTool()->IsColor(label);
        });
    }

    int occt_ocaf_xde_color_is_set(OcctOcafHandle handle, const char* entry, int nativeColorType)
    {
        OcafSession* session = sessionOf(handle);
        return classify(session, entry, [session, nativeColorType](const TDF_Label& label)
        {
            return session->colorTool()->IsSet(label, colorType(nativeColorType));
        });
    }

    const char* occt_ocaf_xde_color_label(OcctOcafHandle handle, const char* entry, int nativeColorType)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            TDF_Label colorLabel;
            if (!XCAFDoc_ColorTool::GetColor(session->resolve(entry), colorType(nativeColorType), colorLabel))
                return std::string();
            return session->entry(colorLabel);
        });
    }

    int occt_ocaf_xde_set_color_label(OcctOcafHandle handle, const char* entry,
                                      const char* colorEntry, int nativeColorType)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            const TDF_Label colorLabel = session->resolve(colorEntry);
            if (!session->colorTool()->IsColor(colorLabel))
                throw std::invalid_argument("The supplied label is not an XDE color definition.");
            session->colorTool()->SetColor(session->resolve(entry), colorLabel, colorType(nativeColorType));
        });
    }

    int occt_ocaf_xde_set_instance_color(OcctOcafHandle handle, OcctModelHandle model,
                                         OcctObjectId shapeId, int nativeColorType,
                                         OcctOcafColor color, int createShuo)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        if (!execute(session, [&]
        {
            result = session->colorTool()->SetInstanceColor(
                modelShape(model, shapeId), colorType(nativeColorType), toColor(color), createShuo != 0) ? 1 : 0;
        })) return 0;
        return result;
    }

    int occt_ocaf_xde_get_instance_color(OcctOcafHandle handle, OcctModelHandle model,
                                         OcctObjectId shapeId, int nativeColorType,
                                         OcctOcafColor* color)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || color == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Quantity_ColorRGBA value;
            found = session->colorTool()->GetInstanceColor(
                modelShape(model, shapeId), colorType(nativeColorType), value) ? 1 : 0;
            if (found != 0) fillColor(value, *color);
        })) return 0;
        return found;
    }

    int occt_ocaf_xde_is_instance_visible(OcctOcafHandle handle, OcctModelHandle model,
                                          OcctObjectId shapeId)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&]
        {
            result = session->colorTool()->IsInstanceVisible(modelShape(model, shapeId)) ? 1 : 0;
        });
        return result;
    }

    const char* occt_ocaf_xde_find_layer(OcctOcafHandle handle, const char* utf8Name,
                                         int findWithProperty, int findVisible)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            return session->entry(session->layerTool()->FindLayer(
                extended(utf8Name), findWithProperty != 0, findVisible != 0));
        });
    }

    int occt_ocaf_xde_is_layer(OcctOcafHandle handle, const char* layerEntry)
    {
        OcafSession* session = sessionOf(handle);
        return classify(session, layerEntry, [session](const TDF_Label& label)
        {
            return session->layerTool()->IsLayer(label);
        });
    }

    int occt_ocaf_xde_layer_is_set(OcctOcafHandle handle, const char* shapeEntry,
                                   const char* layerEntry)
    {
        OcafSession* session = sessionOf(handle);
        return classify(session, shapeEntry, [session, layerEntry](const TDF_Label& shapeLabel)
        {
            return session->layerTool()->IsSet(shapeLabel, session->resolve(layerEntry));
        });
    }

    int occt_ocaf_xde_layer_shape_snapshot(OcctOcafHandle handle, const char* layerEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            const TDF_Label layerLabel = session->resolve(layerEntry);
            if (!session->layerTool()->IsLayer(layerLabel))
                throw std::invalid_argument("The supplied label is not an XDE layer definition.");
            TDF_LabelSequence labels;
            XCAFDoc_LayerTool::GetShapesOfLayer(layerLabel, labels);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_layer_shape_at(OcctOcafHandle handle, int index)
    {
        return snapshotAt(sessionOf(handle), index);
    }

    const char* occt_ocaf_xde_add_material(OcctOcafHandle handle, const char* utf8Name,
                                           const char* utf8Description, double density,
                                           const char* utf8DensityName,
                                           const char* utf8DensityValueType)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            const TDF_Label label = session->materialTool()->AddMaterial(
                asciiHandle(utf8Name), asciiHandle(utf8Description), density,
                asciiHandle(utf8DensityName), asciiHandle(utf8DensityValueType));
            return session->entry(label);
        });
    }

    int occt_ocaf_xde_is_material(OcctOcafHandle handle, const char* materialEntry)
    {
        OcafSession* session = sessionOf(handle);
        return classify(session, materialEntry, [session](const TDF_Label& label)
        {
            return session->materialTool()->IsMaterial(label);
        });
    }

    int occt_ocaf_xde_assign_material(OcctOcafHandle handle, const char* shapeEntry,
                                      const char* materialEntry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            const TDF_Label materialLabel = session->resolve(materialEntry);
            if (!session->materialTool()->IsMaterial(materialLabel))
                throw std::invalid_argument("The supplied label is not an XDE material definition.");
            session->materialTool()->SetMaterial(session->resolve(shapeEntry), materialLabel);
        });
    }
}
