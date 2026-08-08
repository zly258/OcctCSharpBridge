using System.Runtime.InteropServices;
using Avalonia;
using CadCommon;

namespace CadAvalonia;

internal static class Program
{
    private const string ApplicationName = "CAD-Avalonia";
    private static readonly string TraceLogPath = Path.Combine(AppContext.BaseDirectory, "CAD-Avalonia.log");

    [STAThread]
    public static void Main(string[] args)
    {
        InstallGlobalDiagnostics();
        Trace($"Process started. PID={Environment.ProcessId}; BaseDirectory={AppContext.BaseDirectory}");

        try
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            EnsureNativeBridgeIsDiscoverable();
            Trace("OcctNative.dll discovery check passed.");
            Trace("Starting Avalonia classic desktop lifetime.");

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

            Trace("Avalonia classic desktop lifetime returned normally.");
        }
        catch (Exception exception)
        {
            Environment.ExitCode = 1;
            ReportFatal("Application startup failed.", exception);
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    internal static void Trace(string message)
    {
        try
        {
            File.AppendAllText(
                TraceLogPath,
                $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}",
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }
        catch
        {
            // Diagnostics must never affect application startup.
        }
    }

    private static void InstallGlobalDiagnostics()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var exception = args.ExceptionObject as Exception
                ?? new Exception(args.ExceptionObject?.ToString() ?? "Unknown fatal error.");
            CrashReporter.Write(ApplicationName, exception, "AppDomain.UnhandledException");
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            CrashReporter.Write(ApplicationName, args.Exception, "TaskScheduler.UnobservedTaskException");
            args.SetObserved();
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
        Trace($"{message} {exception}");
        var logPath = CrashReporter.Write(ApplicationName, exception, message);
        var details = CrashReporter.BuildUserMessage(exception, logPath);
        if (OperatingSystem.IsWindows())
            MessageBoxW(IntPtr.Zero, details, "CAD-Avalonia startup error", 0x00000010);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hwnd, string text, string caption, uint type);
}
