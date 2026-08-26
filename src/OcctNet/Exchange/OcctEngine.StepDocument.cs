using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Imports a STEP file through STEPCAFControl/XDE and returns its assembly occurrence tree.
    /// </summary>
    public OcctAssemblyDocument ImportStepDocument(string filePath)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        var fullPath = Path.GetFullPath(filePath);
        var primaryShape = ImportStep(fullPath);

        return ReadStepDocument(fullPath, primaryShape);
    }

    /// <summary>
    /// Asynchronously schedules a STEP/XDE document import on the native surface thread.
    /// The retained XDE document belongs to this viewer engine, so this operation intentionally
    /// does not use a headless modeling session. Cancellation is observed before dispatch and
    /// before execution; an in-progress native STEP/XDE import cannot be interrupted safely.
    /// </summary>
    public Task<OcctAssemblyDocument> ImportStepDocumentAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(filePath);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the OCCT engine before importing a STEP/XDE document.");
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = Path.GetFullPath(filePath);
        return RunOnSurfaceThreadAsync(
            () => ImportStepDocument(fullPath),
            cancellationToken);
    }

    /// <summary>Reads a fresh managed snapshot of the retained STEP/XDE document.</summary>
    public OcctAssemblyDocument RefreshStepDocument(OcctAssemblyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        EnsureShape(document.PrimaryShape);
        EnsureInitialized();
        return ReadStepDocument(document.SourcePath, document.PrimaryShape);
    }

    /// <summary>
    /// Asynchronously refreshes a managed snapshot of the retained STEP/XDE document on the
    /// native surface thread.
    /// </summary>
    public Task<OcctAssemblyDocument> RefreshStepDocumentAsync(
        OcctAssemblyDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();
        return RunOnSurfaceThreadAsync(
            () => RefreshStepDocument(document),
            cancellationToken);
    }

    private OcctAssemblyDocument ReadStepDocument(string sourcePath, OcctShape primaryShape)
    {
        var json = GetLastStepDocumentJson();
        if (string.IsNullOrWhiteSpace(json)) throw CreateException(nameof(ImportStepDocument));

        StepDocumentDto? document;
        try
        {
            document = JsonSerializer.Deserialize<StepDocumentDto>(json);
        }
        catch (JsonException exception)
        {
            throw new OcctException(
                "The native STEP/XDE assembly snapshot could not be decoded.",
                nameof(ImportStepDocument),
                json,
                exception);
        }

        if (document?.Nodes is null || document.Nodes.Count == 0)
            throw new OcctException("The STEP/XDE document contains no assembly nodes.", nameof(ImportStepDocument));

        var nodes = new List<OcctAssemblyNode>(document.Nodes.Count);
        for (var index = 0; index < document.Nodes.Count; index++)
        {
            var value = document.Nodes[index];
            OcctShape? shape = value.ObjectId > 0 ? new OcctShape(value.ObjectId, _ownerId) : null;
            nodes.Add(new OcctAssemblyNode(
                value.Id ?? string.Empty,
                index,
                value.Parent,
                Enum.IsDefined(typeof(OcctAssemblyNodeKind), value.Kind)
                    ? (OcctAssemblyNodeKind)value.Kind
                    : OcctAssemblyNodeKind.Part,
                value.Name ?? string.Empty,
                value.ReferenceName ?? string.Empty,
                shape,
                ToStyle(value.Visible, value.SurfaceColor, value.CurveColor),
                OcctAssemblyTransform3d.FromArray(value.LocalTransform),
                OcctAssemblyTransform3d.FromArray(value.GlobalTransform),
                ToSubshapeStyles(value.SubshapeStyles),
                value.Layers ?? Array.Empty<string>()));
        }

        var roots = new List<OcctAssemblyNode>();
        foreach (var node in nodes)
        {
            if (node.ParentIndex >= 0 && node.ParentIndex < nodes.Count)
                nodes[node.ParentIndex].AddChild(node);
            else
                roots.Add(node);
        }

        return new OcctAssemblyDocument(sourcePath, primaryShape, nodes, roots);
    }

    private string GetLastStepDocumentJson()
    {
        var status = StepDocumentNativeMethods.occt_engine_step_document_json_get(
            _handle,
            null,
            0,
            out var requiredBytes);
        if (status != OcctStatus.Ok) throw CreateException(nameof(ImportStepDocument));
        if (requiredBytes <= 1)
            throw new OcctException("The native STEP/XDE assembly snapshot is empty.", nameof(ImportStepDocument));

        var buffer = new byte[requiredBytes];
        status = StepDocumentNativeMethods.occt_engine_step_document_json_get(
            _handle,
            buffer,
            buffer.Length,
            out var writtenBytes);
        if (status != OcctStatus.Ok) throw CreateException(nameof(ImportStepDocument));
        if (writtenBytes <= 1 || writtenBytes > buffer.Length)
            throw new OcctException("The native STEP/XDE assembly snapshot size is invalid.", nameof(ImportStepDocument));

        return Encoding.UTF8.GetString(buffer, 0, writtenBytes - 1);
    }

    private static OcctAssemblyStyle ToStyle(bool visible, double[]? surfaceColor, double[]? curveColor) =>
        new(visible, ToColor(surfaceColor), ToColor(curveColor));

    private static IReadOnlyList<OcctAssemblySubshapeStyle> ToSubshapeStyles(List<StepSubshapeStyleDto>? values)
    {
        if (values is null || values.Count == 0) return Array.Empty<OcctAssemblySubshapeStyle>();

        var result = new List<OcctAssemblySubshapeStyle>(values.Count);
        foreach (var value in values)
        {
            var shapeType = Enum.IsDefined(typeof(OcctShapeType), value.ShapeType)
                ? (OcctShapeType)value.ShapeType
                : OcctShapeType.Shape;
            result.Add(new OcctAssemblySubshapeStyle(
                shapeType,
                value.SubshapeIndex,
                ToStyle(value.Visible, value.SurfaceColor, value.CurveColor)));
        }
        return result;
    }

    private static OcctAssemblyColor? ToColor(double[]? values)
    {
        if (values is not { Length: >= 3 }) return null;
        return new OcctAssemblyColor(
            values[0],
            values[1],
            values[2],
            values.Length >= 4 ? values[3] : 1.0);
    }

    private sealed class StepDocumentDto
    {
        [JsonPropertyName("nodes")]
        public List<StepNodeDto>? Nodes { get; set; }
    }

    private sealed class StepNodeDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("parent")]
        public int Parent { get; set; } = -1;

        [JsonPropertyName("kind")]
        public int Kind { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("referenceName")]
        public string? ReferenceName { get; set; }

        [JsonPropertyName("objectId")]
        public long ObjectId { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;

        [JsonPropertyName("surfaceColor")]
        public double[]? SurfaceColor { get; set; }

        [JsonPropertyName("curveColor")]
        public double[]? CurveColor { get; set; }

        [JsonPropertyName("layers")]
        public string[]? Layers { get; set; }

        [JsonPropertyName("localTransform")]
        public double[]? LocalTransform { get; set; }

        [JsonPropertyName("globalTransform")]
        public double[]? GlobalTransform { get; set; }

        [JsonPropertyName("subshapeStyles")]
        public List<StepSubshapeStyleDto>? SubshapeStyles { get; set; }
    }

    private sealed class StepSubshapeStyleDto
    {
        [JsonPropertyName("shapeType")]
        public int ShapeType { get; set; }

        [JsonPropertyName("subshapeIndex")]
        public int SubshapeIndex { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;

        [JsonPropertyName("surfaceColor")]
        public double[]? SurfaceColor { get; set; }

        [JsonPropertyName("curveColor")]
        public double[]? CurveColor { get; set; }
    }
}
