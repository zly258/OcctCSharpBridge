using OcctDemo.Common;
using OcctNet;
using Controls = System.Windows.Controls;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private void BuildMenus()
    {
        MainMenu.Items.Clear();
        foreach (var definition in DemoMenuCatalog.MainMenus)
        {
            var menu = Menu(MenuHeader(definition.TextKey));
            AddUnifiedMenuItems(menu.Items, definition.Items);
            MainMenu.Items.Add(menu);
        }

        UpdateHistoryUi();
    }

    private void AddUnifiedMenuItems(
        Controls.ItemCollection target,
        IEnumerable<DemoMenuItemDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            switch (definition.Kind)
            {
                case DemoMenuItemKind.Separator:
                    target.Add(new Controls.Separator());
                    break;

                case DemoMenuItemKind.Command:
                {
                    var commandId = definition.Command
                        ?? throw new InvalidOperationException("Command menu item has no command identifier.");
                    var command = DemoLocalization.Localize(DemoCommandCatalog.Get(commandId));
                    var item = MenuItem(command.Text, (_, _) => RunCommand(commandId), command.Shortcut);
                    item.ToolTip = command.Description;
                    target.Add(item);
                    break;
                }

                case DemoMenuItemKind.Action:
                {
                    var action = definition.Action
                        ?? throw new InvalidOperationException("Action menu item has no action identifier.");
                    var item = MenuItem(
                        DemoLocalization.Text(RequiredTextKey(definition)),
                        (_, _) => ExecuteMenuAction(action),
                        definition.Shortcut);
                    if (definition.CheckGroup != DemoMenuCheckGroup.None)
                    {
                        item.IsCheckable = true;
                        item.IsChecked = IsMenuActionChecked(action);
                    }
                    if (action == DemoMenuAction.Undo) _undoMenuItem = item;
                    if (action == DemoMenuAction.Redo) _redoMenuItem = item;
                    target.Add(item);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(definition.Kind), definition.Kind, null);
            }
        }
    }

    private void ExecuteMenuAction(DemoMenuAction action)
    {
        switch (action)
        {
            case DemoMenuAction.NewDocument: NewDocument(); break;
            case DemoMenuAction.OpenDocument: OpenDocument(); break;
            case DemoMenuAction.SaveDocument: SaveDocument(false); break;
            case DemoMenuAction.SaveDocumentAs: SaveDocument(true); break;
            case DemoMenuAction.ImportDocument: ImportDocument(); break;
            case DemoMenuAction.ExportSelected: ExportSelected(); break;
            case DemoMenuAction.Exit: Close(); break;
            case DemoMenuAction.Undo: Undo(); break;
            case DemoMenuAction.Redo: Redo(); break;
            case DemoMenuAction.ClearSelection:
                Session.Engine.ClearSelection();
                Viewport.RaiseSelectionChanged();
                break;
            case DemoMenuAction.ShowAll: Session.Engine.ShowAll(); break;
            case DemoMenuAction.HideAll: Session.Engine.HideAll(); break;
            case DemoMenuAction.ViewFront: Session.Engine.SetView(OcctViewOrientation.Front); break;
            case DemoMenuAction.ViewBack: Session.Engine.SetView(OcctViewOrientation.Back); break;
            case DemoMenuAction.ViewLeft: Session.Engine.SetView(OcctViewOrientation.Left); break;
            case DemoMenuAction.ViewRight: Session.Engine.SetView(OcctViewOrientation.Right); break;
            case DemoMenuAction.ViewTop: Session.Engine.SetView(OcctViewOrientation.Top); break;
            case DemoMenuAction.ViewBottom: Session.Engine.SetView(OcctViewOrientation.Bottom); break;
            case DemoMenuAction.ViewIsometric: Session.Engine.SetView(OcctViewOrientation.Isometric); break;
            case DemoMenuAction.ViewNorthEast: Session.SetIsoView(DemoIsoView.NorthEast); break;
            case DemoMenuAction.ViewNorthWest: Session.SetIsoView(DemoIsoView.NorthWest); break;
            case DemoMenuAction.ViewSouthEast: Session.SetIsoView(DemoIsoView.SouthEast); break;
            case DemoMenuAction.ViewSouthWest: Session.SetIsoView(DemoIsoView.SouthWest); break;
            case DemoMenuAction.ViewWireframe: ApplyMenuVisualStyle(DemoVisualStyle.Wireframe); break;
            case DemoMenuAction.ViewShaded: ApplyMenuVisualStyle(DemoVisualStyle.Shaded); break;
            case DemoMenuAction.ViewShadedEdges: ApplyMenuVisualStyle(DemoVisualStyle.ShadedEdges); break;
            case DemoMenuAction.ViewHiddenLine: ApplyMenuVisualStyle(DemoVisualStyle.HiddenLine); break;
            case DemoMenuAction.FitAll: Session.Engine.FitAll(); break;
            case DemoMenuAction.ViewSettings: ShowAdvancedViewSettingsWindow(); break;
            case DemoMenuAction.TestGeometryInspection: RunModelingTest(Session.RunGeometryInspectionTest); break;
            case DemoMenuAction.TestGeometryAlgorithms: RunModelingTest(Session.RunGeometryAlgorithmsTest); break;
            case DemoMenuAction.TestBSplineSurface: RunModelingTest(Session.RunBSplineSurfaceTest); break;
            case DemoMenuAction.TestMeshGeneration: RunModelingTest(Session.RunMeshGenerationTest); break;
            case DemoMenuAction.TestPipeShell: RunModelingTest(Session.RunPipeShellTest); break;
            case DemoMenuAction.TestTransformCopy: RunModelingTest(Session.RunTransformCopyTest); break;
            case DemoMenuAction.TestShapeValidity: RunModelingTest(Session.RunShapeValidityTest); break;
            case DemoMenuAction.LanguageEnglish: SetLanguage(DemoLanguage.English); break;
            case DemoMenuAction.LanguageChinese: SetLanguage(DemoLanguage.ChineseSimplified); break;
            case DemoMenuAction.MouseHelp: ShowMouseHelp(); break;
            case DemoMenuAction.About: ShowAbout(); break;
            default: throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    private void ApplyMenuVisualStyle(DemoVisualStyle style)
    {
        ApplyVisualStyle(style);
        BuildMenus();
    }

    private bool IsMenuActionChecked(DemoMenuAction action) => action switch
    {
        DemoMenuAction.ViewWireframe => _visualStyle == DemoVisualStyle.Wireframe,
        DemoMenuAction.ViewShaded => _visualStyle == DemoVisualStyle.Shaded,
        DemoMenuAction.ViewShadedEdges => _visualStyle == DemoVisualStyle.ShadedEdges,
        DemoMenuAction.ViewHiddenLine => _visualStyle == DemoVisualStyle.HiddenLine,
        DemoMenuAction.LanguageEnglish => DemoLocalization.CurrentLanguage == DemoLanguage.English,
        DemoMenuAction.LanguageChinese => DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified,
        _ => false
    };

    private static string RequiredTextKey(DemoMenuItemDefinition definition) =>
        definition.TextKey ?? throw new InvalidOperationException("Menu item has no localization key.");
}
