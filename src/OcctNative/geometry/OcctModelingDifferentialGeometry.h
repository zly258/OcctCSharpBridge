#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_edge_parameter_range(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelParameterRange* result);
    OCCTBRIDGE_API OcctStatus occt_model_edge_differential(OcctModelingSessionHandle handle, OcctObjectId edgeId, double parameter, OcctModelCurveDifferential* result);
    OCCTBRIDGE_API OcctStatus occt_model_edge_curvature(OcctModelingSessionHandle handle, OcctObjectId edgeId, double parameter, double resolution, OcctModelCurveCurvature* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_periodicity(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelSurfacePeriodicity* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_differential(OcctModelingSessionHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceDifferential* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_curvature(OcctModelingSessionHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceCurvature* result);
}
