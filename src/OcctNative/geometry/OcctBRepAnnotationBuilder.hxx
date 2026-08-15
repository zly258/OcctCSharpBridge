#pragma once

#include <TopoDS_Edge.hxx>
#include <TopoDS_Shape.hxx>

namespace OcctModelingInternal
{
    TopoDS_Shape buildLengthAnnotation(
        const TopoDS_Edge& edge,
        double offset,
        double textHeight,
        double arrowSize,
        const char* fontName);

    TopoDS_Shape buildAngleAnnotation(
        const TopoDS_Edge& firstEdge,
        const TopoDS_Edge& secondEdge,
        double radius,
        double textHeight,
        double arrowSize,
        const char* fontName);

    TopoDS_Shape buildRadiusAnnotation(
        const TopoDS_Edge& circularEdge,
        double offset,
        double textHeight,
        double arrowSize,
        const char* fontName);

    TopoDS_Shape buildDiameterAnnotation(
        const TopoDS_Edge& circularEdge,
        double offset,
        double textHeight,
        double arrowSize,
        const char* fontName);
}
