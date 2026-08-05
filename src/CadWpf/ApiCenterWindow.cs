using CadCommon;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CadWpf;

public sealed class ApiCenterWindow : Window
{
    private readonly CadSession _session;
    private readonly ListBox _scenarioList = new();
    private readonly DataGrid _apiGrid = new();
    private readonly TextBox _filterBox = new();
    private readonly TextBox _descriptionBox = new();
    private readonly TextBox _logBox = new();
    private readonly Button _runButton = new();
    private readonly Button _runAllButton = new();
    private readonly Button _cancelButton = new();
    private CancellationTokenSource? _cancellation;

    public ApiCenterWindow(CadSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Title = IsChinese ? "OCCT API 中心" : "OCCT API Center";
        Width = 1180;
        Height = 760;
        MinWidth = 920;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei UI");
        BuildUi();
        LoadScenarios();
        RefreshApiGrid();
    }

    private static bool IsChinese => CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified;

    private void BuildUi()
    {
        var root = new DockPanel { Margin = new Thickness(10) };
        Content = root;

        var summary = new TextBlock
        {
            Text = ApiDemoCatalog.CoverageSummary,
            Margin = new Thickness(4, 2, 4, 10),
            TextWrapping = TextWrapping.Wrap
        };
        DockPanel.SetDock(summary, Dock.Top);
        root.Children.Add(summary);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        DockPanel.SetDock(buttonPanel, Dock.Bottom);
        root.Children.Add(buttonPanel);

        _runButton.Content = IsChinese ? "运行所选场景" : "Run Selected";
        _runButton.Margin = new Thickness(4, 0, 4, 0);
        _runButton.Padding = new Thickness(14, 6, 14, 6);
        _runButton.Click += async (_, _) => await RunSelectedAsync();
        _runAllButton.Content = IsChinese ? "运行全部安全场景" : "Run All Safe Scenarios";
        _runAllButton.Margin = new Thickness(4, 0, 4, 0);
        _runAllButton.Padding = new Thickness(14, 6, 14, 6);
        _runAllButton.Click += async (_, _) => await RunAllAsync();
        _cancelButton.Content = IsChinese ? "取消" : "Cancel";
        _cancelButton.Margin = new Thickness(4, 0, 4, 0);
        _cancelButton.Padding = new Thickness(14, 6, 14, 6);
        _cancelButton.IsEnabled = false;
        _cancelButton.Click += (_, _) => _cancellation?.Cancel();
        var closeButton = new Button
        {
            Content = IsChinese ? "关闭" : "Close",
            Margin = new Thickness(4, 0, 0, 0),
            Padding = new Thickness(14, 6, 14, 6)
        };
        closeButton.Click += (_, _) => Close();
        buttonPanel.Children.Add(_runButton);
        buttonPanel.Children.Add(_runAllButton);
        buttonPanel.Children.Add(_cancelButton);
        buttonPanel.Children.Add(closeButton);

        var tabs = new TabControl();
        root.Children.Add(tabs);
        tabs.Items.Add(BuildScenarioTab());
        tabs.Items.Add(BuildApiTab());
        tabs.Items.Add(BuildLogTab());
    }

