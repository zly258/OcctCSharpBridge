using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class OcafNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_storage_format_version(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_storage_format_version(IntPtr handle, int version);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_mark_modified(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_purge_modified(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_modified_snapshot(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_modified_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_init_delta_compaction(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_perform_delta_compaction(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_remove_first_undo(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_child_count(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_attribute_count(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_transaction(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_may_be_modified(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_attributes_modified(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_is_descendant(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string ancestorEntry);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_variable(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, double value, [MarshalAs(UnmanagedType.LPUTF8Str)] string unit, int isConstant);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_variable(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out IntPtr name, out double value, out IntPtr unit, out int isConstant, out int isValued, out int isAssigned);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_assign_variable_expression(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string variableEntry, [MarshalAs(UnmanagedType.LPUTF8Str)] string expression, IntPtr[] variableEntries, int variableCount);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_desassign_variable(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string variableEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_expression(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string expression, IntPtr[] variableEntries, int variableCount);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_expression(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out IntPtr expression);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_relation(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string relation, IntPtr[] variableEntries, int variableCount);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_relation(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out IntPtr relation);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_expression_variable_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int relation);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_expression_variable_at(IntPtr handle, int index);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_new_shape(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_top_level(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_compound(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_component_count(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int recursive);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_user_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int recursive);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_user_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_search_shape(IntPtr handle, IntPtr model, long shapeId, int findInstance, int findComponent, int findSubshape);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_find_subshape(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, long subshapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_add_subshape(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, long subshapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_subshape_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_subshape_at(IntPtr handle, int index);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_add_color(IntPtr handle, OcafColor color);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_find_color(IntPtr handle, OcafColor color);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_color(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string colorEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_color_is_set(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int colorType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_color_label(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int colorType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_color_label(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string colorEntry, int colorType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_instance_color(IntPtr handle, IntPtr model, long shapeId, int colorType, OcafColor color, int createShuo);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_instance_color(IntPtr handle, IntPtr model, long shapeId, int colorType, out OcafColor color);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_instance_visible(IntPtr handle, IntPtr model, long shapeId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_find_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int findWithProperty, int findVisible);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_layer_is_set(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_layer_shape_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_layer_shape_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_add_material(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string description, double density, [MarshalAs(UnmanagedType.LPUTF8Str)] string densityName, [MarshalAs(UnmanagedType.LPUTF8Str)] string densityValueType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_material(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string materialEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_assign_material(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, [MarshalAs(UnmanagedType.LPUTF8Str)] string materialEntry);
}
