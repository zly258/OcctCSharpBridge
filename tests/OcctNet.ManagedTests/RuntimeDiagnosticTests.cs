using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class RuntimeDiagnosticTests
{
    [TestMethod]
    public void RuntimeDiagnosticsRemainSideEffectFreeAndConsistent()
    {
        var variables = new[] { "PATH", "OCCT_BRIDGE_NATIVE_DIR", "OCCT_ROOT", "CASROOT" };
        var before = variables.ToDictionary(name => name, Environment.GetEnvironmentVariable);

        var info = OcctRuntime.GetDiagnosticInfo();
        var report = OcctRuntime.GetDiagnosticReport();

        foreach (var variable in variables)
        {
            if (!string.Equals(before[variable], Environment.GetEnvironmentVariable(variable), StringComparison.Ordinal))
                Assert.Fail($"Runtime diagnostics changed environment variable {variable}.");
        }

        if (string.IsNullOrWhiteSpace(info.FrameworkDescription))
            Assert.Fail("Runtime diagnostics did not report the .NET framework.");
        if (string.IsNullOrWhiteSpace(info.OperatingSystemDescription))
            Assert.Fail("Runtime diagnostics did not report the operating system.");
        if (string.IsNullOrWhiteSpace(info.BaseDirectory) || string.IsNullOrWhiteSpace(info.CurrentDirectory))
            Assert.Fail("Runtime diagnostics did not report process directories.");
        if (string.IsNullOrWhiteSpace(info.DiagnosticReport) || string.IsNullOrWhiteSpace(report))
            Assert.Fail("Runtime diagnostics did not preserve the text diagnostic report.");

        if (!Path.IsPathFullyQualified(info.ApplicationNativeBridgePath) ||
            !Path.IsPathFullyQualified(info.ApplicationOcctKernelPath))
        {
            Assert.Fail("App-local runtime diagnostic paths are not fully qualified.");
        }
        if (info.ApplicationNativeBridgeExists != File.Exists(info.ApplicationNativeBridgePath))
            Assert.Fail("App-local native bridge existence state is inconsistent.");
        if (info.ApplicationOcctKernelExists != File.Exists(info.ApplicationOcctKernelPath))
            Assert.Fail("App-local OCCT kernel existence state is inconsistent.");

        if ((info.ConfiguredNativeBridgePath is null) != (info.ConfiguredNativeBridgeExists is null))
            Assert.Fail("Configured native bridge path/existence state is inconsistent.");
        if ((info.ConfiguredOcctKernelPath is null) != (info.ConfiguredOcctKernelExists is null))
            Assert.Fail("Configured OCCT kernel path/existence state is inconsistent.");
        if (info.NativeBridgeLoaded != (info.LoadedNativeBridgePath is not null))
            Assert.Fail("Native bridge loaded state is inconsistent.");
        if (info.OcctKernelLoaded != (info.LoadedOcctKernelPath is not null))
            Assert.Fail("OCCT kernel loaded state is inconsistent.");

        if (!info.Is64BitProcess)
            Assert.Fail("OcctCSharpBridge managed tests must run as a 64-bit process.");
    }
}