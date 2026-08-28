namespace OcctNet;

/// <summary>One node of a headless XDE occurrence tree.</summary>
public sealed class OcctExchangeNode
{
    private readonly List<OcctExchangeNode> _children = new();

    internal OcctExchangeNode(
        string id,
        int index,
        int parentIndex,
        OcctAssemblyNodeKind kind,
        string name,
        string referenceName,
        OcctModelShape? shape,
        OcctAssemblyStyle style,
        OcctAssemblyTransform3d localTransform,
        OcctAssemblyTransform3d globalTransform,
        IReadOnlyList<OcctAssemblySubshapeStyle> subshapeStyles,
        IReadOnlyList<string> layers)
    {
        Id = id;
        Index = index;
        ParentIndex = parentIndex;
        Kind = kind;
        Name = name;
        ReferenceName = referenceName;
        Shape = shape;
        Style = style;
        LocalTransform = localTransform;
        GlobalTransform = globalTransform;
        SubshapeStyles = subshapeStyles;
        Layers = layers;
    }

    public string Id { get; }
    public int Index { get; }
    public int ParentIndex { get; }
    public OcctAssemblyNodeKind Kind { get; }
    public string Name { get; }
    public string ReferenceName { get; }
    public OcctModelShape? Shape { get; }
    public OcctAssemblyStyle Style { get; }
    public OcctAssemblyTransform3d LocalTransform { get; }
    public OcctAssemblyTransform3d GlobalTransform { get; }
    public IReadOnlyList<OcctAssemblySubshapeStyle> SubshapeStyles { get; }
    public IReadOnlyList<string> Layers { get; }
    public OcctExchangeNode? Parent { get; internal set; }
    public IReadOnlyList<OcctExchangeNode> Children => _children;

    internal void AddChild(OcctExchangeNode child)
    {
        child.Parent = this;
        _children.Add(child);
    }
}

/// <summary>
/// Headless XDE document snapshot imported through STEPCAFControl or IGESCAFControl.
/// Geometry is owned by the originating <see cref="OcctModelingSession"/>.
/// </summary>
public sealed class OcctExchangeDocument
{
    internal OcctExchangeDocument(
        string sourcePath,
        string format,
        OcctModelShape primaryShape,
        IReadOnlyList<OcctExchangeNode> nodes,
        IReadOnlyList<OcctExchangeNode> roots)
    {
        SourcePath = sourcePath;
        Format = format;
        PrimaryShape = primaryShape;
        Nodes = nodes;
        Roots = roots;
    }

    public string SourcePath { get; }
    public string Format { get; }
    public OcctModelShape PrimaryShape { get; }
    public IReadOnlyList<OcctExchangeNode> Nodes { get; }
    public IReadOnlyList<OcctExchangeNode> Roots { get; }
    public IEnumerable<OcctExchangeNode> Assemblies => Nodes.Where(node => node.Kind == OcctAssemblyNodeKind.Assembly);
    public IEnumerable<OcctExchangeNode> Instances => Nodes.Where(node => node.Kind == OcctAssemblyNodeKind.Instance);
    public IEnumerable<OcctExchangeNode> Parts => Nodes.Where(node => node.Kind != OcctAssemblyNodeKind.Assembly);
}
