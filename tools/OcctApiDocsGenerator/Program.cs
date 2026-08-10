using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

internal static partial class Program
{
    private sealed record AssemblyInput(string Name, string DllPath, string XmlPath, string OutputDirectory);
    private sealed record ExceptionDoc(string Cref, string Text);
    private sealed record ApiDoc(
        string Summary,
        string Remarks,
        IReadOnlyDictionary<string, string> Parameters,
        string Returns,
        IReadOnlyList<ExceptionDoc> Exceptions)
    {
        public static readonly ApiDoc Empty = new("", "", new Dictionary<string, string>(), "", []);
    }

    private sealed class XmlDocs
    {
        private readonly Dictionary<string, List<ApiDoc>> _members = new(StringComparer.Ordinal);

        public XmlDocs(string path)
        {
            if (!File.Exists(path)) return;

            var document = XDocument.Load(path);
            foreach (var member in document.Descendants("member"))
            {
                var name = (string?)member.Attribute("name");
                if (string.IsNullOrWhiteSpace(name)) continue;

                var parameters = member.Elements("param")
                    .Where(element => element.Attribute("name") is not null)
                    .ToDictionary(
                        element => (string)element.Attribute("name")!,
                        RenderXml,
                        StringComparer.Ordinal);
                var exceptions = member.Elements("exception")
                    .Select(element => new ExceptionDoc(
                        CleanCref((string?)element.Attribute("cref") ?? "Exception"),
                        RenderXml(element)))
                    .ToArray();
                var value = new ApiDoc(
                    RenderXml(member.Element("summary")),
                    RenderXml(member.Element("remarks")),
                    parameters,
                    RenderXml(member.Element("returns")),
                    exceptions);

                if (!_members.TryGetValue(name, out var values))
                {
                    values = [];
                    _members.Add(name, values);
                }
                values.Add(value);
            }
        }

        public ApiDoc Type(Type type) => Exact("T:" + XmlTypeName(type));
        public ApiDoc Field(Type type, FieldInfo field) => Exact("F:" + XmlTypeName(type) + "." + field.Name);
        public ApiDoc Event(Type type, EventInfo value) => Exact("E:" + XmlTypeName(type) + "." + value.Name);

        public ApiDoc Property(Type type, PropertyInfo property)
        {
            var prefix = "P:" + XmlTypeName(type) + "." + property.Name;
            return Prefix(prefix, property.GetIndexParameters().Length);
        }

        public ApiDoc Method(Type type, MethodBase method)
        {
            var methodName = method.IsConstructor
                ? "#ctor"
                : method.Name + (method.IsGenericMethodDefinition ? "``" + method.GetGenericArguments().Length : "");
            var prefix = "M:" + XmlTypeName(type) + "." + methodName;
            return Prefix(prefix, method.GetParameters().Length);
        }

        private ApiDoc Prefix(string prefix, int parameterCount)
        {
            var candidates = _members
                .Where(pair => pair.Key == prefix || pair.Key.StartsWith(prefix + "(", StringComparison.Ordinal))
                .SelectMany(pair => pair.Value)
                .ToArray();
            if (candidates.Length == 0) return ApiDoc.Empty;
            return candidates.FirstOrDefault(candidate => candidate.Parameters.Count == parameterCount) ?? candidates[0];
        }

        private ApiDoc Exact(string key) => _members.TryGetValue(key, out var values) && values.Count > 0 ? values[0] : ApiDoc.Empty;
        private static string XmlTypeName(Type type) => (type.FullName ?? type.Name).Replace('+', '.');
    }

    private sealed record Language(
        string Code,
        string ApiTitle,
        string GeneratedNotice,
        string ManagedIntro,
        string NativeLink,
        string AssemblyLabel,
        string NamespaceLabel,
        string DeclarationLabel,
        string DescriptionLabel,
        string RemarksLabel,
        string ConstructorsLabel,
        string PropertiesLabel,
        string EventsLabel,
        string MethodsLabel,
        string FieldsLabel,
        string ParametersLabel,
        string ReturnsLabel,
        string ExceptionsLabel,
        string InheritedLabel,
        string NoneText,
        string TypeCountLabel,
        string MemberCountLabel,
        string FallbackTypeDescription,
        string FallbackMemberDescription,
        string NativeTitle,
        string NativeIntro,
        string NativeTypeSection,
        string NativeFunctionSection,
        string CategoryLabel,
        string ParameterNameLabel,
        string ParameterTypeLabel,
        string DirectionLabel,
        string MeaningLabel,
        string Input,
        string Output,
        string InOut,
        string Value,
        string HandleMeaning,
        string ObjectIdMeaning,
        string Utf8Meaning,
        string BufferMeaning,
        string CountMeaning,
        string GenericMeaning);

