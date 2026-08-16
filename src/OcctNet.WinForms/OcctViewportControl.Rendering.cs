namespace OcctNet;

public sealed partial class OcctViewportControl
{
    private bool _renderReady;

    public bool RenderReady => _renderReady;

    public event EventHandler<OcctFirstFrameRenderedEventArgs>? FirstFrameRendered;

    private void ResetRenderReady() => _renderReady = false;

    private void MarkFirstFrameRendered(long generation)
    {
        _renderReady = true;
        try { FirstFrameRendered?.Invoke(this, new OcctFirstFrameRenderedEventArgs(generation)); }
        catch (Exception exception) { ReportLifecycleError(exception); }
    }
}
