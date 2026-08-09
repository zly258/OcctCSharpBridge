using System.Globalization;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    protected override void OnShown(EventArgs e)
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

        return mainApplied && centerRightApplied;
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
}
