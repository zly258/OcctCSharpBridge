#pragma once
#include "modeling/OcctModeling.h"

extern "C" {
    struct OcctFitBSplineOptions {
        uint32_t structSize;
        uint32_t apiVersion;
        int degMin;      // default 3
        int degMax;      // default 8  
        int continuity;  // 0=C0,1=C1,2=C2,3=G1,4=G2
        double tolerance;
        OcctBool periodic;
    };

    OCCTBRIDGE_API OcctStatus occt_model_curve_fit_bspline(
        OcctModelingSessionHandle handle,
        const OcctPoint3d* points,
        int count,
        const OcctFitBSplineOptions* options,
        OcctObjectId* result);
}
