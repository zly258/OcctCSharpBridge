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

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
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

    public bool IsFinite =>
        double.IsFinite(M00) && double.IsFinite(M01) && double.IsFinite(M02) && double.IsFinite(M03) &&
        double.IsFinite(M10) && double.IsFinite(M11) && double.IsFinite(M12) && double.IsFinite(M13) &&
        double.IsFinite(M20) && double.IsFinite(M21) && double.IsFinite(M22) && double.IsFinite(M23);
}

public readonly record struct OcctObjectTransformUpdate(
    IOcctObject Object,
    OcctTransform3d Transformation);
