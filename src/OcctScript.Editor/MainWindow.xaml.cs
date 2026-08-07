using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using OcctNet;
using OcctScript.Application;
using OcctScript.Application.History;
using OcctScript.Domain;
using OcctScript.Expressions;
using OcctScript.Geometry;
using OcctScript.Serialization;

namespace OcctScript.Editor;

public partial class MainWindow : Window
{
    private readonly ExpressionEngine expressionEngine = new();
    private readonly ParameterService parameterService;
    private readonly ScriptDocumentSerializer serializer = new();
    private readonly ScriptBuildCoordinator buildCoordinator = new();
    private readonly CommandRegistry commandRegistry = new();
    private readonly DocumentHistory history = new();
    private readonly ObservableCollection<CommandFieldRow> fieldRows = [];
    private readonly ObservableCollection<BuildMessageRow> buildMessages = [];
    private readonly Dictionary<Guid, OcctShape> displayedShapes = [];
    private readonly Dictionary<long, Guid> commandByDisplayedShape = [];

    private ScriptDocument document = new();
    private ScriptCommand? selectedCommand;
    private string? currentFilePath;
    private string? parameterEditOriginal;
    private string? fieldEditOriginal;
    private bool isDirty;
    private bool viewportReady;
    private bool closing;

    public MainWindow()
    {
        InitializeComponent();
        parameterService = new ParameterService(expressionEngine);
        BuiltInCommandCatalog.RegisterAll(commandRegistry);
        PropertyGrid.ItemsSource = fieldRows;
        BuildMessageGrid.ItemsSource = buildMessages;
        history.Changed += (_, _) => UpdateHistoryState();
        Viewport.EngineInitialized += Viewport_EngineInitialized;
        Viewport.SelectionChanged += Viewport_SelectionChanged;
        Loaded += MainWindow_Loaded;
        CreateNewDocument();
        RegisterInputBindings();
    }

    private void RegisterInputBindings()
    {
        InputBindings.Add(new System.Windows.Input.KeyBinding(new RelayCommand(_ => CreateNewDocument()), new System.Windows.Input.KeyGesture(System.Windows.Input.Key.N, System.Windows.Input.ModifierKeys.Control)));
        InputBindings.Add(new System.Windows.Input.KeyBinding(new RelayCommand(_ => OpenDocument()), new System.Windows.Input.KeyGesture(System.Windows.Input.Key.O, System.Windows.Input.ModifierKeys.Control)));
        InputBindings.Add(new System.Windows.Input.KeyBinding(new RelayCommand(_ => SaveDocument()), new System.Windows.Input.KeyGesture(System.Windows.Input.Key.S, System.Windows.Input.ModifierKeys.Control)));
        InputBindings.Add(new System.Windows.Input.KeyBinding(new RelayCommand(_ => Undo()), new System.Windows.Input.KeyGesture(System.Windows.Input.Key.Z, System.Windows.Input.ModifierKeys.Control)));
        InputBindings.Add(new System.Windows.Input.KeyBinding(new RelayCommand(_ => Redo()), new System.Windows.Input.KeyGesture(System.Windows.Input.Key.Y, System.Windows.Input.ModifierKeys.Control)));
        InputBindings.Add(new System.Windows.Input.KeyBinding(new RelayCommand(_ => RebuildModel()), new System.Windows.Input.KeyGesture(System.Windows.Input.Key.F5)));
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (Viewport.Engine.IsInitialized) InitializeViewport();
    }

    private void Viewport_EngineInitialized(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(InitializeViewport));

    private void InitializeViewport()
    {
        if (viewportReady || !Viewport.Engine.IsInitialized) return;
        viewportReady = true;
        var engine = Viewport.Engine;
        engine.SetGradientBackground(Color.FromArgb(246, 248, 251), Color.FromArgb(203, 213, 225), OcctGradientFillMethod.Vertical);
        engine.SetDisplayMode(OcctDisplayMode.Shaded);
        engine.SetTriedronVisible(true);
        engine.SetViewCubeVisible(true);
        engine.SetView(OcctViewOrientation.Isometric);
        RebuildModel();
    }

