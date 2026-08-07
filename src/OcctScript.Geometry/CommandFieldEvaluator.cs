using OcctScript.Domain;
using OcctScript.Expressions;

namespace OcctScript.Geometry;

public sealed class CommandFieldEvaluator(ExpressionEngine expressionEngine)
{
    public double EvaluateNumber(
        ScriptCommand command,
        string fieldName,
        IReadOnlyDictionary<string, double> parameters,
        double? minimum = null,
        double? maximum = null)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!command.Fields.TryGetValue(fieldName, out var field))
            throw new InvalidOperationException($"Command '{command.Name}' does not contain field '{fieldName}'.");
        if (string.IsNullOrWhiteSpace(field.Expression))
            throw new InvalidOperationException($"Field '{fieldName}' has no expression.");

        var result = expressionEngine.Evaluate(field.Expression, parameters);
        if (!result.Success)
            throw new InvalidOperationException($"Field '{fieldName}' is invalid: {result.Error}");
        if (minimum.HasValue && result.Value < minimum.Value)
            throw new InvalidOperationException($"Field '{fieldName}' must be at least {minimum.Value}.");
        if (maximum.HasValue && result.Value > maximum.Value)
            throw new InvalidOperationException($"Field '{fieldName}' must not exceed {maximum.Value}.");
        return result.Value;
    }

    public Guid EvaluateReference(ScriptCommand command, string fieldName)
    {
        if (!command.Fields.TryGetValue(fieldName, out var field) || field.ReferenceId is null)
            throw new InvalidOperationException($"Field '{fieldName}' does not contain an object reference.");
        return field.ReferenceId.Value;
    }
}
