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
        ValidateParameters(document, messages);
        ValidateCommands(document, messages);
        ValidateOutputs(document, messages);
        ValidateCycles(document, messages);
        return messages;
    }

    private static void ValidateParameters(ScriptDocument document, ICollection<DocumentValidationMessage> messages)
    {
        foreach (var group in document.Parameters.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
        foreach (var parameter in group)
            messages.Add(new("PARAMETER_NAME_DUPLICATE", ValidationSeverity.Error, $"Parameter name '{group.Key}' is duplicated.", parameter.Id));

        foreach (var parameter in document.Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Name))
                messages.Add(new("PARAMETER_NAME_EMPTY", ValidationSeverity.Error, "Parameter name is empty.", parameter.Id));
            else if (!IsIdentifier(parameter.Name))
                messages.Add(new("PARAMETER_NAME_INVALID", ValidationSeverity.Error, $"Parameter name '{parameter.Name}' is not a valid identifier.", parameter.Id));
            if (string.IsNullOrWhiteSpace(parameter.Expression) && parameter.Type is not (ScriptParameterType.Boolean or ScriptParameterType.Text))
                messages.Add(new("PARAMETER_EXPRESSION_EMPTY", ValidationSeverity.Error, $"Parameter '{parameter.Name}' has no expression.", parameter.Id));
        }
    }

    private void ValidateCommands(ScriptDocument document, ICollection<DocumentValidationMessage> messages)
    {
        var commands = document.Commands.ToDictionary(x => x.Id);
        foreach (var group in document.Commands.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
        foreach (var command in group)
            messages.Add(new("COMMAND_NAME_DUPLICATE", ValidationSeverity.Warning, $"Command name '{group.Key}' is duplicated.", command.Id));

        foreach (var command in document.Commands.OrderBy(x => x.Order))
        {
            if (!commandRegistry.TryGet(command.Type, out var definition) || definition is null)
            {
                messages.Add(new("COMMAND_TYPE_UNKNOWN", ValidationSeverity.Error, $"Command type '{command.Type}' is not registered.", command.Id));
                continue;
            }

            foreach (var field in definition.Fields)
            {
                command.Fields.TryGetValue(field.Name, out var value);
                if (field.IsRequired && IsEmpty(value))
                    messages.Add(new("COMMAND_FIELD_REQUIRED", ValidationSeverity.Error, $"Field '{field.Name}' is required.", command.Id, field.Name));

                if (field.FieldType is not (CommandFieldType.CommandReference or CommandFieldType.CommandReferenceList) || value is null)
                    continue;

                var references = value.ReferenceIds.ToList();
                if (value.ReferenceId is Guid single) references.Insert(0, single);
                references = references.Distinct().ToList();
                if (references.Count < field.MinReferences)
                    messages.Add(new("REFERENCE_COUNT_MIN", ValidationSeverity.Error, $"Field '{field.Name}' requires at least {field.MinReferences} reference(s).", command.Id, field.Name));
                if (field.MaxReferences > 0 && references.Count > field.MaxReferences)
                    messages.Add(new("REFERENCE_COUNT_MAX", ValidationSeverity.Error, $"Field '{field.Name}' accepts at most {field.MaxReferences} reference(s).", command.Id, field.Name));

                foreach (var referenceId in references)
                {
                    if (referenceId == command.Id)
                    {
                        messages.Add(new("REFERENCE_SELF", ValidationSeverity.Error, $"Field '{field.Name}' references its own command.", command.Id, field.Name));
                        continue;
                    }
                    if (!commands.TryGetValue(referenceId, out var referenced))
                    {
                        messages.Add(new("REFERENCE_MISSING", ValidationSeverity.Error, $"Referenced command '{referenceId}' does not exist.", command.Id, field.Name));
                        continue;
                    }
                    if (!commandRegistry.TryGet(referenced.Type, out var referencedDefinition) || referencedDefinition is null) continue;
                    if (field.AcceptedTopology != CommandTopologyKind.Any &&
                        (field.AcceptedTopology & referencedDefinition.OutputTopology) == 0)
                    {
                        messages.Add(new(
                            "REFERENCE_TOPOLOGY_INVALID",
                            ValidationSeverity.Error,
                            $"Field '{field.Name}' cannot use command '{referenced.Name}' output '{referencedDefinition.OutputTopology}'.",
                            command.Id,
                            field.Name));
                    }
                }
            }
        }
    }

    private static void ValidateOutputs(ScriptDocument document, ICollection<DocumentValidationMessage> messages)
    {
        var commandIds = document.Commands.Select(x => x.Id).ToHashSet();
        foreach (var outputId in document.OutputCommandIds.Where(x => !commandIds.Contains(x)))
            messages.Add(new("OUTPUT_REFERENCE_MISSING", ValidationSeverity.Error, $"Output command '{outputId}' does not exist.", outputId));
    }

    private static void ValidateCycles(ScriptDocument document, ICollection<DocumentValidationMessage> messages)
    {
        var commands = document.Commands.ToDictionary(x => x.Id);
        var indegree = commands.Keys.ToDictionary(x => x, _ => 0);
        var dependents = commands.Keys.ToDictionary(x => x, _ => new List<Guid>());
        foreach (var command in document.Commands)
        {
            var refs = command.Fields.Values
                .SelectMany(value => value.ReferenceId is Guid single ? value.ReferenceIds.Append(single) : value.ReferenceIds)
                .Where(commands.ContainsKey)
                .Distinct();
            foreach (var reference in refs)
            {
                indegree[command.Id]++;
                dependents[reference].Add(command.Id);
            }
        }

        var queue = new Queue<Guid>(indegree.Where(x => x.Value == 0).Select(x => x.Key));
        while (queue.TryDequeue(out var id))
        foreach (var child in dependents[id])
            if (--indegree[child] == 0) queue.Enqueue(child);

        foreach (var item in indegree.Where(x => x.Value > 0))
            messages.Add(new("DEPENDENCY_CYCLE", ValidationSeverity.Error, $"Command '{commands[item.Key].Name}' is part of a circular dependency.", item.Key));
    }

    private static bool IsEmpty(CommandValue? value) => value is null ||
        string.IsNullOrWhiteSpace(value.Expression) &&
        value.ReferenceId is null &&
        value.ReferenceIds.Count == 0 &&
        string.IsNullOrWhiteSpace(value.Literal);

    private static bool IsIdentifier(string value)
    {
        if (value.Length == 0 || !(char.IsLetter(value[0]) || value[0] == '_')) return false;
        return value.Skip(1).All(x => char.IsLetterOrDigit(x) || x == '_');
    }
}
