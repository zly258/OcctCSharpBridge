#pragma once

#include "OcctModelingSessionInternal.hxx"

#include <BRep_Tool.hxx>
#include <Poly_Triangulation.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Face.hxx>

#include <stdexcept>

namespace OcctModelingInternal
{
    inline Handle(Poly_Triangulation) faceTriangulation(
        ModelSession* model,
        OcctObjectId faceId,
        TopoDS_Face& face,
        TopLoc_Location& location)
    {
        const TopoDS_Shape& shape = model->requireShape(faceId);
        if (shape.ShapeType() != TopAbs_FACE)
            throw std::invalid_argument("Input must be a face.");
        face = TopoDS::Face(shape);
        Handle(Poly_Triangulation) triangulation = BRep_Tool::Triangulation(face, location);
        if (triangulation.IsNull())
            throw std::runtime_error("The face has no triangulation. Call Mesh first.");
        return triangulation;
    }
}
