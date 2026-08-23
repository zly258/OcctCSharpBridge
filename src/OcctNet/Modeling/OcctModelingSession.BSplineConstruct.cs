using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>BSpline curve construction from explicit control-point/knot-vector definition.</summary>
public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Creates a BSpline curve edge from explicit poles, knot vector and optional weights.
    /// </summary>
    /// <param name="degree">Polynomial degree (>= 1).</param>
    /// <param name="poles">Control points (at least 2).</param>
    /// <param name="knots">Unique knot values in ascending order (at least 2).</param>
    /// <param name="multiplicities">Knot multiplicities (same count as <paramref name="knots"/>).</param>
    /// <param name="weights">Control point weights. Pass null for non-rational B-splines.</param>
    /// <param name="periodic">Whether the curve is periodic.</param>
    public OcctModelShape CreateBSplineCurve(
        int degree,
        IEnumerable<OcctPoint3d> poles,
        IEnumerable<double> knots,
        IEnumerable<int> multiplicities,
        IEnumerable<double>? weights = null,
        bool periodic = false)
    {
        EnsureNotDisposed();
        var poleArr = RequiredArray(poles, nameof(poles));
        var knotArr = RequiredArray(knots, nameof(knots));
        var multArr = RequiredArray(multiplicities, nameof(multiplicities));
        var weightArr = weights?.ToArray();

        var def = new NativeBSplineCurveDefinition
        {
            StructSize = (uint)Marshal.SizeOf<NativeBSplineCurveDefinition>(),
            ApiVersion = 1,
            Degree = degree,
            PoleCount = poleArr.Length,
            KnotCount = knotArr.Length,
            Rational = weightArr != null ? 1 : 0,
            Periodic = periodic ? 1 : 0
        };

        var status = ModelNativeMethods.occt_model_curve_bspline_explicit_create(
            _handle, in def, poleArr, weightArr, knotArr, multArr, out var id);
        return CheckShape(status, id);
    }

    public OcctModelShape CreateBSplineSurface(
        int uDegree, int vDegree,
        OcctPoint3d[,] poles,
        double[,]? weights,
        double[] uKnots, int[] uMults,
        double[] vKnots, int[] vMults,
        bool uPeriodic = false, bool vPeriodic = false)
    {
        EnsureNotDisposed();
        int uCount = poles.GetLength(0), vCount = poles.GetLength(1);
        var flatPoles = new OcctPoint3d[uCount * vCount];
        for (int u = 0; u < uCount; u++)
            for (int v = 0; v < vCount; v++)
                flatPoles[u * vCount + v] = poles[u, v];
        
        double[]? flatWeights = null;
        if (weights != null) {
            flatWeights = new double[uCount * vCount];
            for (int u = 0; u < uCount; u++)
                for (int v = 0; v < vCount; v++)
                    flatWeights[u * vCount + v] = weights[u, v];
        }
        var def = new NativeBSplineSurfaceDefinition {
            StructSize = (uint)Marshal.SizeOf<NativeBSplineSurfaceDefinition>(),
            ApiVersion = 1,
            UDegree = uDegree, VDegree = vDegree,
            UPoleCount = uCount, VPoleCount = vCount,
            UKnotCount = uKnots.Length, VKnotCount = vKnots.Length,
            URational = flatWeights != null ? 1 : 0, VRational = flatWeights != null ? 1 : 0,
            UPeriodic = uPeriodic ? 1 : 0, VPeriodic = vPeriodic ? 1 : 0
        };
        var status = ModelNativeMethods.occt_model_face_bspline_explicit_create(
            _handle, in def, flatPoles, flatWeights, uKnots, uMults, vKnots, vMults, out var id);
        return CheckShape(status, id);
    }
}
