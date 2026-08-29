namespace OcctDemo.Common;

public enum DemoMenuItemKind
{
    Action,
    Command,
    Separator
}

public enum DemoMenuCheckGroup
{
    None,
    VisualStyle,
    Language
}

public enum DemoMenuAction
{
    NewDocument,
    OpenDocument,
    SaveDocument,
    SaveDocumentAs,
    ImportDocument,
    ExportSelected,
    Exit,
    Undo,
    Redo,
    ClearSelection,
    ShowAll,
    HideAll,
    ViewFront,
    ViewBack,
    ViewLeft,
    ViewRight,
    ViewTop,
    ViewBottom,
    ViewIsometric,
    ViewNorthEast,
    ViewNorthWest,
    ViewSouthEast,
    ViewSouthWest,
    ViewWireframe,
    ViewShaded,
    ViewShadedEdges,
    ViewHiddenLine,
    FitAll,
    ViewSettings,
    LanguageEnglish,
    LanguageChinese,
    MouseHelp,
    About
}

public sealed record DemoMenuItemDefinition(
    DemoMenuItemKind Kind,
    string? TextKey = null,
    DemoMenuAction? Action = null,
    DemoCommandId? Command = null,
    string? Shortcut = null,
    DemoMenuCheckGroup CheckGroup = DemoMenuCheckGroup.None);

public sealed record DemoMenuDefinition(
    string TextKey,
    IReadOnlyList<DemoMenuItemDefinition> Items);

public static class DemoMenuCatalog
{
    private static DemoMenuItemDefinition Action(
        string textKey,
        DemoMenuAction action,
        string? shortcut = null,
        DemoMenuCheckGroup checkGroup = DemoMenuCheckGroup.None) =>
        new(DemoMenuItemKind.Action, textKey, action, Shortcut: shortcut, CheckGroup: checkGroup);

    private static DemoMenuItemDefinition Command(DemoCommandId command) =>
        new(DemoMenuItemKind.Command, Command: command);

    private static DemoMenuItemDefinition Separator() => new(DemoMenuItemKind.Separator);

    private static DemoMenuDefinition Menu(string textKey, params DemoMenuItemDefinition[] items) =>
        new(textKey, items);

