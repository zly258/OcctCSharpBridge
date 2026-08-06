using OcctScript.Domain;
using OcctScript.Expressions;

namespace OcctScript.Application;

public sealed record ParameterEvaluationResult(
    IReadOnlyDictionary<string, double> Values,
    IReadOnlyDictionary<Guid, string> Errors);

public sealed class ParameterService(ExpressionEngine expressionEngine)
{
    public ParameterEvaluationResult Evaluate(ScriptDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var errors = new Dictionary<Guid, string>();
        var pending = document.Parameters
            .Where(x => x.Type is ScriptParameterType.Number or ScriptParameterType.Length or ScriptParameterType.Angle or ScriptParameterType.Integer)
            .ToList();

        for (var pass = 0; pass <= pending.Count && pending.Count > 0; pass++)
        {
            var resolvedThisPass = 0;
            for (var index = pending.Count - 1; index >= 0; index--)
            {
                var parameter = pending[index];
                var result = expressionEngine.Evaluate(parameter.Expression, values);
                if (!result.Success)
                {
                    errors[parameter.Id] = result.Error;
                    continue;
                }
                values[parameter.Name] = parameter.Type == ScriptParameterType.Integer
                    ? Math.Round(result.Value)
                    : result.Value;
                errors.Remove(parameter.Id);
                pending.RemoveAt(index);
                resolvedThisPass++;
            }
            if (resolvedThisPass == 0) break;
        }

        foreach (var parameter in pending)
        {
            errors[parameter.Id] = "Parameter dependency cannot be resolved. Check missing or circular references.";
        }

        return new ParameterEvaluationResult(values, errors);
    }
}
