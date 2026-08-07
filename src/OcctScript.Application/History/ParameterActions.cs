using System.Text.RegularExpressions;
using OcctScript.Domain;

namespace OcctScript.Application.History;

public sealed class AddParameterAction(ScriptParameter parameter, int index = -1) : IDocumentAction
{
    public string Description => "Add parameter";
    public void Execute(ScriptDocument document)
    {
        if (document.Parameters.Any(x => x.Id == parameter.Id)) return;
        if (index >= 0 && index <= document.Parameters.Count) document.Parameters.Insert(index, parameter);
        else document.Parameters.Add(parameter);
    }
    public void Undo(ScriptDocument document) => document.Parameters.RemoveAll(x => x.Id == parameter.Id);
}

public sealed class RemoveParameterAction : IDocumentAction
{
    private readonly Guid parameterId;
    private ScriptParameter? removedParameter;
    private int removedIndex;

    public RemoveParameterAction(Guid parameterId) => this.parameterId = parameterId;
    public string Description => "Remove parameter";

    public void Execute(ScriptDocument document)
    {
        removedIndex = document.Parameters.FindIndex(x => x.Id == parameterId);
        if (removedIndex < 0) throw new KeyNotFoundException($"Parameter '{parameterId}' was not found.");
        removedParameter = document.Parameters[removedIndex];
        document.Parameters.RemoveAt(removedIndex);
    }

    public void Undo(ScriptDocument document)
    {
        if (removedParameter is null) throw new InvalidOperationException("The action has not been executed.");
        document.Parameters.Insert(Math.Min(removedIndex, document.Parameters.Count), removedParameter);
    }
}

public sealed class ChangeParameterExpressionAction : IDocumentAction
{
    private readonly Guid parameterId;
    private readonly string before;
    private string after;

    public ChangeParameterExpressionAction(Guid parameterId, string before, string after)
    {
        this.parameterId = parameterId;
        this.before = before;
        this.after = after;
    }

    public string Description => "Change parameter expression";
    public void Execute(ScriptDocument document) => Get(document).Expression = after;
    public void Undo(ScriptDocument document) => Get(document).Expression = before;

    public bool TryMerge(IDocumentAction followingAction)
    {
        if (followingAction is not ChangeParameterExpressionAction following || following.parameterId != parameterId) return false;
        after = following.after;
        return true;
    }

    private ScriptParameter Get(ScriptDocument document) =>
        document.FindParameter(parameterId) ?? throw new KeyNotFoundException($"Parameter '{parameterId}' was not found.");
}

public sealed class ChangeParameterUnitAction(Guid parameterId, string before, string after) : IDocumentAction
{
    public string Description => "Change parameter unit";
    public void Execute(ScriptDocument document) => Get(document).Unit = after;
    public void Undo(ScriptDocument document) => Get(document).Unit = before;
    private ScriptParameter Get(ScriptDocument document) =>
        document.FindParameter(parameterId) ?? throw new KeyNotFoundException($"Parameter '{parameterId}' was not found.");
}

public sealed class RenameParameterAction : IDocumentAction
{
    private readonly Guid parameterId;
    private readonly string before;
    private readonly string after;
    private Dictionary<Guid, string>? parameterExpressions;
    private Dictionary<(Guid CommandId, string FieldName), (string Expression, string? Literal)>? commandValues;

    public RenameParameterAction(Guid parameterId, string before, string after)
    {
        this.parameterId = parameterId;
        this.before = before;
        this.after = after;
    }

    public string Description => "Rename parameter";

    public void Execute(ScriptDocument document)
    {
        var parameter = Get(document);
        Capture(document);
        parameter.Name = after;
        parameter.DisplayName = after;
        ReplaceReferences(document, before, after);
    }

    public void Undo(ScriptDocument document)
    {
        var parameter = Get(document);
        parameter.Name = before;
        parameter.DisplayName = before;
        if (parameterExpressions is not null)
        {
            foreach (var item in document.Parameters)
                if (parameterExpressions.TryGetValue(item.Id, out var expression)) item.Expression = expression;
        }
        if (commandValues is not null)
        {
            foreach (var command in document.Commands)
            foreach (var field in command.Fields)
            {
                if (!commandValues.TryGetValue((command.Id, field.Key), out var value)) continue;
                field.Value.Expression = value.Expression;
                field.Value.Literal = value.Literal;
            }
        }
    }

    private void Capture(ScriptDocument document)
    {
        parameterExpressions ??= document.Parameters.ToDictionary(x => x.Id, x => x.Expression);
        commandValues ??= document.Commands
            .SelectMany(command => command.Fields.Select(field => new { command.Id, FieldName = field.Key, field.Value }))
            .ToDictionary(x => (x.Id, x.FieldName), x => (x.Value.Expression, x.Value.Literal));
    }

    private static void ReplaceReferences(ScriptDocument document, string oldName, string newName)
    {
        foreach (var parameter in document.Parameters)
            parameter.Expression = ReplaceIdentifier(parameter.Expression, oldName, newName);
        foreach (var field in document.Commands.SelectMany(x => x.Fields.Values))
        {
            field.Expression = ReplaceIdentifier(field.Expression, oldName, newName);
            if (field.Literal is not null) field.Literal = ReplaceIdentifier(field.Literal, oldName, newName);
        }
    }

    private static string ReplaceIdentifier(string text, string oldName, string newName)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var pattern = $@"(?<![A-Za-z0-9_]){Regex.Escape(oldName)}(?![A-Za-z0-9_])";
        return Regex.Replace(text, pattern, newName, RegexOptions.CultureInvariant);
    }

    private ScriptParameter Get(ScriptDocument document) =>
        document.FindParameter(parameterId) ?? throw new KeyNotFoundException($"Parameter '{parameterId}' was not found.");
}
