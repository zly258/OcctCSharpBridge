using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Imports a STEP file through STEPCAFControl/XDE and returns its assembly occurrence tree.
    /// The existing <see cref="ImportStep"/> API remains available for source compatibility.
    /// </summary>
    public OcctAssemblyDocument ImportStepDocument(string filePath)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        var fullPath = Path.GetFullPath(filePath);
        var primaryShape = ImportStep(fullPath);

        var pointer = NativeMethods.occt_get_last_step_document_json(_handle);
        var json = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
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
            var surfaceColor = ToColor(value.SurfaceColor);
            var curveColor = ToColor(value.CurveColor);
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
                new OcctAssemblyStyle(value.Visible, surfaceColor, curveColor),
                OcctAssemblyTransform3d.FromArray(value.LocalTransform),
                OcctAssemblyTransform3d.FromArray(value.GlobalTransform)));
        }

        var roots = new List<OcctAssemblyNode>();
        foreach (var node in nodes)
        {
            if (node.ParentIndex >= 0 && node.ParentIndex < nodes.Count)
                nodes[node.ParentIndex].AddChild(node);
            else
                roots.Add(node);
        }

        return new OcctAssemblyDocument(fullPath, primaryShape, nodes, roots);
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

        [JsonPropertyName("localTransform")]
        public double[]? LocalTransform { get; set; }

        [JsonPropertyName("globalTransform")]
        public double[]? GlobalTransform { get; set; }
    }
}
