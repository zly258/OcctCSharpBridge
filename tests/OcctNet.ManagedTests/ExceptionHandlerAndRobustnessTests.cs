using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class ExceptionHandlerAndRobustnessTests
{
    [TestMethod]
    public void ExceptionHandler_SafeCall_ReturnsValueOnSuccess()
    {
        var result = OcctExceptionHandler.SafeCall(() => 42, fallback: 0, context: "TestSuccess");
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ExceptionHandler_SafeCall_ReturnsFallbackOnExceptionAndNotifies()
    {
        OcctExceptionEventArgs? capturedArgs = null;
        Action<OcctExceptionEventArgs> observer = args => capturedArgs = args;

        OcctExceptionHandler.ExceptionObserved += observer;
        try
        {
            var result = OcctExceptionHandler.SafeCall<int>(
                () => throw new InvalidOperationException("Simulated CAD engine failure"),
                fallback: -1,
                context: "TestFailureContext");

            Assert.AreEqual(-1, result);
            Assert.IsNotNull(capturedArgs);
            Assert.AreEqual("TestFailureContext", capturedArgs.Context);
            Assert.IsFalse(capturedArgs.IsTerminating);
            Assert.IsInstanceOfType<InvalidOperationException>(capturedArgs.Exception);
            Assert.AreEqual("Simulated CAD engine failure", capturedArgs.Exception.Message);
            Assert.AreEqual(OcctStatus.ErrorUnknown, capturedArgs.OcctException.Status);
        }
        finally
        {
            OcctExceptionHandler.ExceptionObserved -= observer;
        }
    }

    [TestMethod]
    public void ExceptionHandler_SafeCallAction_CatchesException()
    {
        var executed = false;
        var success = OcctExceptionHandler.SafeCall(() =>
        {
            executed = true;
            throw new ArgumentOutOfRangeException("Simulated parameter error");
        }, context: "ActionTest");

        Assert.IsTrue(executed);
        Assert.IsFalse(success);
    }

    [TestMethod]
    public async Task ExceptionHandler_SafeCallAsync_ReturnsFallbackOnAsyncException()
    {
        var result = await OcctExceptionHandler.SafeCallAsync(
            async () =>
            {
                await Task.Yield();
                throw new TimeoutException("Native operation timed out");
#pragma warning disable CS0162 // Unreachable code detected
                return 100;
#pragma warning restore CS0162
            },
            fallback: 999,
            context: "AsyncTest");

        Assert.AreEqual(999, result);
    }

    [TestMethod]
    public void ExceptionHandler_RegisterAndUnregister_Idempotent()
    {
        // Safe to call multiple times without throwing
        OcctExceptionHandler.RegisterGlobalHandler();
        OcctExceptionHandler.RegisterGlobalHandler();
        OcctExceptionHandler.UnregisterGlobalHandler();
        OcctExceptionHandler.UnregisterGlobalHandler();
    }

    [TestMethod]
    public void OcctException_WrapAndTryCatch_WorkCorrectly()
    {
        var rawException = new InvalidOperationException("Test raw exception");
        var wrapped = OcctException.Wrap(rawException, "CustomOp");
        Assert.AreEqual("Test raw exception", wrapped.Message);
        Assert.AreEqual("CustomOp", wrapped.Operation);
        Assert.AreSame(rawException, wrapped.InnerException);

        // Wrapping an existing OcctException returns it directly
        var already = new OcctException("Native error", OcctStatus.ErrorGeometry, "OpA");
        var rewrapped = OcctException.Wrap(already, "OpB");
        Assert.AreSame(already, rewrapped);

        // TryCatch action helper
        var success = OcctException.TryCatch(() => { }, out var err);
        Assert.IsTrue(success);
        Assert.IsNull(err);

        var fail = OcctException.TryCatch(() => throw new FormatException("Format error"), out var errFail);
        Assert.IsFalse(fail);
        Assert.IsNotNull(errFail);
        Assert.IsInstanceOfType<FormatException>(errFail.InnerException);

        // TryCatch func helper with fallback
        var val = OcctException.TryCatch<string>(() => throw new Exception("Boom"), fallback: "fallback-val", out var errVal);
        Assert.AreEqual("fallback-val", val);
        Assert.IsNotNull(errVal);
    }

    [TestMethod]
    public void OcctGuard_InRangeAndFallbackIf_WorkCorrectly()
    {
        // InRange success
        OcctGuard.InRange(5.0, 0.0, 10.0, "param");

        // InRange failure
        Expect<ArgumentOutOfRangeException>(() =>
            OcctGuard.InRange(-1.0, 0.0, 10.0, "param"));
        Expect<ArgumentOutOfRangeException>(() =>
            OcctGuard.InRange(15.0, 0.0, 10.0, "param"));
        Expect<ArgumentOutOfRangeException>(() =>
            OcctGuard.InRange(double.NaN, 0.0, 10.0, "param"));

        // FallbackIf
        Assert.AreEqual("actual", OcctGuard.FallbackIf(true, "actual", "fallback"));
        Assert.AreEqual("fallback", OcctGuard.FallbackIf(false, "actual", "fallback"));
    }

    [TestMethod]
    public void OcctModelBooleanOptions_CreateDefault_ReturnsCopy()
    {
        var opts1 = OcctModelBooleanOptions.CreateDefault();
        var opts2 = OcctModelBooleanOptions.CreateDefault();

        Assert.IsTrue(opts1.RunParallel);
        Assert.IsTrue(opts1.NonDestructive);
        Assert.AreEqual(OcctModelBooleanGlue.Off, opts1.Glue);

        // Mutating opts1 should not mutate opts2
        opts1.RunParallel = false;
        Assert.IsFalse(opts1.RunParallel);
        Assert.IsTrue(opts2.RunParallel);
    }

    private static void Expect<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
            Assert.Fail($"Expected exception of type {typeof(TException).Name} was not thrown.");
        }
        catch (TException)
        {
            // Expected
        }
    }
}
