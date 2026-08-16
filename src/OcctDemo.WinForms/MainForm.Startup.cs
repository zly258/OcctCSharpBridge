using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private bool _depthDefaultsApplied;

    protected override void OnLoad(EventArgs e)
    {
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

        base.OnLoad(e);
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
