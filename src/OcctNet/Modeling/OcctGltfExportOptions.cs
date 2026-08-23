using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>Options for exporting geometry to glTF 2.0 format.</summary>
public sealed class OcctGltfExportOptions
{
    private const uint ApiVersion = 1;

    public static OcctGltfExportOptions Default => new();

    /// <summary>When true, writes binary .glb instead of text .gltf.</summary>
    public bool WriteBinary { get; set; } = false;

    /// <summary>When true, transforms coordinates to the glTF Y-up convention.</summary>
    public bool TransformToGltfCs { get; set; } = true;

    /// <summary>
    /// Mesh deflection. Values &lt;= 0 are treated as automatic (0.01).
    /// </summary>
    public double Deflection { get; set; } = 0.0;

    internal NativeGltfExportOptions ToNative() => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeGltfExportOptions>(),
        ApiVersion = ApiVersion,
        WriteBinary = WriteBinary ? 1 : 0,
        TransformToGltfCs = TransformToGltfCs ? 1 : 0,
        Deflection = Deflection
    };
}
