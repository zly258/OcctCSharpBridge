using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public int ViewClipPlaneLimit
    {
        get
        {
            EnsureInitialized();
            CheckViewportStatus(ViewportNativeMethods.occt_engine_viewport_clip_plane_limit_get(
                _handle,
                out var limit));
            if (limit < 0) throw new InvalidOperationException("Native view clip plane limit is invalid.");
            return limit;
        }
    }

    public void SetViewClipPlanes(IReadOnlyList<OcctViewClipPlane> planes)
    {
        ArgumentNullException.ThrowIfNull(planes);
        EnsureInitialized();
        if (planes.Count > ViewClipPlaneLimit)
            throw new ArgumentException("Clip plane count exceeds the current view limit.", nameof(planes));

        var native = new NativeOcctViewClipPlane[planes.Count];
        for (var index = 0; index < planes.Count; index++)
        {
            var plane = planes[index] ?? throw new ArgumentException("Clip plane entries must not be null.", nameof(planes));
            OcctGuard.Finite(plane.Point, nameof(planes));
            OcctGuard.NonZero(plane.Normal, nameof(planes));
            native[index] = new NativeOcctViewClipPlane
            {
                Point = plane.Point,
                Normal = plane.Normal,
                Enabled = plane.Enabled ? 1 : 0,
                Capping = plane.Capping ? 1 : 0,
                CappingR = plane.CappingColor.R / 255.0,
                CappingG = plane.CappingColor.G / 255.0,
                CappingB = plane.CappingColor.B / 255.0
            };
        }

        GCHandle pinned = default;
        try
        {
            var pointer = IntPtr.Zero;
            if (native.Length > 0)
            {
                pinned = GCHandle.Alloc(native, GCHandleType.Pinned);
                pointer = pinned.AddrOfPinnedObject();
            }
            CheckViewportStatus(ViewportNativeMethods.occt_engine_viewport_clip_planes_set(
                _handle,
                pointer,
                native.Length));
        }
        finally
        {
            if (pinned.IsAllocated) pinned.Free();
        }
    }

    public void ClearViewClipPlanes() => SetViewClipPlanes(Array.Empty<OcctViewClipPlane>());
}
