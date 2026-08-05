from pathlib import Path

path = Path("src/CadWinForms/MainForm.cs")
text = path.read_text(encoding="utf-8-sig")

old_fields = "    private bool _autoZFitEnabled = true;\n"
new_fields = (
    "    private bool _autoZFitEnabled = true;\n"
    "    private bool _initialPanelLayoutApplied;\n"
    "    private bool _initialPanelLayoutScheduled;\n"
)

old_layout = '''    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ApplyInitialPanelLayout();
    }

    private void ApplyInitialPanelLayout()
    {
        SetSplitterDistance(_mainSplitContainer, 270, keepSecondPanel: false);
        SetSplitterDistance(_centerRightSplitContainer, 330, keepSecondPanel: true);

        var preferredPropertyHeight = Math.Max(260, (int)(_rightSplitContainer.ClientSize.Height * 0.62));
        SetSplitterDistance(_rightSplitContainer, preferredPropertyHeight, keepSecondPanel: false);
    }

    private static void SetSplitterDistance(
        SplitContainer container,
        int preferredSize,
        bool keepSecondPanel)
    {
        var available = container.Orientation == Orientation.Vertical
            ? container.ClientSize.Width
            : container.ClientSize.Height;

        if (available <= container.SplitterWidth)
        {
            return;
        }

        var minimum = container.Panel1MinSize;
        var maximum = Math.Max(
            minimum,
            available - container.Panel2MinSize - container.SplitterWidth);

        var distance = keepSecondPanel
            ? available - preferredSize - container.SplitterWidth
            : preferredSize;

        container.SplitterDistance = Math.Clamp(distance, minimum, maximum);
    }
'''

new_layout = '''    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ScheduleInitialPanelLayout();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState != FormWindowState.Minimized)
        {
            ScheduleInitialPanelLayout();
        }
    }

    private void ScheduleInitialPanelLayout()
    {
        if (_initialPanelLayoutApplied
            || _initialPanelLayoutScheduled
            || IsDisposed
            || Disposing
            || !IsHandleCreated)
        {
            return;
        }

        _initialPanelLayoutScheduled = true;
        BeginInvoke((Action)(() =>
        {
            _initialPanelLayoutScheduled = false;
            if (IsDisposed
                || Disposing
                || !IsHandleCreated
                || WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _initialPanelLayoutApplied = ApplyInitialPanelLayout();
        }));
    }

    private bool ApplyInitialPanelLayout()
    {
        var mainApplied = TrySetSplitterDistance(
            _mainSplitContainer,
            270,
            keepSecondPanel: false);
        var centerRightApplied = TrySetSplitterDistance(
            _centerRightSplitContainer,
            330,
            keepSecondPanel: true);

        var preferredPropertyHeight = Math.Max(
            260,
            (int)(_rightSplitContainer.ClientSize.Height * 0.62));
        var rightApplied = TrySetSplitterDistance(
            _rightSplitContainer,
            preferredPropertyHeight,
            keepSecondPanel: false);

        return mainApplied && centerRightApplied && rightApplied;
    }

    private static bool TrySetSplitterDistance(
        SplitContainer container,
        int preferredSize,
        bool keepSecondPanel)
    {
        if (container.IsDisposed || !container.IsHandleCreated)
        {
            return false;
        }

        var available = container.Orientation == Orientation.Vertical
            ? container.ClientSize.Width
            : container.ClientSize.Height;
        var minimum = container.Panel1MinSize;
        var maximum = available - container.Panel2MinSize - container.SplitterWidth;

        // During startup, DPI scaling and maximization can temporarily leave less
        // space than both panel minimums require. In that state no legal splitter
        // distance exists, so defer the layout instead of assigning an invalid value.
        if (available <= 0 || maximum < minimum)
        {
            return false;
        }

        var requested = keepSecondPanel
            ? available - preferredSize - container.SplitterWidth
            : preferredSize;
        var distance = Math.Clamp(requested, minimum, maximum);

        if (container.SplitterDistance != distance)
        {
            container.SplitterDistance = distance;
        }

        return true;
    }
'''

if old_fields in text:
    text = text.replace(old_fields, new_fields, 1)
elif new_fields not in text:
    raise SystemExit("MainForm field anchor not found.")

if old_layout in text:
    text = text.replace(old_layout, new_layout, 1)
elif new_layout not in text:
    raise SystemExit("MainForm layout block not found.")

path.write_text(text, encoding="utf-8", newline="\n")
print("Updated WinForms splitter initialization.")
