namespace OcctNet;

[Flags]
public enum OcctInputModifiers
{
    None = 0,
    Shift = 1 << 0,
    Control = 1 << 1,
    Alt = 1 << 2,
    Meta = 1 << 3
}

public enum OcctPointerInputKind
{
    Pressed = 0,
    Moved = 1,
    Released = 2,
    Wheel = 3
}

public enum OcctPointerButton
{
    None = 0,
    Left = 1,
    Middle = 2,
    Right = 3,
    X1 = 4,
    X2 = 5
}

[Flags]
public enum OcctPointerButtons
{
    None = 0,
    Left = 1 << 0,
    Middle = 1 << 1,
    Right = 1 << 2,
    X1 = 1 << 3,
    X2 = 1 << 4
}

public enum OcctKeyInputKind
{
    Pressed = 0,
    Released = 1
}

public enum OcctKey
{
    Unknown = 0,
    Escape,
    Enter,
    Tab,
    Backspace,
    Space,
    Delete,
    Insert,
    Home,
    End,
    PageUp,
    PageDown,
    Left,
    Right,
    Up,
    Down,
    Shift,
    Control,
    Alt,
    Meta,
    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12,
    D0,
    D1,
    D2,
    D3,
    D4,
    D5,
    D6,
    D7,
    D8,
    D9,
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z
}

[Flags]
public enum OcctViewportInteractionFeatures
{
    None = 0,
    HoverDetection = 1 << 0,
    PointSelection = 1 << 1,
    RectangleSelection = 1 << 2,
    Rotate = 1 << 3,
    Pan = 1 << 4,
    Zoom = 1 << 5,

    Selection = PointSelection | RectangleSelection,
    Navigation = Rotate | Pan | Zoom,
    Default = HoverDetection | Selection | Navigation
}

public sealed class OcctPointerInputEventArgs : EventArgs
{
    public OcctPointerInputEventArgs(
        OcctPointerInputKind kind,
        OcctPointerButton button,
        OcctPointerButtons buttons,
        int x,
        int y,
        int wheelDelta,
        OcctInputModifiers modifiers)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (!Enum.IsDefined(button)) throw new ArgumentOutOfRangeException(nameof(button));
        ValidateFlags(buttons, nameof(buttons));
        ValidateFlags(modifiers, nameof(modifiers));

        Kind = kind;
        Button = button;
        Buttons = buttons;
        X = x;
        Y = y;
        WheelDelta = wheelDelta;
        Modifiers = modifiers;
    }

    public OcctPointerInputKind Kind { get; }
    public OcctPointerButton Button { get; }
    public OcctPointerButtons Buttons { get; }
    public int X { get; }
    public int Y { get; }
    public int WheelDelta { get; }
    public OcctInputModifiers Modifiers { get; }
    public bool Handled { get; set; }

    private static void ValidateFlags<T>(T value, string parameterName) where T : struct, Enum
    {
        var raw = Convert.ToUInt64(value);
        var defined = typeof(T) == typeof(OcctPointerButtons)
            ? Convert.ToUInt64(OcctPointerButtons.Left | OcctPointerButtons.Middle | OcctPointerButtons.Right | OcctPointerButtons.X1 | OcctPointerButtons.X2)
            : Convert.ToUInt64(OcctInputModifiers.Shift | OcctInputModifiers.Control | OcctInputModifiers.Alt | OcctInputModifiers.Meta);
        if ((raw & ~defined) != 0) throw new ArgumentOutOfRangeException(parameterName);
    }
}

public sealed class OcctKeyInputEventArgs : EventArgs
{
    public OcctKeyInputEventArgs(
        OcctKeyInputKind kind,
        OcctKey key,
        OcctInputModifiers modifiers,
        bool isRepeat = false)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (!Enum.IsDefined(key)) throw new ArgumentOutOfRangeException(nameof(key));
        var rawModifiers = Convert.ToUInt64(modifiers);
        var definedModifiers = Convert.ToUInt64(OcctInputModifiers.Shift | OcctInputModifiers.Control | OcctInputModifiers.Alt | OcctInputModifiers.Meta);
        if ((rawModifiers & ~definedModifiers) != 0) throw new ArgumentOutOfRangeException(nameof(modifiers));

        Kind = kind;
        Key = key;
        Modifiers = modifiers;
        IsRepeat = isRepeat;
    }

    public OcctKeyInputKind Kind { get; }
    public OcctKey Key { get; }
    public OcctInputModifiers Modifiers { get; }
    public bool IsRepeat { get; }
    public bool Handled { get; set; }
}
