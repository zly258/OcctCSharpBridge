using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OcctNet;
using OcctScript.Application;
using OcctScript.Application.History;
using OcctScript.Domain;
using OcctScript.Expressions;
using OcctScript.Geometry;
using OcctScript.Serialization;
using DrawingColor = System.Drawing.Color;

namespace OcctScript.Editor;

public partial class MainWindow : Window
{
    private readonly ExpressionEngine expressionEngine = new();
    private readonly ParameterService parameterService;
    private readonly ScriptDocumentSerializer serializer = new();
    private readonly CommandRegistry commandRegistry = new();
    private readonly ScriptBuildCoordinator buildCoordinator;
    private readonly DocumentValidator documentValidator;
    private readonly DocumentHistory history = new();
    private readonly ObservableCollection<CommandFieldRow> fieldRows = [];
    private readonly ObservableCollection<BuildMessageRow> buildMessages = [];
    private readonly Dictionary<Guid, OcctShape> displayedShapes = [];
    private readonly Dictionary<long, Guid> commandByDisplayedShape = [];

    private ScriptDocument document = new();
    private ScriptCommand? selectedCommand;
    private string? currentFilePath;
    private string? parameterEditOriginal;
    private string? parameterEditProperty;
    private string? fieldEditOriginal;
    private string? commandNameOriginal;
    private TransformDefinition? transformOriginal;
    private bool isDirty;
    private bool viewportReady;
    private bool closing;
    private bool isRebuilding;

    public MainWindow()
    {
        BuiltInCommandCatalog.RegisterAll(commandRegistry);
        buildCoordinator = new ScriptBuildCoordinator(commandRegistry, expressionEngine: expressionEngine);
        documentValidator = new DocumentValidator(commandRegistry);
        parameterService = new ParameterService(expressionEngine);

        InitializeComponent();
        PropertyGrid.ItemsSource = fieldRows;
        BuildMessageGrid.ItemsSource = buildMessages;
        history.Changed += (_, _) => UpdateHistoryState();
        Viewport.EngineInitialized += Viewport_EngineInitialized;
        Viewport.SelectionChanged += Viewport_SelectionChanged;
        Loaded += MainWindow_Loaded;

        RegisterInputBindings();
        ApplyLanguage(LanguageService.English);
        InstallDynamicMenus();
        PopulateCommandCatalog();
        CreateNewDocument();
    }

    private void RegisterInputBindings()
    {
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => CreateNewDocument()), new KeyGesture(Key.N, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => OpenDocument()), new KeyGesture(Key.O, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveDocument()), new KeyGesture(Key.S, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => Undo()), new KeyGesture(Key.Z, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => Redo()), new KeyGesture(Key.Y, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => RebuildModel()), new KeyGesture(Key.F5)));
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (Viewport.Engine.IsInitialized) InitializeViewport();
    }

    private void Viewport_EngineInitialized(object? sender, EventArgs e) => Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(InitializeViewport));

    private void InitializeViewport()
    {
        if (viewportReady || !Viewport.Engine.IsInitialized) return;
        viewportReady = true;
        var engine = Viewport.Engine;
        engine.SetGradientBackground(DrawingColor.FromArgb(248, 250, 252), DrawingColor.FromArgb(203, 213, 225), OcctGradientFillMethod.Vertical);
        engine.SetDisplayMode(OcctDisplayMode.Shaded);
        engine.SetTriedronVisible(true);
        engine.SetViewCubeVisible(true);
        engine.SetView(OcctViewOrientation.Isometric);
        RebuildModel();
    }

    private void CreateNewDocument()
    {
        document = CreateDefaultDocument();
        currentFilePath = null;
        isDirty = false;
        history.Clear();
        RefreshDocumentBindings(document.Commands.Last().Id);
        RebuildModel();
    }
}
