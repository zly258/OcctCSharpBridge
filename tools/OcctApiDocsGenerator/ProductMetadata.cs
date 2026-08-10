internal static partial class Program
{
    private sealed record ProductContract(
        string Author,
        string BridgeVersion,
        int NativeAbiVersion,
        string OcctVersion,
        string SdkVersion,
        string LanguageVersion,
        string TargetFramework,
        string Platform,
        string AvaloniaVersion);

    static Program()
    {
        var product = ResolveProductContract();
        var stack = $"Bridge **{product.BridgeVersion}** · Native ABI **{product.NativeAbiVersion}** · OCCT **{product.OcctVersion}** · .NET SDK **{product.SdkVersion}** · C# **{product.LanguageVersion}** · C++17 · Avalonia **{product.AvaloniaVersion}** · `{product.TargetFramework}` · {product.Platform}";

        Zh = Zh with
        {
            GeneratedNotice = Zh.GeneratedNotice + $" Author: **{product.Author}**。",
            ManagedIntro = stack + "。" + Environment.NewLine + Environment.NewLine + Zh.ManagedIntro,
            NativeIntro = $"Author: **{product.Author}**。" + stack + "。" + Environment.NewLine + Environment.NewLine + Zh.NativeIntro
        };
        En = En with
        {
            GeneratedNotice = En.GeneratedNotice + $" Author: **{product.Author}**.",
            ManagedIntro = stack + "." + Environment.NewLine + Environment.NewLine + En.ManagedIntro,
            NativeIntro = $"Author: **{product.Author}**. " + stack + "." + Environment.NewLine + Environment.NewLine + En.NativeIntro
        };
    }

    private static ProductContract ResolveProductContract()
    {
        var repositoryRoot = FindRepositoryRoot();
        if (repositoryRoot is null)
        {
            return new ProductContract("zly258", "unknown", 0, "unknown", "unknown", "unknown", "net10.0-windows", "Windows x64", "unknown");
        }

        var contractPath = Path.Combine(repositoryRoot, "bridge-contract.json");
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(contractPath));
        var root = document.RootElement;
        var dotnet = root.GetProperty("dotnet");

        return new ProductContract(
            root.TryGetProperty("author", out var author) && !string.IsNullOrWhiteSpace(author.GetString()) ? author.GetString()! : "zly258",
            root.GetProperty("bridgeVersion").GetString() ?? "unknown",
            root.GetProperty("nativeAbiVersion").GetInt32(),
            root.GetProperty("occtVersion").GetString() ?? "unknown",
            dotnet.GetProperty("sdkVersion").GetString() ?? "unknown",
            dotnet.GetProperty("languageVersion").GetString() ?? "unknown",
            dotnet.GetProperty("targetFramework").GetString() ?? "unknown",
            FormatPlatform(root.GetProperty("platform").GetString()),
            ReadAvaloniaVersion(repositoryRoot));
    }

    private static string? FindRepositoryRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "bridge-contract.json"))) return directory.FullName;
                directory = directory.Parent;
            }
        }
        return null;
    }

    private static string ReadAvaloniaVersion(string repositoryRoot)
    {
        var projectPath = Path.Combine(repositoryRoot, "src", "OcctNet.Avalonia", "OcctNet.Avalonia.csproj");
        if (!File.Exists(projectPath)) return "unknown";

        var document = System.Xml.Linq.XDocument.Load(projectPath);
        var package = document.Descendants("PackageReference")
            .FirstOrDefault(element => string.Equals((string?)element.Attribute("Include"), "Avalonia", StringComparison.OrdinalIgnoreCase));
        return (string?)package?.Attribute("Version") ?? "unknown";
    }

    private static string FormatPlatform(string? platform) => string.Equals(platform, "windows-x64", StringComparison.OrdinalIgnoreCase)
        ? "Windows x64"
        : platform ?? "unknown";
}
