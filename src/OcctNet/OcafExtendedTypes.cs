namespace OcctNet;

/// <summary>OCAF XML/binary storage version supported by OCCT 7.9.0.</summary>
public enum OcafStorageFormatVersion
{
    Version2 = 2,
    Version3 = 3,
    Version4 = 4,
    Version5 = 5,
    Version6 = 6,
    Version7 = 7,
    Version8 = 8,
    Version9 = 9,
    Version10 = 10,
    Version11 = 11,
    Version12 = 12,
    Current = Version12
}

public sealed record OcafVariableInfo(
    OcafLabel Label,
    string Name,
    double? Value,
    string Unit,
    bool IsConstant,
    bool IsAssigned);
