using System.Collections.Concurrent;
using System.Text;
using OcctNet;

internal static class ConcurrencyStress
{
    private const int ErrorIterations = 500;
    private const int DisposeIterations = 2_000;

    internal static void Run()
    {
        VerifyThreadLocalErrors();
        VerifyErrorBufferContract();
        VerifySafeHandleDisposeRace();
    }

    private static void VerifyThreadLocalErrors()
    {
        using var session = new OcctModelingSession();
        var handle = session.NativeHandle;
        var failures = new ConcurrentQueue<Exception>();

        var shapeErrors = Task.Factory.StartNew(() => Repeat(failures, () =>
        {
            var status = ModelNativeMethods.occt_model_shape_copy(handle, -1, out _);
            if (status != OcctStatus.ErrorInvalidArgument)
                throw new InvalidOperationException($"Unexpected shape-copy status: {status}.");

            var error = NativeError.ReadModelingSession(handle);
            if (error.Status != OcctStatus.ErrorInvalidArgument ||
                error.Message?.Contains("Shape ID", StringComparison.Ordinal) != true)
            {
                throw new InvalidOperationException(
                    $"Shape error was overwritten by another thread: {error.Status} / {error.Message}");
            }
        }, ErrorIterations), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        var operationErrors = Task.Factory.StartNew(() => Repeat(failures, () =>
        {
            var status = ModelNativeMethods.occt_model_operation_report_get(handle, -1, null, 0, out _);
            if (status != OcctStatus.ErrorInvalidArgument)
                throw new InvalidOperationException($"Unexpected operation-report status: {status}.");

            var error = NativeError.ReadModelingSession(handle);
            if (error.Status != OcctStatus.ErrorInvalidArgument ||
                error.Message?.Contains("Operation ID", StringComparison.Ordinal) != true)
            {
                throw new InvalidOperationException(
                    $"Operation error was overwritten by another thread: {error.Status} / {error.Message}");
            }
        }, ErrorIterations), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        Task.WaitAll(shapeErrors, operationErrors);
        ThrowFailures(failures, "Thread-local native error stress failed.");
    }

    private static void VerifyErrorBufferContract()
    {
        using var session = new OcctModelingSession();
        var handle = session.NativeHandle;
        _ = ModelNativeMethods.occt_model_shape_copy(handle, -1, out _);

        var query = ModelNativeMethods.occt_model_session_last_error_message(handle, null, 0, out var required);
        if (query != OcctStatus.Ok || required <= 1)
            throw new InvalidOperationException("Native error-message size query failed.");

        var shortBuffer = new byte[required - 1];
        var shortStatus = ModelNativeMethods.occt_model_session_last_error_message(
            handle, shortBuffer, shortBuffer.Length, out var shortRequired);
        if (shortStatus != OcctStatus.ErrorBufferTooSmall || shortRequired != required)
            throw new InvalidOperationException("Native error-message short-buffer contract failed.");

        var exactBuffer = new byte[required];
        var exactStatus = ModelNativeMethods.occt_model_session_last_error_message(
            handle, exactBuffer, exactBuffer.Length, out var exactRequired);
        if (exactStatus != OcctStatus.Ok || exactRequired != required || exactBuffer[^1] != 0)
            throw new InvalidOperationException("Native error-message exact-buffer contract failed.");

        var message = Encoding.UTF8.GetString(exactBuffer, 0, exactRequired - 1);
        if (!message.Contains("Shape ID", StringComparison.Ordinal))
            throw new InvalidOperationException("Native error-message buffer contained the wrong thread error.");
    }

    private static void VerifySafeHandleDisposeRace()
    {
        var session = new OcctModelingSession();
        var handle = session.NativeHandle;
        var failures = new ConcurrentQueue<Exception>();
        using var start = new ManualResetEventSlim(false);

        var workers = Enumerable.Range(0, Math.Min(8, Math.Max(2, Environment.ProcessorCount / 2)))
            .Select(_ => Task.Run(() =>
            {
                start.Wait();
                for (var index = 0; index < DisposeIterations; index++)
                {
                    try
                    {
                        var status = ModelNativeMethods.occt_model_shape_exists_get(handle, 1, out _);
                        if (status != OcctStatus.Ok)
                            throw new InvalidOperationException($"Unexpected exists status during disposal: {status}.");
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        failures.Enqueue(exception);
                        return;
                    }
                }
            }))
            .ToArray();

        start.Set();
        Thread.Yield();
        session.Dispose();
        Task.WaitAll(workers);
        ThrowFailures(failures, "SafeHandle dispose-race stress failed.");

        try
        {
            _ = ModelNativeMethods.occt_model_shape_exists_get(handle, 1, out _);
            throw new InvalidOperationException("A closed modeling SafeHandle accepted a new native call.");
        }
        catch (ObjectDisposedException)
        {
            // Expected: the handle cannot be reacquired after Dispose.
        }
    }

    private static void Repeat(ConcurrentQueue<Exception> failures, Action action, int iterations)
    {
        try
        {
            for (var index = 0; index < iterations; index++)
                action();
        }
        catch (Exception exception)
        {
            failures.Enqueue(exception);
        }
    }

    private static void ThrowFailures(ConcurrentQueue<Exception> failures, string message)
    {
        if (!failures.IsEmpty)
            throw new AggregateException(message, failures);
    }
}
