using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using OcctNet;
using OcctScript.Application;
using OcctScript.Application.History;
using OcctScript.Domain;
using OcctScript.Geometry;
using DrawingColor = System.Drawing.Color;

namespace OcctScript.Editor;

public partial class MainWindow
{
    private void AddCommand(string commandType)
    {
        var definition = commandRegistry.GetRequired(commandType);
        var order = document.Commands.Count == 0 ? 10 : document.Commands.Max(x => x.Order) + 10;
        var command = BuiltInCommandCatalog.CreateDefault(definition, order);
        command.Name = CreateUniqueCommandName(commandType);
        InitializeCommandReferences(command, definition);
        history.Execute(document, new AddCommandAction(command));
        MarkDirty();
        RefreshDocumentBindings(command.Id);
        RebuildModel();
    }

    private void InitializeCommandReferences(ScriptCommand command, CommandDefinition definition)
    {
        var selected = CommandList.SelectedItems.Cast<ScriptCommand>().ToArray();
        var candidates = selected
            .Concat(document.Commands.OrderByDescending(x => x.Order))
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToArray();
        var used = new HashSet<Guid>();

        foreach (var field in definition.Fields.Where(x => x.FieldType is CommandFieldType.CommandReference or CommandFieldType.CommandReferenceList))
        {
            var compatible = candidates
                .Where(candidate => IsCompatibleReference(candidate, field) && candidate.Id != command.Id)
                .ToArray();
            if (!command.Fields.TryGetValue(field.Name, out var value))
                command.Fields[field.Name] = value = new CommandValue();

            if (field.FieldType == CommandFieldType.CommandReference)
            {
                var candidate = compatible.FirstOrDefault(x => !used.Contains(x.Id)) ?? compatible.FirstOrDefault();
                if (candidate is null) continue;
                value.ReferenceId = candidate.Id;
                used.Add(candidate.Id);
            }
            else
            {
                var preferred = selected.Where(candidate => IsCompatibleReference(candidate, field)).ToArray();
                var chosen = preferred.Length >= field.MinReferences
                    ? preferred
                    : compatible.Take(Math.Max(field.MinReferences, preferred.Length)).ToArray();
                value.ReferenceIds = chosen.Select(x => x.Id).Distinct().ToList();
            }
        }
    }

    private bool IsCompatibleReference(ScriptCommand candidate, CommandFieldDefinition field)
    {
        if (!commandRegistry.TryGet(candidate.Type, out var candidateDefinition) || candidateDefinition is null) return false;
        return field.AcceptedTopology == CommandTopologyKind.Any ||
               (field.AcceptedTopology & candidateDefinition.OutputTopology) != 0;
    }

    private string CreateUniqueCommandName(string commandType)
    {
        for (var index = 1; ; index++)
        {
            var candidate = commandType + index;
            if (document.Commands.All(x => !string.Equals(x.Name, candidate, StringComparison.OrdinalIgnoreCase))) return candidate;
        }
    }

    private void AddParameter()
    {
        var index = 1;
        string name;
        do name = "Parameter" + index++; while (document.Parameters.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
        var parameter = new ScriptParameter { Name = name, DisplayName = name, Type = ScriptParameterType.Length, Expression = "100", Unit = "mm" };
        history.Execute(document, new AddParameterAction(parameter));
        MarkDirty();
        RefreshDocumentBindings(selectedCommand?.Id);
        ParameterGrid.SelectedItem = parameter;
        RebuildModel();
    }

    private void DeleteSelectedCommand()
    {
        var selected = CommandList.SelectedItems.Cast<ScriptCommand>().ToArray();
        if (selected.Length == 0) return;
        using (history.BeginTransaction("Remove commands"))
        foreach (var command in selected.OrderByDescending(x => x.Order))
            history.Execute(document, new RemoveCommandAction(command.Id));
        MarkDirty();
        RefreshDocumentBindings();
        RebuildModel();
    }
}
