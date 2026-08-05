using System.Threading;
using System.Threading.Tasks;
using CadCommon;

namespace CadWinForms;

internal static class Program
{
    private const string ApplicationName = "CAD-Winform";

    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            ApplicationConfiguration.Initialize();
            var mainForm = new MainForm();
            mainForm.AttachApiCenter();
            Application.Run(mainForm);
        }
        catch (Exception exception)
        {
            ShowFatalException(exception, "Application.Main");
        }
    }

    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        ShowFatalException(e.Exception, "Application.ThreadException");
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

    private static void ShowFatalException(Exception exception, string source)
    {
        var logPath = CrashReporter.Write(ApplicationName, exception, source);
        var logLabel = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "日志" : "Log";
        var prefix = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
            ? "程序发生未处理异常："
            : "An unhandled application error occurred: ";
        var logMessage = string.IsNullOrWhiteSpace(logPath)
            ? string.Empty
            : $"{Environment.NewLine}{Environment.NewLine}{logLabel}: {logPath}";

        MessageBox.Show(
            $"{prefix}{exception.Message}{logMessage}",
            CadLocalization.Text("Dialog.ErrorTitle"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
