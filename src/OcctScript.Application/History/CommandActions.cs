using OcctScript.Domain;

namespace OcctScript.Application.History;

public sealed class AddCommandAction(ScriptCommand command, int index = -1) : IDocumentAction
{
    public string Description => "Add command";

    public void Execute(ScriptDocument document)
    {
        if (document.Commands.Any(x => x.Id == command.Id)) return;
        if (index >= 0 && index <= document.Commands.Count) document.Commands.Insert(index, command);
        else document.Commands.Add(command);
    }

    public void Undo(ScriptDocument document) => document.Commands.RemoveAll(x => x.Id == command.Id);
}

public sealed class RemoveCommandAction : IDocumentAction
{
    private readonly Guid commandId;
    private ScriptCommand? removedCommand;
    private int removedIndex;

    public RemoveCommandAction(Guid commandId) => this.commandId = commandId;
    public string Description => "Remove command";

    public void Execute(ScriptDocument document)
    {
        removedIndex = document.Commands.FindIndex(x => x.Id == commandId);
        if (removedIndex < 0) throw new KeyNotFoundException($"Command '{commandId}' was not found.");
        removedCommand = document.Commands[removedIndex];
        document.Commands.RemoveAt(removedIndex);
        document.OutputCommandIds.RemoveAll(x => x == commandId);
    }

    public void Undo(ScriptDocument document)
    {
        if (removedCommand is null) throw new InvalidOperationException("The action has not been executed.");
        document.Commands.Insert(Math.Min(removedIndex, document.Commands.Count), removedCommand);
    }
}

public sealed class ChangeCommandFieldExpressionAction : IDocumentAction
{
    private readonly Guid commandId;
    private readonly string fieldName;
    private readonly string oldExpression;
    private string newExpression;

    public ChangeCommandFieldExpressionAction(Guid commandId, string fieldName, string oldExpression, string newExpression)
    {
        this.commandId = commandId;
        this.fieldName = fieldName;
        this.oldExpression = oldExpression;
        this.newExpression = newExpression;
    }

    public string Description => "Change command field";
    public void Execute(ScriptDocument document) => GetField(document).Expression = newExpression;
    public void Undo(ScriptDocument document) => GetField(document).Expression = oldExpression;

    public bool TryMerge(IDocumentAction followingAction)
    {
        if (followingAction is not ChangeCommandFieldExpressionAction following ||
            following.commandId != commandId ||
            !string.Equals(following.fieldName, fieldName, StringComparison.Ordinal)) return false;
        newExpression = following.newExpression;
        return true;
    }

    private CommandValue GetField(ScriptDocument document)
    {
        var command = document.FindCommand(commandId)
            ?? throw new KeyNotFoundException($"Command '{commandId}' was not found.");
        return command.Fields.TryGetValue(fieldName, out var field)
            ? field
            : throw new KeyNotFoundException($"Field '{fieldName}' was not found on command '{command.Name}'.");
    }
}