    private static readonly Language Zh = new(
        "zh-CN",
        "OcctCSharpBridge 完整 API 参考",
        "此目录由 `tools/OcctApiDocsGenerator` 自动生成。`reference/` 与 `native-abi.md` 不手工维护。",
        "本索引覆盖 Binary SDK 四个公开 .NET 程序集的全部公开类型和成员，并同时提供 Native C ABI 完整参考。",
        "[Native C ABI 完整参考](native-abi.md)",
        "程序集", "命名空间", "声明", "说明", "备注", "构造函数", "属性", "事件", "方法", "字段 / 枚举值", "参数", "返回值", "异常", "继承", "无",
        "公开类型数", "公开成员数",
        "公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。",
        "公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。",
        "OcctNative C ABI 完整参考",
        "本页从 `src/OcctNative/OcctNative.h` 生成，覆盖公开 ABI 类型和全部 `OCCTBRIDGE_API occt_*` 导出，用于 P/Invoke 对等核查、底层集成和 ABI 诊断。",
        "ABI 类型", "导出函数", "分类", "名称", "C 类型", "方向", "含义", "输入", "输出", "输入/输出", "值",
        "Native Engine/Modeling Session 句柄；只在创建它的 Native Owner 生命周期内有效。",
        "Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。",
        "UTF-8 字符串指针。`const char*` 通常为输入字符串。",
        "连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常为输出或输入/输出。",
        "数量、容量、索引、枚举、标志或状态整数；精确语义结合参数名和函数用途确定。",
        "按声明传递的 C ABI 参数。"
    );

    private static readonly Language En = new(
        "en-US",
        "OcctCSharpBridge Complete API Reference",
        "This directory is generated by `tools/OcctApiDocsGenerator`. Do not hand-edit `reference/` or `native-abi.md`.",
        "This index covers every public type and member in the four Binary SDK .NET assemblies and also provides the complete Native C ABI reference.",
        "[Complete Native C ABI Reference](native-abi.md)",
        "Assembly", "Namespace", "Declaration", "Description", "Remarks", "Constructors", "Properties", "Events", "Methods", "Fields / Enum Values", "Parameters", "Returns", "Exceptions", "Inheritance", "None",
        "Public types", "Public members",
        "Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.",
        "Public API member. Exact parameters, return type, and available XML documentation are listed below.",
        "OcctNative Complete C ABI Reference",
        "This page is generated from `src/OcctNative/OcctNative.h` and covers the public ABI types and every `OCCTBRIDGE_API occt_*` export for P/Invoke parity, low-level integration, and ABI diagnostics.",
        "ABI Types", "Exported Functions", "Category", "Name", "C type", "Direction", "Meaning", "Input", "Output", "Input/Output", "Value",
        "Native Engine/Modeling Session handle valid only for the lifetime of the native owner that created it.",
        "Bridge object ID scoped to the object registry owned by the associated native handle.",
        "UTF-8 string pointer. `const char*` is normally an input string.",
        "Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output.",
        "Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose.",
        "C ABI parameter passed according to the declaration."
    );

    private sealed record NativeFunction(string Group, string ReturnType, string Name, string Parameters, string Declaration);
    private sealed record NativeParameter(string Type, string Name);

