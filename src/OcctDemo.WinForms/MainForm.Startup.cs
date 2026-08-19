using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private bool _depthDefaultsApplied;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        BeginInvoke(new Action(ApplyDeferredStartupLayout));
    }

    private void ApplyDeferredStartupLayout()
    {
        if (IsDisposed) return;

        SuspendLayout();
        try
        {
            _initialPanelLayoutApplied = ApplyInitialPanelLayout();
            ApplyDepthDisplayDefaults();
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }

        Refresh();
        _viewport.Invalidate();
    }

    private void ApplyDepthDisplayDefaults()
    {
        if (_depthDefaultsApplied || _session is null)
        {
            return;
        }

        _session.Engine.SetDefaultPolygonOffsets(
            OcctPolygonOffsetMode.Fill,
            factor: 1.0,
            units: 1.0,
            applyExisting: true);
        _session.Engine.SetFaceBoundariesVisible(true, applyExisting: true);
        _depthDefaultsApplied = true;
    }
}
