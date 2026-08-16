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
            if (eventArgs.State != OcctViewportHostState.Ready || completed) return;
            try
            {
                if (!viewport.IsEngineInitialized || !viewport.RenderReady || viewport.EngineGeneration <= 0)
                    throw new InvalidOperationException("WinForms viewport reached Ready without a rendered OCCT engine generation.");

                var box = viewport.Engine.MakeBox(100, 80, 60);
                if (!box.IsValid)
                    throw new InvalidOperationException("WinForms viewport smoke created an invalid OCCT box.");

                viewport.Engine.Fit(box);
                viewport.Engine.Redraw();
                completed = true;
                exitCode = 0;

                _ = Task.Run(async () =>
                {
                    await Task.Delay(300).ConfigureAwait(false);
                    form.BeginInvoke((Action)(() =>
                    {
                        Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
                        Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
                        Console.WriteLine($"Engine generation: {viewport.EngineGeneration}");
                        Console.WriteLine("WinForms viewport lifecycle/render smoke passed.");
                        form.Close();
                    }));
                });
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
                $"WinForms viewport smoke timed out before Ready. Current state: {viewport.HostState}, render ready: {viewport.RenderReady}, generation: {viewport.EngineGeneration}.");
            completed = true;
            exitCode = 2;
            form.Close();
        };
        timeout.Start();

        Application.Run(form);
        return exitCode;
    }
}
