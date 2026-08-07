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
    private void ParameterGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (e.Row.Item is not ScriptParameter parameter) return;
        parameterEditProperty = e.Column.SortMemberPath;
        parameterEditOriginal = parameterEditProperty switch
        {
            "Name" => parameter.Name,
            "Unit" => parameter.Unit,
            _ => parameter.Expression
        };
    }

    private void ParameterGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.Row.Item is not ScriptParameter parameter || e.EditingElement is not System.Windows.Controls.TextBox textBox) return;
        var property = parameterEditProperty ?? e.Column.SortMemberPath;
        var newValue = textBox.Text.Trim();
        var oldValue = parameterEditOriginal ?? string.Empty;
        parameterEditOriginal = null;
        parameterEditProperty = null;
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal)) return;

        try
        {
            e.Cancel = true;
            switch (property)
            {
                case "Name":
                    ValidateParameterName(parameter, newValue);
                    history.Execute(document, new RenameParameterAction(parameter.Id, oldValue, newValue));
                    break;
                case "Unit":
                    history.Execute(document, new ChangeParameterUnitAction(parameter.Id, oldValue, newValue));
                    break;
                default:
                    history.Execute(document, new ChangeParameterExpressionAction(parameter.Id, oldValue, newValue));
                    break;
            }
            MarkDirty();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ParameterGrid.Items.Refresh();
                RefreshFieldRows();
                RebuildModel();
            }));
        }
        catch (Exception ex)
        {
            e.Cancel = true;
            System.Windows.MessageBox.Show(this, ex.Message, ResourceText("Ui.InvalidValue"), MessageBoxButton.OK, MessageBoxImage.Warning);
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ParameterGrid.Items.Refresh()));
        }
    }

    private void ValidateParameterName(ScriptParameter parameter, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !(char.IsLetter(value[0]) || value[0] == '_') ||
            value.Skip(1).Any(x => !char.IsLetterOrDigit(x) && x != '_'))
            throw new InvalidOperationException($"'{value}' is not a valid parameter identifier.");
        if (document.Parameters.Any(x => x.Id != parameter.Id && string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Parameter name '{value}' already exists.");
    }

    private void PropertyGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) =>
        fieldEditOriginal = e.Row.Item is CommandFieldRow row ? row.ValueText : null;

    private void PropertyGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (selectedCommand is null || e.Row.Item is not CommandFieldRow row || e.EditingElement is not System.Windows.Controls.TextBox textBox) return;
        var newValue = textBox.Text.Trim();
        var oldText = fieldEditOriginal ?? row.ValueText;
        fieldEditOriginal = null;
        if (string.Equals(oldText, newValue, StringComparison.Ordinal)) return;

        try
        {
            if (!selectedCommand.Fields.TryGetValue(row.Definition.Name, out var current))
                selectedCommand.Fields[row.Definition.Name] = current = new CommandValue();
            var after = ParseFieldValue(selectedCommand, row.Definition, current, newValue);
            e.Cancel = true;
            history.Execute(document, new ChangeCommandFieldValueAction(selectedCommand.Id, row.Definition.Name, current, after));
            row.ValueText = newValue;
            MarkDirty();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                PropertyGrid.Items.Refresh();
                RebuildModel();
            }));
        }
        catch (Exception ex)
        {
            e.Cancel = true;
            System.Windows.MessageBox.Show(this, ex.Message, ResourceText("Ui.InvalidValue"), MessageBoxButton.OK, MessageBoxImage.Warning);
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RefreshFieldRows));
        }
    }

    private CommandValue ParseFieldValue(
        ScriptCommand owner,
        CommandFieldDefinition definition,
        CommandValue current,
        string text)
    {
        var result = current.Clone();
        switch (definition.FieldType)
        {
            case CommandFieldType.CommandReference:
                result.Expression = string.Empty;
                result.Literal = null;
                result.ReferenceIds.Clear();
                result.ReferenceId = string.IsNullOrWhiteSpace(text) ? null : ResolveCommandReference(owner, definition, text).Id;
                break;
            case CommandFieldType.CommandReferenceList:
                result.Expression = string.Empty;
                result.Literal = null;
                result.ReferenceId = null;
                result.ReferenceIds = string.IsNullOrWhiteSpace(text)
                    ? []
                    : text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(token => ResolveCommandReference(owner, definition, token).Id)
                        .Distinct()
                        .ToList();
                break;
            case CommandFieldType.Expression:
            case CommandFieldType.Number:
            case CommandFieldType.Integer:
                result.Expression = text;
                result.Literal = null;
                result.ReferenceId = null;
                result.ReferenceIds.Clear();
                break;
            default:
                result.Expression = string.Empty;
                result.Literal = text;
                result.ReferenceId = null;
                result.ReferenceIds.Clear();
                break;
        }
        return result;
    }

    private ScriptCommand ResolveCommandReference(
        ScriptCommand owner,
        CommandFieldDefinition field,
        string token)
    {
        ScriptCommand? referenced = null;
        if (Guid.TryParse(token, out var id)) referenced = document.FindCommand(id);
        referenced ??= document.Commands.FirstOrDefault(x => string.Equals(x.Name, token, StringComparison.OrdinalIgnoreCase));
        if (referenced is null) throw new InvalidOperationException($"Command reference '{token}' was not found.");
        if (referenced.Id == owner.Id) throw new InvalidOperationException("A command cannot reference itself.");
        if (!IsCompatibleReference(referenced, field))
            throw new InvalidOperationException($"Command '{referenced.Name}' output is not valid for field '{field.Name}'.");
        return referenced;
    }

    private void CommandName_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        commandNameOriginal = selectedCommand?.Name;

    private void CommandName_LostFocus(object sender, RoutedEventArgs e)
    {
        if (selectedCommand is null || sender is not System.Windows.Controls.TextBox textBox) return;
        var before = commandNameOriginal ?? selectedCommand.Name;
        var after = textBox.Text.Trim();
        commandNameOriginal = null;
        if (string.Equals(before, after, StringComparison.Ordinal)) return;
        if (string.IsNullOrWhiteSpace(after) || document.Commands.Any(x => x.Id != selectedCommand.Id && string.Equals(x.Name, after, StringComparison.OrdinalIgnoreCase)))
        {
            selectedCommand.Name = before;
            textBox.Text = before;
            System.Windows.MessageBox.Show(this, "Command names must be non-empty and unique.", ResourceText("Ui.InvalidValue"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        history.Execute(document, new ChangeCommandNameAction(selectedCommand.Id, before, after));
        MarkDirty();
        CommandList.Items.Refresh();
        RefreshFieldRows();
        RebuildModel(fit: false);
    }

    private void CommandEnabled_Click(object sender, RoutedEventArgs e)
    {
        if (selectedCommand is null) return;
        var after = selectedCommand.IsEnabled;
        history.Execute(document, new ChangeCommandEnabledAction(selectedCommand.Id, !after, after));
        MarkDirty();
        RebuildModel();
    }

    private void Transform_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        transformOriginal ??= selectedCommand?.Transform.Clone();

    private void Transform_LostFocus(object sender, RoutedEventArgs e)
    {
        if (selectedCommand is null || transformOriginal is null) return;
        var before = transformOriginal;
        var after = selectedCommand.Transform.Clone();
        transformOriginal = null;
        if (TransformsEqual(before, after)) return;
        history.Execute(document, new ChangeCommandTransformAction(selectedCommand.Id, before, after));
        MarkDirty();
        RebuildModel(fit: false);
    }

    private static bool TransformsEqual(TransformDefinition left, TransformDefinition right) =>
        left.X == right.X && left.Y == right.Y && left.Z == right.Z &&
        left.RotationX == right.RotationX && left.RotationY == right.RotationY && left.RotationZ == right.RotationZ &&
        left.Scale == right.Scale;
}
