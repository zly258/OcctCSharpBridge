using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using OcctNet;

internal static class Program
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(IntPtr hwnd, out NativeRect rect);

    [STAThread]
    private static int Main()
    {
        var exitCode = 2;
        var completed = false;
        var readyHandled = false;
        var nativeHandleCreated = false;
        var restoreStep = 0;
        var initialNativeHandle = IntPtr.Zero;
        var initialGeneration = 0L;
        var application = new Application
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose
        };
        var viewport = new OcctWpfViewport
        {
            InitialOptions = new OcctViewportInitializationOptions
            {
                BackgroundColor = Color.FromArgb(245, 247, 250),
                ViewOrientation = OcctViewOrientation.Isometric,
                Projection = OcctProjectionType.Orthographic,
                TriedronVisible = true,
                ViewCubeVisible = true
            }
        };
        var window = new Window
        {
            Title = "OcctCSharpBridge WPF Smoke",
            Width = 720,
            Height = 520,
            Content = viewport
        };

        var restoreTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        restoreTimer.Tick += (_, _) =>
        {
            if (completed) return;

            try
            {
                switch (restoreStep++)
                {
                    case 0:
                        window.WindowState = WindowState.Minimized;
                        break;
                    case 1:
                        window.WindowState = WindowState.Normal;
                        break;
                    default:
                        restoreTimer.Stop();
                        if (!viewport.IsEngineInitialized
                            || viewport.HostState != OcctViewportHostState.Ready
                            || viewport.NativeHandle != initialNativeHandle
                            || viewport.EngineGeneration != initialGeneration)
                        {
                            throw new InvalidOperationException(
                                "WPF viewport did not preserve its ready engine/native handle across minimize/restore.");
                        }

                        completed = true;
                        exitCode = 0;
                        Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
                        Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
                        Console.WriteLine($"Engine generation: {viewport.EngineGeneration}");
                        Console.WriteLine($"Native handle: 0x{viewport.NativeHandle.ToInt64():X}");
                        Console.WriteLine("WPF query-only rectangle detection preserved native selection state.");
                        Console.WriteLine("WPF minimize/restore exposure path exercised without pointer input.");
                        Console.WriteLine("WPF viewport lifecycle/render/native-handle smoke passed.");
                        window.Close();
                        break;
                }
            }
            catch (Exception exception)
            {
                restoreTimer.Stop();
                Console.Error.WriteLine(exception);
                completed = true;
                exitCode = 1;
                window.Close();
            }
        };

        viewport.NativeHandleChanged += (_, eventArgs) =>
        {
            if (eventArgs.PreviousHandle == IntPtr.Zero
                && eventArgs.NativeHandle != IntPtr.Zero
                && eventArgs.Generation > 0)
            {
                nativeHandleCreated = true;
            }
        };

        viewport.Faulted += (_, eventArgs) =>
        {
            if (completed) return;
            Console.Error.WriteLine(eventArgs.Exception);
            completed = true;
            exitCode = 1;
            window.Close();
        };

        viewport.HostStateChanged += (_, eventArgs) =>
        {
            if (eventArgs.State != OcctViewportHostState.Ready || completed || readyHandled) return;
            readyHandled = true;
            try
            {
                if (!viewport.IsEngineInitialized
                    || !viewport.RenderReady
                    || viewport.EngineGeneration <= 0
                    || viewport.NativeHandle == IntPtr.Zero
                    || !nativeHandleCreated)
                {
                    throw new InvalidOperationException(
                        "WPF viewport reached Ready without a live engine, native handle, and first rendered frame.");
                }

                var box = viewport.Engine.MakeBox(100, 80, 60);
                if (!box.IsValid)
                    throw new InvalidOperationException("WPF viewport smoke created an invalid OCCT box.");

                viewport.Engine.Fit(box);
                viewport.Engine.Redraw();

                viewport.Engine.ClearSelection();
                if (viewport.Engine.SelectedObjects.Count != 0)
                    throw new InvalidOperationException("WPF rectangle-query smoke did not start with an empty selection.");

                var (nativeWidth, nativeHeight) = GetNativeClientSize(viewport.NativeHandle);
                var queried = viewport.Engine.QueryRectangle(
                    0,
                    0,
                    nativeWidth - 1,
                    nativeHeight - 1,
                    allowOverlap: false);
                if (!queried.Any(item => item.Id == box.Id))
                {
                    throw new InvalidOperationException(
                        $"QueryRectangle did not return the visible fitted box within native client {nativeWidth}x{nativeHeight}.");
                }
                if (viewport.Engine.SelectedObjects.Count != 0)
                    throw new InvalidOperationException("QueryRectangle mutated the native selection set.");

                initialNativeHandle = viewport.NativeHandle;
                initialGeneration = viewport.EngineGeneration;
                restoreTimer.Start();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                completed = true;
                exitCode = 1;
                window.Close();
            }
        };

        var timeout = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        timeout.Tick += (_, _) =>
        {
            timeout.Stop();
            if (completed) return;
            Console.Error.WriteLine(
                $"WPF viewport smoke timed out. Current state: {viewport.HostState}, render ready: {viewport.RenderReady}, generation: {viewport.EngineGeneration}, restore step: {restoreStep}.");
            completed = true;
            exitCode = 2;
            window.Close();
        };
        timeout.Start();

        application.Run(window);
        return exitCode;
    }

    private static (int Width, int Height) GetNativeClientSize(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
            throw new InvalidOperationException("WPF rectangle-query smoke has no native viewport handle.");
        if (!GetClientRect(hwnd, out var rect))
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"GetClientRect failed for the WPF native viewport (Win32 error {error}).");
        }

        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException(
                $"WPF native viewport has an invalid client size: {width}x{height}.");
        }
        return (width, height);
    }
}
