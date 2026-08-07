using OcctScript.Domain;

namespace OcctScript.Application.History;

public interface IDocumentAction
{
    string Description { get; }
    void Execute(ScriptDocument document);
    void Undo(ScriptDocument document);
    bool TryMerge(IDocumentAction followingAction) => false;
}
