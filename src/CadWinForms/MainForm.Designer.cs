#nullable enable
using System.ComponentModel;
using OcctNet;

namespace CadWinForms;

partial class MainForm
{
    private IContainer? components;
    private TableLayoutPanel _rootLayout = null!;
    private MenuStrip _menu = null!;
    private ToolStrip _toolBar = null!;
    private StatusStrip _statusBar = null!;
    private ToolStripStatusLabel _commandStatus = null!;
    private ToolStripStatusLabel _selectionStatus = null!;
    private ToolStripStatusLabel _coordinateStatus = null!;
    private SplitContainer _mainSplitContainer = null!;
    private SplitContainer _centerRightSplitContainer = null!;
    private SplitContainer _rightSplitContainer = null!;
    private GroupBox _objectGroup = null!;
    private GroupBox _propertyGroup = null!;
    private GroupBox _logGroup = null!;
    private TreeView _objectTree = null!;
    private DataGridView _propertyGrid = null!;
    private DataGridViewTextBoxColumn _propertyNameColumn = null!;
    private DataGridViewTextBoxColumn _propertyValueColumn = null!;
    private TextBox _logBox = null!;
    private OcctViewportControl _viewport = null!;
    private ToolStripComboBox _selectionCombo = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();
        _rootLayout = new TableLayoutPanel();
        _menu = new MenuStrip();
        _toolBar = new ToolStrip();
        _statusBar = new StatusStrip();
        _commandStatus = new ToolStripStatusLabel();
        _selectionStatus = new ToolStripStatusLabel();
        _coordinateStatus = new ToolStripStatusLabel();
        _mainSplitContainer = new SplitContainer();
        _centerRightSplitContainer = new SplitContainer();
        _rightSplitContainer = new SplitContainer();
        _objectGroup = new GroupBox();
        _propertyGroup = new GroupBox();
        _logGroup = new GroupBox();
        _objectTree = new TreeView();
        _propertyGrid = new DataGridView();
        _propertyNameColumn = new DataGridViewTextBoxColumn();
        _propertyValueColumn = new DataGridViewTextBoxColumn();
        _logBox = new TextBox();
        _viewport = new OcctViewportControl();
        _selectionCombo = new ToolStripComboBox();
        _rootLayout.SuspendLayout();
        _statusBar.SuspendLayout();
        ((ISupportInitialize)_mainSplitContainer).BeginInit();
        _mainSplitContainer.Panel1.SuspendLayout();
        _mainSplitContainer.Panel2.SuspendLayout();
        _mainSplitContainer.SuspendLayout();
        ((ISupportInitialize)_centerRightSplitContainer).BeginInit();
        _centerRightSplitContainer.Panel1.SuspendLayout();
        _centerRightSplitContainer.Panel2.SuspendLayout();
        _centerRightSplitContainer.SuspendLayout();
        ((ISupportInitialize)_rightSplitContainer).BeginInit();
        _rightSplitContainer.Panel1.SuspendLayout();
        _rightSplitContainer.Panel2.SuspendLayout();
        _rightSplitContainer.SuspendLayout();
        _objectGroup.SuspendLayout();
        _propertyGroup.SuspendLayout();
        _logGroup.SuspendLayout();
        ((ISupportInitialize)_propertyGrid).BeginInit();
        SuspendLayout();
        //
        // _rootLayout
        //
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_menu, 0, 0);
        _rootLayout.Controls.Add(_toolBar, 0, 1);
        _rootLayout.Controls.Add(_mainSplitContainer, 0, 2);
        _rootLayout.Controls.Add(_statusBar, 0, 3);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _rootLayout.Location = new Point(0, 0);
        _rootLayout.Margin = new Padding(0);
        _rootLayout.Name = "_rootLayout";
        _rootLayout.RowCount = 4;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _rootLayout.Size = new Size(1440, 900);
        _rootLayout.TabIndex = 0;
        //
        // _menu
        //
        _menu.Dock = DockStyle.Fill;
        _menu.ImageScalingSize = new Size(20, 20);
        _menu.Location = new Point(0, 0);
        _menu.Margin = new Padding(0);
        _menu.Name = "_menu";
        _menu.Padding = new Padding(6, 2, 0, 2);
        _menu.Size = new Size(1440, 25);
        _menu.TabIndex = 0;
        //
        // _toolBar
        //
        _toolBar.AutoSize = false;
        _toolBar.Dock = DockStyle.Fill;
        _toolBar.GripStyle = ToolStripGripStyle.Hidden;
        _toolBar.ImageScalingSize = new Size(20, 20);
        _toolBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
        _toolBar.Location = new Point(0, 25);
        _toolBar.Margin = new Padding(0);
        _toolBar.Name = "_toolBar";
        _toolBar.Padding = new Padding(6, 2, 6, 2);
        _toolBar.RenderMode = ToolStripRenderMode.System;
        _toolBar.Size = new Size(1440, 34);
        _toolBar.TabIndex = 1;
        //
        // _statusBar
        //
        _statusBar.Dock = DockStyle.Fill;
        _statusBar.ImageScalingSize = new Size(20, 20);
        _statusBar.Items.AddRange(new ToolStripItem[]
        {
            _commandStatus,
            _selectionStatus,
            _coordinateStatus
        });
        _statusBar.Location = new Point(0, 876);
        _statusBar.Margin = new Padding(0);
        _statusBar.Name = "_statusBar";
        _statusBar.Padding = new Padding(1, 0, 16, 0);
        _statusBar.Size = new Size(1440, 24);
        _statusBar.SizingGrip = true;
        _statusBar.TabIndex = 3;
        //
        // _commandStatus
        //
        _commandStatus.Name = "_commandStatus";
        _commandStatus.Size = new Size(1128, 18);
        _commandStatus.Spring = true;
        _commandStatus.Text = "Initializing...";
        _commandStatus.TextAlign = ContentAlignment.MiddleLeft;
        //
        // _selectionStatus
        //
        _selectionStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _selectionStatus.Name = "_selectionStatus";
        _selectionStatus.Padding = new Padding(8, 0, 8, 0);
        _selectionStatus.Size = new Size(126, 18);
        _selectionStatus.Text = "No selection";
        //
        // _coordinateStatus
        //
        _coordinateStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _coordinateStatus.Name = "_coordinateStatus";
        _coordinateStatus.Padding = new Padding(8, 0, 0, 0);
        _coordinateStatus.Size = new Size(169, 18);
        _coordinateStatus.Text = "X 0.000  Y 0.000  Z 0.000";
        //
        // _mainSplitContainer
        //
        _mainSplitContainer.Dock = DockStyle.Fill;
        _mainSplitContainer.FixedPanel = FixedPanel.Panel1;
        _mainSplitContainer.Location = new Point(0, 59);
        _mainSplitContainer.Margin = new Padding(0);
        _mainSplitContainer.Name = "_mainSplitContainer";
        _mainSplitContainer.Panel1MinSize = 240;
        _mainSplitContainer.Panel2MinSize = 760;
        //
        // _mainSplitContainer.Panel1
        //
        _mainSplitContainer.Panel1.Controls.Add(_objectGroup);
        _mainSplitContainer.Panel1.Padding = new Padding(4, 4, 2, 4);
        //
        // _mainSplitContainer.Panel2
        //
        _mainSplitContainer.Panel2.Controls.Add(_centerRightSplitContainer);
        _mainSplitContainer.Panel2.Padding = new Padding(2, 4, 4, 4);
        _mainSplitContainer.Size = new Size(1440, 817);
        _mainSplitContainer.SplitterDistance = 270;
        _mainSplitContainer.SplitterWidth = 5;
        _mainSplitContainer.TabIndex = 2;
        //
        // _objectGroup
        //
        _objectGroup.Controls.Add(_objectTree);
        _objectGroup.Dock = DockStyle.Fill;
        _objectGroup.Location = new Point(4, 4);
        _objectGroup.Margin = new Padding(0);
        _objectGroup.MinimumSize = new Size(220, 0);
        _objectGroup.Name = "_objectGroup";
        _objectGroup.Padding = new Padding(8, 7, 8, 8);
        _objectGroup.Size = new Size(264, 809);
        _objectGroup.TabIndex = 0;
        _objectGroup.TabStop = false;
        _objectGroup.Text = "Model Explorer";
        //
        // _objectTree
        //
        _objectTree.CheckBoxes = true;
        _objectTree.Dock = DockStyle.Fill;
        _objectTree.FullRowSelect = true;
        _objectTree.HideSelection = false;
        _objectTree.HotTracking = true;
        _objectTree.Location = new Point(8, 23);
        _objectTree.Name = "_objectTree";
        _objectTree.PathSeparator = " / ";
        _objectTree.ShowNodeToolTips = true;
        _objectTree.Size = new Size(248, 778);
        _objectTree.TabIndex = 0;
        //
        // _centerRightSplitContainer
        //
        _centerRightSplitContainer.Dock = DockStyle.Fill;
        _centerRightSplitContainer.FixedPanel = FixedPanel.Panel2;
        _centerRightSplitContainer.Location = new Point(2, 4);
        _centerRightSplitContainer.Margin = new Padding(0);
        _centerRightSplitContainer.Name = "_centerRightSplitContainer";
        _centerRightSplitContainer.Panel1MinSize = 500;
        _centerRightSplitContainer.Panel2MinSize = 300;
        //
        // _centerRightSplitContainer.Panel1
        //
        _centerRightSplitContainer.Panel1.Controls.Add(_viewport);
        //
        // _centerRightSplitContainer.Panel2
        //
        _centerRightSplitContainer.Panel2.Controls.Add(_rightSplitContainer);
        _centerRightSplitContainer.Size = new Size(1159, 809);
        _centerRightSplitContainer.SplitterDistance = 824;
        _centerRightSplitContainer.SplitterWidth = 5;
        _centerRightSplitContainer.TabIndex = 0;
        //
        // _viewport
        //
        _viewport.BackColor = Color.FromArgb(232, 237, 242);
        _viewport.Dock = DockStyle.Fill;
        _viewport.EnableRectangleSelection = true;
        _viewport.Location = new Point(0, 0);
        _viewport.Margin = new Padding(0);
        _viewport.Name = "_viewport";
        _viewport.RectangleSelectionThreshold = 5;
        _viewport.Size = new Size(824, 809);
        _viewport.TabIndex = 0;
        //
        // _rightSplitContainer
        //
        _rightSplitContainer.Dock = DockStyle.Fill;
        _rightSplitContainer.FixedPanel = FixedPanel.None;
        _rightSplitContainer.Location = new Point(0, 0);
        _rightSplitContainer.Margin = new Padding(0);
        _rightSplitContainer.Name = "_rightSplitContainer";
        _rightSplitContainer.Orientation = Orientation.Horizontal;
        _rightSplitContainer.Panel1MinSize = 240;
        _rightSplitContainer.Panel2MinSize = 170;
        //
        // _rightSplitContainer.Panel1
        //
        _rightSplitContainer.Panel1.Controls.Add(_propertyGroup);
        _rightSplitContainer.Panel1.Padding = new Padding(0, 0, 0, 2);
        //
        // _rightSplitContainer.Panel2
        //
        _rightSplitContainer.Panel2.Controls.Add(_logGroup);
        _rightSplitContainer.Panel2.Padding = new Padding(0, 2, 0, 0);
        _rightSplitContainer.Size = new Size(330, 809);
        _rightSplitContainer.SplitterDistance = 500;
        _rightSplitContainer.SplitterWidth = 5;
        _rightSplitContainer.TabIndex = 0;
        //
        // _propertyGroup
        //
        _propertyGroup.Controls.Add(_propertyGrid);
        _propertyGroup.Dock = DockStyle.Fill;
        _propertyGroup.Location = new Point(0, 0);
        _propertyGroup.Margin = new Padding(0);
        _propertyGroup.MinimumSize = new Size(280, 220);
        _propertyGroup.Name = "_propertyGroup";
        _propertyGroup.Padding = new Padding(8, 7, 8, 8);
        _propertyGroup.Size = new Size(330, 498);
        _propertyGroup.TabIndex = 0;
        _propertyGroup.TabStop = false;
        _propertyGroup.Text = "Properties";
        //
        // _propertyGrid
        //
        _propertyGrid.AllowUserToAddRows = false;
        _propertyGrid.AllowUserToDeleteRows = false;
        _propertyGrid.AllowUserToResizeRows = false;
        _propertyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _propertyGrid.BackgroundColor = SystemColors.Window;
        _propertyGrid.BorderStyle = BorderStyle.Fixed3D;
        _propertyGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _propertyGrid.Columns.AddRange(new DataGridViewColumn[]
        {
            _propertyNameColumn,
            _propertyValueColumn
        });
        _propertyGrid.Dock = DockStyle.Fill;
        _propertyGrid.Location = new Point(8, 23);
        _propertyGrid.MultiSelect = false;
        _propertyGrid.Name = "_propertyGrid";
        _propertyGrid.ReadOnly = true;
        _propertyGrid.RowHeadersVisible = false;
        _propertyGrid.RowTemplate.Height = 25;
        _propertyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _propertyGrid.Size = new Size(314, 467);
        _propertyGrid.TabIndex = 0;
        //
        // _propertyNameColumn
        //
        _propertyNameColumn.FillWeight = 42F;
        _propertyNameColumn.HeaderText = "Property";
        _propertyNameColumn.MinimumWidth = 90;
        _propertyNameColumn.Name = "Property";
        _propertyNameColumn.ReadOnly = true;
        //
        // _propertyValueColumn
        //
        _propertyValueColumn.FillWeight = 58F;
        _propertyValueColumn.HeaderText = "Value";
        _propertyValueColumn.MinimumWidth = 120;
        _propertyValueColumn.Name = "Value";
        _propertyValueColumn.ReadOnly = true;
        //
        // _logGroup
        //
        _logGroup.Controls.Add(_logBox);
        _logGroup.Dock = DockStyle.Fill;
        _logGroup.Location = new Point(0, 2);
        _logGroup.Margin = new Padding(0);
        _logGroup.MinimumSize = new Size(280, 160);
        _logGroup.Name = "_logGroup";
        _logGroup.Padding = new Padding(8, 7, 8, 8);
        _logGroup.Size = new Size(330, 305);
        _logGroup.TabIndex = 0;
        _logGroup.TabStop = false;
        _logGroup.Text = "Command Line";
        //
        // _logBox
        //
        _logBox.AcceptsReturn = true;
        _logBox.AcceptsTab = true;
        _logBox.BackColor = Color.FromArgb(16, 24, 32);
        _logBox.Dock = DockStyle.Fill;
        _logBox.Font = new Font("Consolas", 9F);
        _logBox.ForeColor = Color.FromArgb(216, 226, 234);
        _logBox.Location = new Point(8, 23);
        _logBox.Multiline = true;
        _logBox.Name = "_logBox";
        _logBox.ReadOnly = true;
        _logBox.ScrollBars = ScrollBars.Both;
        _logBox.Size = new Size(314, 274);
        _logBox.TabIndex = 0;
        _logBox.WordWrap = false;
        //
        // _selectionCombo
        //
        _selectionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _selectionCombo.Name = "_selectionCombo";
        _selectionCombo.Size = new Size(118, 25);
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1440, 900);
        Controls.Add(_rootLayout);
        Font = new Font("Segoe UI", 9F);
        KeyPreview = true;
        MainMenuStrip = _menu;
        MinimumSize = new Size(1200, 760);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "OCCT CAD - WinForms";
        WindowState = FormWindowState.Maximized;
        _rootLayout.ResumeLayout(false);
        _rootLayout.PerformLayout();
        _statusBar.ResumeLayout(false);
        _statusBar.PerformLayout();
        _mainSplitContainer.Panel1.ResumeLayout(false);
        _mainSplitContainer.Panel2.ResumeLayout(false);
        ((ISupportInitialize)_mainSplitContainer).EndInit();
        _mainSplitContainer.ResumeLayout(false);
        _centerRightSplitContainer.Panel1.ResumeLayout(false);
        _centerRightSplitContainer.Panel2.ResumeLayout(false);
        ((ISupportInitialize)_centerRightSplitContainer).EndInit();
        _centerRightSplitContainer.ResumeLayout(false);
        _rightSplitContainer.Panel1.ResumeLayout(false);
        _rightSplitContainer.Panel2.ResumeLayout(false);
        ((ISupportInitialize)_rightSplitContainer).EndInit();
        _rightSplitContainer.ResumeLayout(false);
        _objectGroup.ResumeLayout(false);
        _propertyGroup.ResumeLayout(false);
        _logGroup.ResumeLayout(false);
        _logGroup.PerformLayout();
        ((ISupportInitialize)_propertyGrid).EndInit();
        ResumeLayout(false);
    }
}
