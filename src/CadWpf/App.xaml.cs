using System.Threading.Tasks;
using CadCommon;
using System.Windows.Threading;

namespace CadWpf;

public partial class App : System.Windows.Application
{
    private const string ApplicationName = "CAD-WPF";

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        base.OnStartup(e);
        Dispatcher.BeginInvoke(() =>
        {
            if (Current.MainWindow is MainWindow window)
                window.AttachApiCenter();
        }, DispatcherPriority.Loaded);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logPath = CrashReporter.Write(ApplicationName, e.Exception, "DispatcherUnhandledException");
        var logLabel = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "日志" : "Log";
        var prefix = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
            ? "程序发生未处理异常："
            : "An unhandled application error occurred: ";
        var logMessage = string.IsNullOrWhiteSpace(logPath)
            ? string.Empty
            : $"{Environment.NewLine}{Environment.NewLine}{logLabel}: {logPath}";

        var message = $"{prefix}{e.Exception.Message}{logMessage}";
        if (MainWindow is { } owner)
        {
            System.Windows.MessageBox.Show(
                owner,
                message,
                CadLocalization.Text("Dialog.ErrorTitle"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        else
        {
            System.Windows.MessageBox.Show(
                message,
                CadLocalization.Text("Dialog.ErrorTitle"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }

        e.Handled = true;
    }

    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception
            ?? new InvalidOperationException(e.ExceptionObject?.ToString() ?? "Unknown unhandled exception.");
        CrashReporter.Write(ApplicationName, exception, "AppDomain.UnhandledException");
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        CrashReporter.Write(ApplicationName, e.Exception, "TaskScheduler.UnobservedTaskException");
        e.SetObserved();
    }
}
