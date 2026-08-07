using OcctScript.Domain;

namespace OcctScript.Geometry;

internal sealed record CommandExecutionPlan(
    IReadOnlyList<ScriptCommand> OrderedCommands,
    IReadOnlyDictionary<Guid, IReadOnlyList<ShapeBuildMessage>> Errors);

internal sealed class CommandDependencyResolver
{
    public CommandExecutionPlan Resolve(ScriptDocument document)
    {
        var commands = document.Commands.ToDictionary(x => x.Id);
        var errors = new Dictionary<Guid, List<ShapeBuildMessage>>();
        var dependencies = new Dictionary<Guid, HashSet<Guid>>();
        var dependents = new Dictionary<Guid, List<Guid>>();

        foreach (var command in document.Commands)
        {
            var refs = command.Fields.Values
                .SelectMany(value => value.ReferenceId is Guid single
                    ? value.ReferenceIds.Append(single)
                    : value.ReferenceIds)
                .Distinct()
                .ToHashSet();
            dependencies[command.Id] = refs;

            foreach (var reference in refs)
            {
                if (!commands.ContainsKey(reference))
                {
                    AddError(errors, command.Id, "REFERENCE_MISSING", $"Referenced command '{reference}' does not exist.");
                    continue;
                }
                if (!dependents.TryGetValue(reference, out var list))
                    dependents[reference] = list = [];
                list.Add(command.Id);
            }
        }

        var indegree = document.Commands.ToDictionary(
            command => command.Id,
            command => dependencies[command.Id].Count(reference => commands.ContainsKey(reference)));
        var ready = new PriorityQueue<ScriptCommand, (int Order, string Name, Guid Id)>();
        foreach (var command in document.Commands.Where(command => indegree[command.Id] == 0))
            ready.Enqueue(command, (command.Order, command.Name, command.Id));

        var ordered = new List<ScriptCommand>(document.Commands.Count);
        while (ready.TryDequeue(out var command, out _))
        {
            ordered.Add(command);
            if (!dependents.TryGetValue(command.Id, out var children)) continue;
            foreach (var childId in children)
            {
                indegree[childId]--;
                if (indegree[childId] == 0)
                {
                    var child = commands[childId];
                    ready.Enqueue(child, (child.Order, child.Name, child.Id));
                }
            }
        }

        foreach (var command in document.Commands.Where(command => indegree[command.Id] > 0))
            AddError(errors, command.Id, "DEPENDENCY_CYCLE", $"Command '{command.Name}' is part of a circular dependency.");

        return new CommandExecutionPlan(
            ordered,
            errors.ToDictionary(x => x.Key, x => (IReadOnlyList<ShapeBuildMessage>)x.Value));
    }

    private static void AddError(
        IDictionary<Guid, List<ShapeBuildMessage>> errors,
        Guid commandId,
        string code,
        string message)
    {
        if (!errors.TryGetValue(commandId, out var list)) errors[commandId] = list = [];
        list.Add(ShapeBuildMessage.Error(code, message));
    }
}
