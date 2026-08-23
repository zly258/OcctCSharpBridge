#pragma once
#include "modeling/OcctModeling.h"
#include <cstdint>

extern "C" {
    /// <summary>Frenet frame transition mode for pipe shell.</summary>
    enum OcctPipeShellMode : int {
        OcctPipeShellMode_Default          = 0,  // BRepOffsetAPI_MakePipeShell default
        OcctPipeShellMode_Frenet           = 1,  // Frenet framing
        OcctPipeShellMode_CorrectedFrenet  = 2,  // Corrected Frenet (no torsion jumps)
        OcctPipeShellMode_DiscreteTrihedron = 3, // Discrete trihedron
        OcctPipeShellMode_FixedNormal      = 4   // Fixed binormal direction
    };

    struct OcctPipeShellOptions {
        uint32_t structSize;
        uint32_t apiVersion;
        int mode;            // OcctPipeShellMode
        OcctBool forceC1;    // force C1 continuity
        OcctBool makeSolid;
        OcctVector3d fixedNormal; // used when mode == OcctPipeShellMode_FixedNormal
    };

    OCCTBRIDGE_API OcctStatus occt_model_feature_pipe_shell_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId spineWireId,
        const OcctObjectId* profileIds, int profileCount,
        const OcctPipeShellOptions* options,
        OcctModelAlgorithmResult* result);
}
