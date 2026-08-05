#pragma once

#include "OcctOcaf.h"

extern "C"
{
    // Advanced TDocStd document state and delta management.
    OCCTBRIDGE_API int occt_ocaf_storage_format_version(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_set_storage_format_version(OcctOcafHandle handle, int version);
    OCCTBRIDGE_API int occt_ocaf_mark_modified(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_purge_modified(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_modified_snapshot(OcctOcafHandle handle);
    OCCTBRIDGE_API const char* occt_ocaf_modified_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API int occt_ocaf_init_delta_compaction(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_perform_delta_compaction(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_remove_first_undo(OcctOcafHandle handle);

    // Extended TDF label information.
    OCCTBRIDGE_API int occt_ocaf_label_child_count(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_attribute_count(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_transaction(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_may_be_modified(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_attributes_modified(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_label_is_descendant(OcctOcafHandle handle, const char* entry, const char* ancestorEntry);

    // TDataStd variables, expressions and relations.
    OCCTBRIDGE_API int occt_ocaf_set_variable(OcctOcafHandle handle, const char* entry, const char* utf8Name,
                                               double value, const char* utf8Unit, int isConstant);
    OCCTBRIDGE_API int occt_ocaf_get_variable(OcctOcafHandle handle, const char* entry, const char** utf8Name,
                                               double* value, const char** utf8Unit, int* isConstant,
                                               int* isValued, int* isAssigned);
    OCCTBRIDGE_API int occt_ocaf_assign_variable_expression(OcctOcafHandle handle, const char* variableEntry,
                                                            const char* utf8Expression,
                                                            const char* const* variableEntries, int variableCount);
    OCCTBRIDGE_API int occt_ocaf_desassign_variable(OcctOcafHandle handle, const char* variableEntry);
    OCCTBRIDGE_API int occt_ocaf_set_expression(OcctOcafHandle handle, const char* entry, const char* utf8Expression,
                                                 const char* const* variableEntries, int variableCount);
    OCCTBRIDGE_API int occt_ocaf_get_expression(OcctOcafHandle handle, const char* entry, const char** utf8Expression);
    OCCTBRIDGE_API int occt_ocaf_set_relation(OcctOcafHandle handle, const char* entry, const char* utf8Relation,
                                               const char* const* variableEntries, int variableCount);
    OCCTBRIDGE_API int occt_ocaf_get_relation(OcctOcafHandle handle, const char* entry, const char** utf8Relation);
    OCCTBRIDGE_API int occt_ocaf_expression_variable_snapshot(OcctOcafHandle handle, const char* entry, int relation);
    OCCTBRIDGE_API const char* occt_ocaf_expression_variable_at(OcctOcafHandle handle, int index);

    // Extended XDE shape and assembly queries.
    OCCTBRIDGE_API const char* occt_ocaf_xde_new_shape(OcctOcafHandle handle);
    OCCTBRIDGE_API int occt_ocaf_xde_is_top_level(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_is_compound(OcctOcafHandle handle, const char* entry);
    OCCTBRIDGE_API int occt_ocaf_xde_component_count(OcctOcafHandle handle, const char* entry, int recursive);
    OCCTBRIDGE_API int occt_ocaf_xde_user_snapshot(OcctOcafHandle handle, const char* entry, int recursive);
    OCCTBRIDGE_API const char* occt_ocaf_xde_user_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API const char* occt_ocaf_xde_search_shape(OcctOcafHandle handle, OcctModelHandle model,
                                                          OcctObjectId shapeId, int findInstance,
                                                          int findComponent, int findSubshape);
    OCCTBRIDGE_API const char* occt_ocaf_xde_find_subshape(OcctOcafHandle handle, OcctModelHandle model,
                                                           const char* shapeEntry, OcctObjectId subshapeId);
    OCCTBRIDGE_API const char* occt_ocaf_xde_add_subshape(OcctOcafHandle handle, OcctModelHandle model,
                                                          const char* shapeEntry, OcctObjectId subshapeId);
    OCCTBRIDGE_API int occt_ocaf_xde_subshape_snapshot(OcctOcafHandle handle, const char* shapeEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_subshape_at(OcctOcafHandle handle, int index);

    // Extended XDE color workflows, including instance colors.
    OCCTBRIDGE_API const char* occt_ocaf_xde_add_color(OcctOcafHandle handle, OcctOcafColor color);
    OCCTBRIDGE_API const char* occt_ocaf_xde_find_color(OcctOcafHandle handle, OcctOcafColor color);
    OCCTBRIDGE_API int occt_ocaf_xde_is_color(OcctOcafHandle handle, const char* colorEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_color_is_set(OcctOcafHandle handle, const char* entry, int colorType);
    OCCTBRIDGE_API const char* occt_ocaf_xde_color_label(OcctOcafHandle handle, const char* entry, int colorType);
    OCCTBRIDGE_API int occt_ocaf_xde_set_color_label(OcctOcafHandle handle, const char* entry,
                                                      const char* colorEntry, int colorType);
    OCCTBRIDGE_API int occt_ocaf_xde_set_instance_color(OcctOcafHandle handle, OcctModelHandle model,
                                                        OcctObjectId shapeId, int colorType,
                                                        OcctOcafColor color, int createShuo);
    OCCTBRIDGE_API int occt_ocaf_xde_get_instance_color(OcctOcafHandle handle, OcctModelHandle model,
                                                        OcctObjectId shapeId, int colorType,
                                                        OcctOcafColor* color);
    OCCTBRIDGE_API int occt_ocaf_xde_is_instance_visible(OcctOcafHandle handle, OcctModelHandle model,
                                                         OcctObjectId shapeId);

    // Extended XDE layer and material workflows.
    OCCTBRIDGE_API const char* occt_ocaf_xde_find_layer(OcctOcafHandle handle, const char* utf8Name,
                                                        int findWithProperty, int findVisible);
    OCCTBRIDGE_API int occt_ocaf_xde_is_layer(OcctOcafHandle handle, const char* layerEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_layer_is_set(OcctOcafHandle handle, const char* shapeEntry,
                                                  const char* layerEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_layer_shape_snapshot(OcctOcafHandle handle, const char* layerEntry);
    OCCTBRIDGE_API const char* occt_ocaf_xde_layer_shape_at(OcctOcafHandle handle, int index);
    OCCTBRIDGE_API const char* occt_ocaf_xde_add_material(OcctOcafHandle handle, const char* utf8Name,
                                                          const char* utf8Description, double density,
                                                          const char* utf8DensityName,
                                                          const char* utf8DensityValueType);
    OCCTBRIDGE_API int occt_ocaf_xde_is_material(OcctOcafHandle handle, const char* materialEntry);
    OCCTBRIDGE_API int occt_ocaf_xde_assign_material(OcctOcafHandle handle, const char* shapeEntry,
                                                     const char* materialEntry);
}
