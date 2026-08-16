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

                var engine = viewport.Engine;
                var box = engine.MakeBox(100, 80, 60);
                if (!box.IsValid)
                    throw new InvalidOperationException("Avalonia viewport smoke created an invalid OCCT box.");

                ValidateShapeProjection(engine, box);

                engine.SetBackground(System.Drawing.Color.FromArgb(245, 247, 250));
                engine.SetView(OcctViewOrientation.Isometric);
                engine.Fit(box);
                engine.Redraw();

                completed = true;
                Dispatcher.UIThread.Post(async () =>
                {
                    await Task.Delay(300);
                    Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
                    Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
                    Console.WriteLine($"Platform: {(OperatingSystem.IsWindows() ? "Windows" : "Linux")}");
                    Console.WriteLine($"Engine generation: {viewport.EngineGeneration}");
                    Console.WriteLine("Viewer edge/face point projection: validated");
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

    private static void ValidateShapeProjection(OcctEngine engine, OcctShape box)
    {
        var edge = engine.MakeLine(
            OcctPoint3d.Origin,
            new OcctPoint3d(100, 0, 0));
        var edgeProjection = engine.ProjectPointToEdge(edge, new OcctPoint3d(40, 25, 0));
        if (edgeProjection.Point.DistanceTo(new OcctPoint3d(40, 0, 0)) > 1e-6
            || Math.Abs(edgeProjection.NormalizedParameter - 0.4) > 1e-8
            || Math.Abs(edgeProjection.Distance - 25.0) > 1e-6)
        {
            throw new InvalidOperationException("Point-to-edge projection returned unexpected interior geometry.");
        }

        var evaluatedEdge = engine.EvaluateEdge(edge, edgeProjection.NormalizedParameter);
        if (evaluatedEdge.Point.DistanceTo(edgeProjection.Point) > 1e-6)
            throw new InvalidOperationException("Projected edge parameter does not evaluate back to the projected point.");

        var endpointProjection = engine.ProjectPointToEdge(edge, new OcctPoint3d(150, 10, 0));
        if (endpointProjection.Point.DistanceTo(new OcctPoint3d(100, 0, 0)) > 1e-6
            || Math.Abs(endpointProjection.NormalizedParameter - 1.0) > 1e-8)
        {
            throw new InvalidOperationException("Point-to-edge projection did not respect the trimmed edge endpoint.");
        }

        var face = engine.GetSubshapeAt(box, OcctShapeType.Face, 0);
        var uv = engine.GetFaceUvBounds(face);
        var u = (uv.UMin + uv.UMax) * 0.5;
        var v = (uv.VMin + uv.VMax) * 0.5;
        var facePoint = engine.EvaluateFace(face, u, v);
        var source = facePoint.Point + facePoint.Normal * 20.0;
        var faceProjection = engine.ProjectPointToFace(face, source);
        if (faceProjection.Point.DistanceTo(facePoint.Point) > 1e-5
            || Math.Abs(faceProjection.Distance - 20.0) > 1e-5)
        {
            throw new InvalidOperationException("Point-to-face projection returned unexpected geometry.");
        }

        var evaluatedFace = engine.EvaluateFace(face, faceProjection.U, faceProjection.V);
        if (evaluatedFace.Point.DistanceTo(faceProjection.Point) > 1e-5)
            throw new InvalidOperationException("Projected face parameters do not evaluate back to the projected point.");
    }
}
