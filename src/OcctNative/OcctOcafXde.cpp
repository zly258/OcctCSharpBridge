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
    const char* occt_ocaf_xde_shapes_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            session->requireDocument();
            return session->entry(XCAFDoc_DocumentTool::ShapesLabel(session->document->Main()));
        });
    }

    const char* occt_ocaf_xde_colors_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            session->requireDocument();
            return session->entry(XCAFDoc_DocumentTool::ColorsLabel(session->document->Main()));
        });
    }

    const char* occt_ocaf_xde_layers_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            session->requireDocument();
            return session->entry(XCAFDoc_DocumentTool::LayersLabel(session->document->Main()));
        });
    }

    const char* occt_ocaf_xde_materials_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            session->requireDocument();
            return session->entry(XCAFDoc_DocumentTool::MaterialsLabel(session->document->Main()));
        });
    }

    const char* occt_ocaf_xde_dgts_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { session->requireDocument(); return session->entry(XCAFDoc_DocumentTool::DGTsLabel(session->document->Main())); });
    }

    const char* occt_ocaf_xde_views_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { session->requireDocument(); return session->entry(XCAFDoc_DocumentTool::ViewsLabel(session->document->Main())); });
    }

    const char* occt_ocaf_xde_clipping_planes_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { session->requireDocument(); return session->entry(XCAFDoc_DocumentTool::ClippingPlanesLabel(session->document->Main())); });
    }

    const char* occt_ocaf_xde_notes_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { session->requireDocument(); return session->entry(XCAFDoc_DocumentTool::NotesLabel(session->document->Main())); });
    }

    const char* occt_ocaf_xde_visual_materials_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { session->requireDocument(); return session->entry(XCAFDoc_DocumentTool::VisMaterialLabel(session->document->Main())); });
    }

    int occt_ocaf_xde_get_length_unit(OcctOcafHandle handle, double* meters)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || meters == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            session->requireDocument();
            found = XCAFDoc_DocumentTool::GetLengthUnit(session->document, *meters) ? 1 : 0;
        })) return 0;
        return found;
    }

    int occt_ocaf_xde_set_length_unit(OcctOcafHandle handle, double meters)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            if (meters <= 0.0) throw std::invalid_argument("Length unit must be positive.");
            XCAFDoc_DocumentTool::SetLengthUnit(session->document, meters);
        });
    }

    const char* occt_ocaf_xde_add_shape(OcctOcafHandle handle, OcctModelHandle model, OcctObjectId shapeId, int makeAssembly, int makePrepare)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            const TDF_Label label = session->shapeTool()->AddShape(modelShape(model, shapeId), makeAssembly != 0, makePrepare != 0);
            if (label.IsNull()) throw std::runtime_error("XDE shape creation failed.");
            return session->entry(label);
        });
    }

    int occt_ocaf_xde_set_shape(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId shapeId)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->shapeTool()->SetShape(session->resolve(entry), modelShape(model, shapeId)); });
    }

    OcctObjectId occt_ocaf_xde_get_shape(OcctOcafHandle handle, OcctModelHandle model, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        OcctObjectId result = 0;
        execute(session, [&]
        {
            const TopoDS_Shape shape = XCAFDoc_ShapeTool::GetShape(session->resolve(entry));
            if (shape.IsNull()) throw std::runtime_error("XDE label does not contain a shape.");
            result = addModelShape(model, shape);
        });
        return result;
    }

    int occt_ocaf_xde_remove_shape(OcctOcafHandle handle, const char* entry, int removeCompletely)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int removed = 0;
        if (!execute(session, [&] { removed = session->shapeTool()->RemoveShape(session->resolve(entry), removeCompletely != 0) ? 1 : 0; })) return 0;
        return removed;
    }

    const char* occt_ocaf_xde_find_shape(OcctOcafHandle handle, OcctModelHandle model, OcctObjectId shapeId, int findInstance)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            const TDF_Label label = session->shapeTool()->FindShape(modelShape(model, shapeId), findInstance != 0);
            return session->entry(label);
        });
    }

    int occt_ocaf_xde_shape_snapshot(OcctOcafHandle handle, int freeOnly)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            if (freeOnly != 0) session->shapeTool()->GetFreeShapes(labels);
            else session->shapeTool()->GetShapes(labels);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_shape_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->stringSnapshot.size())) return "";
        return session->stringSnapshot[static_cast<std::size_t>(index)].c_str();
    }

    int occt_ocaf_xde_component_snapshot(OcctOcafHandle handle, const char* assemblyEntry, int recursive)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            if (!XCAFDoc_ShapeTool::GetComponents(session->resolve(assemblyEntry), labels, recursive != 0))
                throw std::runtime_error("Label is not an XDE assembly.");
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_component_at(OcctOcafHandle handle, int index)
    {
        return occt_ocaf_xde_shape_at(handle, index);
    }

    const char* occt_ocaf_xde_add_component(OcctOcafHandle handle, const char* assemblyEntry, const char* componentEntry, const OcctModelLocation* location)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            const TopLoc_Location componentLocation(location == nullptr ? gp_Trsf() : transformOf(*location));
            const TDF_Label label = session->shapeTool()->AddComponent(
                session->resolve(assemblyEntry), session->resolve(componentEntry), componentLocation);
            if (label.IsNull()) throw std::runtime_error("XDE component creation failed.");
            return session->entry(label);
        });
    }

    int occt_ocaf_xde_remove_component(OcctOcafHandle handle, const char* componentEntry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->shapeTool()->RemoveComponent(session->resolve(componentEntry)); });
    }

    const char* occt_ocaf_xde_referred_shape(OcctOcafHandle handle, const char* componentEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            TDF_Label referred;
            if (!XCAFDoc_ShapeTool::GetReferredShape(session->resolve(componentEntry), referred))
                throw std::runtime_error("Label is not an XDE shape reference.");
            return session->entry(referred);
        });
    }

    int occt_ocaf_xde_get_location(OcctOcafHandle handle, const char* componentEntry, OcctModelLocation* location)
    {
        OcafSession* session = sessionOf(handle);
        if (location == nullptr) return 0;
        return execute(session, [&] { fillLocation(XCAFDoc_ShapeTool::GetLocation(session->resolve(componentEntry)), *location); });
    }

    const char* occt_ocaf_xde_set_location(OcctOcafHandle handle, const char* componentEntry, const OcctModelLocation* location)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            if (location == nullptr) throw std::invalid_argument("Location must not be null.");
            TDF_Label reference;
            if (!session->shapeTool()->SetLocation(session->resolve(componentEntry), TopLoc_Location(transformOf(*location)), reference))
                throw std::runtime_error("XDE shape location update failed.");
            return session->entry(reference);
        });
    }

    int occt_ocaf_xde_update_assemblies(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->shapeTool()->UpdateAssemblies(); });
    }

    int occt_ocaf_xde_is_shape(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ShapeTool::IsShape(label); });
    }

    int occt_ocaf_xde_is_simple_shape(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ShapeTool::IsSimpleShape(label); });
    }

    int occt_ocaf_xde_is_assembly(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ShapeTool::IsAssembly(label); });
    }

    int occt_ocaf_xde_is_component(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ShapeTool::IsComponent(label); });
    }

    int occt_ocaf_xde_is_reference(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ShapeTool::IsReference(label); });
    }

    int occt_ocaf_xde_is_free(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ShapeTool::IsFree(label); });
    }

    int occt_ocaf_xde_is_subshape(OcctOcafHandle handle, const char* entry)
    {
        return classify(sessionOf(handle), entry, [](const TDF_Label& label) { return XCAFDoc_ShapeTool::IsSubShape(label); });
    }

}