    private TabItem BuildScenarioTab()
    {
        var tab = new TabItem { Header = IsChinese ? "场景" : "Scenarios" };
        var grid = new Grid { Margin = new Thickness(6) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(310) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        tab.Content = grid;

        _scenarioList.DisplayMemberPath = nameof(ApiDemoScenario.Title);
        _scenarioList.SelectionChanged += (_, _) => ShowScenarioDescription();
        Grid.SetColumn(_scenarioList, 0);
        grid.Children.Add(_scenarioList);

        var splitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(splitter, 1);
        grid.Children.Add(splitter);

        _descriptionBox.IsReadOnly = true;
        _descriptionBox.TextWrapping = TextWrapping.Wrap;
        _descriptionBox.AcceptsReturn = true;
        _descriptionBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _descriptionBox.Padding = new Thickness(10);
        Grid.SetColumn(_descriptionBox, 2);
        grid.Children.Add(_descriptionBox);
        return tab;
    }

    private TabItem BuildApiTab()
    {
        var tab = new TabItem { Header = IsChinese ? "全部公共 API" : "All Public APIs" };
        var panel = new DockPanel { Margin = new Thickness(6) };
        tab.Content = panel;

        _filterBox.Margin = new Thickness(0, 0, 0, 6);
        _filterBox.ToolTip = IsChinese ? "筛选类型、方法、签名、执行条件" : "Filter type, method, signature or requirement";
        _filterBox.TextChanged += (_, _) => RefreshApiGrid();
        DockPanel.SetDock(_filterBox, Dock.Top);
        panel.Children.Add(_filterBox);

        _apiGrid.IsReadOnly = true;
        _apiGrid.CanUserAddRows = false;
        _apiGrid.CanUserDeleteRows = false;
        _apiGrid.AutoGenerateColumns = false;
        _apiGrid.SelectionMode = DataGridSelectionMode.Single;
        _apiGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
        _apiGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
        _apiGrid.Columns.Add(Column(IsChinese ? "模块" : "Area", nameof(ApiDemoMember.Area), 90));
        _apiGrid.Columns.Add(Column(IsChinese ? "类型" : "Type", nameof(ApiDemoMember.TypeName), 150));
        _apiGrid.Columns.Add(Column(IsChinese ? "成员" : "Kind", nameof(ApiDemoMember.Kind), 80));
        _apiGrid.Columns.Add(new DataGridTextColumn
        {
            Header = IsChinese ? "签名" : "Signature",
            Binding = new Binding(nameof(ApiDemoMember.Signature)),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            MinWidth = 280
        });
        _apiGrid.Columns.Add(Column(IsChinese ? "模式" : "Mode", nameof(ApiDemoMember.ExecutionMode), 110));
        _apiGrid.Columns.Add(Column(IsChinese ? "前置条件" : "Requirement", nameof(ApiDemoMember.Requirement), 260));
        panel.Children.Add(_apiGrid);
        return tab;
    }

    private TabItem BuildLogTab()
    {
        var tab = new TabItem { Header = IsChinese ? "执行日志" : "Execution Log" };
        _logBox.IsReadOnly = true;
        _logBox.AcceptsReturn = true;
        _logBox.AcceptsTab = true;
        _logBox.TextWrapping = TextWrapping.NoWrap;
        _logBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        _logBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _logBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        _logBox.Margin = new Thickness(6);
        tab.Content = _logBox;
        return tab;
    }

    private static DataGridTextColumn Column(string header, string property, double width) => new()
    {
        Header = header,
        Binding = new Binding(property),
        Width = width
    };

    private void LoadScenarios()
    {
        _scenarioList.ItemsSource = ApiDemoCatalog.Scenarios;
        if (ApiDemoCatalog.Scenarios.Count > 0) _scenarioList.SelectedIndex = 0;
    }

    private void ShowScenarioDescription()
    {
        if (_scenarioList.SelectedItem is not ApiDemoScenario scenario)
        {
            _descriptionBox.Clear();
            return;
        }
        _descriptionBox.Text = $"{scenario.Category} / {scenario.Title}{Environment.NewLine}{Environment.NewLine}{scenario.Description}{Environment.NewLine}{Environment.NewLine}"
                             + (scenario.RequiresUiThread
                                 ? (IsChinese ? "执行方式：当前 UI/Viewer 线程。" : "Execution: current UI/viewer thread.")
                                 : (IsChinese ? "执行方式：后台任务，不操作 Viewer。" : "Execution: background task without viewer access."));
    }

    private void RefreshApiGrid()
    {
        var filter = _filterBox.Text.Trim();
        IEnumerable<ApiDemoMember> members = ApiDemoCatalog.Members;
        if (filter.Length > 0)
        {
            members = members.Where(item =>
                item.Area.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || item.TypeName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || item.Signature.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || item.Requirement.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || item.ExecutionMode.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
        _apiGrid.ItemsSource = members.ToList();
    }

    private async Task RunSelectedAsync()
    {
        if (_scenarioList.SelectedItem is ApiDemoScenario scenario)
            await RunScenariosAsync(new[] { scenario });
    }

    private async Task RunAllAsync() => await RunScenariosAsync(ApiDemoCatalog.Scenarios);

    private async Task RunScenariosAsync(IEnumerable<ApiDemoScenario> scenarios)
    {
        if (_cancellation is not null) return;
        _cancellation = new CancellationTokenSource();
        SetRunning(true);
        var progress = new Progress<string>(AppendLog);
        try
        {
            foreach (var scenario in scenarios)
            {
                _cancellation.Token.ThrowIfCancellationRequested();
                AppendLog($"[{DateTime.Now:HH:mm:ss}] >>> {scenario.Category} / {scenario.Title}");
                ApiDemoResult result;
                if (scenario.RequiresUiThread)
                {
                    await Dispatcher.Yield();
                    result = scenario.Execute(_session, progress, _cancellation.Token);
                }
                else
                {
                    result = await Task.Run(() => scenario.Execute(_session, progress, _cancellation.Token), _cancellation.Token);
                }
                AppendLog($"[{DateTime.Now:HH:mm:ss}] OK  {result.Summary} ({result.Duration.TotalMilliseconds:N0} ms)");
                foreach (var detail in result.Details) AppendLog($"    {detail}");
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog(IsChinese ? "执行已取消。" : "Execution cancelled.");
        }
        catch (Exception ex)
        {
            AppendLog($"ERROR: {ex.Message}{Environment.NewLine}{ex}");
            MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _cancellation.Dispose();
            _cancellation = null;
            SetRunning(false);
        }
    }

    private void SetRunning(bool running)
    {
        _runButton.IsEnabled = !running;
        _runAllButton.IsEnabled = !running;
        _scenarioList.IsEnabled = !running;
        _cancelButton.IsEnabled = running;
        Cursor = running ? System.Windows.Input.Cursors.Wait : null;
    }

    private void AppendLog(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => AppendLog(message));
            return;
        }
        _logBox.AppendText(message + Environment.NewLine);
        _logBox.ScrollToEnd();
    }
}