    public static int Main(string[] args)
    {
        try
        {
            var options = ParseArgs(args);
            var root = Path.GetFullPath(options.GetValueOrDefault("repository-root") ?? Directory.GetCurrentDirectory());
            var configuration = options.GetValueOrDefault("configuration") ?? "Release";
            var contractPath = Path.Combine(root, "bridge-contract.json");
            if (!File.Exists(contractPath)) throw new FileNotFoundException("bridge-contract.json was not found.", contractPath);

            const string targetFramework = "net10.0-windows";
            var inputs = new[]
            {
                Input(root, "OcctNet", "src/OcctNet", configuration, targetFramework),
                Input(root, "OcctNet.WinForms", "src/OcctNet.WinForms", configuration, targetFramework),
                Input(root, "OcctNet.Wpf", "src/OcctNet.Wpf", configuration, targetFramework),
                Input(root, "OcctNet.Avalonia", "src/OcctNet.Avalonia", configuration, targetFramework)
            };

            foreach (var input in inputs)
            {
                if (!File.Exists(input.DllPath)) throw new FileNotFoundException($"Managed assembly was not found: {input.DllPath}");
            }

            var assemblyPaths = inputs
                .SelectMany(input => Directory.Exists(input.OutputDirectory)
                    ? Directory.EnumerateFiles(input.OutputDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    : Enumerable.Empty<string>())
                .GroupBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            AssemblyLoadContext.Default.Resolving += (_, assemblyName) =>
            {
                if (assemblyName.Name is not null && assemblyPaths.TryGetValue(assemblyName.Name, out var dependency))
                {
                    try { return AssemblyLoadContext.Default.LoadFromAssemblyPath(dependency); }
                    catch (BadImageFormatException) { return null; }
                }
                return null;
            };

            var loaded = inputs.Select(input => (
                Input: input,
                Assembly: AssemblyLoadContext.Default.LoadFromAssemblyPath(input.DllPath),
                Docs: new XmlDocs(input.XmlPath))).ToArray();

            using var contract = JsonDocument.Parse(File.ReadAllText(contractPath));
            var expectedNativeExports = contract.RootElement.GetProperty("api").GetProperty("nativeExports").GetInt32();
            var expectedPublicTypes = contract.RootElement.GetProperty("api").GetProperty("publicNetTypes").GetInt32();
            var bridgeVersion = contract.RootElement.GetProperty("bridgeVersion").GetString() ?? string.Empty;
            var abiVersion = contract.RootElement.GetProperty("nativeAbiVersion").GetInt32();
            var publicTypes = loaded.Sum(item => item.Assembly.GetExportedTypes().Length);
            if (publicTypes != expectedPublicTypes)
                throw new InvalidOperationException($"Managed API docs found {publicTypes} public types; bridge-contract.json requires {expectedPublicTypes}.");

            var headerPath = Path.Combine(root, "src", "OcctNative", "OcctNative.h");
            if (!File.Exists(headerPath)) throw new FileNotFoundException("OcctNative.h was not found.", headerPath);
            var header = File.ReadAllText(headerPath);
            var nativeFunctions = ParseNativeFunctions(header);
            if (nativeFunctions.Count != expectedNativeExports)
                throw new InvalidOperationException($"Native API docs found {nativeFunctions.Count} exports; bridge-contract.json requires {expectedNativeExports}.");

            WriteManagedLanguage(root, loaded, Zh);
            WriteManagedLanguage(root, loaded, En);
            WriteNativeLanguage(root, Zh, bridgeVersion, abiVersion, expectedNativeExports, header, nativeFunctions);
            WriteNativeLanguage(root, En, bridgeVersion, abiVersion, expectedNativeExports, header, nativeFunctions);

            Console.WriteLine($"Generated bilingual API reference for {publicTypes} public .NET types and {nativeFunctions.Count} Native C exports.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static AssemblyInput Input(string root, string name, string relativeProjectDirectory, string configuration, string targetFramework)
    {
        var output = Path.Combine(root, relativeProjectDirectory.Replace('/', Path.DirectorySeparatorChar), "bin", "x64", configuration, targetFramework);
        return new AssemblyInput(name, Path.Combine(output, name + ".dll"), Path.Combine(output, name + ".xml"), output);
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
            var key = args[i][2..];
            var value = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal) ? args[++i] : "true";
            result[key] = value;
        }
        return result;
    }

    private static void WriteManagedLanguage(string root, (AssemblyInput Input, Assembly Assembly, XmlDocs Docs)[] loaded, Language language)
    {
        var apiRoot = Path.Combine(root, "docs", language.Code, "api");
        var referenceRoot = Path.Combine(apiRoot, "reference");
        if (Directory.Exists(referenceRoot)) Directory.Delete(referenceRoot, true);
        Directory.CreateDirectory(referenceRoot);

        var index = new StringBuilder();
        index.AppendLine("# " + language.ApiTitle).AppendLine();
        index.AppendLine(language.GeneratedNotice).AppendLine();
        index.AppendLine(language.ManagedIntro).AppendLine();
        index.AppendLine("- " + language.NativeLink).AppendLine();

        var totalTypes = 0;
        var totalMembers = 0;
        foreach (var item in loaded)
        {
            var types = item.Assembly.GetExportedTypes()
                .Where(type => type.IsPublic || type.IsNestedPublic)
                .OrderBy(type => type.Namespace, StringComparer.Ordinal)
                .ThenBy(type => type.Name, StringComparer.Ordinal)
                .ToArray();

            totalTypes += types.Length;
            index.AppendLine("## " + item.Input.Name).AppendLine();
            foreach (var type in types)
            {
                var members = CountMembers(type);
                totalMembers += members;
                var file = SafeFileName(type) + ".md";
                index.AppendLine($"- [`{DisplayType(type)}`](reference/{file}) — {members}");
                File.WriteAllText(Path.Combine(referenceRoot, file), RenderType(item.Input.Name, type, item.Docs, language), new UTF8Encoding(false));
            }
            index.AppendLine();
        }

        var summary = $"- **{language.TypeCountLabel}:** {totalTypes}{Environment.NewLine}- **{language.MemberCountLabel}:** {totalMembers}{Environment.NewLine}{Environment.NewLine}";
        var marker = language.ManagedIntro + Environment.NewLine + Environment.NewLine;
        index.Replace(marker, marker + summary);
        Directory.CreateDirectory(apiRoot);
        File.WriteAllText(Path.Combine(apiRoot, "README.md"), index.ToString(), new UTF8Encoding(false));
    }

    private static string RenderType(string assemblyName, Type type, XmlDocs docs, Language language)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# " + DisplayType(type)).AppendLine();
        builder.AppendLine($"- **{language.AssemblyLabel}:** `{assemblyName}.dll`");
        builder.AppendLine($"- **{language.NamespaceLabel}:** `{type.Namespace ?? "(global)"}`");
        if (type.BaseType is not null && type.BaseType != typeof(object) && !type.IsEnum)
            builder.AppendLine($"- **{language.InheritedLabel}:** `{DisplayType(type.BaseType)}`");
        builder.AppendLine();
        builder.AppendLine("## " + language.DeclarationLabel).AppendLine();
        builder.AppendLine("```csharp").AppendLine(TypeDeclaration(type)).AppendLine("```").AppendLine();
        AppendDoc(builder, docs.Type(type), language, language.FallbackTypeDescription, includeReturns: false);

        if (type.IsEnum)
        {
            RenderFields(builder, type, docs, language, enumOnly: true);
            return builder.ToString();
        }

        RenderConstructors(builder, type, docs, language);
        RenderProperties(builder, type, docs, language);
        RenderEvents(builder, type, docs, language);
        RenderMethods(builder, type, docs, language);
        RenderFields(builder, type, docs, language, enumOnly: false);
        return builder.ToString();
    }

    private static void AppendDoc(StringBuilder builder, ApiDoc doc, Language language, string fallback, bool includeReturns)
    {
        builder.AppendLine("## " + language.DescriptionLabel).AppendLine();
        builder.AppendLine(string.IsNullOrWhiteSpace(doc.Summary) ? fallback : doc.Summary).AppendLine();
        if (!string.IsNullOrWhiteSpace(doc.Remarks))
            builder.AppendLine("**" + language.RemarksLabel + ":** " + doc.Remarks).AppendLine();
        if (includeReturns && !string.IsNullOrWhiteSpace(doc.Returns))
            builder.AppendLine("**" + language.ReturnsLabel + ":** " + doc.Returns).AppendLine();
        if (doc.Exceptions.Count > 0)
        {
            builder.AppendLine("**" + language.ExceptionsLabel + "**").AppendLine();
            foreach (var exception in doc.Exceptions)
                builder.AppendLine($"- `{exception.Cref}` — {exception.Text}");
            builder.AppendLine();
        }
    }

    private static void RenderConstructors(StringBuilder builder, Type type, XmlDocs docs, Language language)
    {
        var values = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).OrderBy(Signature).Cast<MethodBase>().ToArray();
        RenderMethodsCore(builder, type, values, docs, language, language.ConstructorsLabel);
    }

