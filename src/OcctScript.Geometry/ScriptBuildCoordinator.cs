using OcctNet;
using OcctScript.Domain;
using OcctScript.Expressions;

namespace OcctScript.Geometry;

public sealed record CommandBuildState(
    Guid CommandId,
    bool Success,
    OcctModelShape Shape,
    IReadOnlyList<ShapeBuildMessage> Messages);

public sealed record DocumentBuildResult(
    bool Success,
    IReadOnlyDictionary<Guid, OcctModelShape> Shapes,
    IReadOnlyList<CommandBuildState> Commands,
    TimeSpan Duration);

public sealed class ScriptBuildCoordinator : IDisposable
{
    private readonly OcctModelingSession session;
    private readonly CommandShapeBuilderRegistry builders;
    private readonly CommandFieldEvaluator fields;
    private bool disposed;

    public ScriptBuildCoordinator(
        CommandShapeBuilderRegistry? builders = null,
        ExpressionEngine? expressionEngine = null)
    {
        session = new OcctModelingSession();
        this.builders = builders ?? CreateDefaultRegistry();
        fields = new CommandFieldEvaluator(expressionEngine ?? new ExpressionEngine());
    }

    public OcctModelingSession Session => disposed
        ? throw new ObjectDisposedException(nameof(ScriptBuildCoordinator))
        : session;

    public DocumentBuildResult Build(
        ScriptDocument document,
        IReadOnlyDictionary<string, double> parameters)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(parameters);

        var started = DateTime.UtcNow;
        session.Clear();
        var shapes = new Dictionary<Guid, OcctModelShape>();
        var states = new List<CommandBuildState>();

        foreach (var command in document.Commands.OrderBy(x => x.Order))
        {
            if (!command.IsEnabled) continue;

            if (!builders.TryGet(command.Type, out var builder) || builder is null)
            {
                var message = new ShapeBuildMessage(
                    "BUILDER_NOT_REGISTERED",
                    $"No shape builder is registered for command type '{command.Type}'.",
                    true);
                states.Add(new CommandBuildState(command.Id, false, default, [message]));
                continue;
            }

            var context = new ShapeBuildContext
            {
                Session = session,
                Parameters = parameters,
                CommandShapes = shapes,
                Fields = fields
            };

            var result = builder.Build(command, context);
            states.Add(new CommandBuildState(command.Id, result.Success, result.Shape, result.Messages));
            if (result.Success) shapes[command.Id] = result.Shape;
        }

        return new DocumentBuildResult(
            states.All(x => x.Success),
            shapes,
            states,
            DateTime.UtcNow - started);
    }

    public static CommandShapeBuilderRegistry CreateDefaultRegistry()
    {
        var registry = new CommandShapeBuilderRegistry();
        registry.Register(new BoxCommandBuilder());
        registry.Register(new CylinderCommandBuilder());
        return registry;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        session.Dispose();
        GC.SuppressFinalize(this);
    }
}
