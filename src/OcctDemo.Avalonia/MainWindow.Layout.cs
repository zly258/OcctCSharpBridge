using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaBrushes = Avalonia.Media.Brushes;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using Control = Avalonia.Controls.Control;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private Control BuildLayout()
    {
        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*,5,160,Auto")
        };
        root.RowDefinitions[2].MinHeight = 420;
        root.RowDefinitions[4].MinHeight = 110;

        Grid.SetRow(_mainMenu, 0);
        root.Children.Add(_mainMenu);

        var toolbarBorder = new Border
        {
            Background = new SolidColorBrush(AvaloniaColor.Parse("#E7EAED")),
            BorderBrush = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3")),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = new ScrollViewer
            {
                HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                Content = _toolbar
            }
        };
        Grid.SetRow(toolbarBorder, 1);
        root.Children.Add(toolbarBorder);

        var workspace = new Grid
        {
            Margin = new Thickness(2),
            ColumnDefinitions = new ColumnDefinitions("260,5,*,5,330")
        };
        workspace.ColumnDefinitions[0].MinWidth = 220;
        workspace.ColumnDefinitions[2].MinWidth = 520;
        workspace.ColumnDefinitions[4].MinWidth = 280;

        _modelExplorerGroup.Margin = new Thickness(4);
        _modelExplorerGroup.Padding = new Thickness(4);
        _modelExplorerGroup.Content = _objectTree;
        Grid.SetColumn(_modelExplorerGroup, 0);
        workspace.Children.Add(_modelExplorerGroup);

        var leftSplitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
            Background = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3"))
        };
        Grid.SetColumn(leftSplitter, 1);
        workspace.Children.Add(leftSplitter);

        var viewportBorder = new Border
        {
            Margin = new Thickness(4),
            BorderBrush = new SolidColorBrush(AvaloniaColor.Parse("#AEB6BE")),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(AvaloniaColor.Parse("#E8EDF2")),
            Child = _viewport
        };
        Grid.SetColumn(viewportBorder, 2);
        workspace.Children.Add(viewportBorder);

        var rightSplitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
            Background = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3"))
        };
        Grid.SetColumn(rightSplitter, 3);
        workspace.Children.Add(rightSplitter);

        _propertiesGroup.Margin = new Thickness(4);
        _propertiesGroup.Padding = new Thickness(4);
        _propertiesGroup.Content = new ScrollViewer
        {
            Content = _propertyPanel,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        Grid.SetColumn(_propertiesGroup, 4);
        workspace.Children.Add(_propertiesGroup);

        Grid.SetRow(workspace, 2);
        root.Children.Add(workspace);

        var logSplitter = new GridSplitter
        {
            Height = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
            Background = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3"))
        };
        Grid.SetRow(logSplitter, 3);
        root.Children.Add(logSplitter);

        _commandLineGroup.Margin = new Thickness(4, 0, 4, 4);
        _commandLineGroup.Padding = new Thickness(4);
        _commandLineGroup.Content = _logBox;
        Grid.SetRow(_commandLineGroup, 4);
        root.Children.Add(_commandLineGroup);

        var statusGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            Background = new SolidColorBrush(AvaloniaColor.Parse("#2F3B46")),
            Margin = new Thickness(0),
            MinHeight = 26
        };
        _commandStatus.Foreground = AvaloniaBrushes.White;
        _commandStatus.Margin = new Thickness(8, 3);
        _selectionStatus.Foreground = AvaloniaBrushes.White;
        _selectionStatus.Margin = new Thickness(8, 3);
        _coordinateStatus.Foreground = AvaloniaBrushes.White;
        _coordinateStatus.Margin = new Thickness(8, 3);
        Grid.SetColumn(_commandStatus, 0);
        Grid.SetColumn(_selectionStatus, 1);
        Grid.SetColumn(_coordinateStatus, 2);
        statusGrid.Children.Add(_commandStatus);
        statusGrid.Children.Add(_selectionStatus);
        statusGrid.Children.Add(_coordinateStatus);
        Grid.SetRow(statusGrid, 5);
        root.Children.Add(statusGrid);

        return root;
    }
}
