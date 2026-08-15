#include "geometry/OcctModelingPlanarOperations.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepOffsetAPI_MakeOffset.hxx>
#include <BRep_Tool.hxx>
#include <Geom_Curve.hxx>
#include <GeomAbs_JoinType.hxx>
#include <Precision.hxx>

#include <cmath>
#include <stdexcept>
#include <string>

using namespace OcctModelingInternal;

namespace
{
    GeomAbs_JoinType toJoinType(int value)
    {
        switch (value)
        {
            case OcctModelJoin_Tangent: return GeomAbs_Tangent;
            case OcctModelJoin_Intersection: return GeomAbs_Intersection;
            default: return GeomAbs_Arc;
        }
    }

    TopoDS_Wire requireWire(ModelSession* model, OcctObjectId id, const char* name)
    {
        const TopoDS_Shape& shape = model->requireShape(id);
        if (shape.ShapeType() != TopAbs_WIRE)
            throw std::invalid_argument(std::string(name) + " must be a wire.");
        return TopoDS::Wire(shape);
    }

    TopoDS_Edge requireEdge(ModelSession* model, OcctObjectId id)
    {
        const TopoDS_Shape& shape = model->requireShape(id);
        if (shape.ShapeType() != TopAbs_EDGE)
            throw std::invalid_argument("Input must be an edge.");
        return TopoDS::Edge(shape);
    }
}

extern "C"
{
    OcctStatus occt_model_make_face_with_holes(
        OcctModelingSessionHandle handle,
        OcctObjectId outerWireId,
        const OcctObjectId* innerWireIds,
        int innerWireCount,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (innerWireCount < 0)
                throw std::invalid_argument("Inner wire count must not be negative.");
            if (innerWireCount > 0 && innerWireIds == nullptr)
                throw std::invalid_argument("Inner wire ID array is null.");

            BRepBuilderAPI_MakeFace maker(requireWire(model, outerWireId, "Outer wire"), Standard_True);
            if (!maker.IsDone())
                throw std::runtime_error("Outer planar face creation failed.");

            for (int index = 0; index < innerWireCount; ++index)
            {
                const TopoDS_Wire inner = requireWire(model, innerWireIds[index], "Inner wire");
                maker.Add(TopoDS::Wire(inner.Reversed()));
            }

            maker.Build();
            if (!maker.IsDone() || maker.Face().IsNull())
                throw std::runtime_error("Planar face with holes creation failed.");
            return TopoDS_Shape(maker.Face());
        });
    }

    OcctStatus occt_model_trim_edge(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double firstParameter,
        double lastParameter,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (!std::isfinite(firstParameter) ||
                !std::isfinite(lastParameter) ||
                firstParameter >= lastParameter)
            {
                throw std::invalid_argument("Trim parameters must be finite and strictly increasing.");
            }

            const TopoDS_Edge edge = requireEdge(model, edgeId);
            double sourceFirst = 0.0;
            double sourceLast = 0.0;
            Handle(Geom_Curve) curve = BRep_Tool::Curve(edge, sourceFirst, sourceLast);
            if (curve.IsNull()) throw std::runtime_error("Edge has no 3D curve.");
            if (firstParameter < sourceFirst - Precision::PConfusion() ||
                lastParameter > sourceLast + Precision::PConfusion())
            {
                throw std::out_of_range("Trim parameters are outside the source edge parameter range.");
            }

            BRepBuilderAPI_MakeEdge maker(curve, firstParameter, lastParameter);
            if (!maker.IsDone() || maker.Edge().IsNull())
                throw std::runtime_error("Trimmed edge creation failed.");
            return TopoDS_Shape(maker.Edge());
        });
    }

    OcctStatus occt_model_offset_wire(
        OcctModelingSessionHandle handle,
        OcctObjectId wireId,
        double offset,
        double altitude,
        int joinType,
        OcctBool openResult,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (!std::isfinite(offset) || std::abs(offset) <= Precision::Confusion())
                throw std::invalid_argument("Wire offset must be finite and non-zero.");
            if (!std::isfinite(altitude))
                throw std::invalid_argument("Wire offset altitude must be finite.");
            if (joinType < OcctModelJoin_Arc || joinType > OcctModelJoin_Intersection)
                throw std::invalid_argument("Wire offset join type is invalid.");

            BRepOffsetAPI_MakeOffset maker(
                requireWire(model, wireId, "Input"),
                toJoinType(joinType),
                openResult != 0 ? Standard_True : Standard_False);
            maker.Perform(offset, altitude);
            if (!maker.IsDone() || maker.Shape().IsNull())
                throw std::runtime_error("Planar wire offset failed.");
            return maker.Shape();
        });
    }
}
