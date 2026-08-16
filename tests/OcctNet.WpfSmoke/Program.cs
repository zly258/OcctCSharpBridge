using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using OcctNet;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var exitCode = 2;
        var completed = false;
        var nativeHandleCreated = false;
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
            if (eventArgs.State != OcctViewportHostState.Ready || completed) return;
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
                completed = true;
                exitCode = 0;

                var closeTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromMilliseconds(300)
                };
                closeTimer.Tick += (_, _) =>
                {
                    closeTimer.Stop();
                    Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
                    Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
                    Console.WriteLine($"Engine generation: {viewport.EngineGeneration}");
                    Console.WriteLine($"Native handle: 0x{viewport.NativeHandle.ToInt64():X}");
                    Console.WriteLine("WPF viewport lifecycle/render/native-handle smoke passed.");
                    window.Close();
                };
                closeTimer.Start();
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
                $"WPF viewport smoke timed out before Ready. Current state: {viewport.HostState}, render ready: {viewport.RenderReady}, generation: {viewport.EngineGeneration}.");
            completed = true;
            exitCode = 2;
            window.Close();
        };
        timeout.Start();

        application.Run(window);
        return exitCode;
    }
}
