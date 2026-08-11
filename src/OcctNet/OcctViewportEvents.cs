namespace OcctNet;

/// <summary>Selection state reported by a reusable viewport host.</summary>
public sealed class OcctViewportSelectionEventArgs : EventArgs
{
    public OcctViewportSelectionEventArgs(IOcctObject? selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
    {
        SelectedObject = selectedObject;
        SelectedObjects = selectedObjects ?? throw new ArgumentNullException(nameof(selectedObjects));
    }

    public IOcctObject? SelectedObject { get; }
    public IReadOnlyList<IOcctObject> SelectedObjects { get; }
}

/// <summary>Viewport adapter error reported without terminating the UI event loop.</summary>
public sealed class OcctViewportErrorEventArgs : EventArgs
{
    public OcctViewportErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public Exception Exception { get; }
}

/// <summary>World-space point corresponding to a viewport screen position.</summary>
public sealed class OcctViewportWorldPointEventArgs : EventArgs
{
    public OcctViewportWorldPointEventArgs(int screenX, int screenY, OcctPoint3d worldPoint)
    {
        ScreenX = screenX;
        ScreenY = screenY;
        WorldPoint = worldPoint;
    }

    public int ScreenX { get; }
    public int ScreenY { get; }
    public OcctPoint3d WorldPoint { get; }
}
