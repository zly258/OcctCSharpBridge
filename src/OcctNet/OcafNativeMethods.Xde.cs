using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class OcafNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_shapes_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_colors_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_layers_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_materials_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_dgts_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_views_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_clipping_planes_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_notes_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_visual_materials_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_length_unit(IntPtr handle, out double meters);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_length_unit(IntPtr handle, double meters);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_add_shape(IntPtr handle, IntPtr model, long shapeId, int makeAssembly, int makePrepare);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_shape(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_ocaf_xde_get_shape(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_remove_shape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int removeCompletely);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_find_shape(IntPtr handle, IntPtr model, long shapeId, int findInstance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_shape_snapshot(IntPtr handle, int freeOnly);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_shape_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_component_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string assemblyEntry, int recursive);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_component_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_add_component(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string assemblyEntry, [MarshalAs(UnmanagedType.LPUTF8Str)] string componentEntry, in OcctModelLocation location);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_remove_component(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string componentEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_referred_shape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string componentEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_location(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string componentEntry, out OcctModelLocation location);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_set_location(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string componentEntry, in OcctModelLocation location);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_update_assemblies(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_shape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_simple_shape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_assembly(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_component(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_reference(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_free(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_subshape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_color_snapshot(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_color_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_color_definition(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string colorEntry, out OcafColor color);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_remove_color(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string colorEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_color(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int colorType, OcafColor color);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_color(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int colorType, out OcafColor color);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_unset_color(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int colorType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_visibility(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int visible);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_visible(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_color_by_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_color_by_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_add_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int findVisible);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_remove_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_layer_name(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_layer_snapshot(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_layer_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry, int shapeInOneLayer);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_unset_layer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_unset_layers(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_shape_layer_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_layer_visibility(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry, int visible);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_is_layer_visible(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string layerEntry);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_material(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string description, double density, [MarshalAs(UnmanagedType.LPUTF8Str)] string densityName, [MarshalAs(UnmanagedType.LPUTF8Str)] string densityValueType);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_material_for_shape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_material_snapshot(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_material_name(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string materialEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_material_description(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string materialEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern double occt_ocaf_xde_material_density(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string materialEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_material_density_name(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string materialEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_material_density_value_type(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string materialEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern double occt_ocaf_xde_density_for_shape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_xde_material_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_area(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, double area);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_area(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, out double area);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_volume(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, double volume);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_volume(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, out double volume);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_set_centroid(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, OcctPoint3d centroid);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_xde_get_centroid(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeEntry, out OcctPoint3d centroid);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_import_step(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_export_step(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_import_iges(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_export_iges(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
}
