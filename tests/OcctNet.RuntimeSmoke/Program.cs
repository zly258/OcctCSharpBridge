using System.Runtime.InteropServices;
using OcctNet;

if (Environment.Version.Major != 10)
    throw new InvalidOperationException(
        $"Stable package smoke requires .NET 10; running on {RuntimeInformation.FrameworkDescription}.");

OcctRuntime.Configure();

if (OcctBridgeInfo.NativeAbiVersion != OcctBridgeInfo.ExpectedAbiVersion)
    throw new InvalidOperationException("Native bridge ABI validation failed.");
if (!string.Equals(OcctBridgeInfo.NativeVersion, OcctBridgeInfo.ManagedVersion, StringComparison.Ordinal))
    throw new InvalidOperationException(
        $"Managed/native bridge version mismatch: {OcctBridgeInfo.ManagedVersion} / {OcctBridgeInfo.NativeVersion}.");
if (!string.Equals(OcctBridgeInfo.OcctVersion, "7.9.0", StringComparison.Ordinal))
    throw new InvalidOperationException($"Unexpected OCCT runtime version: {OcctBridgeInfo.OcctVersion}.");

using var model = new OcctModelingSession();
var box = model.MakeBox(40, 30, 20);
var tool = model.MakeCylinder(new OcctPoint3d(20, 15, -5), OcctVector3d.UnitZ, 4, 30);
var cut = model.Cut(box, tool);

if (!cut.Succeeded || !model.IsShapeValid(cut.Shape))
    throw new InvalidOperationException("Stable package Boolean smoke failed.");

var faceCount = model.GetTopologyCount(cut.Shape, OcctShapeType.Face);
if (faceCount <= 0)
    throw new InvalidOperationException("Stable package topology smoke failed.");

Console.WriteLine(
    $"OcctCSharpBridge {OcctBridgeInfo.ManagedVersion} package smoke passed on " +
    $"{RuntimeInformation.FrameworkDescription}; ABI {OcctBridgeInfo.NativeAbiVersion}; OCCT {OcctBridgeInfo.OcctVersion}.");
