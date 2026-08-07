using OcctScript.Domain;

namespace OcctScript.Application.History;

public sealed class DocumentHistory
{
    private readonly Stack<IDocumentAction> undoStack = new();
    private readonly Stack<IDocumentAction> redoStack = new();
    private TransactionAction? activeTransaction;

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;
    public string UndoDescription => CanUndo ? undoStack.Peek().Description : string.Empty;
    public string RedoDescription => CanRedo ? redoStack.Peek().Description : string.Empty;

    public event EventHandler? Changed;

    public void Execute(ScriptDocument document, IDocumentAction action)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(action);
        action.Execute(document);
        redoStack.Clear();

        if (activeTransaction is not null)
        {
            activeTransaction.Add(action);
        }
        else if (undoStack.TryPeek(out var previous) && previous.TryMerge(action))
        {
        }
        else
        {
            undoStack.Push(action);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Undo(ScriptDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        EnsureNoActiveTransaction();
        if (!undoStack.TryPop(out var action)) return;
        action.Undo(document);
        redoStack.Push(action);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Redo(ScriptDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        EnsureNoActiveTransaction();
        if (!redoStack.TryPop(out var action)) return;
        action.Execute(document);
        undoStack.Push(action);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public IDisposable BeginTransaction(string description)
    {
        if (activeTransaction is not null) throw new InvalidOperationException("A history transaction is already active.");
        activeTransaction = new TransactionAction(description);
        return new TransactionScope(this);
    }

    public void Clear()
    {
        EnsureNoActiveTransaction();
        undoStack.Clear();
        redoStack.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void CommitTransaction()
    {
        var transaction = activeTransaction ?? throw new InvalidOperationException("No history transaction is active.");
        activeTransaction = null;
        if (transaction.Count > 0) undoStack.Push(transaction);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureNoActiveTransaction()
    {
        if (activeTransaction is not null) throw new InvalidOperationException("Complete the active history transaction first.");
    }

    private sealed class TransactionScope(DocumentHistory owner) : IDisposable
    {
        private bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            owner.CommitTransaction();
        }
    }
}

internal sealed class TransactionAction(string description) : IDocumentAction
{
    private readonly List<IDocumentAction> actions = [];
    public string Description { get; } = description;
    public int Count => actions.Count;
    public void Add(IDocumentAction action) => actions.Add(action);
    public void Execute(ScriptDocument document)
    {
        foreach (var action in actions) action.Execute(document);
    }
    public void Undo(ScriptDocument document)
    {
        for (var index = actions.Count - 1; index >= 0; index--) actions[index].Undo(document);
    }
}
