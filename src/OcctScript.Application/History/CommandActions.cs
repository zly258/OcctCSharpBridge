using OcctScript.Domain;

namespace OcctScript.Application.History;

public sealed class AddCommandAction : IDocumentAction
{
    private readonly ScriptCommand command;
    private readonly int index;
    private readonly bool addAsOutput;

    public AddCommandAction(ScriptCommand command, int index = -1, bool addAsOutput = true)
    {
        this.command = command ?? throw new ArgumentNullException(nameof(command));
        this.index = index;
        this.addAsOutput = addAsOutput;
    }

    public string Description => "Add command";

    public void Execute(ScriptDocument document)
    {
        if (!document.Commands.Any(x => x.Id == command.Id))
        {
            if (index >= 0 && index <= document.Commands.Count) document.Commands.Insert(index, command);
            else document.Commands.Add(command);
        }
        if (addAsOutput && !document.OutputCommandIds.Contains(command.Id)) document.OutputCommandIds.Add(command.Id);
    }

    public void Undo(ScriptDocument document)
    {
        document.Commands.RemoveAll(x => x.Id == command.Id);
        document.OutputCommandIds.RemoveAll(x => x == command.Id);
    }
}

public sealed class RemoveCommandAction : IDocumentAction
{
    private readonly Guid commandId;
    private ScriptCommand? removedCommand;
    private int removedIndex;
    private readonly List<int> outputIndices = [];

    public RemoveCommandAction(Guid commandId) => this.commandId = commandId;
    public string Description => "Remove command";

    public void Execute(ScriptDocument document)
    {
        removedIndex = document.Commands.FindIndex(x => x.Id == commandId);
        if (removedIndex < 0) throw new KeyNotFoundException($"Command '{commandId}' was not found.");
        removedCommand = document.Commands[removedIndex];
        document.Commands.RemoveAt(removedIndex);
        outputIndices.Clear();
        for (var index = document.OutputCommandIds.Count - 1; index >= 0; index--)
        {
            if (document.OutputCommandIds[index] != commandId) continue;
            outputIndices.Add(index);
            document.OutputCommandIds.RemoveAt(index);
        }
    }

    public void Undo(ScriptDocument document)
    {
        if (removedCommand is null) throw new InvalidOperationException("The action has not been executed.");
        document.Commands.Insert(Math.Min(removedIndex, document.Commands.Count), removedCommand);
        foreach (var index in outputIndices.OrderBy(x => x))
            document.OutputCommandIds.Insert(Math.Min(index, document.OutputCommandIds.Count), commandId);
    }
}

public sealed class ChangeCommandFieldValueAction : IDocumentAction
{
    private readonly Guid commandId;
    private readonly string fieldName;
    private readonly CommandValue before;
    private CommandValue after;

    public ChangeCommandFieldValueAction(Guid commandId, string fieldName, CommandValue before, CommandValue after)
    {
        this.commandId = commandId;
        this.fieldName = fieldName;
        this.before = before.Clone();
        this.after = after.Clone();
    }

    public string Description => "Change command field";
    public void Execute(ScriptDocument document) => GetField(document).CopyFrom(after);
    public void Undo(ScriptDocument document) => GetField(document).CopyFrom(before);

    public bool TryMerge(IDocumentAction followingAction)
    {
        if (followingAction is not ChangeCommandFieldValueAction following ||
            following.commandId != commandId ||
            !string.Equals(following.fieldName, fieldName, StringComparison.Ordinal)) return false;
        after = following.after.Clone();
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

public sealed class ChangeCommandNameAction(Guid commandId, string before, string after) : IDocumentAction
{
    public string Description => "Rename command";
    public void Execute(ScriptDocument document) => Get(document).Name = after;
    public void Undo(ScriptDocument document) => Get(document).Name = before;
    private ScriptCommand Get(ScriptDocument document) =>
        document.FindCommand(commandId) ?? throw new KeyNotFoundException($"Command '{commandId}' was not found.");
}

public sealed class ChangeCommandEnabledAction(Guid commandId, bool before, bool after) : IDocumentAction
{
    public string Description => "Enable or disable command";
    public void Execute(ScriptDocument document) => Get(document).IsEnabled = after;
    public void Undo(ScriptDocument document) => Get(document).IsEnabled = before;
    private ScriptCommand Get(ScriptDocument document) =>
        document.FindCommand(commandId) ?? throw new KeyNotFoundException($"Command '{commandId}' was not found.");
}

public sealed class ChangeCommandTransformAction : IDocumentAction
{
    private readonly Guid commandId;
    private readonly TransformDefinition before;
    private TransformDefinition after;

    public ChangeCommandTransformAction(Guid commandId, TransformDefinition before, TransformDefinition after)
    {
        this.commandId = commandId;
        this.before = before.Clone();
        this.after = after.Clone();
    }

    public string Description => "Change command transform";
    public void Execute(ScriptDocument document) => Get(document).Transform.CopyFrom(after);
    public void Undo(ScriptDocument document) => Get(document).Transform.CopyFrom(before);

    public bool TryMerge(IDocumentAction followingAction)
    {
        if (followingAction is not ChangeCommandTransformAction following || following.commandId != commandId) return false;
        after = following.after.Clone();
        return true;
    }

    private ScriptCommand Get(ScriptDocument document) =>
        document.FindCommand(commandId) ?? throw new KeyNotFoundException($"Command '{commandId}' was not found.");
}
