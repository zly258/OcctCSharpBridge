using CadCommon;

namespace CadWinForms;

public sealed class ApiCenterForm : Form
{
    private readonly CadSession _session;
    private readonly ListBox _scenarioList = new();
    private readonly DataGridView _apiGrid = new();
    private readonly TextBox _filterBox = new();
    private readonly TextBox _logBox = new();
    private readonly Label _summaryLabel = new();
    private readonly Button _runButton = new();
    private readonly Button _runAllButton = new();
    private readonly Button _cancelButton = new();
    private CancellationTokenSource? _cancellation;

    public ApiCenterForm(CadSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Text = IsChinese ? "OCCT API 中心" : "OCCT API Center";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(920, 620);
        Size = new Size(1180, 760);
        Font = new Font("Microsoft YaHei UI", 9F);
        BuildUi();
        LoadScenarios();
        RefreshApiGrid();
    }

    private static bool IsChinese => CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified;

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        _summaryLabel.AutoSize = true;
        _summaryLabel.Padding = new Padding(3, 3, 3, 8);
        _summaryLabel.Text = ApiDemoCatalog.CoverageSummary;
        root.Controls.Add(_summaryLabel, 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        root.Controls.Add(tabs, 0, 1);

        var scenarioPage = new TabPage(IsChinese ? "场景" : "Scenarios");
        var apiPage = new TabPage(IsChinese ? "全部公共 API" : "All Public APIs");
        var logPage = new TabPage(IsChinese ? "执行日志" : "Execution Log");
        tabs.TabPages.AddRange(new[] { scenarioPage, apiPage, logPage });

        var scenarioSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 310,
            Panel1MinSize = 250,
            Panel2MinSize = 400
        };
        scenarioPage.Controls.Add(scenarioSplit);

        _scenarioList.Dock = DockStyle.Fill;
        _scenarioList.DisplayMember = nameof(ApiDemoScenario.Title);
        _scenarioList.IntegralHeight = false;
        _scenarioList.SelectedIndexChanged += (_, _) => ShowScenarioDescription();
        scenarioSplit.Panel1.Controls.Add(_scenarioList);

        var description = new TextBox
        {
            Name = "ScenarioDescription",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle
        };
        scenarioSplit.Panel2.Controls.Add(description);

        var apiRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        apiRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        apiRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        apiPage.Controls.Add(apiRoot);

        _filterBox.Dock = DockStyle.Top;
        _filterBox.PlaceholderText = IsChinese ? "筛选类型、方法、签名、执行条件……" : "Filter type, method, signature or requirement...";
        _filterBox.Margin = new Padding(6);
        _filterBox.TextChanged += (_, _) => RefreshApiGrid();
        apiRoot.Controls.Add(_filterBox, 0, 0);

        _apiGrid.Dock = DockStyle.Fill;
        _apiGrid.ReadOnly = true;
        _apiGrid.AllowUserToAddRows = false;
        _apiGrid.AllowUserToDeleteRows = false;
        _apiGrid.AutoGenerateColumns = false;
        _apiGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _apiGrid.MultiSelect = false;
        _apiGrid.RowHeadersVisible = false;
        _apiGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        _apiGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ApiDemoMember.Area), HeaderText = IsChinese ? "模块" : "Area", Width = 90 });
        _apiGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ApiDemoMember.TypeName), HeaderText = IsChinese ? "类型" : "Type", Width = 150 });
        _apiGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ApiDemoMember.Kind), HeaderText = IsChinese ? "成员" : "Kind", Width = 85 });
        _apiGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ApiDemoMember.Signature), HeaderText = IsChinese ? "签名" : "Signature", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 260 });
        _apiGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ApiDemoMember.ExecutionMode), HeaderText = IsChinese ? "模式" : "Mode", Width = 110 });
        _apiGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ApiDemoMember.Requirement), HeaderText = IsChinese ? "前置条件" : "Requirement", Width = 240 });
        apiRoot.Controls.Add(_apiGrid, 0, 1);

        _logBox.Dock = DockStyle.Fill;
        _logBox.Multiline = true;
        _logBox.ReadOnly = true;
        _logBox.ScrollBars = ScrollBars.Both;
        _logBox.WordWrap = false;
        _logBox.Font = new Font("Consolas", 9F);
        logPage.Controls.Add(_logBox);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        root.Controls.Add(buttons, 0, 2);

        var closeButton = new Button { Text = IsChinese ? "关闭" : "Close", AutoSize = true };
        closeButton.Click += (_, _) => Close();
        _cancelButton.Text = IsChinese ? "取消" : "Cancel";
        _cancelButton.AutoSize = true;
        _cancelButton.Enabled = false;
        _cancelButton.Click += (_, _) => _cancellation?.Cancel();
        _runAllButton.Text = IsChinese ? "运行全部安全场景" : "Run All Safe Scenarios";
        _runAllButton.AutoSize = true;
        _runAllButton.Click += async (_, _) => await RunAllAsync();
        _runButton.Text = IsChinese ? "运行所选场景" : "Run Selected";
        _runButton.AutoSize = true;
        _runButton.Click += async (_, _) => await RunSelectedAsync();
        buttons.Controls.Add(closeButton);
        buttons.Controls.Add(_cancelButton);
        buttons.Controls.Add(_runAllButton);
        buttons.Controls.Add(_runButton);
    }

    private void LoadScenarios()
    {
        _scenarioList.DataSource = ApiDemoCatalog.Scenarios.ToList();
        if (_scenarioList.Items.Count > 0) _scenarioList.SelectedIndex = 0;
    }

    private void ShowScenarioDescription()
    {
        var description = Controls.Find("ScenarioDescription", true).OfType<TextBox>().FirstOrDefault();
        if (description is null) return;
        if (_scenarioList.SelectedItem is not ApiDemoScenario scenario)
        {
            description.Clear();
            return;
        }
        description.Text = $"{scenario.Category} / {scenario.Title}{Environment.NewLine}{Environment.NewLine}{scenario.Description}{Environment.NewLine}{Environment.NewLine}"
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
        _apiGrid.DataSource = members.ToList();
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
                    await Task.Yield();
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
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        _runButton.Enabled = !running;
        _runAllButton.Enabled = !running;
        _scenarioList.Enabled = !running;
        _cancelButton.Enabled = running;
        UseWaitCursor = running;
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(message));
            return;
        }
        _logBox.AppendText(message + Environment.NewLine);
    }
}
