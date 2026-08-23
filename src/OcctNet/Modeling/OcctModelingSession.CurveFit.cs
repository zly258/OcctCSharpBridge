using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>Curve fitting operations.</summary>
public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Fits a BSpline curve through or near the given points.
    /// </summary>
    public OcctModelShape FitBSplineCurve(
        IEnumerable<OcctPoint3d> points,
        int degMin = 3,
        int degMax = 8,
        OcctContinuity continuity = OcctContinuity.C1,
        double tolerance = 0.0,
        bool periodic = false)
    {
        EnsureNotDisposed();
        var pointArr = RequiredArray(points, nameof(points));
        var opts = new NativeFitBSplineOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeFitBSplineOptions>(),
            ApiVersion = 1,
            DegMin = degMin,
            DegMax = degMax,
            Continuity = (int)continuity,
            Tolerance = tolerance,
            Periodic = periodic ? 1 : 0
        };
        var status = ModelNativeMethods.occt_model_curve_fit_bspline(
            _handle, pointArr, pointArr.Length, in opts, out var id);
        return CheckShape(status, id);
    }
}
