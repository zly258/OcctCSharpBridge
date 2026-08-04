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
}

extern "C"
{
    int occt_ocaf_xde_color_snapshot(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            session->colorTool()->GetColors(labels);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_color_at(OcctOcafHandle handle, int index)
    {
        return occt_ocaf_xde_shape_at(handle, index);
    }

    int occt_ocaf_xde_get_color_definition(OcctOcafHandle handle, const char* colorEntry, OcctOcafColor* color)
    {
        OcafSession* session = sessionOf(handle);
        if (color == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Quantity_ColorRGBA value;
            found = session->colorTool()->GetColor(session->resolve(colorEntry), value) ? 1 : 0;
            if (found != 0) fillColor(value, *color);
        })) return 0;
        return found;
    }

    int occt_ocaf_xde_remove_color(OcctOcafHandle handle, const char* colorEntry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->colorTool()->RemoveColor(session->resolve(colorEntry)); });
    }

    int occt_ocaf_xde_set_color(OcctOcafHandle handle, const char* entry, int nativeColorType, OcctOcafColor color)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->colorTool()->SetColor(session->resolve(entry), toColor(color), colorType(nativeColorType)); });
    }

    int occt_ocaf_xde_get_color(OcctOcafHandle handle, const char* entry, int nativeColorType, OcctOcafColor* color)
    {
        OcafSession* session = sessionOf(handle);
        if (color == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            Quantity_ColorRGBA value;
            found = XCAFDoc_ColorTool::GetColor(session->resolve(entry), colorType(nativeColorType), value) ? 1 : 0;
            if (found != 0) fillColor(value, *color);
        })) return 0;
        return found;
    }

    int occt_ocaf_xde_unset_color(OcctOcafHandle handle, const char* entry, int nativeColorType)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->colorTool()->UnSetColor(session->resolve(entry), colorType(nativeColorType)); });
    }

    int occt_ocaf_xde_set_visibility(OcctOcafHandle handle, const char* entry, int visible)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->colorTool()->SetVisibility(session->resolve(entry), visible != 0); });
    }

    int occt_ocaf_xde_is_visible(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ColorTool::IsVisible(label); });
    }

    int occt_ocaf_xde_set_color_by_layer(OcctOcafHandle handle, const char* entry, int enabled)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->colorTool()->SetColorByLayer(session->resolve(entry), enabled != 0); });
    }

    int occt_ocaf_xde_is_color_by_layer(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [session = sessionOf(handle)](const TDF_Label& label) { return session->colorTool()->IsColorByLayer(label); });
    }

    const char* occt_ocaf_xde_add_layer(OcctOcafHandle handle, const char* utf8Name, int findVisible)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            return session->entry(session->layerTool()->AddLayer(extended(utf8Name), findVisible != 0));
        });
    }

    int occt_ocaf_xde_remove_layer(OcctOcafHandle handle, const char* layerEntry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->layerTool()->RemoveLayer(session->resolve(layerEntry)); });
    }

    const char* occt_ocaf_xde_layer_name(OcctOcafHandle handle, const char* layerEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            TCollection_ExtendedString name;
            if (!session->layerTool()->GetLayer(session->resolve(layerEntry), name))
                throw std::runtime_error("Label does not define an XDE layer.");
            return utf8(name);
        });
    }

    int occt_ocaf_xde_layer_snapshot(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            session->layerTool()->GetLayerLabels(labels);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_layer_at(OcctOcafHandle handle, int index)
    {
        return occt_ocaf_xde_shape_at(handle, index);
    }

    int occt_ocaf_xde_set_layer(OcctOcafHandle handle, const char* shapeEntry, const char* layerEntry, int shapeInOneLayer)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->layerTool()->SetLayer(session->resolve(shapeEntry), session->resolve(layerEntry), shapeInOneLayer != 0);
        });
    }

    int occt_ocaf_xde_unset_layer(OcctOcafHandle handle, const char* shapeEntry, const char* layerEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int removed = 0;
        if (!execute(session, [&]
        {
            removed = session->layerTool()->UnSetOneLayer(session->resolve(shapeEntry), session->resolve(layerEntry)) ? 1 : 0;
        })) return 0;
        return removed;
    }

    int occt_ocaf_xde_unset_layers(OcctOcafHandle handle, const char* shapeEntry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->layerTool()->UnSetLayers(session->resolve(shapeEntry)); });
    }

    int occt_ocaf_xde_shape_layer_snapshot(OcctOcafHandle handle, const char* shapeEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            session->layerTool()->GetLayers(session->resolve(shapeEntry), labels);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    int occt_ocaf_xde_set_layer_visibility(OcctOcafHandle handle, const char* layerEntry, int visible)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->layerTool()->SetVisibility(session->resolve(layerEntry), visible != 0); });
    }

    int occt_ocaf_xde_is_layer_visible(OcctOcafHandle handle, const char* layerEntry)
    {
        return classify(sessionOf(handle), layerEntry, [session = sessionOf(handle)](const TDF_Label& label) { return session->layerTool()->IsVisible(label); });
    }

}
