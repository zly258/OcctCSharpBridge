using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OcctScript.Domain;

namespace OcctScript.Serialization;

public sealed class ScriptDocumentSerializer
{
    private static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string Serialize(ScriptDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.Serialize(document, options);
    }

    public ScriptDocument Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("Document content is empty.");
        var document = JsonSerializer.Deserialize<ScriptDocument>(json, options)
            ?? throw new InvalidDataException("Document content is invalid.");
        ValidateHeader(document);
        return document;
    }

    public async Task SaveAsync(string filePath, ScriptDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var fullPath = Path.GetFullPath(filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + ".tmp";
        await File.WriteAllTextAsync(temporaryPath, Serialize(document), Utf8Bom, cancellationToken);
        File.Move(temporaryPath, fullPath, overwrite: true);
    }

    public async Task<ScriptDocument> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Deserialize(json);
    }

    private static void ValidateHeader(ScriptDocument document)
    {
        if (!string.Equals(document.Format, ScriptDocument.CurrentFormat, StringComparison.Ordinal))
            throw new InvalidDataException($"Unsupported document format '{document.Format}'.");
        if (document.Version != ScriptDocument.CurrentVersion)
            throw new InvalidDataException($"Unsupported document version '{document.Version}'.");
    }
}
