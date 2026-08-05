using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class OcafNativeMethods
{
    private const string LibraryName = "OcctNative";

    static OcafNativeMethods()
    {
        OcctRuntime.Configure();
        RuntimeHelpers.RunClassConstructor(typeof(NativeMethods).TypeHandle);
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_create();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern void occt_ocaf_destroy(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_last_error(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_version();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_capabilities();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_new_document(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string format);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_open_document(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_save_document(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_save_as(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_close_document(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_is_open(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_document_path(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_storage_format(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_change_storage_format(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string format);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_is_saved(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_is_changed(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_is_empty(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_is_valid(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_document_json(IntPtr handle, int depth);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_new_command(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_open_command(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_commit_command(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_abort_command(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_has_open_command(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_undo_limit(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_undo_limit(IntPtr handle, int limit);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_available_undos(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_available_redos(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_undo(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_redo(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_clear_undos(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_clear_redos(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_nested_transaction_mode(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_nested_transaction_mode(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_modification_mode(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_modification_mode(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_empty_labels_saving_mode(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_empty_labels_saving_mode(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_root_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_main_entry(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_exists(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_create_label(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_new_child(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string parentEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_find_child(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string parentEntry, int tag, int create);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_father(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_tag(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_depth(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_is_root(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_label_is_imported(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_label_imported(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int imported);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_child_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int recursive);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_child_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_attribute_snapshot(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int includeForgotten);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_attribute_type_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_attribute_guid_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_attribute_json_at(IntPtr handle, int index, int depth);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_forget_attribute(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string guid);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_forget_all_attributes(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int clearChildren);
}
