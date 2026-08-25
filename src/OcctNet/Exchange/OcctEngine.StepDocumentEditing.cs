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

    /// <summary>Adds another occurrence of an existing STEP definition to an assembly.</summary>
    public OcctAssemblyDocument AddStepComponent(
        OcctAssemblyDocument document,
        OcctAssemblyNode parentAssembly,
        OcctAssemblyNode reference,
        OcctAssemblyTransform3d transform)
    {
        ValidateStepDocumentNode(document, parentAssembly, nameof(parentAssembly));
        ValidateStepDocumentNode(document, reference, nameof(reference));
        if (parentAssembly.Kind != OcctAssemblyNodeKind.Assembly)
            throw new ArgumentException("Parent node must be an assembly.", nameof(parentAssembly));
        if (!transform.IsFinite)
            throw new ArgumentException("Component transform must contain only finite values.", nameof(transform));

        EnsureInitialized();
        var native = transform.ToNative();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_component_add(
            _handle,
            parentAssembly.Id,
            reference.Id,
            in native,
            out var viewerObjectId), nameof(AddStepComponent));
        if (viewerObjectId <= 0)
            throw new InvalidOperationException("Native component creation returned no Viewer object.");
        return RefreshStepDocument(document);
    }

    /// <summary>Removes a leaf component occurrence from an imported STEP assembly.</summary>
    public OcctAssemblyDocument RemoveStepComponent(
        OcctAssemblyDocument document,
        OcctAssemblyNode component)
    {
        ValidateStepDocumentNode(document, component, nameof(component));
        if (component.Kind != OcctAssemblyNodeKind.Instance)
            throw new ArgumentException("Only leaf component occurrences can be removed.", nameof(component));

        EnsureInitialized();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_component_remove(
            _handle, component.Id), nameof(RemoveStepComponent));
        return RefreshStepDocument(document);
    }

    private void ValidateStepDocumentNode(
        OcctAssemblyDocument document,
        OcctAssemblyNode node,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(node);
        EnsureShape(document.PrimaryShape);
        if (!document.Nodes.Contains(node))
            throw new ArgumentException("Node does not belong to the supplied STEP document snapshot.", parameterName);
    }

    private void CheckStepNodeEdit(OcctStatus status, string operation)
    {
        if (status != OcctStatus.Ok) throw CreateException(operation);
    }
}
