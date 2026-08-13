using Avalonia;
using OcctDemo.Common;

namespace OcctDemo.Avalonia;

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
            Trace("Native bridge discovery check passed.");
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
        var libraryName = OperatingSystem.IsWindows()
            ? "OcctNative.dll"
            : OperatingSystem.IsLinux()
                ? "libOcctNative.so"
                : throw new PlatformNotSupportedException("CAD-Avalonia currently supports Windows x64 and Linux x64.");

        var localBridge = Path.Combine(AppContext.BaseDirectory, libraryName);
        if (File.Exists(localBridge))
            return;

        var configuredDirectory = Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR");
        if (!string.IsNullOrWhiteSpace(configuredDirectory) &&
            File.Exists(Path.Combine(configuredDirectory, libraryName)))
            return;

        throw new FileNotFoundException(
            $"{libraryName} was not found beside the CAD-Avalonia application. " +
            "Build with build.ps1 avalonia on Windows or ./build.sh avalonia on Linux so the native bridge is deployed next to the demo.",
            localBridge);
    }

    private static void ReportFatal(string message, Exception exception)
    {
        Trace($"{message} {exception}");
        var logPath = CrashReporter.Write(ApplicationName, exception, message);
        var details = CrashReporter.BuildUserMessage(exception, logPath);
        Console.Error.WriteLine(details);
    }
}
