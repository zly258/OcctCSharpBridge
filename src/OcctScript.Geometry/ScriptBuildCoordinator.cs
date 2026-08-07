using OcctNet;
using OcctScript.Domain;
using OcctScript.Expressions;

namespace OcctScript.Geometry;

public enum CommandBuildStatus
{
    Succeeded,
    Failed,
    Skipped
}

public sealed record CommandBuildState(
    Guid CommandId,
    CommandBuildStatus Status,
    OcctModelShape Shape,
    IReadOnlyList<ShapeBuildMessage> Messages)
{
    public bool Success => Status != CommandBuildStatus.Failed;
}

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
    private readonly CommandRegistry commandDefinitions;
    private readonly CommandDependencyResolver dependencyResolver = new();
    private bool disposed;

    public ScriptBuildCoordinator(
        CommandRegistry? commandDefinitions = null,
        CommandShapeBuilderRegistry? builders = null,
        ExpressionEngine? expressionEngine = null)
    {
        session = new OcctModelingSession();
        this.commandDefinitions = commandDefinitions ?? CreateDefaultCommandRegistry();
        this.builders = builders ?? CreateDefaultBuilderRegistry();
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
        var states = new Dictionary<Guid, CommandBuildState>();
        var plan = dependencyResolver.Resolve(document);

        foreach (var pair in plan.Errors)
            states[pair.Key] = new CommandBuildState(pair.Key, CommandBuildStatus.Failed, default, pair.Value);

        foreach (var command in plan.OrderedCommands)
        {
            if (states.ContainsKey(command.Id)) continue;
            if (!command.IsEnabled)
            {
                states[command.Id] = new CommandBuildState(
                    command.Id,
                    CommandBuildStatus.Skipped,
                    default,
                    [ShapeBuildMessage.Warning("COMMAND_DISABLED", $"Command '{command.Name}' is disabled.")]);
                continue;
            }

            if (!commandDefinitions.TryGet(command.Type, out var definition) || definition is null)
            {
                states[command.Id] = Failure(command, "COMMAND_NOT_REGISTERED", $"Command type '{command.Type}' is not registered.");
                continue;
            }

            if (!builders.TryGet(command.Type, out var builder) || builder is null)
            {
                states[command.Id] = Failure(command, "BUILDER_NOT_REGISTERED", $"No shape builder is registered for command type '{command.Type}'.");
                continue;
            }

            var context = new ShapeBuildContext
            {
                Session = session,
                Parameters = parameters,
                CommandShapes = shapes,
                Fields = fields
            };

            ShapeBuildResult result;
            try
            {
                result = builder.Build(command, context);
                if (result.Success)
                {
                    var transformed = ShapeBuilderUtilities.ApplyTransform(session, result.Shape, command.Transform);
                    var actualTopology = ShapeBuilderUtilities.ToTopology(session.GetShapeType(transformed));
                    if (definition.OutputTopology != CommandTopologyKind.Any &&
                        (definition.OutputTopology & actualTopology) == 0)
                    {
                        if (session.Exists(transformed)) session.Delete(transformed);
                        result = ShapeBuildResult.Failed(
                            "OUTPUT_TOPOLOGY_INVALID",
                            $"Command '{command.Name}' produced '{actualTopology}', expected '{definition.OutputTopology}'.");
                    }
                    else
                    {
                        result = result with { Shape = transformed };
                    }
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
            {
                result = ShapeBuildResult.Failed("COMMAND_BUILD_FAILED", ex.Message);
            }

            var status = result.Success ? CommandBuildStatus.Succeeded : CommandBuildStatus.Failed;
            states[command.Id] = new CommandBuildState(command.Id, status, result.Shape, result.Messages);
            if (result.Success) shapes[command.Id] = result.Shape;
        }

        var orderedStates = document.Commands
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Select(command => states.TryGetValue(command.Id, out var state)
                ? state
                : Failure(command, "COMMAND_NOT_PLANNED", $"Command '{command.Name}' could not be scheduled."))
            .ToArray();

        return new DocumentBuildResult(
            orderedStates.All(x => x.Status != CommandBuildStatus.Failed),
            shapes,
            orderedStates,
            DateTime.UtcNow - started);
    }

    public static CommandRegistry CreateDefaultCommandRegistry()
    {
        var registry = new CommandRegistry();
        BuiltInCommandCatalog.RegisterAll(registry);
        return registry;
    }

    public static CommandShapeBuilderRegistry CreateDefaultBuilderRegistry()
    {
        var registry = new CommandShapeBuilderRegistry();
        registry.Register(new VertexCommandBuilder());
        registry.Register(new LineCommandBuilder());
        registry.Register(new PolylineCommandBuilder());
        registry.Register(new CircleCommandBuilder());
        registry.Register(new ArcCommandBuilder());
        registry.Register(new RectangleCommandBuilder());
        registry.Register(new WireCommandBuilder());
        registry.Register(new FaceCommandBuilder());
        registry.Register(new PlaneFaceCommandBuilder());
        registry.Register(new BoxCommandBuilder());
        registry.Register(new CylinderCommandBuilder());
        registry.Register(new ExtrudeCommandBuilder());
        registry.Register(new RevolveCommandBuilder());
        registry.Register(new SweepCommandBuilder());
        registry.Register(new LoftCommandBuilder());
        registry.Register(new BooleanCommandBuilder(BuiltInCommandCatalog.Fuse, OcctBooleanOperation.Fuse));
        registry.Register(new BooleanCommandBuilder(BuiltInCommandCatalog.Cut, OcctBooleanOperation.Cut));
        registry.Register(new BooleanCommandBuilder(BuiltInCommandCatalog.Common, OcctBooleanOperation.Common));
        registry.Register(new BooleanCommandBuilder(BuiltInCommandCatalog.Section, OcctBooleanOperation.Section));
        return registry;
    }

    private static CommandBuildState Failure(ScriptCommand command, string code, string message) =>
        new(command.Id, CommandBuildStatus.Failed, default, [ShapeBuildMessage.Error(code, message)]);

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        session.Dispose();
        GC.SuppressFinalize(this);
    }
}
