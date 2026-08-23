namespace OcctNet;

/// <summary>Frenet frame mode for pipe shell sweep.</summary>
public enum OcctPipeShellMode
{
    Default           = 0,
    Frenet            = 1,
    CorrectedFrenet   = 2,
    DiscreteTrihedron = 3,
    FixedNormal       = 4
}
