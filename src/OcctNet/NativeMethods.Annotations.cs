namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_text_shape(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, OcctPoint3d position, OcctVector3d normal, OcctVector3d xDirection, double height, double extrusionDepth, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName, int bold, int italic);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_length_annotation_shape(IntPtr handle, long edgeId, double flyout, double textHeight, double arrowSize, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_angle_annotation_shape(IntPtr handle, long firstEdgeId, long secondEdgeId, double radius, double textHeight, double arrowSize, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_radius_annotation_shape(IntPtr handle, long circularEdgeId, double flyout, double textHeight, double arrowSize, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_diameter_annotation_shape(IntPtr handle, long circularEdgeId, double flyout, double textHeight, double arrowSize, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_add_text(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, OcctPoint3d position, double height, double r, double g, double b, int zoomable);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_text(IntPtr handle, long textId, [MarshalAs(UnmanagedType.LPUTF8Str)] string text);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_text_position(IntPtr handle, long textId, OcctPoint3d position);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_text_height(IntPtr handle, long textId, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_text_font(IntPtr handle, long textId, [MarshalAs(UnmanagedType.LPUTF8Str)] string fontName);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_text_angle(IntPtr handle, long textId, double angleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_text_zoomable(IntPtr handle, long textId, int zoomable);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_set_dimension_flyout(IntPtr handle, long dimensionId, double flyout);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_add_length_dimension(IntPtr handle, long edgeId, double flyout, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_add_angle_dimension(IntPtr handle, long firstEdgeId, long secondEdgeId, double flyout, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_add_radius_dimension(IntPtr handle, long circularShapeId, double flyout, double r, double g, double b);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_add_diameter_dimension(IntPtr handle, long circularShapeId, double flyout, double r, double g, double b);
}
