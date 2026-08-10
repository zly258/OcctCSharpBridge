namespace OcctNet;

public sealed partial class OcctEngine
{
    public IReadOnlyList<IOcctObject> SelectedObjects
    {
        get
        {
            EnsureInitialized();
            var hits = GetSelectedHits();
            if (hits.Count == 0) return Array.Empty<IOcctObject>();

            var result = new List<IOcctObject>(hits.Count);
            var seen = new HashSet<long>();
            foreach (var hit in hits)
            {
                if (seen.Add(hit.Owner.Id))
                    result.Add(hit.Owner);
            }
            return result;
        }
    }

    public IOcctObject? FirstSelectedObject =>
        SelectedObjects.Count == 0 ? null : SelectedObjects[0];

    public OcctShape? FirstSelected =>
        FirstSelectedObject is OcctShape shape ? shape : null;

    public void ClearSelection() =>
        CheckInitialized(() => NativeMethods.occt_clear_selection(_handle));
}