    private static void RenderMethods(StringBuilder builder, Type type, XmlDocs docs, Language language)
    {
        var values = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ThenBy(Signature)
            .Cast<MethodBase>()
            .ToArray();
        RenderMethodsCore(builder, type, values, docs, language, language.MethodsLabel);
    }

    private static void RenderMethodsCore(StringBuilder builder, Type type, MethodBase[] values, XmlDocs docs, Language language, string heading)
    {
        builder.AppendLine("## " + heading).AppendLine();
        if (values.Length == 0) { builder.AppendLine(language.NoneText).AppendLine(); return; }

        foreach (var value in values)
        {
            var doc = docs.Method(type, value);
            builder.AppendLine("### `" + MemberTitle(value) + "`").AppendLine();
            builder.AppendLine(string.IsNullOrWhiteSpace(doc.Summary) ? language.FallbackMemberDescription : doc.Summary).AppendLine();
            if (!string.IsNullOrWhiteSpace(doc.Remarks)) builder.AppendLine("**" + language.RemarksLabel + ":** " + doc.Remarks).AppendLine();
            builder.AppendLine("```csharp").AppendLine(Signature(value)).AppendLine("```").AppendLine();

            var parameters = value.GetParameters();
            if (parameters.Length > 0)
            {
                builder.AppendLine("**" + language.ParametersLabel + "**").AppendLine();
                foreach (var parameter in parameters)
                {
                    doc.Parameters.TryGetValue(parameter.Name ?? "", out var description);
                    var suffix = string.IsNullOrWhiteSpace(description) ? "" : " — " + EscapeTable(description);
                    builder.AppendLine($"- `{parameter.Name}` — `{ParameterType(parameter)}`{DefaultValue(parameter)}{suffix}");
                }
                builder.AppendLine();
            }

            if (value is MethodInfo method)
            {
                var returnDescription = string.IsNullOrWhiteSpace(doc.Returns) ? "" : " — " + doc.Returns;
                builder.AppendLine($"**{language.ReturnsLabel}:** `{DisplayType(method.ReturnType)}`{returnDescription}").AppendLine();
            }

            if (doc.Exceptions.Count > 0)
            {
                builder.AppendLine("**" + language.ExceptionsLabel + "**").AppendLine();
                foreach (var exception in doc.Exceptions)
                    builder.AppendLine($"- `{exception.Cref}` — {exception.Text}");
                builder.AppendLine();
            }
        }
    }

