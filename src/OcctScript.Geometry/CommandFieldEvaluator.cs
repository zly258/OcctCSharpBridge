using OcctScript.Domain;
using OcctScript.Expressions;

namespace OcctScript.Geometry;

public sealed class CommandFieldEvaluator(ExpressionEngine expressionEngine)
{
    public double RequiredNumber(
        ScriptCommand command,
        string fieldName,
        IReadOnlyDictionary<string, double> parameters,
        double? minimum = null,
        double? maximum = null)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!command.Fields.TryGetValue(fieldName, out var field))
            throw new InvalidOperationException($"Required field '{fieldName}' is missing from command '{command.Name}'.");

        var result = expressionEngine.Evaluate(field.Expression, parameters);
        if (!result.Success)
            throw new InvalidOperationException($"Field '{fieldName}' of command '{command.Name}' is invalid: {result.Error}");

        if (minimum.HasValue && result.Value < minimum.Value)
            throw new InvalidOperationException($"Field '{fieldName}' of command '{command.Name}' must be greater than or equal to {minimum.Value}.");

        if (maximum.HasValue && result.Value > maximum.Value)
            throw new InvalidOperationException($"Field '{fieldName}' of command '{command.Name}' must be less than or equal to {maximum.Value}.");

        return result.Value;
    }
}