    public static IReadOnlyList<DemoMenuDefinition> MainMenus { get; } =
    [
        Menu("Menu.File",
            Action("Menu.New", DemoMenuAction.NewDocument, "Ctrl+N"),
            Action("Menu.Open", DemoMenuAction.OpenDocument, "Ctrl+O"),
            Action("Menu.Save", DemoMenuAction.SaveDocument, "Ctrl+S"),
            Action("Menu.SaveAs", DemoMenuAction.SaveDocumentAs, "Ctrl+Shift+S"),
            Separator(),
            Action("Menu.Import", DemoMenuAction.ImportDocument),
            Action("Menu.ExportSelected", DemoMenuAction.ExportSelected),
            Separator(),
            Action("Menu.Exit", DemoMenuAction.Exit, "Alt+F4")),

        Menu("Menu.Edit",
            Action("Menu.Undo", DemoMenuAction.Undo, "Ctrl+Z"),
            Action("Menu.Redo", DemoMenuAction.Redo, "Ctrl+Y"),
            Separator(),
            Command(DemoCommandId.Translate),
            Command(DemoCommandId.Rotate),
            Command(DemoCommandId.Scale),
            Command(DemoCommandId.Mirror),
            Command(DemoCommandId.Copy),
            Command(DemoCommandId.Delete),
            Separator(),
            Action("Menu.ClearSelection", DemoMenuAction.ClearSelection),
            Action("Menu.ShowAll", DemoMenuAction.ShowAll),
            Action("Menu.HideAll", DemoMenuAction.HideAll)),

        Menu("Menu.Draw",
            Command(DemoCommandId.Point),
            Command(DemoCommandId.Line),
            Command(DemoCommandId.Polyline),
            Command(DemoCommandId.Circle),
            Command(DemoCommandId.ArcThreePoints),
            Command(DemoCommandId.ArcCenter),
            Command(DemoCommandId.Ellipse),
            Command(DemoCommandId.Rectangle),
            Command(DemoCommandId.Polygon),
            Command(DemoCommandId.Bezier),
            Command(DemoCommandId.BSpline)),

        Menu("Menu.Solid",
            Command(DemoCommandId.Box),
            Command(DemoCommandId.Cylinder),
            Command(DemoCommandId.Frustum),
            Command(DemoCommandId.Cone),
            Command(DemoCommandId.Torus),
            Command(DemoCommandId.Sphere),
            Command(DemoCommandId.Wedge),
            Command(DemoCommandId.Pipe)),

        Menu("Menu.Features",
            Command(DemoCommandId.Extrude),
            Command(DemoCommandId.Revolve),
            Command(DemoCommandId.Sweep),
            Command(DemoCommandId.Loft),
            Separator(),
            Command(DemoCommandId.Fillet),
            Command(DemoCommandId.Chamfer),
            Command(DemoCommandId.Offset),
            Command(DemoCommandId.Shell),
            Command(DemoCommandId.Drill),
            Separator(),
            Command(DemoCommandId.Fuse),
            Command(DemoCommandId.Cut),
            Command(DemoCommandId.Common),
            Command(DemoCommandId.Section)),

        Menu("Menu.Annotate",
            Command(DemoCommandId.Text),
            Command(DemoCommandId.LengthDimension),
            Command(DemoCommandId.AngleDimension),
            Command(DemoCommandId.RadiusDimension),
            Command(DemoCommandId.DiameterDimension)),

        Menu("Menu.View",
            Action("Menu.Front", DemoMenuAction.ViewFront, "1"),
            Action("Menu.Back", DemoMenuAction.ViewBack),
            Action("Menu.Left", DemoMenuAction.ViewLeft, "2"),
            Action("Menu.Right", DemoMenuAction.ViewRight),
            Action("Menu.Top", DemoMenuAction.ViewTop, "3"),
            Action("Menu.Bottom", DemoMenuAction.ViewBottom),
            Separator(),
            Action("Menu.Isometric", DemoMenuAction.ViewIsometric, "0"),
            Action("Menu.NorthEast", DemoMenuAction.ViewNorthEast),
            Action("Menu.NorthWest", DemoMenuAction.ViewNorthWest),
            Action("Menu.SouthEast", DemoMenuAction.ViewSouthEast),
            Action("Menu.SouthWest", DemoMenuAction.ViewSouthWest),
            Separator(),
            Action("Menu.FitAll", DemoMenuAction.FitAll),
            Action("Menu.ViewSettings", DemoMenuAction.ViewSettings)),

        Menu("Menu.Display",
            Action("Menu.Wireframe", DemoMenuAction.ViewWireframe, checkGroup: DemoMenuCheckGroup.VisualStyle),
            Action("Menu.Shaded", DemoMenuAction.ViewShaded, checkGroup: DemoMenuCheckGroup.VisualStyle),
            Action("Menu.ShadedEdges", DemoMenuAction.ViewShadedEdges, checkGroup: DemoMenuCheckGroup.VisualStyle),
            Action("Menu.Hlr", DemoMenuAction.ViewHiddenLine, checkGroup: DemoMenuCheckGroup.VisualStyle)),

        Menu("Menu.Samples",
            Command(DemoCommandId.DemoSectionAnalysis),
            Command(DemoCommandId.DemoDrawingProjection),
            Command(DemoCommandId.DemoDistanceExtrema),
            Command(DemoCommandId.DemoModelRepair),
            Separator(),
            Command(DemoCommandId.DemoPrimitives),
            Command(DemoCommandId.DemoPipe),
            Command(DemoCommandId.DemoTee),
            Command(DemoCommandId.DemoReducer),
            Command(DemoCommandId.DemoLoft),
            Command(DemoCommandId.DemoBoolean),
            Separator(),
            Command(DemoCommandId.DemoBracket),
            Command(DemoCommandId.DemoFlange),
            Command(DemoCommandId.DemoAnnotations),
            Separator(),
            Command(DemoCommandId.DemoLinearCopies),
            Command(DemoCommandId.DemoRadialCopies),
            Command(DemoCommandId.DemoMirrorCopies),
            Separator(),
            Command(DemoCommandId.DemoElements),
            Command(DemoCommandId.DemoGear),
            Command(DemoCommandId.DemoManifold),
            Command(DemoCommandId.DemoTwistedDuct)),

        Menu("Menu.Tools",
            Action("Menu.English", DemoMenuAction.LanguageEnglish, checkGroup: DemoMenuCheckGroup.Language),
            Action("Menu.Chinese", DemoMenuAction.LanguageChinese, checkGroup: DemoMenuCheckGroup.Language),
            Separator(),
            Action("Menu.MouseHelp", DemoMenuAction.MouseHelp),
            Action("Menu.About", DemoMenuAction.About))
    ];

}