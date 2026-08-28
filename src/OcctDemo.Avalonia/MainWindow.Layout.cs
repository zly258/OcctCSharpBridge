using Avalonia;
using Avalonia.Controls;
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

        var toolbarScroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = _toolbar
        };
        Grid.SetRow(toolbarScroll, 1);
        root.Children.Add(toolbarScroll);

        var workspace = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("260,5,*,5,330")
        };
        workspace.ColumnDefinitions[0].MinWidth = 220;
        workspace.ColumnDefinitions[2].MinWidth = 520;
        workspace.ColumnDefinitions[4].MinWidth = 280;

        _modelExplorerGroup.Content = _objectTree;
        Grid.SetColumn(_modelExplorerGroup, 0);
        workspace.Children.Add(_modelExplorerGroup);

        var leftSplitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch
        };
        Grid.SetColumn(leftSplitter, 1);
        workspace.Children.Add(leftSplitter);

        Grid.SetColumn(_viewport, 2);
        workspace.Children.Add(_viewport);

        var rightSplitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch
        };
        Grid.SetColumn(rightSplitter, 3);
        workspace.Children.Add(rightSplitter);

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
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch
        };
        Grid.SetRow(logSplitter, 3);
        root.Children.Add(logSplitter);

        _commandLineGroup.Content = _logBox;
        Grid.SetRow(_commandLineGroup, 4);
        root.Children.Add(_commandLineGroup);

        var statusGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*")
        };
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
