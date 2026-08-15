using OcctNet;

namespace OcctDemo.Common;

public static class DemoProductInfo
{
    public const string ProductName = "OcctCSharpBridge Demo";
    public const string Author = "zly258";
    public const string Email = "zhangly1403@gmail.com";
    public const string DotNetVersion = ".NET 10";
    public const string CSharpVersion = "C# 14";
    public const string NativeLanguage = "C++17";
    public const string Platform = "Windows x64 / Linux x64";
    public const string UiHosts = "WinForms / WPF / Avalonia";
    public const string AvaloniaVersion = "12.1.0";
    public const string Repository = "https://github.com/zly258/OcctCSharpBridge";
    public const string License = "GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0";

    public static string BridgeVersion => OcctBridgeInfo.ManagedVersion;
    public static int NativeAbiVersion => OcctBridgeInfo.ExpectedAbiVersion;
    public static string OcctVersion => OcctEngine.OcctVersion;

    public static string AboutText(DemoLanguage language) => language == DemoLanguage.ChineseSimplified
        ? $"OCCT CAD 统一演示应用\nBridge {BridgeVersion} · Native ABI {NativeAbiVersion}\nOCCT {OcctVersion} · {DotNetVersion} · {CSharpVersion} · {NativeLanguage}\nWindows: {UiHosts} · Linux: Avalonia · Avalonia {AvaloniaVersion}\n\nRepository: {Repository}\nLicense: {License}\nAuthor: {Author}\nEmail: {Email}"
        : $"OCCT CAD unified demonstration application\nBridge {BridgeVersion} · Native ABI {NativeAbiVersion}\nOCCT {OcctVersion} · {DotNetVersion} · {CSharpVersion} · {NativeLanguage}\nWindows: {UiHosts} · Linux: Avalonia · Avalonia {AvaloniaVersion}\n\nRepository: {Repository}\nLicense: {License}\nAuthor: {Author}\nEmail: {Email}";
}
