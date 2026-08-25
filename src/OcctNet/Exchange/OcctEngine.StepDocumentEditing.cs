namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>Updates the XDE label name of a node in the most recently imported STEP document.</summary>
    public void SetStepNodeName(OcctAssemblyNode node, string name)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(name);
        EnsureInitialized();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_node_name_set(
            _handle, node.Id, name), nameof(SetStepNodeName));
        node.Name = name;
    }

    /// <summary>Updates XDE visibility for a node in the most recently imported STEP document.</summary>
    public void SetStepNodeVisibility(OcctAssemblyNode node, bool visible)
    {
        ArgumentNullException.ThrowIfNull(node);
        EnsureInitialized();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_node_visibility_set(
            _handle, node.Id, visible ? 1 : 0), nameof(SetStepNodeVisibility));
        node.Style = node.Style with { Visible = visible };
    }

    /// <summary>Updates the local XDE location of a component occurrence.</summary>
    public void SetStepOccurrenceTransform(
        OcctAssemblyNode occurrence,
        OcctAssemblyTransform3d transform)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        if (occurrence.Kind != OcctAssemblyNodeKind.Instance)
            throw new ArgumentException("Only STEP component occurrences have editable transforms.", nameof(occurrence));
        if (!transform.IsFinite)
            throw new ArgumentException("Occurrence transform must contain only finite values.", nameof(transform));

        EnsureInitialized();
        var native = transform.ToNative();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_node_transform_set(
            _handle, occurrence.Id, in native), nameof(SetStepOccurrenceTransform));
        occurrence.LocalTransform = transform;
    }

    private void CheckStepNodeEdit(OcctStatus status, string operation)
    {
        if (status != OcctStatus.Ok) throw CreateException(operation);
    }
}
