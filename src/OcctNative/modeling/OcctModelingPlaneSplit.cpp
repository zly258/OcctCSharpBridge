#include "modeling/OcctModelingPlaneSplit.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepPrimAPI_MakeHalfSpace.hxx>
#include <gp_Pln.hxx>

#include <stdexcept>

using namespace OcctModelingInternal;

namespace
{
    OcctObjectId addIfPresent(ModelSession* model, const TopoDS_Shape& shape)
    {
        return shape.IsNull() ? 0 : model->addShape(shape);
    }
}

extern "C"
{
    OcctStatus occt_model_split_by_plane(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d origin,
        OcctVector3d normal,
        OcctModelPlaneSplitResult* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& source = model->requireShape(shapeId);
            const gp_Dir planeNormal = toDirection(normal);
            const gp_Pnt planeOrigin = toPoint(origin);
            const gp_Pln plane(planeOrigin, planeNormal);

            BRepBuilderAPI_MakeFace faceMaker(plane);
            if (!faceMaker.IsDone())
                throw std::runtime_error("Cutting plane creation failed.");
            const TopoDS_Face cuttingFace = faceMaker.Face();

            BRepAlgoAPI_Section section(source, cuttingFace, Standard_False);
            section.Build();
            if (!section.IsDone())
                throw std::runtime_error("Plane section calculation failed.");

            const gp_Vec normalVector(planeNormal);
            BRepPrimAPI_MakeHalfSpace positiveHalfSpace(
                cuttingFace,
                planeOrigin.Translated(normalVector));
            BRepPrimAPI_MakeHalfSpace negativeHalfSpace(
                cuttingFace,
                planeOrigin.Translated(-normalVector));
            if (!positiveHalfSpace.IsDone() || !negativeHalfSpace.IsDone())
                throw std::runtime_error("Plane half-space creation failed.");

            BRepAlgoAPI_Common positive(source, positiveHalfSpace.Shape());
            positive.Build();
            if (!positive.IsDone())
                throw std::runtime_error("Positive plane split failed.");

            BRepAlgoAPI_Common negative(source, negativeHalfSpace.Shape());
            negative.Build();
            if (!negative.IsDone())
                throw std::runtime_error("Negative plane split failed.");

            result->positiveShapeId = addIfPresent(model, positive.Shape());
            result->negativeShapeId = addIfPresent(model, negative.Shape());
            result->sectionShapeId = addIfPresent(model, section.Shape());
        });
    }
}
