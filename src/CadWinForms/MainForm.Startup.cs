using OcctNet;

namespace CadWinForms;

public sealed partial class MainForm
{
    protected override void OnLoad(EventArgs e)
    {
        SuspendLayout();
        try
        {
            _initialPanelLayoutApplied = ApplyInitialPanelLayout();
            _session?.Engine.SetDefaultPolygonOffsets(
                OcctPolygonOffsetMode.Fill,
                factor: 1.0,
                units: 1.0,
                applyExisting: true);
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }

        base.OnLoad(e);
    }
}
