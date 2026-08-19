using System.Drawing;
using System.Windows.Forms;
using OcctNet;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        ApplicationConfiguration.Initialize();

        var exitCode = 2;
        var completed = false;
        var readyHandled = false;
        var nativeHandleCreated = false;
        var restoreStep = 0;
        var initialNativeHandle = IntPtr.Zero;
        var initialGeneration = 0L;

        using var viewport = new OcctViewportControl
        {
            Dock = DockStyle.Fill,
            InitialOptions = new OcctViewportInitializationOptions
            {
                BackgroundColor = Color.FromArgb(245, 247, 250),
                ViewOrientation = OcctViewOrientation.Isometric,
                Projection = OcctProjectionType.Orthographic,
                TriedronVisible = true,
                ViewCubeVisible = true
            }
        };
        using var form = new Form
        {
            Text = "OcctCSharpBridge WinForms Smoke",
            Width = 720,
            Height = 520
        };
        form.Controls.Add(viewport);

        using var restoreTimer = new System.Windows.Forms.Timer { Interval = 250 };
        restoreTimer.Tick += (_, _) =>
        {
            if (completed) return;

            try
            {
                switch (restoreStep++)
                {
                    case 0:
                        form.WindowState = FormWindowState.Minimized;
                        break;
                    case 1:
                        form.WindowState = FormWindowState.Normal;
                        break;
                    default:
                        restoreTimer.Stop();
                        if (!viewport.IsEngineInitialized
                            || viewport.HostState != OcctViewportHostState.Ready
                            || viewport.NativeHandle != initialNativeHandle
                            || viewport.EngineGeneration != initialGeneration)
                        {
                            throw new InvalidOperationException(
                                "WinForms viewport did not preserve its ready engine/native handle across minimize/restore.");
                        }

                        completed = true;
                        exitCode = 0;
                        Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
                        Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
                        Console.WriteLine($"Engine generation: {viewport.EngineGeneration}");
                        Console.WriteLine($"Native handle: 0x{viewport.NativeHandle.ToInt64():X}");
                        Console.WriteLine("WinForms minimize/restore exposure path exercised without pointer input.");
                        Console.WriteLine("WinForms highlight mode/color style calls passed.");
                        Console.WriteLine("WinForms triedron/view-cube position style calls passed.");
                        Console.WriteLine("WinForms viewport lifecycle/render/native-handle smoke passed.");
                        form.Close();
                        break;
                }
            }
            catch (Exception exception)
            {
                restoreTimer.Stop();
                Console.Error.WriteLine(exception);
                completed = true;
                exitCode = 1;
                form.Close();
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
            form.BeginInvoke(form.Close);
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
                        "WinForms viewport reached Ready without a live engine, native handle, and first rendered frame.");
                }

                var box = viewport.Engine.MakeBox(100, 80, 60);
                if (!box.IsValid)
                    throw new InvalidOperationException("WinForms viewport smoke created an invalid OCCT box.");

                viewport.Engine.SetSelectionHighlightStyle(new OcctViewerHighlightStyle(OcctHighlightMode.BoundingBox, Color.Orange));
                viewport.Engine.SetHoverHighlightStyle(new OcctViewerHighlightStyle(OcctHighlightMode.Wireframe, Color.Cyan));
                viewport.Engine.SetSelectionHighlightMode(OcctHighlightMode.Shaded);
                viewport.Engine.SetHoverHighlightMode(OcctHighlightMode.BoundingBox);
                viewport.Engine.SetSelectionHighlightColor(Color.Gold);
                viewport.Engine.SetHoverHighlightColor(Color.DeepSkyBlue);
                viewport.Engine.SetSelectionHighlightMode(OcctHighlightMode.Wireframe);
                viewport.Engine.SetHoverHighlightMode(OcctHighlightMode.Wireframe);

                foreach (var corner in Enum.GetValues<OcctCornerPosition>())
                {
                    viewport.Engine.SetTriedronPosition(corner);
                    viewport.Engine.SetViewCubePosition(corner);
                }
                viewport.Engine.SetTriedron(new OcctTriedronOptions
                {
                    Visible = true,
                    Position = OcctCornerPosition.RightLower,
                    Scale = 0.10,
                    Color = Color.White
                });
                viewport.Engine.SetTriedronScale(0.08);
                viewport.Engine.SetTriedronColor(Color.White);
                viewport.Engine.SetViewCube(new OcctViewCubeOptions
                {
                    Visible = true,
                    Position = OcctCornerPosition.RightUpper,
                    SizePixels = 96,
                    OffsetX = 12,
                    OffsetY = 12
                });
                viewport.Engine.SetViewCubeOptions(new OcctViewCubeOptions
                {
                    Visible = true,
                    Position = OcctCornerPosition.RightUpper,
                    SizePixels = 90,
                    OffsetX = 10,
                    OffsetY = 10
                });
                viewport.Engine.SetViewCubeSize(92);
                viewport.Engine.SetViewCubeOffset(10, 10);
                viewport.Engine.SetViewCubePosition(OcctCornerPosition.RightUpper);

                viewport.Engine.Fit(box);
                viewport.Engine.Redraw();
                initialNativeHandle = viewport.NativeHandle;
                initialGeneration = viewport.EngineGeneration;
                restoreTimer.Start();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                completed = true;
                exitCode = 1;
                form.BeginInvoke(form.Close);
            }
        };

        using var timeout = new System.Windows.Forms.Timer { Interval = 10_000 };
        timeout.Tick += (_, _) =>
        {
            timeout.Stop();
            if (completed) return;
            Console.Error.WriteLine(
                $"WinForms viewport smoke timed out. Current state: {viewport.HostState}, render ready: {viewport.RenderReady}, generation: {viewport.EngineGeneration}, restore step: {restoreStep}.");
            completed = true;
            exitCode = 2;
            form.Close();
        };
        timeout.Start();

        Application.Run(form);
        return exitCode;
    }
}
