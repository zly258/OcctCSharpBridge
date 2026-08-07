using System.Runtime.InteropServices;
using Avalonia;

namespace CadAvalonia;

internal static class Program
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "CAD-Avalonia.log");

    [STAThread]
    public static void Main(string[] args)
    {
        InstallGlobalDiagnostics();

        try
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            EnsureNativeBridgeIsDiscoverable();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            ReportFatal("Application startup failed.", exception);
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void InstallGlobalDiagnostics()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var exception = args.ExceptionObject as Exception ?? new Exception(args.ExceptionObject?.ToString() ?? "Unknown fatal error.");
            AppendLog("Unhandled AppDomain exception", exception);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            AppendLog("Unobserved task exception", args.Exception);
        };
    }

    private static void EnsureNativeBridgeIsDiscoverable()
    {
        var localBridge = Path.Combine(AppContext.BaseDirectory, "OcctNative.dll");
        if (File.Exists(localBridge))
            return;

        var configuredDirectory = Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR");
        if (!string.IsNullOrWhiteSpace(configuredDirectory) &&
            File.Exists(Path.Combine(configuredDirectory, "OcctNative.dll")))
            return;

        throw new FileNotFoundException(
            "OcctNative.dll was not found beside CAD-Avalonia.exe. " +
            "Build with build.ps1 avalonia so the OCCT runtime is deployed next to the executable.",
            localBridge);
    }

    private static void ReportFatal(string message, Exception exception)
    {
        AppendLog(message, exception);

        var details = $"{message}\n\n{exception.GetType().Name}: {exception.Message}\n\nLog: {LogPath}";
        if (OperatingSystem.IsWindows())
            MessageBoxW(IntPtr.Zero, details, "CAD-Avalonia startup error", 0x00000010);
    }

    private static void AppendLog(string message, Exception exception)
    {
        try
        {
            File.AppendAllText(
                LogPath,
                $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}",
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }
        catch
        {
            // Diagnostics must never hide the original startup failure.
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hwnd, string text, string caption, uint type);
}
