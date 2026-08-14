using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

internal static partial class Program
{
    private sealed record AssemblyInput(string Name, string ProjectDirectory, string DllPath, string XmlPath, string OutputDirectory);
    private sealed record NativeHeader(string Name, string Content);
    private sealed record NativeFunction(string Header, string ReturnType, string Name, string Parameters, string Declaration);

    private sealed class XmlDocs
    {
        private readonly Dictionary<string, string> _summaries = new(StringComparer.Ordinal);
        public XmlDocs(string path)
        {
            if (!File.Exists(path)) return;
            var document = XDocument.Load(path);
            foreach (var member in document.Descendants("member"))
            {
                var name = (string?)member.Attribute("name");
                if (string.IsNullOrWhiteSpace(name)) continue;
                var summary = Normalize(member.Element("summary")?.Value ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(summary)) _summaries[name] = summary;
            }
        }
        public string Type(Type type) => Get("T:" + XmlTypeName(type));
        public string Property(Type type, PropertyInfo property) => Get("P:" + XmlTypeName(type) + "." + property.Name);
        public string Event(Type type, EventInfo value) => Get("E:" + XmlTypeName(type) + "." + value.Name);
        public string Field(Type type, FieldInfo field) => Get("F:" + XmlTypeName(type) + "." + field.Name);
        public string Method(Type type, MethodBase method)
        {
            var methodName = method.IsConstructor ? "#ctor" : method.Name;
            var prefix = "M:" + XmlTypeName(type) + "." + methodName;
            var exact = _summaries.FirstOrDefault(pair => pair.Key == prefix || pair.Key.StartsWith(prefix + "(", StringComparison.Ordinal));
            return exact.Value ?? string.Empty;
        }
        private string Get(string key) => _summaries.TryGetValue(key, out var value) ? value : string.Empty;
        private static string XmlTypeName(Type type) => (type.FullName ?? type.Name).Replace('+', '.');
        private static string Normalize(string value) => WhitespaceRegex().Replace(value, " ").Trim();
    }

