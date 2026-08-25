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

    /// <summary>Sets or clears the XDE surface color and transparency of a STEP node.</summary>
    public void SetStepNodeSurfaceColor(OcctAssemblyNode node, OcctAssemblyColor? color)
    {
        SetStepNodeColor(node, color, colorKind: 0, nameof(SetStepNodeSurfaceColor));
        node.Style = node.Style with { SurfaceColor = color };
    }

    /// <summary>Sets or clears the XDE curve color of a STEP node.</summary>
    public void SetStepNodeCurveColor(OcctAssemblyNode node, OcctAssemblyColor? color)
    {
        SetStepNodeColor(node, color, colorKind: 1, nameof(SetStepNodeCurveColor));
        node.Style = node.Style with { CurveColor = color };
    }

    private void SetStepNodeColor(
        OcctAssemblyNode node,
        OcctAssemblyColor? color,
        int colorKind,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(node);
        var native = default(NativeStepColor);
        if (color is { } value)
        {
            ValidateStepColor(value, nameof(color));
            native = new NativeStepColor
            {
                R = value.R,
                G = value.G,
                B = value.B,
                A = value.A
            };
        }

        EnsureInitialized();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_node_color_set(
            _handle,
            node.Id,
            colorKind,
            in native,
            color.HasValue ? 1 : 0), operation);
    }

    private static void ValidateStepColor(OcctAssemblyColor color, string parameterName)
    {
        if (!double.IsFinite(color.R) || !double.IsFinite(color.G) ||
            !double.IsFinite(color.B) || !double.IsFinite(color.A) ||
            color.R < 0 || color.R > 1 ||
            color.G < 0 || color.G > 1 ||
            color.B < 0 || color.B > 1 ||
            color.A < 0 || color.A > 1)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "STEP color components must be finite values from zero to one.");
        }
    }

    /// <summary>Adds or removes an XDE layer assignment for a STEP node.</summary>
    public void SetStepNodeLayer(OcctAssemblyNode node, string layerName, bool assigned = true)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(layerName);
        EnsureInitialized();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_node_layer_set(
            _handle, node.Id, layerName, assigned ? 1 : 0), nameof(SetStepNodeLayer));

        var layers = node.Layers.ToList();
        var index = layers.FindIndex(value => string.Equals(value, layerName, StringComparison.Ordinal));
        if (assigned && index < 0) layers.Add(layerName);
        if (!assigned && index >= 0) layers.RemoveAt(index);
        node.Layers = layers;
    }

    /// <summary>Updates the global XDE visibility state of an existing STEP layer.</summary>
    public void SetStepLayerVisibility(string layerName, bool visible)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layerName);
        EnsureInitialized();
        CheckStepNodeEdit(StepDocumentNativeMethods.occt_engine_step_layer_visibility_set(
            _handle, layerName, visible ? 1 : 0), nameof(SetStepLayerVisibility));
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
