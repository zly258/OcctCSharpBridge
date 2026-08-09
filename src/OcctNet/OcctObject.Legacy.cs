namespace OcctNet;

/// <summary>
/// Backward-compatible, engine-unbound viewer object handle.
/// Prefer the owner-aware handles returned by <see cref="OcctEngine.GetObject(long)"/>
/// for new code. This type exists so applications that persist an AIS object ID
/// and kind can migrate to Bridge 2.6 without manufacturing owner tokens.
/// </summary>
public readonly record struct OcctObject(long Id, OcctObjectKind Kind) : IOcctObject
{
    public bool IsValid => Id > 0;
}
