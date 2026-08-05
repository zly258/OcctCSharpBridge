#pragma once

#include "OcctModeling.h"

#include <cstdint>

extern "C"
{
    using OcctOcafHandle = void*;

    enum OcctOcafColorType
    {
        OcctOcafColor_General = 0,
        OcctOcafColor_Surface = 1,
        OcctOcafColor_Curve = 2
    };

    enum OcctOcafNamedShapeEvolution
    {
        OcctOcafEvolution_Primitive = 0,
        OcctOcafEvolution_Generated = 1,
        OcctOcafEvolution_Modify = 2,
        OcctOcafEvolution_Delete = 3,
        OcctOcafEvolution_Replace = 4,
        OcctOcafEvolution_Selected = 5,
        OcctOcafEvolution_Unknown = 6
    };

    struct OcctOcafColor
    {
        double red;
        double green;
        double blue;
        double alpha;
    };

    // Session, document, persistence, transactions and diagnostics.
    OCCTBRIDGE_API OcctOcafHandle occt_ocaf_create();
    OCCTBRIDGE_API void occt_ocaf_destroy(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_last_error(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_version();
    OCCTBRIDGE_API const char* occt_ocaf_capabilities();
    OCCTBRIDGE_API int occt_ocaf_new_document(OcctOcafHandle handle, const char* utf8Format);
    OCCTBRIDGE_API int occt_ocaf_open_document(OcctOcafHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_ocaf_save_document(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_save_as(OcctOcafHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_ocaf_close_document(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_is_open(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_document_path(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_storage_format(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_change_storage_format(OcctOcafHandle handle, const char* utf8Format);
    OCCTBRIDGE_API int occt_ocaf_is_saved(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_is_changed(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_is_empty(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_is_valid(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_document_json(OcctOcafHandle handle, int depth);
    OCCTBRIDGE_API int occt_ocaf_new_command(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_open_command(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_commit_command(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_abort_command(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_has_open_command(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_get_undo_limit(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_set_undo_limit(OcctOcafHandle handle, int limit);
    OCCTBRIDGE_API int occt_ocaf_available_undos(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_available_redos(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_undo(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_redo(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_clear_undos(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_clear_redos(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_set_nested_transaction_mode(OcctOcafHandle handle, int enabled);
    OCCTBRIDGE_API int occt_ocaf_nested_transaction_mode(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_set_modification_mode(OcctOcafHandle handle, int enabled);
    OCCTBRIDGE_API int occt_ocaf_modification_mode(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_set_empty_labels_saving_mode(OcctOcafHandle handle, int enabled);
    OCCTBRIDGE_API int occt_ocaf_empty_labels_saving_mode(OcctOcafHandle handle);

    // Labels and generic attribute inspection.
    OCCTBRIDGE_API const char* occt_ocaf_root_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_main_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_label_exists(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_create_label(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API const char* occt_ocaf_new_child(OcctOcafHandle handle, const char* parentEntry);
    OCCTBRIDGE_API const char* occt_ocaf_find_child(OcctOcafHandle handle, const char* parentEntry, int tag, int create);
    OCCTBRIDGE_API const char* occt_ocaf_father(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_tag(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_depth(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_is_root(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_is_imported(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_set_label_imported(OcctOcafHandle handle, const char* entry, int imported);
    OCCTBRIDGE_API int occt_ocaf_child_snapshot(OcctOcafHandle handle, const char* entry, int recursive);
    OCCTBRIDGE_API const char* occt_ocaf_child_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_attribute_snapshot(OcctOcafHandle handle, const char* entry, int includeForgotten);
    OCCTBRIDGE_API const char* occt_ocaf_attribute_type_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API const char* occt_ocaf_attribute_guid_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API const char* occt_ocaf_attribute_json_at(OcctOcafHandle handle, int index, int depth);
    OCCTBRIDGE_API int occt_ocaf_forget_attribute(OcctOcafHandle handle, const char* entry, const char* guid);
    OCCTBRIDGE_API int occt_ocaf_forget_all_attributes(OcctOcafHandle handle, const char* entry, int clearChildren);

    // Standard scalar, reference, collection and geometric attributes.
    OCCTBRIDGE_API int occt_ocaf_set_name(OcctOcafHandle handle, const char* entry, const char* utf8Value);
    OCCTBRIDGE_API int occt_ocaf_get_name(OcctOcafHandle handle, const char* entry, const char** utf8Value);
    OCCTBRIDGE_API int occt_ocaf_set_comment(OcctOcafHandle handle, const char* entry, const char* utf8Value);
    OCCTBRIDGE_API int occt_ocaf_get_comment(OcctOcafHandle handle, const char* entry, const char** utf8Value);
    OCCTBRIDGE_API int occt_ocaf_set_ascii_string(OcctOcafHandle handle, const char* entry, const char* utf8Value);
    OCCTBRIDGE_API int occt_ocaf_get_ascii_string(OcctOcafHandle handle, const char* entry, const char** utf8Value);
    OCCTBRIDGE_API int occt_ocaf_set_integer(OcctOcafHandle handle, const char* entry, int value);
    OCCTBRIDGE_API int occt_ocaf_get_integer(OcctOcafHandle handle, const char* entry, int* value);
    OCCTBRIDGE_API int occt_ocaf_set_real(OcctOcafHandle handle, const char* entry, double value);
    OCCTBRIDGE_API int occt_ocaf_get_real(OcctOcafHandle handle, const char* entry, double* value);
    OCCTBRIDGE_API int occt_ocaf_set_uattribute(OcctOcafHandle handle, const char* entry, const char* guid);
    OCCTBRIDGE_API int occt_ocaf_has_uattribute(OcctOcafHandle handle, const char* entry, const char* guid);
    OCCTBRIDGE_API int occt_ocaf_set_reference(OcctOcafHandle handle, const char* entry, const char* targetEntry);
    OCCTBRIDGE_API int occt_ocaf_get_reference(OcctOcafHandle handle, const char* entry, const char** targetEntry);
    OCCTBRIDGE_API int occt_ocaf_set_integer_array(OcctOcafHandle handle, const char* entry, const int* values, int count, int lower);
    OCCTBRIDGE_API int occt_ocaf_get_integer_array(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_set_real_array(OcctOcafHandle handle, const char* entry, const double* values, int count, int lower);
    OCCTBRIDGE_API int occt_ocaf_get_real_array(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_set_boolean_array(OcctOcafHandle handle, const char* entry, const int* values, int count, int lower);
    OCCTBRIDGE_API int occt_ocaf_get_boolean_array(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_set_byte_array(OcctOcafHandle handle, const char* entry, const unsigned char* values, int count, int lower);
    OCCTBRIDGE_API int occt_ocaf_get_byte_array(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_set_string_array(OcctOcafHandle handle, const char* entry, const char* const* utf8Values, int count, int lower);
    OCCTBRIDGE_API int occt_ocaf_get_string_array(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_array_lower(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_array_count(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_array_int_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API double occt_ocaf_array_real_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API const char* occt_ocaf_array_string_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_set_position(OcctOcafHandle handle, const char* entry, OcctPoint3d point);
    OCCTBRIDGE_API int occt_ocaf_get_position(OcctOcafHandle handle, const char* entry, OcctPoint3d* point);
    OCCTBRIDGE_API int occt_ocaf_set_shape_attribute(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId shapeId);
    OCCTBRIDGE_API OcctObjectId occt_ocaf_get_shape_attribute(OcctOcafHandle handle, OcctModelHandle model, const char* entry);

    // TNaming named-shape history and selection.
    OCCTBRIDGE_API int occt_ocaf_naming_generated(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId newShapeId);
    OCCTBRIDGE_API int occt_ocaf_naming_generated_from(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId oldShapeId, OcctObjectId newShapeId);
    OCCTBRIDGE_API int occt_ocaf_naming_modify(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId oldShapeId, OcctObjectId newShapeId);
    OCCTBRIDGE_API int occt_ocaf_naming_delete(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId oldShapeId);
    OCCTBRIDGE_API int occt_ocaf_naming_select(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId selectedShapeId, OcctObjectId contextShapeId);
    OCCTBRIDGE_API int occt_ocaf_named_shape_exists(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_named_shape_is_empty(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_named_shape_evolution(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_named_shape_version(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_set_named_shape_version(OcctOcafHandle handle, const char* entry, int version);
    OCCTBRIDGE_API OcctObjectId occt_ocaf_named_shape_get(OcctOcafHandle handle, OcctModelHandle model, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_named_shape_pair_snapshot(OcctOcafHandle handle, OcctModelHandle model, const char* entry);
    OCCTBRIDGE_API OcctObjectId occt_ocaf_named_shape_old_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API OcctObjectId occt_ocaf_named_shape_new_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_selector_select(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId selectedShapeId, OcctObjectId contextShapeId, int geometryMode);
    OCCTBRIDGE_API int occt_ocaf_selector_solve(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_selector_is_identified(OcctOcafHandle handle, const char* entry, OcctModelHandle model, OcctObjectId shapeId);

    // XDE shapes and assemblies.
    OCCTBRIDGE_API const char* occt_ocaf_xde_shapes_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_colors_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_layers_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_materials_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_dgts_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_views_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_clipping_planes_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_notes_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_visual_materials_entry(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_xde_get_length_unit(OcctOcafHandle handle, double* meters);
    OCCTBRIDGE_API int occt_ocaf_xde_set_length_unit(OcctOcafHandle handle, double meters);
    OCCTBRIDGE_API const char* occt_ocaf_xde_add_shape(OcctOcafHandle handle, OcctModelHandle model, OcctObjectId shapeId, int makeAssembly, int makePrepare);
    OCCTBRIDGE_API int occt_ocaf_xde_set_shape(OcctOcafHandle handle, OcctModelHandle model, const char* entry, OcctObjectId shapeId);
    OCCTBRIDGE_API OcctObjectId occt_ocaf_xde_get_shape(OcctOcafHandle handle, OcctModelHandle model, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_remove_shape(OcctOcafHandle handle, const char* entry, int removeCompletely);
    OCCTBRIDGE_API const char* occt_ocaf_xde_find_shape(OcctOcafHandle handle, OcctModelHandle model, OcctObjectId shapeId, int findInstance);
    OCCTBRIDGE_API int occt_ocaf_xde_shape_snapshot(OcctOcafHandle handle, int freeOnly);
    OCCTBRIDGE_API const char* occt_ocaf_xde_shape_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_xde_component_snapshot(OcctOcafHandle handle, const char* assemblyEntry, int recursive);
    OCCTBRIDGE_API const char* occt_ocaf_xde_component_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API const char* occt_ocaf_xde_add_component(OcctOcafHandle handle, const char* assemblyEntry, const char* componentEntry, const OcctModelLocation* location);
    OCCTBRIDGE_API int occt_ocaf_xde_remove_component(OcctOcafHandle handle, const char* componentEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_referred_shape(OcctOcafHandle handle, const char* componentEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_get_location(OcctOcafHandle handle, const char* componentEntry, OcctModelLocation* location);
    OCCTBRIDGE_API const char* occt_ocaf_xde_set_location(OcctOcafHandle handle, const char* componentEntry, const OcctModelLocation* location);
    OCCTBRIDGE_API int occt_ocaf_xde_update_assemblies(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_xde_is_shape(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_is_simple_shape(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_is_assembly(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_is_component(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_is_reference(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_is_free(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_is_subshape(OcctOcafHandle handle, const char* entry);

    // XDE colors, layers, materials and validation properties.
    OCCTBRIDGE_API int occt_ocaf_xde_color_snapshot(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_color_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_xde_get_color_definition(OcctOcafHandle handle, const char* colorEntry, OcctOcafColor* color);
    OCCTBRIDGE_API int occt_ocaf_xde_remove_color(OcctOcafHandle handle, const char* colorEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_set_color(OcctOcafHandle handle, const char* entry, int colorType, OcctOcafColor color);
    OCCTBRIDGE_API int occt_ocaf_xde_get_color(OcctOcafHandle handle, const char* entry, int colorType, OcctOcafColor* color);
    OCCTBRIDGE_API int occt_ocaf_xde_unset_color(OcctOcafHandle handle, const char* entry, int colorType);
    OCCTBRIDGE_API int occt_ocaf_xde_set_visibility(OcctOcafHandle handle, const char* entry, int visible);
    OCCTBRIDGE_API int occt_ocaf_xde_is_visible(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_set_color_by_layer(OcctOcafHandle handle, const char* entry, int enabled);
    OCCTBRIDGE_API int occt_ocaf_xde_is_color_by_layer(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_add_layer(OcctOcafHandle handle, const char* utf8Name, int findVisible);
    OCCTBRIDGE_API int occt_ocaf_xde_remove_layer(OcctOcafHandle handle, const char* layerEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_layer_name(OcctOcafHandle handle, const char* layerEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_layer_snapshot(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_layer_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_xde_set_layer(OcctOcafHandle handle, const char* shapeEntry, const char* layerEntry, int shapeInOneLayer);
    OCCTBRIDGE_API int occt_ocaf_xde_unset_layer(OcctOcafHandle handle, const char* shapeEntry, const char* layerEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_unset_layers(OcctOcafHandle handle, const char* shapeEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_shape_layer_snapshot(OcctOcafHandle handle, const char* shapeEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_set_layer_visibility(OcctOcafHandle handle, const char* layerEntry, int visible);
    OCCTBRIDGE_API int occt_ocaf_xde_is_layer_visible(OcctOcafHandle handle, const char* layerEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_set_material(OcctOcafHandle handle, const char* shapeEntry, const char* utf8Name, const char* utf8Description, double density, const char* utf8DensityName, const char* utf8DensityValueType);
    OCCTBRIDGE_API const char* occt_ocaf_xde_material_for_shape(OcctOcafHandle handle, const char* shapeEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_material_snapshot(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_xde_material_name(OcctOcafHandle handle, const char* materialEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_material_description(OcctOcafHandle handle, const char* materialEntry);
    OCCTBRIDGE_API double occt_ocaf_xde_material_density(OcctOcafHandle handle, const char* materialEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_material_density_name(OcctOcafHandle handle, const char* materialEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_material_density_value_type(OcctOcafHandle handle, const char* materialEntry);
    OCCTBRIDGE_API double occt_ocaf_xde_density_for_shape(OcctOcafHandle handle, const char* shapeEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_material_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_xde_set_area(OcctOcafHandle handle, const char* shapeEntry, double area);
    OCCTBRIDGE_API int occt_ocaf_xde_get_area(OcctOcafHandle handle, const char* shapeEntry, double* area);
    OCCTBRIDGE_API int occt_ocaf_xde_set_volume(OcctOcafHandle handle, const char* shapeEntry, double volume);
    OCCTBRIDGE_API int occt_ocaf_xde_get_volume(OcctOcafHandle handle, const char* shapeEntry, double* volume);
    OCCTBRIDGE_API int occt_ocaf_xde_set_centroid(OcctOcafHandle handle, const char* shapeEntry, OcctPoint3d centroid);
    OCCTBRIDGE_API int occt_ocaf_xde_get_centroid(OcctOcafHandle handle, const char* shapeEntry, OcctPoint3d* centroid);

    // Metadata-preserving XDE exchange.
    OCCTBRIDGE_API int occt_ocaf_import_step(OcctOcafHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_ocaf_export_step(OcctOcafHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_ocaf_import_iges(OcctOcafHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_ocaf_export_iges(OcctOcafHandle handle, const char* utf8Path);
}
