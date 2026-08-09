using System.Runtime.CompilerServices;

internal static class PublicApiSnapshotInitializer
{
    [ModuleInitializer]
    internal static void Initialize() => PublicApiSnapshot.Validate();
}
