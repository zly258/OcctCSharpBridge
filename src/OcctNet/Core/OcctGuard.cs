namespace OcctNet;

/// <summary>Common precondition helpers used throughout the bridge managed layer.</summary>
internal static class OcctGuard
{
    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="value"/> is not a finite number.</summary>
    internal static void Finite(double value, string parameterName)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
    }

    /// <summary>Throws <see cref="ArgumentException"/> when any coordinate of <paramref name="value"/> is not finite.</summary>
    internal static void Finite(OcctPoint3d value, string parameterName)
    {
        if (!value.IsFinite)
            throw new ArgumentException("Point coordinates must be finite.", parameterName);
    }

    /// <summary>Throws <see cref="ArgumentException"/> when any component of <paramref name="value"/> is not finite.</summary>
    internal static void Finite(OcctVector3d value, string parameterName)
    {
        if (!value.IsFinite)
            throw new ArgumentException("Vector components must be finite.", parameterName);
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="value"/> is not finite or is &lt;= 0.</summary>
    internal static void Positive(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite and greater than zero.");
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="value"/> is not finite or is &lt; 0.</summary>
    internal static void NonNegative(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite and greater than or equal to zero.");
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="value"/> is outside [0, 1].</summary>
    internal static void UnitInterval(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be between 0 and 1 inclusive.");
    }

    /// <summary>Throws <see cref="ArgumentException"/> when <paramref name="vector"/> is not finite or has near-zero length.</summary>
    internal static void NonZero(OcctVector3d vector, string parameterName)
    {
        if (!vector.IsFinite || vector.LengthSquared <= 1e-30)
            throw new ArgumentException("Vector must be finite and non-zero.", parameterName);
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="value"/> is negative.</summary>
    internal static void PositiveIndex(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Index must be zero or greater.");
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="value"/> is less than <paramref name="minimum"/>.</summary>
    internal static void AtLeast(int value, int minimum, string parameterName)
    {
        if (value < minimum)
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be at least {minimum}.");
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="value"/> is outside [<paramref name="min"/>, <paramref name="max"/>].</summary>
    internal static void InRange(double value, double min, double max, string parameterName)
    {
        if (!double.IsFinite(value) || value < min || value > max)
            throw new ArgumentOutOfRangeException(parameterName, value,
                $"Value must be between {min} and {max} inclusive.");
    }

    /// <summary>
    /// Returns <paramref name="value"/> when <paramref name="condition"/> is <see langword="true"/>,
    /// otherwise returns <paramref name="fallback"/>.
    /// For use only on diagnostic or best-effort code paths where failure must not propagate.
    /// </summary>
    internal static T FallbackIf<T>(bool condition, T value, T fallback)
        => condition ? value : fallback;
}
