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
    private readonly string oldExpression;
    private string newExpression;

    public ChangeParameterExpressionAction(Guid parameterId, string oldExpression, string newExpression)
    {
        this.parameterId = parameterId;
        this.oldExpression = oldExpression;
        this.newExpression = newExpression;
    }

    public string Description => "Change parameter expression";
    public void Execute(ScriptDocument document) => Get(document).Expression = newExpression;
    public void Undo(ScriptDocument document) => Get(document).Expression = oldExpression;

    public bool TryMerge(IDocumentAction followingAction)
    {
        if (followingAction is not ChangeParameterExpressionAction following || following.parameterId != parameterId) return false;
        newExpression = following.newExpression;
        return true;
    }

    private ScriptParameter Get(ScriptDocument document) =>
        document.FindParameter(parameterId) ?? throw new KeyNotFoundException($"Parameter '{parameterId}' was not found.");
}
