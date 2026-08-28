using System.Runtime.InteropServices;
using OcctNet;

OcctRuntime.Configure();

if (OcctBridgeInfo.NativeAbiVersion != OcctBridgeInfo.ExpectedAbiVersion)
    throw new InvalidOperationException("Native bridge ABI validation failed.");
if (!string.Equals(OcctBridgeInfo.NativeVersion, OcctBridgeInfo.ManagedVersion, StringComparison.Ordinal))
    throw new InvalidOperationException(
        $"Managed/native bridge version mismatch: {OcctBridgeInfo.ManagedVersion} / {OcctBridgeInfo.NativeVersion}.");
if (string.IsNullOrWhiteSpace(OcctBridgeInfo.OcctVersion))
    throw new InvalidOperationException("OCCT runtime version is empty.");

using var model = new OcctModelingSession();
var box = model.MakeBox(40, 30, 20);
if (!model.IsShapeValid(box) || model.GetTopologyCount(box, OcctShapeType.Face) != 6)
    throw new InvalidOperationException("Minimal native modeling smoke failed.");

Console.WriteLine(
    $"OcctCSharpBridge {OcctBridgeInfo.ManagedVersion} smoke passed on " +
    $"{RuntimeInformation.FrameworkDescription}; ABI {OcctBridgeInfo.NativeAbiVersion}; OCCT {OcctBridgeInfo.OcctVersion}.");
