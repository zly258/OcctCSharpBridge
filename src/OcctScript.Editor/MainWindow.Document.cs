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
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "OcctScript project (*.ocsproj)|*.ocsproj|JSON document (*.json)|*.json|All files (*.*)|*.*"
        };
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
            System.Windows.MessageBox.Show(this, ex.Message, ResourceText("Ui.OpenFailed"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SaveDocument()
    {
        var path = currentFilePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
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
            CommitPendingEdits();
            await serializer.SaveAsync(path, document);
            currentFilePath = path;
            isDirty = false;
            UpdateTitle();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, ResourceText("Ui.SaveFailed"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MarkDirty()
    {
        isDirty = true;
        UpdateTitle();
    }

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

    private string ResourceText(string key) => System.Windows.Application.Current.TryFindResource(key)?.ToString() ?? key;

    private void ApplyLanguage(string culture)
    {
        LanguageService.Apply(culture);
        EnglishLanguageMenu.IsChecked = culture == LanguageService.English;
        ChineseLanguageMenu.IsChecked = culture == LanguageService.Chinese;
        if (CommandCatalogCombo is not null) PopulateCommandCatalog();
        if (CommandList is not null) RefreshFieldRows();
        UpdateTitle();
    }

    private void UpdateCatalogDescription()
    {
        CommandCatalogDescription.Text = (CommandCatalogCombo.SelectedItem as CommandCatalogItem)?.Description
            ?? ResourceText("Ui.NoDescription");
    }

    private void CommandCatalog_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateCatalogDescription();

    private void CommandList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedCommand = CommandList.SelectedItem as ScriptCommand;
        RefreshFieldRows();
        SelectDisplayedCommand(selectedCommand);
    }
}
