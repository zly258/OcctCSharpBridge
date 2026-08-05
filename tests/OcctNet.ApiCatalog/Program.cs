using System.Reflection;
using CadCommon;
using OcctNet;

const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
var catalog = ApiDemoCatalog.Members;
var exportedTypes = typeof(OcctEngine).Assembly.GetExportedTypes()
    .Where(type => string.Equals(type.Namespace, "OcctNet", StringComparison.Ordinal))
    .ToArray();

if (catalog.Count == 0)
    throw new InvalidOperationException("The API demo catalog is empty.");

foreach (var type in exportedTypes)
{
    var entries = catalog.Where(item => string.Equals(item.TypeName, type.Name, StringComparison.Ordinal)).ToArray();
    if (entries.Count(item => item.Kind == "Type") != 1)
        throw new InvalidOperationException($"Catalog type entry mismatch: {type.FullName}");

    VerifyCount(type, "Constructor", type.GetConstructors(flags).Length, entries);
    VerifyCount(type, "Property", type.GetProperties(flags).Length, entries);
    VerifyCount(type, "Field", type.GetFields(flags).Length, entries);
    VerifyCount(type, "Event", type.GetEvents(flags).Length, entries);
    VerifyCount(type, "Method", type.GetMethods(flags).Count(method => !method.IsSpecialName), entries);
}

foreach (var required in new[] { nameof(OcctEngine), nameof(OcctModelingSession), nameof(OcafDocument) })
{
    if (!catalog.Any(item => item.TypeName == required))
        throw new InvalidOperationException($"Required wrapper type is absent from API catalog: {required}");
}

Console.WriteLine(ApiDemoCatalog.CoverageSummary);
Console.WriteLine($"Verified {exportedTypes.Length} exported OcctNet types and {catalog.Count} catalog entries.");

static void VerifyCount(Type type, string kind, int expected, IReadOnlyCollection<ApiDemoMember> entries)
{
    var actual = entries.Count(item => item.Kind == kind);
    if (actual != expected)
        throw new InvalidOperationException($"{type.FullName} {kind} count mismatch: expected {expected}, actual {actual}.");
}
