namespace OcctScript.Domain;

public sealed class ScriptCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
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

    public CommandValue Clone() => new()
    {
        Expression = Expression,
        ReferenceId = ReferenceId,
        ReferenceIds = [.. ReferenceIds],
        Literal = Literal
    };

    public void CopyFrom(CommandValue source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Expression = source.Expression;
        ReferenceId = source.ReferenceId;
        ReferenceIds = [.. source.ReferenceIds];
        Literal = source.Literal;
    }
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

    public TransformDefinition Clone() => new()
    {
        X = X,
        Y = Y,
        Z = Z,
        RotationX = RotationX,
        RotationY = RotationY,
        RotationZ = RotationZ,
        Scale = Scale
    };

    public void CopyFrom(TransformDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);
        X = source.X;
        Y = source.Y;
        Z = source.Z;
        RotationX = source.RotationX;
        RotationY = source.RotationY;
        RotationZ = source.RotationZ;
        Scale = source.Scale;
    }
}

public sealed class DisplayDefinition
{
    public bool IsVisible { get; set; } = true;
    public string Color { get; set; } = "#D9E6F2";
    public double Transparency { get; set; }
}
