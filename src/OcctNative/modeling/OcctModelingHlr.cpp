#include "modeling/OcctModelingHlr.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <HLRAlgo_Projector.hxx>
#include <HLRBRep_Algo.hxx>
#include <HLRBRep_HLRToShape.hxx>
#include <gp_Ax3.hxx>
#include <gp_Trsf.hxx>

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
    OcctStatus occt_model_hlr_project(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctVector3d viewDirection,
        OcctVector3d upDirection,
        OcctModelHlrResult* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& source = model->requireShape(shapeId);
            const gp_Dir viewDir = toDirection(viewDirection);
            const gp_Dir upDir = toDirection(upDirection);
            gp_Vec rightVector = gp_Vec(upDir).Crossed(gp_Vec(viewDir));
            if (rightVector.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::invalid_argument("HLR view and up directions must not be parallel.");
            rightVector.Normalize();

            const gp_Ax3 projectionAxes(
                gp_Pnt(0.0, 0.0, 0.0),
                viewDir,
                gp_Dir(rightVector));
            gp_Trsf projectionTransform;
            projectionTransform.SetTransformation(projectionAxes);

            Handle(HLRBRep_Algo) algorithm = new HLRBRep_Algo();
            algorithm->Add(source);
            algorithm->Projector(HLRAlgo_Projector(projectionTransform, Standard_False, 0.0));
            algorithm->Update();
            algorithm->Hide();

            HLRBRep_HLRToShape converter(algorithm);
            result->visibleShapeId = addIfPresent(model, converter.VCompound(source));
            result->hiddenShapeId = addIfPresent(model, converter.HCompound(source));
            result->outlineShapeId = addIfPresent(model, converter.OutLineVCompound(source));
            result->visibleSharpShapeId = addIfPresent(model, converter.RgNLineVCompound(source));
            result->hiddenSharpShapeId = addIfPresent(model, converter.RgNLineHCompound(source));
        });
    }
}
