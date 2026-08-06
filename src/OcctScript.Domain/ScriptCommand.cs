namespace OcctScript.Domain;

public sealed class ScriptCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Order { get; set; }
    public Dictionary<string, CommandValue> Fields { get; init; } = new(StringComparer.Ordinal);
    public TransformDefinition Transform { get; set; } = new();
    public DisplayDefinition Display { get; set; } = new();
}

public sealed class CommandValue
{
    public string Expression { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public List<Guid> ReferenceIds { get; set; } = [];
    public string? Literal { get; set; }
}

public sealed class TransformDefinition
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double RotationX { get; set; }
    public double RotationY { get; set; }
    public double RotationZ { get; set; }
    public double Scale { get; set; } = 1.0;
}

public sealed class DisplayDefinition
{
    public bool IsVisible { get; set; } = true;
    public string Color { get; set; } = "#D9E6F2";
    public double Transparency { get; set; }
}
