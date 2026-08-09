using System.Globalization;
using System.Reflection;
using System.Text;
using OcctNet;

internal static class PublicApiSnapshot
{
    internal static void Validate()
    {
        var assemblies = new[]
        {
            typeof(OcctEngine).Assembly,
            typeof(OcctViewportControl).Assembly,
            typeof(OcctWpfViewport).Assembly,
            typeof(OcctAvaloniaViewport).Assembly
        }.Distinct().OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal).ToArray();

        var actual = Generate(assemblies).Replace("\r\n", "\n").TrimEnd() + "\n";
        var baselinePath = Path.Combine(AppContext.BaseDirectory, "PublicApi.approved.txt");
        if (!File.Exists(baselinePath))
            throw new InvalidOperationException($"Public API snapshot baseline is missing: {baselinePath}");

        var expected = File.ReadAllText(baselinePath).Replace("\r\n", "\n").TrimEnd() + "\n";
        if (string.Equals(expected, actual, StringComparison.Ordinal))
            return;

        Console.Error.WriteLine("----- BEGIN GENERATED PUBLIC API SNAPSHOT -----");
        Console.Error.Write(actual);
        Console.Error.WriteLine("----- END GENERATED PUBLIC API SNAPSHOT -----");
        throw new InvalidOperationException("Public managed API signature snapshot changed. Review the generated snapshot and update PublicApi.approved.txt only for intentional API changes.");
    }

    private static string Generate(IEnumerable<Assembly> assemblies)
    {
        var lines = new List<string>
        {
            "# OcctCSharpBridge public managed API signature snapshot",
            "# Includes primary and compatibility public APIs; generated deterministically by OcctNet.ManagedTests."
        };

        foreach (var assembly in assemblies)
        {
            lines.Add($"ASSEMBLY {assembly.GetName().Name}");
            foreach (var type in assembly.GetExportedTypes().OrderBy(type => type.FullName, StringComparer.Ordinal))
            {
                lines.Add(FormatTypeDeclaration(type));
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

                foreach (var field in type.GetFields(flags).OrderBy(field => field.Name, StringComparer.Ordinal))
                    lines.Add($"  FIELD {FormatType(field.FieldType)} {field.Name}{FormatConstant(field)}");

                foreach (var constructor in type.GetConstructors(flags).OrderBy(FormatConstructor, StringComparer.Ordinal))
                    lines.Add("  " + FormatConstructor(constructor));

                foreach (var property in type.GetProperties(flags).OrderBy(property => property.Name, StringComparer.Ordinal))
                    lines.Add("  " + FormatProperty(property));

                foreach (var eventInfo in type.GetEvents(flags).OrderBy(eventInfo => eventInfo.Name, StringComparer.Ordinal))
                    lines.Add($"  EVENT {FormatType(eventInfo.EventHandlerType!)} {eventInfo.Name}");

                foreach (var method in type.GetMethods(flags)
                             .Where(method => !method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal))
                             .OrderBy(FormatMethod, StringComparer.Ordinal))
                    lines.Add("  " + FormatMethod(method));
            }
        }

        return string.Join("\n", lines);
    }

    private static string FormatTypeDeclaration(Type type)
    {
        var kind = type.IsEnum ? "enum" : type.IsInterface ? "interface" : type.IsValueType ? "struct" : "class";
        var suffix = new List<string>();
        if (type.BaseType is not null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType) && type.BaseType != typeof(Enum))
            suffix.Add("base=" + FormatType(type.BaseType));
        var interfaces = type.GetInterfaces().Select(FormatType).OrderBy(value => value, StringComparer.Ordinal).ToArray();
        if (interfaces.Length > 0) suffix.Add("interfaces=" + string.Join(",", interfaces));
        return suffix.Count == 0
            ? $"TYPE {kind} {FormatType(type)}"
            : $"TYPE {kind} {FormatType(type)} [{string.Join(";", suffix)}]";
    }

    private static string FormatConstructor(ConstructorInfo constructor) =>
        $"CTOR {constructor.DeclaringType!.Name}({string.Join(", ", constructor.GetParameters().Select(FormatParameter))})";

    private static string FormatMethod(MethodInfo method)
    {
        var generic = method.IsGenericMethodDefinition
            ? "<" + string.Join(",", method.GetGenericArguments().Select(argument => argument.Name)) + ">"
            : string.Empty;
        return $"METHOD {(method.IsStatic ? "static " : string.Empty)}{FormatType(method.ReturnType)} {method.Name}{generic}({string.Join(", ", method.GetParameters().Select(FormatParameter))})";
    }

    private static string FormatProperty(PropertyInfo property)
    {
        var accessors = new List<string>();
        if (property.GetMethod?.IsPublic == true) accessors.Add("get");
        if (property.SetMethod?.IsPublic == true) accessors.Add("set");
        var index = property.GetIndexParameters();
        var indexer = index.Length == 0 ? string.Empty : $"[{string.Join(", ", index.Select(FormatParameter))}]";
        return $"PROPERTY {FormatType(property.PropertyType)} {property.Name}{indexer} {{{string.Join(";", accessors)}}}";
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        var modifier = parameter.IsOut ? "out " : parameter.ParameterType.IsByRef ? "ref " : string.Empty;
        var type = parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType;
        var optional = parameter.HasDefaultValue ? " = " + FormatValue(parameter.DefaultValue) : string.Empty;
        return $"{modifier}{FormatType(type)} {parameter.Name}{optional}";
    }

    private static string FormatConstant(FieldInfo field)
    {
        if (!field.IsLiteral) return string.Empty;
        return " = " + FormatValue(field.GetRawConstantValue());
    }

    private static string FormatValue(object? value) => value switch
    {
        null => "null",
        string text => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
        char character => "'" + character + "'",
        bool boolean => boolean ? "true" : "false",
        double number when double.IsPositiveInfinity(number) => "Infinity",
        double number when double.IsNegativeInfinity(number) => "-Infinity",
        double number when double.IsNaN(number) => "NaN",
        float number when float.IsPositiveInfinity(number) => "Infinity",
        float number when float.IsNegativeInfinity(number) => "-Infinity",
        float number when float.IsNaN(number) => "NaN",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
        _ => value.ToString() ?? string.Empty
    };

    private static string FormatType(Type type)
    {
        if (type.IsArray) return FormatType(type.GetElementType()!) + "[]";
        if (type.IsGenericParameter) return type.Name;
        if (!type.IsGenericType) return type.FullName ?? type.Name;

        var definitionName = type.GetGenericTypeDefinition().FullName ?? type.Name;
        var tick = definitionName.IndexOf('`');
        if (tick >= 0) definitionName = definitionName[..tick];
        return definitionName + "<" + string.Join(",", type.GetGenericArguments().Select(FormatType)) + ">";
    }
}
