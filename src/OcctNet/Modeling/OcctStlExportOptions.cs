using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>Options for exporting geometry to STL format.</summary>
public sealed class OcctStlExportOptions
{
    private const uint ApiVersion = 1;

    public static OcctStlExportOptions Default => new();

    public double LinearDeflection { get; set; } = 0.1;
    public double AngularDeflection { get; set; } = 0.5;
    public bool Ascii { get; set; } = false;

    internal NativeStlExportOptions ToNative() => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeStlExportOptions>(),
        ApiVersion = ApiVersion,
        LinearDeflection = LinearDeflection,
        AngularDeflection = AngularDeflection,
        Ascii = Ascii ? 1 : 0
    };
}
