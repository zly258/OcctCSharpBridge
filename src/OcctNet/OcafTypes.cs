using System.Runtime.InteropServices;

namespace OcctNet;

public static class OcafDocumentFormats
{
    public const string BinaryXde = "BinXCAF";
    public const string XmlXde = "XmlXCAF";
    public const string BinaryOcaf = "BinOcaf";
    public const string XmlOcaf = "XmlOcaf";
}

public enum OcafColorType
{
    General = 0,
    Surface = 1,
    Curve = 2
}

public enum OcafNamedShapeEvolution
{
    Primitive = 0,
    Generated = 1,
    Modify = 2,
    Delete = 3,
    Replace = 4,
    Selected = 5,
    Unknown = 6
}

[StructLayout(LayoutKind.Sequential)]
public struct OcafColor
{
    public double Red;
    public double Green;
    public double Blue;
    public double Alpha;

    public OcafColor(double red, double green, double blue, double alpha = 1.0)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }
}

public readonly record struct OcafLabel(string Entry)
{
    public bool IsValid => !string.IsNullOrWhiteSpace(Entry);
    public override string ToString() => Entry ?? string.Empty;
}

public sealed record OcafAttributeInfo(string TypeName, string Guid, string Json);

public sealed record OcafArray<T>(int Lower, IReadOnlyList<T> Values)
{
    public int Upper => Lower + Values.Count - 1;
}

public sealed record OcafNamedShapePair(OcctModelShape OldShape, OcctModelShape NewShape);

public sealed record OcafMaterial(
    OcafLabel Label,
    string Name,
    string Description,
    double Density,
    string DensityName,
    string DensityValueType);

public sealed class OcafCommandScope : IDisposable
{
    private readonly object _syncRoot = new();
    private OcafDocument? _document;
    private bool _completed;

    internal OcafCommandScope(OcafDocument document)
    {
        _document = document;
        document.NewCommand();
    }

    /// <summary>Commits the command and returns whether an undo delta was created.</summary>
    public bool Commit()
    {
        lock (_syncRoot)
        {
            var document = GetActiveDocument();
            var producedDelta = document.CommitCommand();
            _completed = true;
            _document = null;
            return producedDelta;
        }
    }

    public void Abort()
    {
        lock (_syncRoot)
        {
            var document = GetActiveDocument();
            document.AbortCommand();
            _completed = true;
            _document = null;
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            var document = _document;
            _document = null;
            if (document is null || _completed) return;
            if (document.HasOpenCommand) document.AbortCommand();
            _completed = true;
        }
    }

    private OcafDocument GetActiveDocument() =>
        _document is not null && !_completed
            ? _document
            : throw new ObjectDisposedException(nameof(OcafCommandScope));
}
