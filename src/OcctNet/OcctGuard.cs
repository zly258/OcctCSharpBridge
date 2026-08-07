namespace OcctNet;

internal static class OcctGuard
{
    internal static void Finite(double value, string parameterName)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
    }

    internal static void Finite(OcctPoint3d value, string parameterName)
    {
        if (!value.IsFinite)
            throw new ArgumentException("Point coordinates must be finite.", parameterName);
    }

    internal static void Finite(OcctVector3d value, string parameterName)
    {
        if (!value.IsFinite)
            throw new ArgumentException("Vector components must be finite.", parameterName);
    }

    internal static void Positive(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite and greater than zero.");
    }

    internal static void NonNegative(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite and greater than or equal to zero.");
    }

    internal static void UnitInterval(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be between 0 and 1 inclusive.");
    }

    internal static void NonZero(OcctVector3d vector, string parameterName)
    {
        if (!vector.IsFinite || vector.LengthSquared <= 1e-30)
            throw new ArgumentException("Vector must be finite and non-zero.", parameterName);
    }

    internal static void PositiveIndex(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Index must be zero or greater.");
    }

    internal static void AtLeast(int value, int minimum, string parameterName)
    {
        if (value < minimum)
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be at least {minimum}.");
    }
}
