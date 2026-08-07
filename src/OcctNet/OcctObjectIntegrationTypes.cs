using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
public enum OcctShapeUpdateOptions
{
    None = 0,
    PreserveAppearance = 1 << 0,
    PreserveTransformation = 1 << 1,
    PreserveSelection = 1 << 2,
    PreserveSelectability = 1 << 3,
    RecomputePresentation = 1 << 4,
    RecomputeSelection = 1 << 5,
    PreserveAll = PreserveAppearance
        | PreserveTransformation
        | PreserveSelection
        | PreserveSelectability
        | RecomputePresentation
        | RecomputeSelection
}

public enum OcctSelectionOperation
{
    Replace = 0,
    Add = 1,
    Remove = 2,
    Toggle = 3,
    Clear = 4
}

public enum OcctViewCubeLanguage
{
    English = 0,
    ChineseSimplified = 1
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct OcctTransform3d(
    double M00, double M01, double M02, double M03,
    double M10, double M11, double M12, double M13,
    double M20, double M21, double M22, double M23)
{
    public static OcctTransform3d Identity => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0);

    public static OcctTransform3d Translation(double x, double y, double z) => new(
        1, 0, 0, x,
        0, 1, 0, y,
        0, 0, 1, z);

    internal double[] ToArray() =>
    [
        M00, M01, M02, M03,
        M10, M11, M12, M13,
        M20, M21, M22, M23
    ];

    internal static OcctTransform3d FromArray(IReadOnlyList<double> values)
    {
        if (values.Count != 12) throw new ArgumentException("A 3x4 transformation matrix requires 12 values.", nameof(values));
        return new(
            values[0], values[1], values[2], values[3],
            values[4], values[5], values[6], values[7],
            values[8], values[9], values[10], values[11]);
    }
}

public readonly record struct OcctObjectTransformUpdate(
    IOcctObject Object,
    OcctTransform3d Transformation);