    private void CreateNewDocument()
    {
        document = new ScriptDocument { Name = "Untitled" };
        document.Parameters.Add(new ScriptParameter { Name = "Width", DisplayName = "Width", Type = ScriptParameterType.Length, Expression = "1000", Unit = "mm" });
        document.Parameters.Add(new ScriptParameter { Name = "Depth", DisplayName = "Depth", Type = ScriptParameterType.Length, Expression = "800", Unit = "mm" });
        document.Parameters.Add(new ScriptParameter { Name = "Height", DisplayName = "Height", Type = ScriptParameterType.Length, Expression = "500", Unit = "mm" });

        var box = BuiltInCommandCatalog.CreateDefault(commandRegistry.GetRequired(BuiltInCommandCatalog.Box), 10);
        box.Name = "Box1";
        box.Fields["width"].Expression = "Width";
        box.Fields["depth"].Expression = "Depth";
        box.Fields["height"].Expression = "Height";
        document.Commands.Add(box);
        document.OutputCommandIds.Add(box.Id);

        currentFilePath = null;
        isDirty = false;
        history.Clear();
        RefreshDocumentBindings(box.Id);
        RebuildModel();
    }

    private void RefreshDocumentBindings(Guid? preferredCommandId = null)
    {
        var commandId = preferredCommandId ?? selectedCommand?.Id;
        CommandList.ItemsSource = document.Commands.OrderBy(x => x.Order).ToArray();
        ParameterGrid.ItemsSource = document.Parameters;
        selectedCommand = commandId.HasValue ? document.FindCommand(commandId.Value) : document.Commands.FirstOrDefault();
        CommandList.SelectedItem = selectedCommand;
        RefreshFieldRows();
        UpdateTitle();
        UpdateHistoryState();
    }

    private void RefreshFieldRows()
    {
        fieldRows.Clear();
        selectedCommand = CommandList.SelectedItem as ScriptCommand;
        if (selectedCommand is null) return;
        var definition = commandRegistry.GetRequired(selectedCommand.Type);
        foreach (var fieldDefinition in definition.Fields)
        {
            selectedCommand.Fields.TryGetValue(fieldDefinition.Name, out var fieldValue);
            fieldRows.Add(new CommandFieldRow
            {
                Name = fieldDefinition.Name,
                Expression = fieldValue?.Expression ?? fieldDefinition.DefaultValue,
                UnitType = fieldDefinition.UnitType,
                IsRequired = fieldDefinition.IsRequired
            });
        }
    }

    private void RebuildModel(bool fit = true)
    {
        if (!viewportReady || closing) return;
        CommitPendingEdits();
        buildMessages.Clear();
        try
        {
            var engine = Viewport.Engine;
            engine.Clear();
            displayedShapes.Clear();
            commandByDisplayedShape.Clear();

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
                    buildMessages.Add(new BuildMessageRow(message.IsError ? ResourceText("Ui.Error") : ResourceText("Ui.Information"), command?.Name ?? state.CommandId.ToString(), message.Message));
                }
            }

            foreach (var pair in result.Shapes)
            {
                var command = document.FindCommand(pair.Key);
                if (command is null) continue;
                var displayed = engine.Display(buildCoordinator.Session, pair.Value, fit: false);
                engine.SetName(displayed, command.Name);
                engine.SetColor(displayed, ParseColor(command.Display.Color));
                engine.SetTransparency(displayed, Math.Clamp(command.Display.Transparency, 0.0, 1.0));
                engine.SetVisible(displayed, command.Display.IsVisible);
                engine.SetMaterial(displayed, OcctMaterial.Steel);
                displayedShapes[pair.Key] = displayed;
                commandByDisplayedShape[displayed.Id] = pair.Key;
            }

