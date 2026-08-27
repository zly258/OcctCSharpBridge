using Avalonia.Controls;
using OcctDemo.Common;
using OcctNet;
using MenuItem = Avalonia.Controls.MenuItem;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private void BuildMenus()
    {
        _mainMenu.ItemsSource = DemoMenuCatalog.MainMenus.Select(BuildUnifiedMenu).ToArray();
        UpdateHistoryUi();
    }

    private MenuItem BuildUnifiedMenu(DemoMenuDefinition definition) =>
        Menu(
            MenuHeader(definition.TextKey),
            definition.Items.Select(BuildUnifiedMenuItem).ToArray());

    private object BuildUnifiedMenuItem(DemoMenuItemDefinition definition)
    {
        switch (definition.Kind)
        {
            case DemoMenuItemKind.Separator:
                return new Separator();

            case DemoMenuItemKind.Command:
            {
                var commandId = definition.Command
                    ?? throw new InvalidOperationException("Command menu item has no command identifier.");
                var command = DemoLocalization.Localize(DemoCommandCatalog.Get(commandId));
                var item = AsyncMenuItem(
                    command.Text,
                    () => RunCommandAsync(commandId),
                    ShortcutFromText(command.Shortcut));
                ToolTip.SetTip(item, command.Description);
                return item;
            }

            case DemoMenuItemKind.Action:
            {
                var action = definition.Action
                    ?? throw new InvalidOperationException("Action menu item has no action identifier.");
                MenuItem item;
                if (definition.CheckGroup != DemoMenuCheckGroup.None)
                {
                    item = CheckMenuItem(
                        DemoLocalization.Text(RequiredTextKey(definition)),
                        IsMenuActionChecked(action),
                        ignored => { _ = ExecuteMenuActionAsync(action); },
                        radio: true,
                        groupName: definition.CheckGroup.ToString());
                }
                else
                {
                    item = AsyncMenuItem(
                        DemoLocalization.Text(RequiredTextKey(definition)),
                        () => ExecuteMenuActionAsync(action),
                        ShortcutFromText(definition.Shortcut));
                }

                if (action == DemoMenuAction.Undo) _undoMenuItem = item;
                if (action == DemoMenuAction.Redo) _redoMenuItem = item;
                return item;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(definition.Kind), definition.Kind, null);
        }
    }

    private async Task ExecuteMenuActionAsync(DemoMenuAction action)
    {
        switch (action)
        {
            case DemoMenuAction.NewDocument: await NewDocumentAsync(); break;
            case DemoMenuAction.OpenDocument: await OpenDocumentAsync(); break;
            case DemoMenuAction.SaveDocument: await SaveDocumentAsync(false); break;
            case DemoMenuAction.SaveDocumentAs: await SaveDocumentAsync(true); break;
            case DemoMenuAction.ImportDocument: await ImportDocumentAsync(); break;
            case DemoMenuAction.ExportSelected: await ExportSelectedAsync(); break;
            case DemoMenuAction.Exit: Close(); break;
            case DemoMenuAction.Undo: Undo(); break;
            case DemoMenuAction.Redo: Redo(); break;
            case DemoMenuAction.ClearSelection:
                Session.Engine.ClearSelection();
                _viewport.RaiseSelectionChanged();
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
            case DemoMenuAction.TestBSplineSurface: RunModelingTest(Session.RunBSplineSurfaceTest); break;
            case DemoMenuAction.TestMeshGeneration: RunModelingTest(Session.RunMeshGenerationTest); break;
            case DemoMenuAction.TestPipeShell: RunModelingTest(Session.RunPipeShellTest); break;
            case DemoMenuAction.TestTransformCopy: RunModelingTest(Session.RunTransformCopyTest); break;
            case DemoMenuAction.TestShapeValidity: RunModelingTest(Session.RunShapeValidityTest); break;
            case DemoMenuAction.LanguageEnglish: SetLanguage(DemoLanguage.English); break;
            case DemoMenuAction.LanguageChinese: SetLanguage(DemoLanguage.ChineseSimplified); break;
            case DemoMenuAction.MouseHelp: await ShowMouseHelpAsync(); break;
            case DemoMenuAction.About: await ShowAboutAsync(); break;
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
