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
    private void RebuildModel(bool fit = true)
    {
        if (!viewportReady || closing || isRebuilding) return;
        isRebuilding = true;
        try
        {
            CommitPendingEdits();
            buildMessages.Clear();
            var engine = Viewport.Engine;
            engine.Clear();
            displayedShapes.Clear();
            commandByDisplayedShape.Clear();

            foreach (var message in documentValidator.Validate(document))
            {
                var source = message.ObjectId is Guid objectId
                    ? document.FindCommand(objectId)?.Name ?? document.FindParameter(objectId)?.Name ?? objectId.ToString()
                    : ResourceText("Ui.Build");
                buildMessages.Add(new BuildMessageRow(
                    SeverityText(message.Severity),
                    source,
                    message.Message));
            }

            var parameterResult = parameterService.Evaluate(document);
            foreach (var error in parameterResult.Errors)
            {
                var parameter = document.FindParameter(error.Key);
                buildMessages.Add(new BuildMessageRow(ResourceText("Ui.Error"), parameter?.Name ?? error.Key.ToString(), error.Value));
            }

            var result = buildCoordinator.Build(document, parameterResult.Values);
            foreach (var state in result.Commands)
            {
                var command = document.FindCommand(state.CommandId);
                foreach (var message in state.Messages)
                {
                    buildMessages.Add(new BuildMessageRow(
                        message.IsError ? ResourceText("Ui.Error") : ResourceText("Ui.Warning"),
                        command?.Name ?? state.CommandId.ToString(),
                        message.Message));
                }
            }

            foreach (var pair in result.Shapes)
            {
                var command = document.FindCommand(pair.Key);
                if (command is null || !command.Display.IsVisible) continue;
                var displayed = engine.Display(buildCoordinator.Session, pair.Value, fit: false);
                engine.SetName(displayed, command.Name);
                engine.SetColor(displayed, ParseColor(command.Display.Color));
                engine.SetTransparency(displayed, Math.Clamp(command.Display.Transparency, 0.0, 1.0));
                engine.SetVisible(displayed, true);
                engine.SetMaterial(displayed, OcctMaterial.Steel);
                displayedShapes[pair.Key] = displayed;
                commandByDisplayedShape[displayed.Id] = pair.Key;
            }

            engine.Redraw();
            if (fit && displayedShapes.Count > 0) engine.FitAll();
            SelectDisplayedCommand(selectedCommand);
            StatusText.Text = result.Success
                ? string.Format(ResourceText("Ui.BuildCompleted"), displayedShapes.Count, Math.Round(result.Duration.TotalMilliseconds))
                : ResourceText("Ui.BuildFailed");
        }
        catch (Exception ex)
        {
            buildMessages.Add(new BuildMessageRow(ResourceText("Ui.Error"), ResourceText("Ui.Build"), ex.Message));
            StatusText.Text = ResourceText("Ui.BuildFailed");
        }
        finally
        {
            isRebuilding = false;
        }
    }

    private string SeverityText(ValidationSeverity severity) => severity switch
    {
        ValidationSeverity.Error => ResourceText("Ui.Error"),
        ValidationSeverity.Warning => ResourceText("Ui.Warning"),
        _ => ResourceText("Ui.Information")
    };

    private static DrawingColor ParseColor(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.StartsWith('#') && value.Length == 7 &&
            int.TryParse(value.AsSpan(1, 2), NumberStyles.HexNumber, null, out var red) &&
            int.TryParse(value.AsSpan(3, 2), NumberStyles.HexNumber, null, out var green) &&
            int.TryParse(value.AsSpan(5, 2), NumberStyles.HexNumber, null, out var blue))
            return DrawingColor.FromArgb(red, green, blue);
        return DrawingColor.LightSteelBlue;
    }

    private void SelectDisplayedCommand(ScriptCommand? command)
    {
        if (!viewportReady) return;
        var engine = Viewport.Engine;
        engine.ClearSelection();
        if (command is not null && displayedShapes.TryGetValue(command.Id, out var shape))
            engine.SelectObject(new OcctObject(shape.Id, OcctObjectKind.Shape));
    }

    private void Viewport_SelectionChanged(object? sender, OcctShape? shape)
    {
        if (shape is null || !commandByDisplayedShape.TryGetValue(shape.Value.Id, out var commandId)) return;
        var command = document.FindCommand(commandId);
        if (command is null || ReferenceEquals(CommandList.SelectedItem, command)) return;
        CommandList.SelectedItem = command;
        CommandList.ScrollIntoView(command);
    }
}
