using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace CadAvalonia;

public sealed class App : Application
{
    public override void Initialize()
    {
        Program.Trace("App.Initialize entered.");
        Styles.Add(new FluentTheme());
        RequestedThemeVariant = ThemeVariant.Light;
        Program.Trace("App.Initialize completed.");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Program.Trace($"Framework initialization completed. Lifetime={ApplicationLifetime?.GetType().FullName ?? "<null>"}");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            Program.Trace("MainWindow constructed.");
            window.Opened += (_, _) => Program.Trace($"MainWindow opened. Handle={window.TryGetPlatformHandle()?.Handle}");
            window.Closed += (_, _) => Program.Trace("MainWindow closed.");
            desktop.MainWindow = window;
            Program.Trace("MainWindow assigned to desktop lifetime.");
        }
        else
        {
            Program.Trace("Classic desktop lifetime was not available; no MainWindow was assigned.");
        }

        base.OnFrameworkInitializationCompleted();
    }
}
