using CadCommon;
using Data = System.Windows.Data;
using Wpf = System.Windows;
using Controls = System.Windows.Controls;

namespace CadWpf;

public sealed class ApiCenterWindow : Wpf.Window
{
    private readonly CadSession _session;
    private readonly Controls.ListBox _scenarioList = new();
    private readonly Controls.DataGrid _apiGrid = new();
    private readonly Controls.TextBox _filterBox = new();
    private readonly Controls.TextBox _descriptionBox = new();
    private readonly Controls.TextBox _logBox = new();
    private readonly Controls.Button _runButton = new();
    private readonly Controls.Button _runAllButton = new();
    private readonly Controls.Button _cancelButton = new();
    private CancellationTokenSource? _cancellation;

    public ApiCenterWindow(CadSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Title = IsChinese ? "OCCT API 中心" : "OCCT API Center";
        Width = 1180;
        Height = 760;
        MinWidth = 920;
        MinHeight = 620;
        WindowStartupLocation = Wpf.WindowStartupLocation.CenterOwner;
        FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei UI");
        BuildUi();
        LoadScenarios();
        RefreshApiGrid();
    }

    private static bool IsChinese => CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified;

    private void BuildUi()
    {
        var root = new Controls.DockPanel { Margin = new Wpf.Thickness(10) };
        Content = root;

        var summary = new Controls.TextBlock
        {
            Text = ApiDemoCatalog.CoverageSummary,
            Margin = new Wpf.Thickness(4, 2, 4, 10),
            TextWrapping = Wpf.TextWrapping.Wrap
        };
        Controls.DockPanel.SetDock(summary, Controls.Dock.Top);
        root.Children.Add(summary);

        var buttonPanel = new Controls.StackPanel
        {
            Orientation = Controls.Orientation.Horizontal,
            HorizontalAlignment = Wpf.HorizontalAlignment.Right,
            Margin = new Wpf.Thickness(0, 10, 0, 0)
        };
        Controls.DockPanel.SetDock(buttonPanel, Controls.Dock.Bottom);
        root.Children.Add(buttonPanel);

        ConfigureButton(_runButton, IsChinese ? "运行所选场景" : "Run Selected");
        _runButton.Click += async (_, _) => await RunSelectedAsync();
        ConfigureButton(_runAllButton, IsChinese ? "运行全部安全场景" : "Run All Safe Scenarios");
        _runAllButton.Click += async (_, _) => await RunAllAsync();
        ConfigureButton(_cancelButton, IsChinese ? "取消" : "Cancel");
        _cancelButton.IsEnabled = false;
        _cancelButton.Click += (_, _) => _cancellation?.Cancel();
        var closeButton = new Controls.Button
        {
            Content = IsChinese ? "关闭" : "Close",
            Margin = new Wpf.Thickness(4, 0, 0, 0),
            Padding = new Wpf.Thickness(14, 6, 14, 6)
        };
        closeButton.Click += (_, _) => Close();
        buttonPanel.Children.Add(_runButton);
        buttonPanel.Children.Add(_runAllButton);
        buttonPanel.Children.Add(_cancelButton);
        buttonPanel.Children.Add(closeButton);

        var tabs = new Controls.TabControl();
        root.Children.Add(tabs);
        tabs.Items.Add(BuildScenarioTab());
        tabs.Items.Add(BuildApiTab());
        tabs.Items.Add(BuildLogTab());
    }

    private static void ConfigureButton(Controls.Button button, string text)
    {
        button.Content = text;
        button.Margin = new Wpf.Thickness(4, 0, 4, 0);
        button.Padding = new Wpf.Thickness(14, 6, 14, 6);
    }

