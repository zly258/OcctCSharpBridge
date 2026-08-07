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
    private ScriptDocument CreateDefaultDocument()
    {
        var result = new ScriptDocument { Name = "Untitled" };
        result.Parameters.Add(new ScriptParameter { Name = "Width", DisplayName = "Width", Type = ScriptParameterType.Length, Expression = "1000", Unit = "mm" });
        result.Parameters.Add(new ScriptParameter { Name = "Depth", DisplayName = "Depth", Type = ScriptParameterType.Length, Expression = "800", Unit = "mm" });
        result.Parameters.Add(new ScriptParameter { Name = "Height", DisplayName = "Height", Type = ScriptParameterType.Length, Expression = "500", Unit = "mm" });

        var line1 = CreateCommand(BuiltInCommandCatalog.Line, "Line1", 10);
        SetLiteral(line1, "start", "0, 0, 0");
        SetLiteral(line1, "end", "Width, 0, 0");
        var line2 = CreateCommand(BuiltInCommandCatalog.Line, "Line2", 20);
        SetLiteral(line2, "start", "Width, 0, 0");
        SetLiteral(line2, "end", "Width, Depth, 0");
        var line3 = CreateCommand(BuiltInCommandCatalog.Line, "Line3", 30);
        SetLiteral(line3, "start", "Width, Depth, 0");
        SetLiteral(line3, "end", "0, Depth, 0");
        var line4 = CreateCommand(BuiltInCommandCatalog.Line, "Line4", 40);
        SetLiteral(line4, "start", "0, Depth, 0");
        SetLiteral(line4, "end", "0, 0, 0");
        var wire = CreateCommand(BuiltInCommandCatalog.Wire, "Wire1", 50);
        SetReferences(wire, "curves", line1.Id, line2.Id, line3.Id, line4.Id);
        var face = CreateCommand(BuiltInCommandCatalog.Face, "Face1", 60);
        SetReference(face, "profile", wire.Id);
        var extrude = CreateCommand(BuiltInCommandCatalog.Extrude, "Extrude1", 70);
        SetReference(extrude, "profile", face.Id);
        SetExpression(extrude, "distance", "Height");
        extrude.Display.Color = "#60A5FA";

        foreach (var intermediate in new[] { line1, line2, line3, line4, wire, face })
            intermediate.Display.IsVisible = false;
        result.Commands.AddRange([line1, line2, line3, line4, wire, face, extrude]);
        result.OutputCommandIds.Add(extrude.Id);
        return result;
    }

    private ScriptCommand CreateCommand(string type, string name, int order)
    {
        var command = BuiltInCommandCatalog.CreateDefault(commandRegistry.GetRequired(type), order);
        command.Name = name;
        return command;
    }

    private static void SetExpression(ScriptCommand command, string fieldName, string expression) =>
        command.Fields[fieldName].Expression = expression;

    private static void SetLiteral(ScriptCommand command, string fieldName, string literal) =>
        command.Fields[fieldName].Literal = literal;

    private static void SetReference(ScriptCommand command, string fieldName, Guid id) =>
        command.Fields[fieldName].ReferenceId = id;

    private static void SetReferences(ScriptCommand command, string fieldName, params Guid[] ids) =>
        command.Fields[fieldName].ReferenceIds = [.. ids];
}
