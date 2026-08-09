#pragma once

#include "OcctModelingSessionInternal.hxx"

#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRep_Tool.hxx>
#include <GProp_GProps.hxx>
#include <Precision.hxx>
#include <TopAbs_State.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <TopTools_ListOfShape.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS_Vertex.hxx>
#include <TopoDS_Wire.hxx>
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>

#include <algorithm>
#include <stdexcept>
#include <string>

namespace OcctModelingInternal
{
    inline gp_Pnt toPoint(OcctPoint3d value)
    {
        return gp_Pnt(value.x, value.y, value.z);
    }

    inline gp_Vec toVector(OcctVector3d value)
    {
        return gp_Vec(value.x, value.y, value.z);
    }

    inline gp_Dir toDirection(OcctVector3d value)
    {
        gp_Vec vector = toVector(value);
        if (vector.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::invalid_argument("Direction vector must not be zero.");
        return gp_Dir(vector);
    }

    inline gp_Ax2 toAxis2(OcctPoint3d origin, OcctVector3d normal)
    {
        return gp_Ax2(toPoint(origin), toDirection(normal));
    }

    inline void requirePositive(double value, const char* name)
    {
        if (value <= 0.0) throw std::invalid_argument(std::string(name) + " must be greater than zero.");
    }

    inline void requireCount(int count, int minimum, const char* name)
    {
        if (count < minimum) throw std::invalid_argument(std::string(name) + " has too few items.");
    }

    inline TopAbs_ShapeEnum toShapeEnum(int value)
    {
        switch (value)
        {
            case OcctShape_Compound: return TopAbs_COMPOUND;
            case OcctShape_CompSolid: return TopAbs_COMPSOLID;
            case OcctShape_Solid: return TopAbs_SOLID;
            case OcctShape_Shell: return TopAbs_SHELL;
            case OcctShape_Face: return TopAbs_FACE;
            case OcctShape_Wire: return TopAbs_WIRE;
            case OcctShape_Edge: return TopAbs_EDGE;
            case OcctShape_Vertex: return TopAbs_VERTEX;
            default: return TopAbs_SHAPE;
        }
    }

    inline int toModelState(TopAbs_State state)
    {
        switch (state)
        {
            case TopAbs_IN: return OcctModelState_Inside;
            case TopAbs_OUT: return OcctModelState_Outside;
            case TopAbs_ON: return OcctModelState_On;
            default: return OcctModelState_Unknown;
        }
    }

    inline TopoDS_Edge indexedEdge(const TopoDS_Shape& shape, int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0) throw std::out_of_range("Edge index must not be negative.");
        TopTools_IndexedMapOfShape edges;
        TopExp::MapShapes(shape, TopAbs_EDGE, edges);
        const int oneBased = zeroBasedIndex + 1;
        if (oneBased > edges.Extent()) throw std::out_of_range("Edge index is out of range.");
        return TopoDS::Edge(edges(oneBased));
    }

    inline TopoDS_Face indexedFace(const TopoDS_Shape& shape, int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0) throw std::out_of_range("Face index must not be negative.");
        TopTools_IndexedMapOfShape faces;
        TopExp::MapShapes(shape, TopAbs_FACE, faces);
        const int oneBased = zeroBasedIndex + 1;
        if (oneBased > faces.Extent()) throw std::out_of_range("Face index is out of range.");
        return TopoDS::Face(faces(oneBased));
    }

    inline TopoDS_Wire modelRectangleWire(
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height)
    {
        requirePositive(width, "Width");
        requirePositive(height, "Height");
        const gp_Ax2 plane(toPoint(origin), toDirection(normal), toDirection(xDirection));
        const gp_Pnt p0 = plane.Location();
        const gp_Vec xVector(plane.XDirection());
        const gp_Vec yVector(plane.YDirection());
        const gp_Pnt p1 = p0.Translated(xVector * width);
        const gp_Pnt p2 = p1.Translated(yVector * height);
        const gp_Pnt p3 = p0.Translated(yVector * height);
        BRepBuilderAPI_MakePolygon polygon;
        polygon.Add(p0);
        polygon.Add(p1);
        polygon.Add(p2);
        polygon.Add(p3);
        polygon.Close();
        if (!polygon.IsDone()) throw std::runtime_error("Rectangle wire creation failed.");
        return polygon.Wire();
    }

    inline void fillProperties(const GProp_GProps& properties, OcctMassProperties* result)
    {
        if (result == nullptr) throw std::invalid_argument("Result pointer is null.");
        const gp_Pnt center = properties.CentreOfMass();
        result->mass = properties.Mass();
        result->centerX = center.X();
        result->centerY = center.Y();
        result->centerZ = center.Z();
    }

    inline double maximumTolerance(const TopoDS_Shape& shape)
    {
        double result = 0.0;
        for (TopExp_Explorer explorer(shape, TopAbs_VERTEX); explorer.More(); explorer.Next())
            result = std::max(result, BRep_Tool::Tolerance(TopoDS::Vertex(explorer.Current())));
        for (TopExp_Explorer explorer(shape, TopAbs_EDGE); explorer.More(); explorer.Next())
            result = std::max(result, BRep_Tool::Tolerance(TopoDS::Edge(explorer.Current())));
        for (TopExp_Explorer explorer(shape, TopAbs_FACE); explorer.More(); explorer.Next())
            result = std::max(result, BRep_Tool::Tolerance(TopoDS::Face(explorer.Current())));
        return result;
    }

    inline TopTools_ListOfShape shapeList(ModelSession* model, const OcctObjectId* ids, int count, const char* name)
    {
        requireCount(count, 1, name);
        if (ids == nullptr) throw std::invalid_argument(std::string(name) + " array is null.");
        TopTools_ListOfShape result;
        for (int index = 0; index < count; ++index) result.Append(model->requireShape(ids[index]));
        return result;
    }
}