            engine.Redraw();
            if (fit && displayedShapes.Count > 0) engine.FitAll();
            SelectDisplayedCommand(selectedCommand);
            StatusText.Text = string.Format(ResourceText("Ui.BuildCompleted"), displayedShapes.Count, Math.Round(result.Duration.TotalMilliseconds));
        }
        catch (Exception ex)
        {
            buildMessages.Add(new BuildMessageRow(ResourceText("Ui.Error"), ResourceText("Ui.Build"), ex.Message));
            StatusText.Text = ResourceText("Ui.BuildFailed");
        }
    }

    private static Color ParseColor(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.StartsWith('#') && value.Length == 7 &&
            int.TryParse(value.AsSpan(1, 2), System.Globalization.NumberStyles.HexNumber, null, out var red) &&
            int.TryParse(value.AsSpan(3, 2), System.Globalization.NumberStyles.HexNumber, null, out var green) &&
            int.TryParse(value.AsSpan(5, 2), System.Globalization.NumberStyles.HexNumber, null, out var blue))
            return Color.FromArgb(red, green, blue);
        return Color.LightSteelBlue;
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
        if (command is not null && !ReferenceEquals(CommandList.SelectedItem, command))
        {
            CommandList.SelectedItem = command;
            CommandList.ScrollIntoView(command);
        }
    }

    private void AddCommand(string commandType)
    {
        var definition = commandRegistry.GetRequired(commandType);
        var command = BuiltInCommandCatalog.CreateDefault(definition, document.Commands.Count == 0 ? 10 : document.Commands.Max(x => x.Order) + 10);
        command.Name = CreateUniqueCommandName(commandType);
        history.Execute(document, new AddCommandAction(command));
        MarkDirty();
        RefreshDocumentBindings(command.Id);
        RebuildModel();
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
        if (CommandList.SelectedItem is not ScriptCommand command) return;
        history.Execute(document, new RemoveCommandAction(command.Id));
        MarkDirty();
        RefreshDocumentBindings();
        RebuildModel();
    }

    private void DeleteSelectedParameter()
    {
        if (ParameterGrid.SelectedItem is not ScriptParameter parameter) return;
        history.Execute(document, new RemoveParameterAction(parameter.Id));
        MarkDirty();
        RefreshDocumentBindings(selectedCommand?.Id);
        RebuildModel();
    }

    private void Undo()
    {
        if (!history.CanUndo) return;
        var selectedId = selectedCommand?.Id;
        history.Undo(document);
        MarkDirty();
        RefreshDocumentBindings(selectedId);
        RebuildModel();
    }

    private void Redo()
    {
        if (!history.CanRedo) return;
        var selectedId = selectedCommand?.Id;
        history.Redo(document);
        MarkDirty();
        RefreshDocumentBindings(selectedId);
        RebuildModel();
    }

    private async void OpenDocument()
    {
        var dialog = new OpenFileDialog { Filter = "OcctScript project (*.ocsproj)|*.ocsproj|JSON document (*.json)|*.json|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            document = await serializer.LoadAsync(dialog.FileName);
            currentFilePath = dialog.FileName;
            isDirty = false;
            history.Clear();
            RefreshDocumentBindings();
            RebuildModel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, ResourceText("Ui.OpenFailed"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SaveDocument()
    {
        var path = currentFilePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            var dialog = new SaveFileDialog
            {
                Filter = "OcctScript project (*.ocsproj)|*.ocsproj|JSON document (*.json)|*.json",
                FileName = document.Name + ".ocsproj",
                AddExtension = true,
                DefaultExt = ".ocsproj"
            };
            if (dialog.ShowDialog(this) != true) return;
            path = dialog.FileName;
        }
        try
        {
            await serializer.SaveAsync(path, document);
            currentFilePath = path;
            isDirty = false;
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, ResourceText("Ui.SaveFailed"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MarkDirty() { isDirty = true; UpdateTitle(); }

    private void UpdateTitle()
    {
        var marker = isDirty ? " *" : string.Empty;
        Title = $"{document.Name}{marker} - {ResourceText("Ui.AppTitle")}";
        DocumentPathText.Text = currentFilePath ?? "Untitled.ocsproj";
    }

    private void UpdateHistoryState()
    {
        UndoButton.IsEnabled = history.CanUndo;
        RedoButton.IsEnabled = history.CanRedo;
        UndoMenuItem.IsEnabled = history.CanUndo;
        RedoMenuItem.IsEnabled = history.CanRedo;
    }

    private void CommitPendingEdits()
    {
        ParameterGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        ParameterGrid.CommitEdit(DataGridEditingUnit.Row, true);
        PropertyGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        PropertyGrid.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private string ResourceText(string key) => FindResource(key)?.ToString() ?? key;

    private void CommandList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedCommand = CommandList.SelectedItem as ScriptCommand;
        RefreshFieldRows();
        SelectDisplayedCommand(selectedCommand);
    }

    private void ParameterGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) =>
        parameterEditOriginal = e.Row.Item is ScriptParameter parameter ? parameter.Expression : null;

    private void ParameterGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.Row.Item is not ScriptParameter parameter || e.EditingElement is not TextBox textBox) return;
        var newValue = textBox.Text.Trim();
        var oldValue = parameterEditOriginal ?? parameter.Expression;
        parameterEditOriginal = null;
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal)) return;
        e.Cancel = true;
        history.Execute(document, new ChangeParameterExpressionAction(parameter.Id, oldValue, newValue));
        MarkDirty();
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { ParameterGrid.Items.Refresh(); RebuildModel(); }));
    }

    private void PropertyGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) =>
        fieldEditOriginal = e.Row.Item is CommandFieldRow row ? row.Expression : null;

    private void PropertyGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (selectedCommand is null || e.Row.Item is not CommandFieldRow row || e.EditingElement is not TextBox textBox) return;
        var newValue = textBox.Text.Trim();
        var oldValue = fieldEditOriginal ?? row.Expression;
        fieldEditOriginal = null;
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal)) return;
        e.Cancel = true;
        history.Execute(document, new ChangeCommandFieldExpressionAction(selectedCommand.Id, row.Name, oldValue, newValue));
        row.Expression = newValue;
        MarkDirty();
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { PropertyGrid.Items.Refresh(); RebuildModel(); }));
    }

    private void CommandName_LostFocus(object sender, RoutedEventArgs e) { if (selectedCommand is null) return; MarkDirty(); CommandList.Items.Refresh(); }
    private void CommandEnabled_Click(object sender, RoutedEventArgs e) { if (selectedCommand is null) return; MarkDirty(); RebuildModel(); }
    private void Transform_LostFocus(object sender, RoutedEventArgs e) { if (selectedCommand is null) return; MarkDirty(); RebuildModel(); }
    private void New_Click(object sender, RoutedEventArgs e) => CreateNewDocument();
    private void Open_Click(object sender, RoutedEventArgs e) => OpenDocument();
    private void Save_Click(object sender, RoutedEventArgs e) => SaveDocument();
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    private void Undo_Click(object sender, RoutedEventArgs e) => Undo();
    private void Redo_Click(object sender, RoutedEventArgs e) => Redo();
    private void AddBox_Click(object sender, RoutedEventArgs e) => AddCommand(BuiltInCommandCatalog.Box);
    private void AddCylinder_Click(object sender, RoutedEventArgs e) => AddCommand(BuiltInCommandCatalog.Cylinder);
    private void AddParameter_Click(object sender, RoutedEventArgs e) => AddParameter();
    private void DeleteCommand_Click(object sender, RoutedEventArgs e) => DeleteSelectedCommand();
    private void DeleteParameter_Click(object sender, RoutedEventArgs e) => DeleteSelectedParameter();
    private void Rebuild_Click(object sender, RoutedEventArgs e) => RebuildModel();
    private void Fit_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.FitAll(); }
    private void Isometric_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Isometric); }
    private void Front_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Front); }
    private void Top_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Top); }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        closing = true;
        buildCoordinator.Dispose();
    }
}

internal sealed class RelayCommand(Action<object?> execute) : System.Windows.Input.ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute(parameter);
}
