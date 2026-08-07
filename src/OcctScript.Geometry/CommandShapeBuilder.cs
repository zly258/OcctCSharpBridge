using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed record ShapeBuildMessage(string Code, string Message, bool IsError)
{
    public static ShapeBuildMessage Error(string code, string message) => new(code, message, true);
    public static ShapeBuildMessage Warning(string code, string message) => new(code, message, false);
}

public sealed record ShapeBuildResult(
    bool Success,
    OcctModelShape Shape,
    IReadOnlyList<ShapeBuildMessage> Messages)
{
    public static ShapeBuildResult Failed(string code, string message) =>
        new(false, default, [ShapeBuildMessage.Error(code, message)]);

    public static ShapeBuildResult Succeeded(
        OcctModelShape shape,
        IReadOnlyList<ShapeBuildMessage>? messages = null) =>
        new(true, shape, messages ?? Array.Empty<ShapeBuildMessage>());
}

public sealed class ShapeBuildContext
{
    public required OcctModelingSession Session { get; init; }
    public required IReadOnlyDictionary<string, double> Parameters { get; init; }
    public required IReadOnlyDictionary<Guid, OcctModelShape> CommandShapes { get; init; }
    public required CommandFieldEvaluator Fields { get; init; }

    public OcctModelShape RequiredShape(ScriptCommand command, string fieldName)
    {
        if (!command.Fields.TryGetValue(fieldName, out var value) || value.ReferenceId is null)
            throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' requires one command reference.");
        if (!CommandShapes.TryGetValue(value.ReferenceId.Value, out var shape))
            throw new InvalidOperationException($"Referenced command '{value.ReferenceId}' has not produced a shape.");
        return shape;
    }

    public IReadOnlyList<OcctModelShape> RequiredShapes(ScriptCommand command, string fieldName, int minimumCount = 1)
    {
        if (!command.Fields.TryGetValue(fieldName, out var value))
            throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' is missing.");

        var ids = value.ReferenceIds.Count > 0
            ? value.ReferenceIds
            : value.ReferenceId is Guid single ? [single] : [];
        if (ids.Count < minimumCount)
            throw new InvalidOperationException($"Command '{command.Name}' field '{fieldName}' requires at least {minimumCount} reference(s).");

        var result = new List<OcctModelShape>(ids.Count);
        foreach (var id in ids)
        {
            if (!CommandShapes.TryGetValue(id, out var shape))
                throw new InvalidOperationException($"Referenced command '{id}' has not produced a shape.");
            result.Add(shape);
        }
        return result;
    }
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

    public bool TryGet(string commandType, out ICommandShapeBuilder? builder) =>
        builders.TryGetValue(commandType, out builder);

    public ICommandShapeBuilder GetRequired(string commandType) =>
        builders.TryGetValue(commandType, out var builder)
            ? builder
            : throw new KeyNotFoundException($"Shape builder '{commandType}' is not registered.");
}
