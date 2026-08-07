using OcctNet;
using OcctScript.Domain;
using OcctScript.Expressions;

namespace OcctScript.Geometry;

public sealed class CommandFieldEvaluator(ExpressionEngine expressionEngine)
{
    public double RequiredNumber(ScriptCommand command, string fieldName, IReadOnlyDictionary<string, double> parameters, double? minimum = null, double? maximum = null)
    {
        var text = RequiredText(command, fieldName, preferExpression: true);
        var result = expressionEngine.Evaluate(text, parameters);
        if (!result.Success) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}': {result.Error}");
        if (minimum.HasValue && result.Value < minimum.Value) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must be at least {minimum.Value:G}.");
        if (maximum.HasValue && result.Value > maximum.Value) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must be at most {maximum.Value:G}.");
        return result.Value;
    }

    public int RequiredInteger(ScriptCommand command, string fieldName, IReadOnlyDictionary<string, double> parameters, int? minimum = null, int? maximum = null)
    {
        var number = RequiredNumber(command, fieldName, parameters);
        if (!double.IsFinite(number)) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must be a finite integer.");
        var rounded = Math.Round(number);
        if (Math.Abs(number - rounded) > 1e-9 || rounded < int.MinValue || rounded > int.MaxValue)
            throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must be an integer.");
        var value = checked((int)rounded);
        if (minimum.HasValue && value < minimum.Value) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must be at least {minimum.Value}.");
        if (maximum.HasValue && value > maximum.Value) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must be at most {maximum.Value}.");
        return value;
    }

    public IReadOnlyList<int> RequiredIntegers(ScriptCommand command, string fieldName, int minimumCount = 1)
    {
        var text = RequiredText(command, fieldName, preferExpression: false);
        var tokens = text.Split([',', ';', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var values = new List<int>(tokens.Length);
        foreach (var token in tokens)
        {
            if (!int.TryParse(token, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var value) || value < 0)
                throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' contains invalid non-negative index '{token}'.");
            if (!values.Contains(value)) values.Add(value);
        }
        if (values.Count < minimumCount) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' requires at least {minimumCount} index value(s).");
        return values;
    }

    public bool RequiredBoolean(ScriptCommand command, string fieldName)
    {
        var text = RequiredText(command, fieldName, preferExpression: false);
        if (bool.TryParse(text, out var result)) return result;
        if (text == "1") return true;
        if (text == "0") return false;
        throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must be true or false.");
    }

    public OcctPoint3d RequiredPoint(ScriptCommand command, string fieldName, IReadOnlyDictionary<string, double> parameters)
    {
        var values = EvaluateComponents(command, fieldName, 3, parameters);
        return new OcctPoint3d(values[0], values[1], values[2]);
    }

    public OcctVector3d RequiredVector(ScriptCommand command, string fieldName, IReadOnlyDictionary<string, double> parameters, bool normalize = false)
    {
        var values = EvaluateComponents(command, fieldName, 3, parameters);
        var vector = new OcctVector3d(values[0], values[1], values[2]);
        if (vector.Length <= 1e-12) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' must not be a zero vector.");
        return normalize ? vector.Normalized() : vector;
    }

    public IReadOnlyList<OcctPoint3d> RequiredPoints(ScriptCommand command, string fieldName, IReadOnlyDictionary<string, double> parameters, int minimumCount = 2)
    {
        var text = RequiredText(command, fieldName, preferExpression: false);
        var pointTokens = text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (pointTokens.Length < minimumCount) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' requires at least {minimumCount} points.");
        var result = new List<OcctPoint3d>(pointTokens.Length);
        for (var index = 0; index < pointTokens.Length; index++)
        {
            var values = EvaluateComponents(command.Name, $"{fieldName}[{index}]", pointTokens[index], 3, parameters);
            result.Add(new OcctPoint3d(values[0], values[1], values[2]));
        }
        return result;
    }

    private double[] EvaluateComponents(ScriptCommand command, string fieldName, int expectedCount, IReadOnlyDictionary<string, double> parameters) =>
        EvaluateComponents(command.Name, fieldName, RequiredText(command, fieldName, preferExpression: false), expectedCount, parameters);

    private double[] EvaluateComponents(string commandName, string fieldName, string text, int expectedCount, IReadOnlyDictionary<string, double> parameters)
    {
        var parts = text.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != expectedCount) throw new InvalidOperationException($"Command '{commandName}' field '{fieldName}' requires {expectedCount} comma-separated values.");
        var values = new double[expectedCount];
        for (var index = 0; index < parts.Length; index++)
        {
            var evaluated = expressionEngine.Evaluate(parts[index], parameters);
            if (!evaluated.Success) throw new InvalidOperationException($"Command '{commandName}' field '{fieldName}' component {index + 1}: {evaluated.Error}");
            values[index] = evaluated.Value;
        }
        return values;
    }

    private static string RequiredText(ScriptCommand command, string fieldName, bool preferExpression)
    {
        if (!command.Fields.TryGetValue(fieldName, out var field)) throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' is missing.");
        var value = preferExpression ? field.Expression.NullIfWhiteSpace() ?? field.Literal.NullIfWhiteSpace() : field.Literal.NullIfWhiteSpace() ?? field.Expression.NullIfWhiteSpace();
        return value ?? throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' is empty.");
    }
}

internal static class StringValueExtensions
{
    public static string? NullIfWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
