using OcctScript.Domain;

namespace OcctScript.Application;

public enum ValidationSeverity
{
    Information,
    Warning,
    Error
}

public sealed record DocumentValidationMessage(
    string Code,
    ValidationSeverity Severity,
    string Message,
    Guid? ObjectId = null,
    string FieldName = "");

public sealed class DocumentValidator(CommandRegistry commandRegistry)
{
    public IReadOnlyList<DocumentValidationMessage> Validate(ScriptDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var messages = new List<DocumentValidationMessage>();

        foreach (var group in document.Parameters.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
        {
            foreach (var parameter in group)
                messages.Add(new("PARAMETER_NAME_DUPLICATE", ValidationSeverity.Error, $"Parameter name '{group.Key}' is duplicated.", parameter.Id));
        }

        foreach (var parameter in document.Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Name))
                messages.Add(new("PARAMETER_NAME_EMPTY", ValidationSeverity.Error, "Parameter name is empty.", parameter.Id));
            else if (!IsIdentifier(parameter.Name))
                messages.Add(new("PARAMETER_NAME_INVALID", ValidationSeverity.Error, $"Parameter name '{parameter.Name}' is not a valid identifier.", parameter.Id));
        }

        foreach (var command in document.Commands.OrderBy(x => x.Order))
        {
            CommandDefinition definition;
            try { definition = commandRegistry.GetRequired(command.Type); }
            catch (KeyNotFoundException)
            {
                messages.Add(new("COMMAND_TYPE_UNKNOWN", ValidationSeverity.Error, $"Command type '{command.Type}' is not registered.", command.Id));
                continue;
            }

            foreach (var field in definition.Fields.Where(x => x.IsRequired))
            {
                if (!command.Fields.TryGetValue(field.Name, out var value) ||
                    string.IsNullOrWhiteSpace(value.Expression) && value.ReferenceId is null && value.ReferenceIds.Count == 0 && string.IsNullOrWhiteSpace(value.Literal))
                {
                    messages.Add(new("COMMAND_FIELD_REQUIRED", ValidationSeverity.Error, $"Field '{field.Name}' is required.", command.Id, field.Name));
                }
            }
        }

        var commandIds = document.Commands.Select(x => x.Id).ToHashSet();
        foreach (var outputId in document.OutputCommandIds.Where(x => !commandIds.Contains(x)))
            messages.Add(new("OUTPUT_REFERENCE_MISSING", ValidationSeverity.Error, $"Output command '{outputId}' does not exist.", outputId));

        return messages;
    }

    private static bool IsIdentifier(string value)
    {
        if (value.Length == 0 || !(char.IsLetter(value[0]) || value[0] == '_')) return false;
        return value.Skip(1).All(x => char.IsLetterOrDigit(x) || x == '_');
    }
}