    public static int Main(string[] args)
    {
        try
        {
            var options = ParseArgs(args);
            var root = Path.GetFullPath(options.GetValueOrDefault("repository-root") ?? Directory.GetCurrentDirectory());
            var configuration = options.GetValueOrDefault("configuration") ?? "Release";
            var contractPath = Path.Combine(root, "bridge-contract.json");
            if (!File.Exists(contractPath)) throw new FileNotFoundException("bridge-contract.json was not found.", contractPath);
            using var contract = JsonDocument.Parse(File.ReadAllText(contractPath));
            var contractRoot = contract.RootElement;
            var api = contractRoot.GetProperty("api");
            var targetFramework = contractRoot.GetProperty("dotnet").GetProperty("targetFramework").GetString()
                ?? throw new InvalidOperationException("dotnet.targetFramework is missing from bridge-contract.json.");
            var bridgeVersion = contractRoot.GetProperty("bridgeVersion").GetString() ?? string.Empty;
            var desktopTargetFramework = contractRoot.GetProperty("dotnet").GetProperty("desktopTargetFramework").GetString()
                ?? throw new InvalidOperationException("dotnet.desktopTargetFramework is missing from bridge-contract.json.");
            var abiVersion = contractRoot.GetProperty("nativeAbi").GetProperty("current").GetInt32();
            var expectedPublicTypes = api.GetProperty("publicNetTypes").GetInt32();
            var expectedNativeExports = api.GetProperty("nativeExports").GetInt32();
            var expectedViewerExports = api.GetProperty("viewer").GetInt32();
            var expectedModelingExports = api.GetProperty("modeling").GetInt32();

            var inputs = DiscoverAssemblies(root, configuration, targetFramework, desktopTargetFramework);
            if (inputs.Length == 0) throw new InvalidOperationException("No public managed Bridge assemblies were discovered.");
            foreach (var input in inputs)
                if (!File.Exists(input.DllPath)) throw new FileNotFoundException($"Managed assembly was not found: {input.DllPath}");

            var resolutionDirectories = inputs.Select(input => input.OutputDirectory).ToList();
            var avaloniaSmokeOutput = Path.Combine(root, "tests", "OcctNet.AvaloniaSmoke", "bin", "x64", configuration, targetFramework);
            if (Directory.Exists(avaloniaSmokeOutput)) resolutionDirectories.Add(avaloniaSmokeOutput);

            var assemblyPaths = resolutionDirectories.SelectMany(directory => Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
                .GroupBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            AssemblyLoadContext.Default.Resolving += (_, name) =>
            {
                if (name.Name is null || !assemblyPaths.TryGetValue(name.Name, out var path)) return null;
                try { return AssemblyLoadContext.Default.LoadFromAssemblyPath(path); }
                catch (BadImageFormatException) { return null; }
            };

            var loaded = inputs.Select(input => (Input: input, Assembly: AssemblyLoadContext.Default.LoadFromAssemblyPath(input.DllPath), Docs: new XmlDocs(input.XmlPath))).ToArray();
            var publicTypeCount = loaded.Sum(item => item.Assembly.GetExportedTypes().Length);
            if (publicTypeCount != expectedPublicTypes)
                throw new InvalidOperationException($"Managed API docs found {publicTypeCount} public types; bridge-contract.json requires {expectedPublicTypes}.");

            var headers = LoadNativeHeaders(root, api);
            var functions = headers.SelectMany(ParseNativeFunctions).ToArray();
            if (functions.Length != expectedNativeExports)
                throw new InvalidOperationException($"Native API docs found {functions.Length} exports; expected {expectedNativeExports}.");
            var modeling = functions.Count(function => function.Name.StartsWith("occt_model_", StringComparison.Ordinal));
            var viewer = functions.Length - modeling;
            if (viewer != expectedViewerExports || modeling != expectedModelingExports)
                throw new InvalidOperationException($"Native API groups differ from bridge-contract.json: Viewer={viewer}/{expectedViewerExports}, Modeling={modeling}/{expectedModelingExports}.");

            WriteLanguage(root, "en-US", false, loaded, headers, functions, bridgeVersion, abiVersion);
            WriteLanguage(root, "zh-CN", true, loaded, headers, functions, bridgeVersion, abiVersion);
            Console.WriteLine($"Generated branch-aware API reference for {inputs.Length} assemblies, {publicTypeCount} public types and {functions.Length} Native exports.");
            return 0;
        }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }

    private static AssemblyInput[] DiscoverAssemblies(string root, string configuration, string targetFramework, string desktopTargetFramework)
    {
        var candidates = new[]
        {
            (Name: "OcctNet", Directory: "src/OcctNet", Framework: targetFramework),
            (Name: "OcctNet.WinForms", Directory: "src/OcctNet.WinForms", Framework: desktopTargetFramework),
            (Name: "OcctNet.Wpf", Directory: "src/OcctNet.Wpf", Framework: desktopTargetFramework),
            (Name: "OcctNet.Avalonia", Directory: "src/OcctNet.Avalonia", Framework: targetFramework)
        };
        return candidates.Where(candidate => File.Exists(Path.Combine(root, candidate.Directory.Replace('/', Path.DirectorySeparatorChar), candidate.Name + ".csproj")))
            .Select(candidate =>
            {
                var projectDirectory = Path.Combine(root, candidate.Directory.Replace('/', Path.DirectorySeparatorChar));
                var output = Path.Combine(projectDirectory, "bin", "x64", configuration, candidate.Framework);
                return new AssemblyInput(candidate.Name, projectDirectory, Path.Combine(output, candidate.Name + ".dll"), Path.Combine(output, candidate.Name + ".xml"), output);
            }).ToArray();
    }

    private static NativeHeader[] LoadNativeHeaders(string root, JsonElement api)
    {
        if (!api.TryGetProperty("nativeHeaders", out var values) || values.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("api.nativeHeaders is missing from bridge-contract.json.");
        var nativeRoot = Path.Combine(root, "src", "OcctNative");
        return values.EnumerateArray().Select(value => value.GetString() ?? string.Empty).Where(name => name.Length > 0).Select(name =>
        {
            var path = Path.Combine(nativeRoot, name);
            if (!File.Exists(path)) throw new FileNotFoundException($"Public Native header was not found: {name}", path);
            return new NativeHeader(name, File.ReadAllText(path));
        }).ToArray();
    }

    private static IEnumerable<NativeFunction> ParseNativeFunctions(NativeHeader header)
    {
        var declaration = new StringBuilder();
        var collecting = false;
        foreach (var rawLine in header.Content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = rawLine.Trim();
            if (!collecting && line.StartsWith("OCCTBRIDGE_API ", StringComparison.Ordinal)) { collecting = true; declaration.Clear(); }
            if (!collecting) continue;
            if (declaration.Length > 0) declaration.Append(' ');
            declaration.Append(line);
            if (!line.Contains(';', StringComparison.Ordinal)) continue;
            var normalized = WhitespaceRegex().Replace(declaration.ToString(), " ").Trim();
            var match = NativeFunctionRegex().Match(normalized);
            if (!match.Success) throw new InvalidOperationException($"Unable to parse Native declaration in {header.Name}: {normalized}");
            yield return new NativeFunction(header.Name, match.Groups["return"].Value.Trim(), match.Groups["name"].Value, match.Groups["params"].Value.Trim(), normalized);
            collecting = false;
        }
        if (collecting) throw new InvalidOperationException($"Unterminated Native declaration in {header.Name}.");
    }

    private static void WriteLanguage(string root, string language, bool chinese, (AssemblyInput Input, Assembly Assembly, XmlDocs Docs)[] loaded, NativeHeader[] headers, NativeFunction[] functions, string bridgeVersion, int abiVersion)
    {
        var apiRoot = Path.Combine(root, "docs", language, "api");
        var referenceRoot = Path.Combine(apiRoot, "reference");
        if (Directory.Exists(referenceRoot)) Directory.Delete(referenceRoot, true);
        Directory.CreateDirectory(referenceRoot);
        var index = new StringBuilder();
        index.AppendLine(chinese ? "# OcctCSharpBridge 完整 API 参考" : "# OcctCSharpBridge Complete API Reference").AppendLine();
        index.AppendLine(chinese ? "此目录由 `tools/OcctApiDocsGenerator` 根据当前分支实际存在的公开程序集自动生成。" : "This directory is generated by `tools/OcctApiDocsGenerator` from the public assemblies that actually exist on the current branch.").AppendLine();
        index.AppendLine($"- **Bridge:** `{bridgeVersion}`");
        index.AppendLine($"- **Native ABI:** `{abiVersion}`");
        index.AppendLine($"- **{(chinese ? "公开程序集" : "Public assemblies")}:** {string.Join(", ", loaded.Select(item => "`" + item.Input.Name + "`"))}");
        index.AppendLine($"- **{(chinese ? "公开类型" : "Public types")}:** {loaded.Sum(item => item.Assembly.GetExportedTypes().Length)}");
        index.AppendLine($"- **Native exports:** {functions.Length}").AppendLine();
        index.AppendLine(chinese ? "- [Native C ABI 完整参考](native-abi.md)" : "- [Complete Native C ABI Reference](native-abi.md)").AppendLine();
        foreach (var item in loaded)
        {
            index.AppendLine("## " + item.Input.Name).AppendLine();
            foreach (var type in item.Assembly.GetExportedTypes().OrderBy(type => type.Namespace).ThenBy(type => type.Name))
            {
                var fileName = SafeFileName(type) + ".md";
                index.AppendLine($"- [`{DisplayType(type)}`](reference/{fileName})");
                File.WriteAllText(Path.Combine(referenceRoot, fileName), RenderType(item.Input.Name, type, item.Docs, chinese), new UTF8Encoding(false));
            }
            index.AppendLine();
        }
        File.WriteAllText(Path.Combine(apiRoot, "README.md"), index.ToString(), new UTF8Encoding(false));

        var native = new StringBuilder();
        native.AppendLine(chinese ? "# OcctNative C ABI 完整参考" : "# OcctNative Complete C ABI Reference").AppendLine();
        native.AppendLine($"- **Bridge:** `{bridgeVersion}`");
        native.AppendLine($"- **Native ABI:** `{abiVersion}`");
        native.AppendLine($"- **Exports:** `{functions.Length}`").AppendLine();
        foreach (var header in headers)
        {
            var group = functions.Where(function => function.Header == header.Name).ToArray();
            if (group.Length == 0) continue;
            native.AppendLine($"## `{header.Name}`").AppendLine();
            foreach (var function in group)
            {
                native.AppendLine($"### `{function.Name}`").AppendLine();
                native.AppendLine($"- **{(chinese ? "返回" : "Returns")}:** `{function.ReturnType}`").AppendLine();
                native.AppendLine("```cpp").AppendLine(function.Declaration).AppendLine("```").AppendLine();
            }
        }
        File.WriteAllText(Path.Combine(apiRoot, "native-abi.md"), native.ToString(), new UTF8Encoding(false));
    }

    private static string RenderType(string assemblyName, Type type, XmlDocs docs, bool chinese)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# " + DisplayType(type)).AppendLine();
        builder.AppendLine($"- **{(chinese ? "程序集" : "Assembly")}:** `{assemblyName}.dll`");
        builder.AppendLine($"- **{(chinese ? "命名空间" : "Namespace")}:** `{type.Namespace ?? "(global)"}`").AppendLine();
        var description = docs.Type(type); if (!string.IsNullOrWhiteSpace(description)) builder.AppendLine(description).AppendLine();
        RenderMethods(builder, type, type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Cast<MethodBase>(), docs, chinese ? "构造函数" : "Constructors", chinese);
        RenderProperties(builder, type, type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly), docs, chinese);
        RenderEvents(builder, type, type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly), docs, chinese);
        RenderMethods(builder, type, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName).Cast<MethodBase>(), docs, chinese ? "方法" : "Methods", chinese);
        RenderFields(builder, type, type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(field => type.IsEnum ? field.IsLiteral : !field.IsSpecialName), docs, chinese);
        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static void RenderMethods(StringBuilder builder, Type type, IEnumerable<MethodBase> methods, XmlDocs docs, string title, bool chinese)
    {
        builder.AppendLine("## " + title).AppendLine(); var values = methods.OrderBy(value => value.Name).ThenBy(Signature).ToArray();
        if (values.Length == 0) { builder.AppendLine(chinese ? "无。" : "None.").AppendLine(); return; }
        foreach (var method in values) { builder.AppendLine("### `" + (method.IsConstructor ? type.Name : method.Name) + "`").AppendLine(); var summary = docs.Method(type, method); if (!string.IsNullOrWhiteSpace(summary)) builder.AppendLine(summary).AppendLine(); builder.AppendLine("```csharp").AppendLine(Signature(method)).AppendLine("```").AppendLine(); }
    }
    private static void RenderProperties(StringBuilder builder, Type type, IEnumerable<PropertyInfo> properties, XmlDocs docs, bool chinese)
    {
        builder.AppendLine("## " + (chinese ? "属性" : "Properties")).AppendLine(); var values = properties.OrderBy(value => value.Name).ToArray();
        if (values.Length == 0) { builder.AppendLine(chinese ? "无。" : "None.").AppendLine(); return; }
        foreach (var value in values) { var access = value.CanRead && value.CanWrite ? "{ get; set; }" : value.CanRead ? "{ get; }" : "{ set; }"; builder.AppendLine($"### `{value.Name}`").AppendLine(); var summary = docs.Property(type, value); if (!string.IsNullOrWhiteSpace(summary)) builder.AppendLine(summary).AppendLine(); builder.AppendLine("```csharp").AppendLine($"public {DisplayType(value.PropertyType)} {value.Name} {access}").AppendLine("```").AppendLine(); }
    }
    private static void RenderEvents(StringBuilder builder, Type type, IEnumerable<EventInfo> events, XmlDocs docs, bool chinese)
    {
        builder.AppendLine("## " + (chinese ? "事件" : "Events")).AppendLine(); var values = events.OrderBy(value => value.Name).ToArray();
        if (values.Length == 0) { builder.AppendLine(chinese ? "无。" : "None.").AppendLine(); return; }
        foreach (var value in values) { builder.AppendLine($"### `{value.Name}`").AppendLine(); var summary = docs.Event(type, value); if (!string.IsNullOrWhiteSpace(summary)) builder.AppendLine(summary).AppendLine(); builder.AppendLine("```csharp").AppendLine($"public event {DisplayType(value.EventHandlerType ?? typeof(Delegate))} {value.Name};").AppendLine("```").AppendLine(); }
    }
    private static void RenderFields(StringBuilder builder, Type type, IEnumerable<FieldInfo> fields, XmlDocs docs, bool chinese)
    {
        builder.AppendLine("## " + (chinese ? "字段 / 枚举值" : "Fields / Enum Values")).AppendLine(); var values = fields.OrderBy(value => value.Name).ToArray();
        if (values.Length == 0) { builder.AppendLine(chinese ? "无。" : "None.").AppendLine(); return; }
        foreach (var value in values) { var literal = value.IsLiteral ? " = " + Convert.ToString(value.GetRawConstantValue(), System.Globalization.CultureInfo.InvariantCulture) : string.Empty; var summary = docs.Field(type, value); builder.AppendLine($"- `{value.Name}` — `{DisplayType(value.FieldType)}`{literal}{(string.IsNullOrWhiteSpace(summary) ? string.Empty : " — " + summary)}"); } builder.AppendLine();
    }
    private static string Signature(MethodBase method)
    {
        var parameters = string.Join(", ", method.GetParameters().Select(parameter => $"{DisplayType(parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType)} {parameter.Name}"));
        if (method.IsConstructor) return $"public {method.DeclaringType?.Name}({parameters})"; var info = (MethodInfo)method; var modifier = method.IsStatic ? "static " : string.Empty; return $"public {modifier}{DisplayType(info.ReturnType)} {method.Name}({parameters})";
    }
    private static string DisplayType(Type type)
    {
        if (type.IsByRef) return DisplayType(type.GetElementType()!); if (type.IsArray) return DisplayType(type.GetElementType()!) + "[]"; var nullable = Nullable.GetUnderlyingType(type); if (nullable is not null) return DisplayType(nullable) + "?";
        var aliases = new Dictionary<Type, string> { [typeof(void)] = "void", [typeof(bool)] = "bool", [typeof(byte)] = "byte", [typeof(short)] = "short", [typeof(int)] = "int", [typeof(uint)] = "uint", [typeof(long)] = "long", [typeof(ulong)] = "ulong", [typeof(float)] = "float", [typeof(double)] = "double", [typeof(string)] = "string", [typeof(object)] = "object" };
        if (aliases.TryGetValue(type, out var alias)) return alias; if (!type.IsGenericType) return type.Name; var name = type.Name[..type.Name.IndexOf('`')]; return name + "<" + string.Join(", ", type.GetGenericArguments().Select(DisplayType)) + ">";
    }
    private static string SafeFileName(Type type) { var value = (type.FullName ?? type.Name).Replace('+', '.').Replace('`', '_'); foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_'); return value; }
    private static Dictionary<string, string> ParseArgs(string[] args) { var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); for (var i = 0; i < args.Length; i++) { if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue; var key = args[i][2..]; result[key] = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal) ? args[++i] : "true"; } return result; }
    [GeneratedRegex(@"\s+")] private static partial Regex WhitespaceRegex();
    [GeneratedRegex(@"^OCCTBRIDGE_API\s+(?<return>.+?)\s+(?<name>occt_[A-Za-z0-9_]+)\s*\((?<params>.*)\)\s*;$")] private static partial Regex NativeFunctionRegex();
}