    private Controls.TabItem BuildScenarioTab()
    {
        var tab = new Controls.TabItem { Header = IsChinese ? "场景" : "Scenarios" };
        var grid = new Controls.Grid { Margin = new Wpf.Thickness(6) };
        grid.ColumnDefinitions.Add(new Controls.ColumnDefinition { Width = new Wpf.GridLength(310) });
        grid.ColumnDefinitions.Add(new Controls.ColumnDefinition { Width = new Wpf.GridLength(8) });
        grid.ColumnDefinitions.Add(new Controls.ColumnDefinition { Width = new Wpf.GridLength(1, Wpf.GridUnitType.Star) });
        tab.Content = grid;

        _scenarioList.DisplayMemberPath = nameof(ApiDemoScenario.Title);
        _scenarioList.SelectionChanged += (_, _) => ShowScenarioDescription();
        Controls.Grid.SetColumn(_scenarioList, 0);
        grid.Children.Add(_scenarioList);

        var splitter = new Controls.GridSplitter
        {
            Width = 5,
            HorizontalAlignment = Wpf.HorizontalAlignment.Center,
            VerticalAlignment = Wpf.VerticalAlignment.Stretch
        };
        Controls.Grid.SetColumn(splitter, 1);
        grid.Children.Add(splitter);

        _descriptionBox.IsReadOnly = true;
        _descriptionBox.TextWrapping = Wpf.TextWrapping.Wrap;
        _descriptionBox.AcceptsReturn = true;
        _descriptionBox.VerticalScrollBarVisibility = Controls.ScrollBarVisibility.Auto;
        _descriptionBox.Padding = new Wpf.Thickness(10);
        Controls.Grid.SetColumn(_descriptionBox, 2);
        grid.Children.Add(_descriptionBox);
        return tab;
    }

    private Controls.TabItem BuildApiTab()
    {
        var tab = new Controls.TabItem { Header = IsChinese ? "全部公共 API" : "All Public APIs" };
        var panel = new Controls.DockPanel { Margin = new Wpf.Thickness(6) };
        tab.Content = panel;

        _filterBox.Margin = new Wpf.Thickness(0, 0, 0, 6);
        _filterBox.ToolTip = IsChinese ? "筛选类型、方法、签名、执行条件" : "Filter type, method, signature or requirement";
        _filterBox.TextChanged += (_, _) => RefreshApiGrid();
        Controls.DockPanel.SetDock(_filterBox, Controls.Dock.Top);
        panel.Children.Add(_filterBox);

        _apiGrid.IsReadOnly = true;
        _apiGrid.CanUserAddRows = false;
        _apiGrid.CanUserDeleteRows = false;
        _apiGrid.AutoGenerateColumns = false;
        _apiGrid.SelectionMode = Controls.DataGridSelectionMode.Single;
        _apiGrid.SelectionUnit = Controls.DataGridSelectionUnit.FullRow;
        _apiGrid.HeadersVisibility = Controls.DataGridHeadersVisibility.Column;
        _apiGrid.Columns.Add(Column(IsChinese ? "模块" : "Area", nameof(ApiDemoMember.Area), 90));
        _apiGrid.Columns.Add(Column(IsChinese ? "类型" : "Type", nameof(ApiDemoMember.TypeName), 150));
        _apiGrid.Columns.Add(Column(IsChinese ? "成员" : "Kind", nameof(ApiDemoMember.Kind), 80));
        _apiGrid.Columns.Add(new Controls.DataGridTextColumn
        {
            Header = IsChinese ? "签名" : "Signature",
            Binding = new Data.Binding(nameof(ApiDemoMember.Signature)),
            Width = new Controls.DataGridLength(1, Controls.DataGridLengthUnitType.Star),
            MinWidth = 280
        });
        _apiGrid.Columns.Add(Column(IsChinese ? "模式" : "Mode", nameof(ApiDemoMember.ExecutionMode), 110));
        _apiGrid.Columns.Add(Column(IsChinese ? "前置条件" : "Requirement", nameof(ApiDemoMember.Requirement), 260));
        panel.Children.Add(_apiGrid);
        return tab;
    }

    private Controls.TabItem BuildLogTab()
    {
        var tab = new Controls.TabItem { Header = IsChinese ? "执行日志" : "Execution Log" };
        _logBox.IsReadOnly = true;
        _logBox.AcceptsReturn = true;
        _logBox.AcceptsTab = true;
        _logBox.TextWrapping = Wpf.TextWrapping.NoWrap;
        _logBox.HorizontalScrollBarVisibility = Controls.ScrollBarVisibility.Auto;
        _logBox.VerticalScrollBarVisibility = Controls.ScrollBarVisibility.Auto;
        _logBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        _logBox.Margin = new Wpf.Thickness(6);
        tab.Content = _logBox;
        return tab;
    }

    private static Controls.DataGridTextColumn Column(string header, string property, double width) => new()
    {
        Header = header,
        Binding = new Data.Binding(property),
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
                    await System.Windows.Threading.Dispatcher.Yield();
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
            Wpf.MessageBox.Show(this, ex.Message, Title, Wpf.MessageBoxButton.OK, Wpf.MessageBoxImage.Error);
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
            Dispatcher.BeginInvoke(new Action(() => AppendLog(message)));
            return;
        }
        _logBox.AppendText(message + Environment.NewLine);
        _logBox.ScrollToEnd();
    }
}
