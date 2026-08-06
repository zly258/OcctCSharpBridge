using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed record ShapeBuildMessage(string Code, string Message, bool IsError);

public sealed record ShapeBuildResult(
    bool Success,
    object? Shape,
    IReadOnlyList<ShapeBuildMessage> Messages)
{
    public static ShapeBuildResult Failed(string code, string message) =>
        new(false, null, [new ShapeBuildMessage(code, message, true)]);
}

public sealed class ShapeBuildContext
{
    public required IReadOnlyDictionary<string, double> Parameters { get; init; }
    public required IReadOnlyDictionary<Guid, object> CommandShapes { get; init; }
}

public interface ICommandShapeBuilder
{
    string CommandType { get; }
    ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context);
}

public sealed class CommandShapeBuilderRegistry
{
    private readonly Dictionary<string, ICommandShapeBuilder> builders = new(StringComparer.Ordinal);

    public void Register(ICommandShapeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (!builders.TryAdd(builder.CommandType, builder))
            throw new InvalidOperationException($"Shape builder '{builder.CommandType}' is already registered.");
    }

    public ICommandShapeBuilder GetRequired(string commandType) =>
        builders.TryGetValue(commandType, out var builder)
            ? builder
            : throw new KeyNotFoundException($"Shape builder '{commandType}' is not registered.");
}
