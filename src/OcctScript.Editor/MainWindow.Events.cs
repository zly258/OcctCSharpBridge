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
    private void New_Click(object sender, RoutedEventArgs e) => CreateNewDocument();
    private void Open_Click(object sender, RoutedEventArgs e) => OpenDocument();
    private void Save_Click(object sender, RoutedEventArgs e) => SaveDocument();
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    private void Undo_Click(object sender, RoutedEventArgs e) => Undo();
    private void Redo_Click(object sender, RoutedEventArgs e) => Redo();
    private void AddSelectedCommand_Click(object sender, RoutedEventArgs e)
    {
        if (CommandCatalogCombo.SelectedItem is CommandCatalogItem item) AddCommand(item.Type);
    }
    private void AddCommandMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: string type }) AddCommand(type);
    }
    private void AddParameter_Click(object sender, RoutedEventArgs e) => AddParameter();
    private void DeleteCommand_Click(object sender, RoutedEventArgs e) => DeleteSelectedCommand();
    private void DeleteParameter_Click(object sender, RoutedEventArgs e) => DeleteSelectedParameter();
    private void Rebuild_Click(object sender, RoutedEventArgs e) => RebuildModel();
    private void Fit_Click(object sender, RoutedEventArgs e) { if (viewportReady && displayedShapes.Count > 0) Viewport.Engine.FitAll(); }
    private void Isometric_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Isometric); }
    private void Front_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Front); }
    private void Top_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Top); }
    private void Left_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Left); }
    private void Right_Click(object sender, RoutedEventArgs e) { if (viewportReady) Viewport.Engine.SetView(OcctViewOrientation.Right); }
    private void EnglishLanguage_Click(object sender, RoutedEventArgs e) => ApplyLanguage(LanguageService.English);
    private void ChineseLanguage_Click(object sender, RoutedEventArgs e) => ApplyLanguage(LanguageService.Chinese);

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        closing = true;
        buildCoordinator.Dispose();
    }
}
