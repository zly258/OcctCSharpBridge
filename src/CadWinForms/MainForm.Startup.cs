namespace CadWinForms;

public sealed partial class MainForm
{
    protected override void OnLoad(EventArgs e)
    {
        SuspendLayout();
        try
        {
            _initialPanelLayoutApplied = ApplyInitialPanelLayout();
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }

        base.OnLoad(e);
    }
}
