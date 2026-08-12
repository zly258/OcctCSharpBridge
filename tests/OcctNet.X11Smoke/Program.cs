using System.Runtime.InteropServices;
using OcctNet;

if (!OperatingSystem.IsLinux())
    throw new PlatformNotSupportedException("The X11 smoke test supports Linux only.");
if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
    throw new PlatformNotSupportedException("The X11 smoke test currently supports Linux x64 only.");
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")))
    throw new InvalidOperationException("DISPLAY is not set. Run the X11 smoke test in an X11 or XWayland desktop session.");

var display = XOpenDisplay(IntPtr.Zero);
if (display == IntPtr.Zero)
    throw new InvalidOperationException("Unable to open the X11 display.");

IntPtr window = IntPtr.Zero;
try
{
    var root = XDefaultRootWindow(display);
    if (root == IntPtr.Zero)
        throw new InvalidOperationException("Unable to resolve the X11 root window.");

    window = XCreateSimpleWindow(
        display,
        root,
        50,
        50,
        640,
        480,
        0,
        UIntPtr.Zero,
        UIntPtr.Zero);
    if (window == IntPtr.Zero)
        throw new InvalidOperationException("Unable to create the X11 OCCT smoke window.");

    _ = XStoreName(display, window, "OcctCSharpBridge X11 Smoke");
    _ = XMapWindow(display, window);
    _ = XSync(display, false);

    using var engine = new OcctEngine();
    engine.Initialize(window);
    var box = engine.MakeBox(100, 80, 60);
    engine.SetBackground(System.Drawing.Color.FromArgb(245, 247, 250));
    engine.Fit(box);
    engine.Redraw();

    if (!engine.IsInitialized)
        throw new InvalidOperationException("The OCCT X11 viewer did not initialize.");
    if (!box.IsValid)
        throw new InvalidOperationException("The OCCT X11 viewer box is invalid.");

    _ = XSync(display, false);
    Thread.Sleep(250);

    Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
    Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
    Console.WriteLine($"Build: {OcctBridgeInfo.BuildInfo}");
    Console.WriteLine("X11 viewer smoke passed.");
}
finally
{
    if (window != IntPtr.Zero)
    {
        _ = XDestroyWindow(display, window);
        _ = XSync(display, false);
    }
    _ = XCloseDisplay(display);
}

[DllImport("libX11.so.6")]
static extern IntPtr XOpenDisplay(IntPtr displayName);

[DllImport("libX11.so.6")]
static extern int XCloseDisplay(IntPtr display);

[DllImport("libX11.so.6")]
static extern IntPtr XDefaultRootWindow(IntPtr display);

[DllImport("libX11.so.6")]
static extern IntPtr XCreateSimpleWindow(
    IntPtr display,
    IntPtr parent,
    int x,
    int y,
    uint width,
    uint height,
    uint borderWidth,
    UIntPtr border,
    UIntPtr background);

[DllImport("libX11.so.6", CharSet = CharSet.Ansi)]
static extern int XStoreName(IntPtr display, IntPtr window, string windowName);

[DllImport("libX11.so.6")]
static extern int XMapWindow(IntPtr display, IntPtr window);

[DllImport("libX11.so.6")]
static extern int XDestroyWindow(IntPtr display, IntPtr window);

[DllImport("libX11.so.6")]
static extern int XSync(IntPtr display, [MarshalAs(UnmanagedType.Bool)] bool discard);
