using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>Advanced pipe shell sweep operations.</summary>
public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Sweeps one or more profiles along a spine wire using BRepOffsetAPI_MakePipeShell
    /// with configurable Frenet frame mode.
    /// </summary>
    public OcctModelAlgorithmResult SweepPipeShell(
        OcctModelShape spineWire,
        IEnumerable<OcctModelShape> profiles,
        OcctPipeShellMode mode = OcctPipeShellMode.CorrectedFrenet,
        bool solid = false,
        bool forceC1 = false,
        OcctVector3d fixedNormal = default)
    {
        EnsureShape(spineWire);
        var profileArr = ShapeIds(profiles);
        var opts = new NativePipeShellOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativePipeShellOptions>(),
            ApiVersion = 1,
            Mode = (int)mode,
            ForceC1 = forceC1 ? 1 : 0,
            MakeSolid = solid ? 1 : 0,
            FixedNormal = fixedNormal
        };
        var status = ModelNativeMethods.occt_model_feature_pipe_shell_execute(
            NativeHandle, spineWire.Id, profileArr, profileArr.Length, in opts, out var r);
        return CheckAlgorithm(status, r);
    }
}
