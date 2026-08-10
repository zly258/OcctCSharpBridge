using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static partial class Program
{
    private sealed record Function(string Group, string ReturnType, string Name, string Parameters, string Declaration);
    private sealed record Parameter(string Type, string Name);

    private sealed record Language(
        string Code,
        string Title,
        string Intro,
        string ContractLabel,
        string TypeSection,
        string FunctionSection,
        string CategoryLabel,
        string DeclarationLabel,
        string ReturnLabel,
        string ParametersLabel,
        string ParameterNameLabel,
        string ParameterTypeLabel,
        string DirectionLabel,
        string MeaningLabel,
        string NoParameters,
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
        "OcctNative C ABI 完整参考",
        "本页由 `src/OcctNative/OcctNative.h` 自动生成，覆盖公开 Native C ABI 类型声明和全部 `OCCTBRIDGE_API occt_*` 导出。它用于 ABI 对接、P/Invoke 核对和底层诊断；常规 C# 调用优先使用 Managed API Reference。",
        "契约导出数量", "ABI 类型", "导出函数", "分类", "声明", "返回类型", "参数", "名称", "C 类型", "方向", "含义", "无参数", "输入", "输出", "输入/输出", "值",
        "Native Engine/Modeling Session 句柄；必须由对应创建 API 获得，并遵守所属实例生命周期。",
        "Bridge 对象 ID；只在所属 Native Handle 的对象注册表中有意义。",
        "UTF-8 字符串指针。`const char*` 为输入；非 const 字符缓冲区按具体函数契约使用。",
        "连续缓冲区指针。`const` 指针通常为输入数组，非 const 指针通常用于输出或输入/输出。",
        "数量、容量、索引或枚举/状态整数；具体语义由参数名和函数用途共同确定。",
        "按声明传递的 C ABI 参数。"
    );

    private static readonly Language En = new(
        "en-US",
        "OcctNative Complete C ABI Reference",
        "This page is generated from `src/OcctNative/OcctNative.h`. It covers the public Native C ABI type declarations and every `OCCTBRIDGE_API occt_*` export. Use it for ABI integration, P/Invoke parity, and low-level diagnostics; normal C# consumers should prefer the Managed API Reference.",
        "Contract export count", "ABI Types", "Exported Functions", "Category", "Declaration", "Return type", "Parameters", "Name", "C type", "Direction", "Meaning", "No parameters", "Input", "Output", "Input/Output", "Value",
        "Native Engine/Modeling Session handle obtained from the corresponding creation API and valid only for that owner lifetime.",
        "Bridge object ID scoped to the object registry owned by the associated native handle.",
        "UTF-8 string pointer. `const char*` is input; a non-const character buffer follows the specific function contract.",
        "Contiguous buffer pointer. A `const` pointer is normally input; a non-const pointer is normally output or input/output.",
        "Count, capacity, index, enum, flag, or status integer; exact semantics follow the parameter name and function purpose.",
        "C ABI parameter passed according to the declaration."
    );

    public static int Main(string[] args)
    {
        try
        {
            var root = ResolveRepositoryRoot(args);
            var headerPath = Path.Combine(root, "src", "OcctNative", "OcctNative.h");
            var contractPath = Path.Combine(root, "bridge-contract.json");
            if (!File.Exists(headerPath)) throw new FileNotFoundException("OcctNative.h was not found.", headerPath);
            if (!File.Exists(contractPath)) throw new FileNotFoundException("bridge-contract.json was not found.", contractPath);

            var header = File.ReadAllText(headerPath);
            using var contract = JsonDocument.Parse(File.ReadAllText(contractPath));
            var expectedExports = contract.RootElement.GetProperty("api").GetProperty("nativeExports").GetInt32();
            var abiVersion = contract.RootElement.GetProperty("nativeAbiVersion").GetInt32();
            var bridgeVersion = contract.RootElement.GetProperty("bridgeVersion").GetString() ?? string.Empty;

            var functions = ParseFunctions(header);
            if (functions.Count != expectedExports)
            {
                throw new InvalidOperationException($"Native ABI documentation parser found {functions.Count} exports, but bridge-contract.json requires {expectedExports}.");
            }

            var typeBlock = ExtractTypeBlock(header);
            Write(root, Zh, bridgeVersion, abiVersion, expectedExports, typeBlock, functions);
            Write(root, En, bridgeVersion, abiVersion, expectedExports, typeBlock, functions);

            Console.WriteLine($"Generated bilingual Native C ABI reference for {functions.Count} exports.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static string ResolveRepositoryRoot(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--repository-root", StringComparison.OrdinalIgnoreCase))
                return Path.GetFullPath(args[i + 1]);
        }
        return Directory.GetCurrentDirectory();
    }

    private static List<Function> ParseFunctions(string header)
    {
        var functions = new List<Function>();
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

            if (!collecting && line.Contains("OCCTBRIDGE_API", StringComparison.Ordinal))
            {
                collecting = true;
                declaration.Clear();
            }

            if (!collecting) continue;
            if (declaration.Length > 0) declaration.Append(' ');
            declaration.Append(line);

            if (!line.Contains(';')) continue;

            var normalized = WhitespaceRegex().Replace(declaration.ToString(), " ").Trim();
            var match = FunctionRegex().Match(normalized);
            if (!match.Success)
                throw new InvalidOperationException($"Unable to parse Native ABI declaration: {normalized}");

            functions.Add(new Function(
                group,
                match.Groups["return"].Value.Trim(),
                match.Groups["name"].Value.Trim(),
                match.Groups["params"].Value.Trim(),
                normalized));
            collecting = false;
        }

        return functions;
    }

    private static string ExtractTypeBlock(string header)
    {
        var start = header.IndexOf("using OcctHandle", StringComparison.Ordinal);
        var firstExport = header.IndexOf("OCCTBRIDGE_API", StringComparison.Ordinal);
        if (start < 0 || firstExport <= start) return "";

        var block = header[start..firstExport].Trim();
        return block.TrimEnd('{', '}', ' ', '\r', '\n', '\t');
    }

    private static void Write(
        string root,
        Language language,
        string bridgeVersion,
        int abiVersion,
        int expectedExports,
        string typeBlock,
        IReadOnlyList<Function> functions)
    {
        var apiRoot = Path.Combine(root, "docs", language.Code, "api");
        Directory.CreateDirectory(apiRoot);
        var path = Path.Combine(apiRoot, "native-abi.md");
        var builder = new StringBuilder();

        builder.AppendLine("# " + language.Title).AppendLine();
        builder.AppendLine(language.Intro).AppendLine();
        builder.AppendLine($"- **Bridge:** `{bridgeVersion}`");
        builder.AppendLine($"- **Native ABI:** `{abiVersion}`");
        builder.AppendLine($"- **{language.ContractLabel}:** `{expectedExports}`").AppendLine();

        builder.AppendLine("## " + language.TypeSection).AppendLine();
        builder.AppendLine("```cpp");
        builder.AppendLine(typeBlock);
        builder.AppendLine("```").AppendLine();

        builder.AppendLine("## " + language.FunctionSection).AppendLine();
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
            builder.AppendLine($"- **{language.ReturnLabel}:** `{function.ReturnType}`").AppendLine();
            builder.AppendLine("**" + language.DeclarationLabel + "**").AppendLine();
            builder.AppendLine("```cpp").AppendLine(function.Declaration).AppendLine("```").AppendLine();

            var parameters = ParseParameters(function.Parameters);
            builder.AppendLine("**" + language.ParametersLabel + "**").AppendLine();
            if (parameters.Count == 0)
            {
                builder.AppendLine(language.NoParameters).AppendLine();
                continue;
            }

            builder.AppendLine($"| {language.ParameterNameLabel} | {language.ParameterTypeLabel} | {language.DirectionLabel} | {language.MeaningLabel} |");
            builder.AppendLine("|---|---|---|---|");
            foreach (var parameter in parameters)
            {
                var direction = Direction(parameter.Type, language);
                var meaning = Meaning(parameter, language);
                builder.AppendLine($"| `{parameter.Name}` | `{parameter.Type}` | {direction} | {meaning} |");
            }
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
        AppendNativeLink(Path.Combine(apiRoot, "README.md"), language);
    }

    private static List<Parameter> ParseParameters(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "void") return [];

        var result = new List<Parameter>();
        foreach (var raw in value.Split(','))
        {
            var parameter = WhitespaceRegex().Replace(raw.Trim(), " ");
            var match = ParameterRegex().Match(parameter);
            if (!match.Success)
            {
                result.Add(new Parameter(parameter, "?"));
                continue;
            }
            result.Add(new Parameter(match.Groups["type"].Value.Trim(), match.Groups["name"].Value.Trim()));
        }
        return result;
    }

    private static string Direction(string type, Language language)
    {
        if (!type.Contains('*')) return language.Value;
        if (type.Contains("const", StringComparison.Ordinal)) return language.Input;
        if (type.Contains("char", StringComparison.Ordinal)) return language.InOut;
        return language.Output;
    }

    private static string Meaning(Parameter parameter, Language language)
    {
        var type = parameter.Type;
        var name = parameter.Name;
        if (type.Contains("OcctHandle", StringComparison.Ordinal)) return language.HandleMeaning;
        if (type.Contains("OcctObjectId", StringComparison.Ordinal) || name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || name.EndsWith("Ids", StringComparison.OrdinalIgnoreCase)) return language.ObjectIdMeaning;
        if (type.Contains("char", StringComparison.Ordinal)) return language.Utf8Meaning;
        if (type.Contains('*')) return language.BufferMeaning;
        if (name.Contains("count", StringComparison.OrdinalIgnoreCase) || name.Contains("capacity", StringComparison.OrdinalIgnoreCase) || name.Contains("index", StringComparison.OrdinalIgnoreCase) || type == "int") return language.CountMeaning;
        return language.GenericMeaning;
    }

    private static void AppendNativeLink(string indexPath, Language language)
    {
        if (!File.Exists(indexPath)) return;
        var text = File.ReadAllText(indexPath);
        if (text.Contains("(native-abi.md)", StringComparison.Ordinal)) return;

        var label = language.Code == "zh-CN" ? "[Native C ABI 完整参考](native-abi.md)" : "[Complete Native C ABI Reference](native-abi.md)";
        File.AppendAllText(indexPath, $"{Environment.NewLine}## Native C ABI{Environment.NewLine}{Environment.NewLine}- {label}{Environment.NewLine}", new UTF8Encoding(false));
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"^OCCTBRIDGE_API\s+(?<return>.+?)\s+(?<name>occt_[A-Za-z0-9_]+)\s*\((?<params>.*)\)\s*;$")]
    private static partial Regex FunctionRegex();

    [GeneratedRegex(@"^(?<type>.+?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)$")]
    private static partial Regex ParameterRegex();
}
