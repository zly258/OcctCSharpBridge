using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using OcctNet;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
                throw new PlatformNotSupportedException("Avalonia viewer smoke supports Windows x64 and Linux x64 only.");

            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<SmokeApp>().UsePlatformDetect();
}

internal sealed class SmokeApp : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            throw new InvalidOperationException("Classic desktop lifetime is required for the Avalonia viewer smoke test.");

        var viewport = new OcctAvaloniaViewport();
        var window = new Window
        {
            Title = "OcctCSharpBridge Avalonia Smoke",
            Width = 720,
            Height = 520,
            Content = viewport
        };

        var completed = false;
        var readyHandled = false;
        viewport.Faulted += (_, eventArgs) =>
        {
            if (completed) return;
            Console.Error.WriteLine(eventArgs.Exception);
            completed = true;
            desktop.Shutdown(1);
        };
        viewport.HostStateChanged += (_, eventArgs) =>
        {
            if (eventArgs.State != OcctViewportHostState.Ready || readyHandled) return;
            readyHandled = true;
            try
            {
                if (!viewport.IsEngineInitialized || viewport.EngineGeneration <= 0 || !viewport.RenderReady)
                {
                    throw new InvalidOperationException(
                        "Avalonia viewport reached Ready without a live, first-frame-rendered OCCT engine generation.");
                }

                var box = viewport.Engine.MakeBox(100, 80, 60);
                if (!box.IsValid)
                    throw new InvalidOperationException("Avalonia viewport smoke created an invalid OCCT box.");

                viewport.Engine.SetBackground(System.Drawing.Color.FromArgb(245, 247, 250));
                viewport.Engine.SetView(OcctViewOrientation.Isometric);
                viewport.Engine.Fit(box);
                viewport.Engine.Redraw();

                completed = true;
                Dispatcher.UIThread.Post(async () =>
                {
                    await Task.Delay(300);
                    Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
                    Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
                    Console.WriteLine($"Platform: {(OperatingSystem.IsWindows() ? "Windows" : "Linux")}");
                    Console.WriteLine($"Engine generation: {viewport.EngineGeneration}");
                    Console.WriteLine("Avalonia viewer lifecycle/render smoke passed.");
                    desktop.Shutdown(0);
                });
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                completed = true;
                desktop.Shutdown(1);
            }
        };

        window.Opened += (_, _) =>
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                if (completed) return;
                Console.Error.WriteLine(
                    $"Avalonia viewer smoke timed out before Ready. Current state: {viewport.HostState}, render ready: {viewport.RenderReady}, generation: {viewport.EngineGeneration}.");
                desktop.Shutdown(2);
            });
        };

        desktop.MainWindow = window;
        base.OnFrameworkInitializationCompleted();
    }
}
