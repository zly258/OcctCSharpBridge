using System.Threading;


namespace OcctNet;

/// <summary>
/// Provides global unhandled-exception registration and safe-call helpers with
/// automatic exception logging and fallback value support.
/// </summary>
public static class OcctExceptionHandler
{
    private static int _globalHandlerRegistered;

    /// <summary>
    /// Delegate used to receive exception notifications from the global handler
    /// and from safe-call helpers.
    /// </summary>
    public static event Action<OcctExceptionEventArgs>? ExceptionObserved;

    /// <summary>
    /// Registers a global <see cref="AppDomain.UnhandledException"/> handler that
    /// translates unhandled exceptions into <see cref="ExceptionObserved"/> notifications.
    /// Safe to call multiple times — registration happens at most once.
    /// </summary>
    public static void RegisterGlobalHandler()
    {
        if (Interlocked.CompareExchange(ref _globalHandlerRegistered, 1, 0) != 0) return;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>
    /// Unregisters the global handler registered by <see cref="RegisterGlobalHandler"/>.
    /// </summary>
    public static void UnregisterGlobalHandler()
    {
        if (Interlocked.CompareExchange(ref _globalHandlerRegistered, 0, 1) != 1) return;

        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
    }

    // -------------------------------------------------------------------------
    // Safe-call helpers — execute an action and return a fallback on failure
    // -------------------------------------------------------------------------

    /// <summary>
    /// Executes <paramref name="action"/> safely, catching all exceptions.
    /// Notifies <see cref="ExceptionObserved"/> on failure.
    /// </summary>
    /// <returns><see langword="true"/> if the action succeeded.</returns>
    public static bool SafeCall(Action action, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            Notify(ex, context, isTerminating: false);
            return false;
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> safely, returning <paramref name="fallback"/> on failure.
    /// Notifies <see cref="ExceptionObserved"/> on failure.
    /// </summary>
    public static T SafeCall<T>(Func<T> func, T fallback, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(func);
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            Notify(ex, context, isTerminating: false);
            return fallback;
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> safely, returning <see langword="null"/> on failure.
    /// Notifies <see cref="ExceptionObserved"/> on failure.
    /// </summary>
    public static T? SafeCallNullable<T>(Func<T> func, string? context = null)
        where T : class
        => SafeCall<T?>(func, null, context);

    /// <summary>
    /// Executes <paramref name="asyncFunc"/> safely, returning <paramref name="fallback"/> on failure.
    /// Notifies <see cref="ExceptionObserved"/> on failure.
    /// </summary>
    public static async Task<T> SafeCallAsync<T>(Func<Task<T>> asyncFunc, T fallback, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(asyncFunc);
        try
        {
            return await asyncFunc().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Notify(ex, context, isTerminating: false);
            return fallback;
        }
    }

    /// <summary>
    /// Executes <paramref name="asyncAction"/> safely.
    /// Notifies <see cref="ExceptionObserved"/> on failure.
    /// </summary>
    public static async Task SafeCallAsync(Func<Task> asyncAction, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);
        try
        {
            await asyncAction().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Notify(ex, context, isTerminating: false);
        }
    }

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------

    internal static void Notify(Exception exception, string? context, bool isTerminating)
    {
        var observers = ExceptionObserved;
        if (observers is null) return;
        try
        {
            var args = new OcctExceptionEventArgs(exception, context, isTerminating);
            observers(args);
        }
        catch
        {
            // Never let observer errors propagate.
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception
            ?? new InvalidOperationException($"Non-exception unhandled object: {e.ExceptionObject}");
        Notify(exception, "AppDomain.UnhandledException", e.IsTerminating);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved(); // Prevent process termination
        Notify(e.Exception, "TaskScheduler.UnobservedTaskException", isTerminating: false);
    }
}

/// <summary>Event arguments for <see cref="OcctExceptionHandler.ExceptionObserved"/>.</summary>
public sealed class OcctExceptionEventArgs : EventArgs
{
    internal OcctExceptionEventArgs(Exception exception, string? context, bool isTerminating)
    {
        Exception = exception;
        Context = context;
        IsTerminating = isTerminating;
        OcctException = exception as OcctException ?? OcctException.Wrap(exception, context);
        CapturedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>The raw exception that was caught or observed.</summary>
    public Exception Exception { get; }

    /// <summary>The <see cref="OcctException"/> wrapper (may be the original exception itself).</summary>
    public OcctException OcctException { get; }

    /// <summary>Optional context string describing where the exception was caught.</summary>
    public string? Context { get; }

    /// <summary>Whether the process is about to terminate due to this exception.</summary>
    public bool IsTerminating { get; }

    /// <summary>When the notification was created.</summary>
    public DateTimeOffset CapturedAtUtc { get; }
}