    private static void RenderProperties(StringBuilder builder, Type type, XmlDocs docs, Language language)
    {
        var values = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal).ToArray();
        builder.AppendLine("## " + language.PropertiesLabel).AppendLine();
        if (values.Length == 0) { builder.AppendLine(language.NoneText).AppendLine(); return; }

        foreach (var value in values)
        {
            var doc = docs.Property(type, value);
            var access = (value.CanRead, value.CanWrite) switch { (true, true) => "{ get; set; }", (true, false) => "{ get; }", _ => "{ set; }" };
            var indexParameters = value.GetIndexParameters();
            var name = indexParameters.Length == 0
                ? value.Name
                : "this[" + string.Join(", ", indexParameters.Select(ParameterDeclaration)) + "]";
            builder.AppendLine("### `" + value.Name + "`").AppendLine();
            builder.AppendLine(string.IsNullOrWhiteSpace(doc.Summary) ? language.FallbackMemberDescription : doc.Summary).AppendLine();
            builder.AppendLine("```csharp").AppendLine($"public {DisplayType(value.PropertyType)} {name} {access}").AppendLine("```").AppendLine();
            if (!string.IsNullOrWhiteSpace(doc.Remarks)) builder.AppendLine("**" + language.RemarksLabel + ":** " + doc.Remarks).AppendLine();
        }
    }

    private static void RenderEvents(StringBuilder builder, Type type, XmlDocs docs, Language language)
    {
        var values = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(value => value.Name, StringComparer.Ordinal).ToArray();
        builder.AppendLine("## " + language.EventsLabel).AppendLine();
        if (values.Length == 0) { builder.AppendLine(language.NoneText).AppendLine(); return; }

        foreach (var value in values)
        {
            var doc = docs.Event(type, value);
            builder.AppendLine("### `" + value.Name + "`").AppendLine();
            builder.AppendLine(string.IsNullOrWhiteSpace(doc.Summary) ? language.FallbackMemberDescription : doc.Summary).AppendLine();
            builder.AppendLine("```csharp").AppendLine($"public event {DisplayType(value.EventHandlerType ?? typeof(Delegate))} {value.Name};").AppendLine("```").AppendLine();
        }
    }

    private static void RenderFields(StringBuilder builder, Type type, XmlDocs docs, Language language, bool enumOnly)
    {
        var values = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(field => enumOnly ? field.IsLiteral : !field.IsSpecialName)
            .OrderBy(field => field.Name, StringComparer.Ordinal).ToArray();
        builder.AppendLine("## " + language.FieldsLabel).AppendLine();
        if (values.Length == 0) { builder.AppendLine(language.NoneText).AppendLine(); return; }

        foreach (var value in values)
        {
            var doc = docs.Field(type, value);
            var literal = value.IsLiteral ? $" = {value.GetRawConstantValue()}" : string.Empty;
            var description = string.IsNullOrWhiteSpace(doc.Summary) ? language.FallbackMemberDescription : doc.Summary;
            builder.AppendLine($"- `{value.Name}` — `{DisplayType(value.FieldType)}`{literal} — {description}");
        }
        builder.AppendLine();
    }

    private static List<NativeFunction> ParseNativeFunctions(string header)
    {
        var functions = new List<NativeFunction>();
        var group = "General";
        var declaration = new StringBuilder();
        var collecting = false;

        foreach (var rawLine in header.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (!collecting && line.StartsWith("//", StringComparison.Ordinal))
            {
                var text = line[2..].Trim().TrimEnd('.');
                if (!string.IsNullOrWhiteSpace(text)) group = text;
                continue;
            }
            if (!collecting && line.StartsWith("OCCTBRIDGE_API ", StringComparison.Ordinal))
            {
                collecting = true;
                declaration.Clear();
            }
            if (!collecting) continue;

            if (declaration.Length > 0) declaration.Append(' ');
            declaration.Append(line);
            if (!line.Contains(';')) continue;

            var normalized = WhitespaceRegex().Replace(declaration.ToString(), " ").Trim();
            var match = NativeFunctionRegex().Match(normalized);
            if (!match.Success) throw new InvalidOperationException($"Unable to parse Native ABI declaration: {normalized}");
            functions.Add(new NativeFunction(group, match.Groups["return"].Value.Trim(), match.Groups["name"].Value, match.Groups["params"].Value.Trim(), normalized));
            collecting = false;
        }
        return functions;
    }

    private static void WriteNativeLanguage(string root, Language language, string bridgeVersion, int abiVersion, int exportCount, string header, IReadOnlyList<NativeFunction> functions)
    {
        var apiRoot = Path.Combine(root, "docs", language.Code, "api");
        Directory.CreateDirectory(apiRoot);
        var builder = new StringBuilder();
        builder.AppendLine("# " + language.NativeTitle).AppendLine();
        builder.AppendLine(language.NativeIntro).AppendLine();
        builder.AppendLine($"- **Bridge:** `{bridgeVersion}`");
        builder.AppendLine($"- **Native ABI:** `{abiVersion}`");
        builder.AppendLine($"- **Exports:** `{exportCount}`").AppendLine();
        builder.AppendLine("## " + language.NativeTypeSection).AppendLine();
        builder.AppendLine("```cpp").AppendLine(ExtractNativeTypeBlock(header)).AppendLine("```").AppendLine();
        builder.AppendLine("## " + language.NativeFunctionSection).AppendLine();

        string? currentGroup = null;
        foreach (var function in functions)
        {
            if (!string.Equals(currentGroup, function.Group, StringComparison.Ordinal))
            {
                currentGroup = function.Group;
                builder.AppendLine("### " + currentGroup).AppendLine();
            }

            builder.AppendLine("#### `" + function.Name + "`").AppendLine();
            builder.AppendLine($"- **{language.CategoryLabel}:** {function.Group}");
            builder.AppendLine($"- **{language.ReturnsLabel}:** `{function.ReturnType}`").AppendLine();
            builder.AppendLine("```cpp").AppendLine(function.Declaration).AppendLine("```").AppendLine();
            var parameters = ParseNativeParameters(function.Parameters);
            if (parameters.Count == 0)
            {
                builder.AppendLine("**" + language.ParametersLabel + ":** " + language.NoneText).AppendLine();
                continue;
            }

            builder.AppendLine($"| {language.ParameterNameLabel} | {language.ParameterTypeLabel} | {language.DirectionLabel} | {language.MeaningLabel} |");
            builder.AppendLine("|---|---|---|---|");
            foreach (var parameter in parameters)
                builder.AppendLine($"| `{parameter.Name}` | `{parameter.Type}` | {NativeDirection(parameter.Type, language)} | {NativeMeaning(parameter, language)} |");
            builder.AppendLine();
        }

        File.WriteAllText(Path.Combine(apiRoot, "native-abi.md"), builder.ToString(), new UTF8Encoding(false));
    }

    private static string ExtractNativeTypeBlock(string header)
    {
        var start = header.IndexOf("using OcctHandle", StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        var remainder = header[start..];
        var firstExport = NativeExportLineRegex().Match(remainder);
        if (!firstExport.Success) return string.Empty;
        return remainder[..firstExport.Index].Trim();
    }

    private static List<NativeParameter> ParseNativeParameters(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "void") return [];
        var result = new List<NativeParameter>();
        foreach (var raw in value.Split(','))
        {
            var parameter = WhitespaceRegex().Replace(raw.Trim(), " ");
            var match = NativeParameterRegex().Match(parameter);
            result.Add(match.Success
                ? new NativeParameter(match.Groups["type"].Value.Trim(), match.Groups["name"].Value.Trim())
                : new NativeParameter(parameter, "?"));
        }
        return result;
    }

    private static string NativeDirection(string type, Language language)
    {
        if (!type.Contains('*')) return language.Value;
        if (type.Contains("const", StringComparison.Ordinal)) return language.Input;
        if (type.Contains("char", StringComparison.Ordinal)) return language.InOut;
        return language.Output;
    }

    private static string NativeMeaning(NativeParameter parameter, Language language)
    {
        if (parameter.Type.Contains("OcctHandle", StringComparison.Ordinal)) return language.HandleMeaning;
        if (parameter.Type.Contains("OcctObjectId", StringComparison.Ordinal) || parameter.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || parameter.Name.EndsWith("Ids", StringComparison.OrdinalIgnoreCase)) return language.ObjectIdMeaning;
        if (parameter.Type.Contains("char", StringComparison.Ordinal)) return language.Utf8Meaning;
        if (parameter.Type.Contains('*')) return language.BufferMeaning;
        if (parameter.Name.Contains("count", StringComparison.OrdinalIgnoreCase) || parameter.Name.Contains("capacity", StringComparison.OrdinalIgnoreCase) || parameter.Name.Contains("index", StringComparison.OrdinalIgnoreCase) || parameter.Type == "int") return language.CountMeaning;
        return language.GenericMeaning;
    }

    private static int CountMembers(Type type) =>
        type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length +
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length +
        type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length +
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Count(method => !method.IsSpecialName) +
        type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Count(field => type.IsEnum ? field.IsLiteral : !field.IsSpecialName);

    private static string TypeDeclaration(Type type)
    {
        if (type.IsEnum) return $"public enum {DisplayType(type)}";
        if (type.IsInterface) return $"public interface {DisplayType(type)}";
        if (type.IsValueType) return $"public {(type.IsByRefLike ? "ref " : string.Empty)}struct {DisplayType(type)}";
        var modifier = type.IsAbstract && type.IsSealed ? "static " : type.IsAbstract ? "abstract " : type.IsSealed ? "sealed " : string.Empty;
        return $"public {modifier}class {DisplayType(type)}";
    }

    private static string Signature(MethodBase method)
    {
        var modifiers = method.IsStatic ? "static " : method.IsAbstract ? "abstract " : method.IsVirtual && !method.IsFinal ? "virtual " : string.Empty;
        var parameters = string.Join(", ", method.GetParameters().Select(ParameterDeclaration));
        if (method.IsConstructor) return $"public {method.DeclaringType?.Name}({parameters})";
        var info = (MethodInfo)method;
        var generic = method.IsGenericMethodDefinition ? "<" + string.Join(", ", method.GetGenericArguments().Select(argument => argument.Name)) + ">" : string.Empty;
        return $"public {modifiers}{DisplayType(info.ReturnType)} {method.Name}{generic}({parameters})";
    }

    private static string ParameterDeclaration(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        var prefix = parameter.GetCustomAttribute<ParamArrayAttribute>() is not null ? "params "
            : parameter.IsOut ? "out "
            : type.IsByRef && parameter.IsIn ? "in "
            : type.IsByRef ? "ref "
            : string.Empty;
        if (type.IsByRef) type = type.GetElementType()!;
        return $"{prefix}{DisplayType(type)} {parameter.Name}{DefaultValue(parameter)}";
    }

    private static string ParameterType(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        var prefix = parameter.IsOut ? "out " : type.IsByRef && parameter.IsIn ? "in " : type.IsByRef ? "ref " : string.Empty;
        if (type.IsByRef) type = type.GetElementType()!;
        return prefix + DisplayType(type);
    }

    private static string MemberTitle(MethodBase method) => method.IsConstructor ? method.DeclaringType?.Name ?? "ctor" : method.Name;

    private static string DefaultValue(ParameterInfo parameter)
    {
        if (!parameter.HasDefaultValue) return string.Empty;
        if (parameter.DefaultValue is null) return " = null";
        if (parameter.DefaultValue is string text) return " = \"" + text.Replace("\"", "\\\"") + "\"";
        if (parameter.DefaultValue is bool flag) return flag ? " = true" : " = false";
        if (parameter.DefaultValue is char character) return " = '" + character + "'";
        if (parameter.DefaultValue is Enum enumValue) return " = " + enumValue.GetType().Name + "." + enumValue;
        return " = " + Convert.ToString(parameter.DefaultValue, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string DisplayType(Type type)
    {
        if (type.IsByRef) return DisplayType(type.GetElementType()!);
        if (type.IsArray) return DisplayType(type.GetElementType()!) + "[]";
        if (type.IsGenericParameter) return type.Name;
        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable is not null) return DisplayType(nullable) + "?";
        var aliases = new Dictionary<Type, string>
        {
            [typeof(void)] = "void", [typeof(bool)] = "bool", [typeof(byte)] = "byte", [typeof(sbyte)] = "sbyte",
            [typeof(short)] = "short", [typeof(ushort)] = "ushort", [typeof(int)] = "int", [typeof(uint)] = "uint",
            [typeof(long)] = "long", [typeof(ulong)] = "ulong", [typeof(float)] = "float", [typeof(double)] = "double",
            [typeof(decimal)] = "decimal", [typeof(char)] = "char", [typeof(string)] = "string", [typeof(object)] = "object"
        };
        if (aliases.TryGetValue(type, out var alias)) return alias;
        if (!type.IsGenericType) return type.Name;
        var name = type.Name[..type.Name.IndexOf('`')];
        return name + "<" + string.Join(", ", type.GetGenericArguments().Select(DisplayType)) + ">";
    }

    private static string SafeFileName(Type type)
    {
        var value = (type.FullName ?? type.Name).Replace('+', '.');
        foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
        return value.Replace('`', '_');
    }

    private static string RenderXml(XElement? element)
    {
        if (element is null) return string.Empty;
        var builder = new StringBuilder();
        foreach (var node in element.Nodes()) RenderNode(node, builder);
        return NormalizeWhitespace(builder.ToString());
    }

    private static void RenderNode(XNode node, StringBuilder builder)
    {
        switch (node)
        {
            case XText text:
                builder.Append(text.Value);
                break;
            case XElement element when element.Name.LocalName == "see":
                builder.Append(CleanCref((string?)element.Attribute("cref") ?? (string?)element.Attribute("langword") ?? ""));
                break;
            case XElement element when element.Name.LocalName == "paramref":
                builder.Append((string?)element.Attribute("name") ?? "");
                break;
            case XElement element when element.Name.LocalName is "c" or "code":
                builder.Append('`').Append(NormalizeWhitespace(element.Value)).Append('`');
                break;
            case XElement element:
                foreach (var child in element.Nodes()) RenderNode(child, builder);
                break;
        }
    }

    private static string CleanCref(string value)
    {
        var colon = value.IndexOf(':');
        return colon >= 0 ? value[(colon + 1)..].Replace('{', '<').Replace('}', '>') : value;
    }

    private static string NormalizeWhitespace(string value) => WhitespaceRegex().Replace(value, " ").Trim();
    private static string EscapeTable(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"^OCCTBRIDGE_API\s+(?<return>.+?)\s+(?<name>occt_[A-Za-z0-9_]+)\s*\((?<params>.*)\)\s*;$")]
    private static partial Regex NativeFunctionRegex();

    [GeneratedRegex(@"^(?<type>.+?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)$")]
    private static partial Regex NativeParameterRegex();

    [GeneratedRegex(@"(?m)^\s*OCCTBRIDGE_API\s+.+\bocct_[A-Za-z0-9_]+\s*\(")]
    private static partial Regex NativeExportLineRegex();
}
