using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctExchangeDocument ImportStepDocument(string filePath) =>
        ImportXdeDocument(filePath, ModelNativeMethods.occt_model_step_document_import, nameof(ImportStepDocument));

    public OcctExchangeDocument ImportIgesDocument(string filePath) =>
        ImportXdeDocument(filePath, ModelNativeMethods.occt_model_iges_document_import, nameof(ImportIgesDocument));

    private delegate OcctStatus XdeImportCall(
        OcctModelingSafeHandle handle,
        string path,
        out long primaryShapeId);

    private OcctExchangeDocument ImportXdeDocument(
        string filePath,
        XdeImportCall import,
        string operation)
    {
        ValidateExchangePath(filePath);
        EnsureNotDisposed();
        var fullPath = Path.GetFullPath(filePath);
        var status = import(_handle, fullPath, out var primaryShapeId);
        var primaryShape = CheckExchangeShape(status, primaryShapeId, operation);
        return ReadXdeDocument(fullPath, primaryShape, operation);
    }

    private OcctExchangeDocument ReadXdeDocument(
        string sourcePath,
        OcctModelShape primaryShape,
        string operation)
    {
        var json = GetLastXdeDocumentJson(operation);
        XdeDocumentDto? document;
        try
        {
            document = JsonSerializer.Deserialize<XdeDocumentDto>(json);
        }
        catch (JsonException exception)
        {
            throw new OcctException(
                "The native headless XDE snapshot could not be decoded.",
                operation,
                json,
                exception);
        }

        if (document?.Nodes is null || document.Nodes.Count == 0)
            throw new OcctException("The XDE document contains no assembly nodes.", operation);

        var nodes = new List<OcctExchangeNode>(document.Nodes.Count);
        for (var index = 0; index < document.Nodes.Count; ++index)
        {
            var value = document.Nodes[index];
            OcctModelShape? shape = value.ShapeId > 0
                ? new OcctModelShape(value.ShapeId, _ownerId)
                : null;
            nodes.Add(new OcctExchangeNode(
                value.Id ?? string.Empty,
                index,
                value.Parent,
                Enum.IsDefined(typeof(OcctAssemblyNodeKind), value.Kind)
                    ? (OcctAssemblyNodeKind)value.Kind
                    : OcctAssemblyNodeKind.Part,
                value.Name ?? string.Empty,
                value.ReferenceName ?? string.Empty,
                shape,
                ToXdeStyle(value.Visible, value.SurfaceColor, value.CurveColor),
                OcctAssemblyTransform3d.FromArray(value.LocalTransform),
                OcctAssemblyTransform3d.FromArray(value.GlobalTransform),
                value.Layers ?? Array.Empty<string>()));
        }

        var roots = new List<OcctExchangeNode>();
        foreach (var node in nodes)
        {
            if (node.ParentIndex >= 0 && node.ParentIndex < nodes.Count)
                nodes[node.ParentIndex].AddChild(node);
            else
                roots.Add(node);
        }

        return new OcctExchangeDocument(
            sourcePath,
            document.Format ?? string.Empty,
            primaryShape,
            nodes,
            roots);
    }

    private string GetLastXdeDocumentJson(string operation)
    {
        var status = ModelNativeMethods.occt_model_xde_document_json_get(
            _handle, null, 0, out var requiredBytes);
        CheckStatus(status, operation);
        if (requiredBytes <= 1)
            throw new OcctException("The native headless XDE snapshot is empty.", operation);

        var buffer = new byte[requiredBytes];
        status = ModelNativeMethods.occt_model_xde_document_json_get(
            _handle, buffer, buffer.Length, out var writtenBytes);
        CheckStatus(status, operation);
        if (writtenBytes <= 1 || writtenBytes > buffer.Length)
            throw new OcctException("The native headless XDE snapshot size is invalid.", operation);

        return Encoding.UTF8.GetString(buffer, 0, writtenBytes - 1);
    }

    private static OcctAssemblyStyle ToXdeStyle(
        bool visible,
        double[]? surfaceColor,
        double[]? curveColor) =>
        new(visible, ToXdeColor(surfaceColor), ToXdeColor(curveColor));

    private static OcctAssemblyColor? ToXdeColor(double[]? values)
    {
        if (values is not { Length: >= 3 }) return null;
        return new OcctAssemblyColor(
            values[0],
            values[1],
            values[2],
            values.Length >= 4 ? values[3] : 1.0);
    }

    private sealed class XdeDocumentDto
    {
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        [JsonPropertyName("nodes")]
        public List<XdeNodeDto>? Nodes { get; set; }
    }

    private sealed class XdeNodeDto
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

        [JsonPropertyName("shapeId")]
        public long ShapeId { get; set; }

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
    }
}
