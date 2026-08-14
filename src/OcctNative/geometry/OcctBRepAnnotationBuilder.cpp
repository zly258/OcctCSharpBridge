#include "geometry/OcctBRepAnnotationBuilder.hxx"
#include "geometry/OcctBRepTextBuilder.hxx"
#include "core/OcctInternal.hxx"

#include <BRep_Builder.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <GeomAbs_CurveType.hxx>
#include <Graphic3d_HorizontalTextAlignment.hxx>
#include <Graphic3d_VerticalTextAlignment.hxx>
#include <Precision.hxx>
#include <TopoDS_Compound.hxx>
#include <gp_Ax1.hxx>
#include <gp_Ax2.hxx>
#include <gp_Circ.hxx>

#include <array>
#include <cmath>
#include <iomanip>
#include <limits>
#include <sstream>
#include <stdexcept>
#include <string>

namespace
{
    class ShapeCollector
    {
    public:
        ShapeCollector() { myBuilder.MakeCompound(myCompound); }
        void add(const TopoDS_Shape& shape) { if (!shape.IsNull()) myBuilder.Add(myCompound, shape); }
        const TopoDS_Compound& shape() const { return myCompound; }

    private:
        BRep_Builder myBuilder;
        TopoDS_Compound myCompound;
    };

    void requireAnnotationOptions(double textHeight, double arrowSize)
    {
        OcctBridge::requirePositive(textHeight, "Text height");
        OcctBridge::requirePositive(arrowSize, "Arrow size");
    }

    TopoDS_Shape line(const gp_Pnt& start, const gp_Pnt& end)
    {
        if (start.Distance(end) <= Precision::Confusion()) return TopoDS_Shape();
        BRepBuilderAPI_MakeEdge edge(start, end);
        if (!edge.IsDone()) throw std::runtime_error("Annotation line creation failed.");
        return edge.Shape();
    }

    void addArrow(
        ShapeCollector& collector,
        const gp_Pnt& tip,
        const gp_Dir& inwardDirection,
        const gp_Dir& sideDirection,
        double size)
    {
        const gp_Pnt base = tip.Translated(gp_Vec(inwardDirection) * size);
        const gp_Vec side = gp_Vec(sideDirection) * (size * 0.38);
        BRepBuilderAPI_MakePolygon polygon;
        polygon.Add(tip);
        polygon.Add(base.Translated(side));
        polygon.Add(base.Translated(side.Reversed()));
        polygon.Close();
        if (!polygon.IsDone()) throw std::runtime_error("Annotation arrow creation failed.");

        BRepBuilderAPI_MakeFace face(polygon.Wire(), Standard_True);
        collector.add(face.IsDone() ? face.Shape() : polygon.Shape());
    }

    std::string formatValue(double value)
    {
        std::ostringstream stream;
        stream << std::fixed << std::setprecision(2) << value;
        std::string result = stream.str();
        while (!result.empty() && result.back() == '0') result.pop_back();
        if (!result.empty() && result.back() == '.') result.pop_back();
        return result.empty() ? std::string("0") : result;
    }

    void requireLineEdge(const TopoDS_Edge& edge, const char* name)
    {
        BRepAdaptor_Curve curve(edge);
        if (curve.GetType() != GeomAbs_Line)
            throw std::invalid_argument(std::string(name) + " must be a straight edge.");
    }

    void requireCircularEdge(const TopoDS_Edge& edge)
    {
        BRepAdaptor_Curve curve(edge);
        if (curve.GetType() != GeomAbs_Circle)
            throw std::invalid_argument("Circular annotation input must be a circular edge.");
    }

    std::array<gp_Pnt, 2> edgeEndpoints(const TopoDS_Edge& edge)
    {
        BRepAdaptor_Curve curve(edge);
        return { curve.Value(curve.FirstParameter()), curve.Value(curve.LastParameter()) };
    }

    struct EdgeRay
    {
        gp_Pnt vertex;
        gp_Dir firstDirection;
        gp_Dir secondDirection;
    };

    EdgeRay angleRays(const TopoDS_Edge& firstEdge, const TopoDS_Edge& secondEdge)
    {
        const auto first = edgeEndpoints(firstEdge);
        const auto second = edgeEndpoints(secondEdge);
        double closestDistance = std::numeric_limits<double>::max();
        int firstIndex = 0;
        int secondIndex = 0;

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                const double distance = first[static_cast<std::size_t>(i)].Distance(second[static_cast<std::size_t>(j)]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    firstIndex = i;
                    secondIndex = j;
                }
            }
        }

        const gp_Pnt vertex(
            (first[static_cast<std::size_t>(firstIndex)].X() + second[static_cast<std::size_t>(secondIndex)].X()) * 0.5,
            (first[static_cast<std::size_t>(firstIndex)].Y() + second[static_cast<std::size_t>(secondIndex)].Y()) * 0.5,
            (first[static_cast<std::size_t>(firstIndex)].Z() + second[static_cast<std::size_t>(secondIndex)].Z()) * 0.5);
        const gp_Vec firstVector(vertex, first[static_cast<std::size_t>(1 - firstIndex)]);
        const gp_Vec secondVector(vertex, second[static_cast<std::size_t>(1 - secondIndex)]);
        if (firstVector.SquareMagnitude() <= Precision::SquareConfusion()
            || secondVector.SquareMagnitude() <= Precision::SquareConfusion())
        {
            throw std::runtime_error("Angular annotation edges have invalid endpoints.");
        }
        return { vertex, gp_Dir(firstVector), gp_Dir(secondVector) };
    }
}

namespace OcctModelingInternal
{
    TopoDS_Shape buildLengthAnnotation(
        const TopoDS_Edge& edge,
        double offset,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        requireAnnotationOptions(textHeight, arrowSize);
        if (!std::isfinite(offset)) throw std::invalid_argument("Annotation offset must be finite.");
        requireLineEdge(edge, "Length annotation input");

        const auto endpoints = edgeEndpoints(edge);
        const gp_Vec edgeVector(endpoints[0], endpoints[1]);
        const double length = edgeVector.Magnitude();
        if (length <= Precision::Confusion()) throw std::runtime_error("Length annotation edge has zero length.");
        const gp_Dir edgeDirection(edgeVector);
        gp_Dir reference(0.0, 0.0, 1.0);
        if (std::abs(edgeDirection.Dot(reference)) > 0.95) reference = gp_Dir(1.0, 0.0, 0.0);
        const gp_Dir planeNormal = edgeDirection.Crossed(reference);
        const gp_Dir offsetDirection = planeNormal.Crossed(edgeDirection);
        const gp_Vec offsetVector = gp_Vec(offsetDirection) * offset;
        const gp_Pnt firstDimension = endpoints[0].Translated(offsetVector);
        const gp_Pnt secondDimension = endpoints[1].Translated(offsetVector);

        ShapeCollector collector;
        collector.add(line(endpoints[0], firstDimension));
        collector.add(line(endpoints[1], secondDimension));
        collector.add(line(firstDimension, secondDimension));
        addArrow(collector, firstDimension, edgeDirection, offsetDirection, arrowSize);
        addArrow(collector, secondDimension, edgeDirection.Reversed(), offsetDirection, arrowSize);

        const gp_Pnt midpoint(
            (firstDimension.X() + secondDimension.X()) * 0.5,
            (firstDimension.Y() + secondDimension.Y()) * 0.5,
            (firstDimension.Z() + secondDimension.Z()) * 0.5);
        const double textSide = offset < 0.0 ? -1.0 : 1.0;
        const gp_Pnt textPosition = midpoint.Translated(gp_Vec(offsetDirection) * (textHeight * 0.35 * textSide));
        const std::string label = formatValue(length);
        collector.add(buildBRepText(
            label.c_str(), fontName,
            { textPosition.X(), textPosition.Y(), textPosition.Z() },
            { planeNormal.X(), planeNormal.Y(), planeNormal.Z() },
            { edgeDirection.X(), edgeDirection.Y(), edgeDirection.Z() },
            textHeight, 0.0, false, false,
            Graphic3d_HTA_CENTER, Graphic3d_VTA_CENTER));
        return collector.shape();
    }

    TopoDS_Shape buildAngleAnnotation(
        const TopoDS_Edge& firstEdge,
        const TopoDS_Edge& secondEdge,
        double radius,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        requireAnnotationOptions(textHeight, arrowSize);
        OcctBridge::requirePositive(radius, "Angular annotation radius");
        requireLineEdge(firstEdge, "First angular annotation input");
        requireLineEdge(secondEdge, "Second angular annotation input");
        const EdgeRay rays = angleRays(firstEdge, secondEdge);
        const gp_Vec cross = gp_Vec(rays.firstDirection).Crossed(gp_Vec(rays.secondDirection));
        if (cross.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::invalid_argument("Angular annotation edges must not be parallel.");
        const gp_Dir normal(cross);
        const double angle = rays.firstDirection.Angle(rays.secondDirection);
        if (angle <= Precision::Angular()) throw std::invalid_argument("Angular annotation angle is too small.");

        const gp_Pnt firstPoint = rays.vertex.Translated(gp_Vec(rays.firstDirection) * radius);
        const gp_Pnt secondPoint = rays.vertex.Translated(gp_Vec(rays.secondDirection) * radius);
        ShapeCollector collector;
        collector.add(line(rays.vertex, firstPoint));
        collector.add(line(rays.vertex, secondPoint));
        BRepBuilderAPI_MakeEdge arc(gp_Circ(gp_Ax2(rays.vertex, normal, rays.firstDirection), radius), 0.0, angle);
        if (!arc.IsDone()) throw std::runtime_error("Angular annotation arc creation failed.");
        collector.add(arc.Shape());

        const gp_Dir firstTangent = normal.Crossed(rays.firstDirection);
        const gp_Dir secondTangent = normal.Crossed(rays.secondDirection);
        addArrow(collector, firstPoint, firstTangent, rays.firstDirection, arrowSize);
        addArrow(collector, secondPoint, secondTangent.Reversed(), rays.secondDirection, arrowSize);

        gp_Dir middleDirection = rays.firstDirection;
        middleDirection.Rotate(gp_Ax1(rays.vertex, normal), angle * 0.5);
        const gp_Dir textDirection = normal.Crossed(middleDirection);
        const gp_Pnt textPosition = rays.vertex.Translated(gp_Vec(middleDirection) * (radius + textHeight * 0.9));
        const std::string label = formatValue(angle * 180.0 / 3.14159265358979323846) + "\xC2\xB0";
        collector.add(buildBRepText(
            label.c_str(), fontName,
            { textPosition.X(), textPosition.Y(), textPosition.Z() },
            { normal.X(), normal.Y(), normal.Z() },
            { textDirection.X(), textDirection.Y(), textDirection.Z() },
            textHeight, 0.0, false, false,
            Graphic3d_HTA_CENTER, Graphic3d_VTA_CENTER));
        return collector.shape();
    }

    TopoDS_Shape buildRadiusAnnotation(
        const TopoDS_Edge& circularEdge,
        double offset,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        requireAnnotationOptions(textHeight, arrowSize);
        if (!std::isfinite(offset)) throw std::invalid_argument("Annotation offset must be finite.");
        requireCircularEdge(circularEdge);
        BRepAdaptor_Curve curve(circularEdge);
        const gp_Circ circle = curve.Circle();
        const gp_Pnt center = circle.Location();
        const gp_Pnt circlePoint = curve.Value(curve.FirstParameter());
        const gp_Vec radiusVector(center, circlePoint);
        const double radius = radiusVector.Magnitude();
        const gp_Dir radiusDirection(radiusVector);
        const gp_Dir normal = circle.Axis().Direction();
        const gp_Dir sideDirection = normal.Crossed(radiusDirection);
        const gp_Pnt leaderEnd = circlePoint.Translated(gp_Vec(radiusDirection) * offset);

        ShapeCollector collector;
        collector.add(line(center, leaderEnd));
        addArrow(collector, circlePoint, radiusDirection, sideDirection, arrowSize);
        const gp_Pnt textPosition = leaderEnd.Translated(gp_Vec(sideDirection) * (textHeight * 0.3));
        const std::string label = std::string("R") + formatValue(radius);
        collector.add(buildBRepText(
            label.c_str(), fontName,
            { textPosition.X(), textPosition.Y(), textPosition.Z() },
            { normal.X(), normal.Y(), normal.Z() },
            { radiusDirection.X(), radiusDirection.Y(), radiusDirection.Z() },
            textHeight, 0.0, false, false,
            Graphic3d_HTA_LEFT, Graphic3d_VTA_CENTER));
        return collector.shape();
    }

    TopoDS_Shape buildDiameterAnnotation(
        const TopoDS_Edge& circularEdge,
        double offset,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        requireAnnotationOptions(textHeight, arrowSize);
        if (!std::isfinite(offset)) throw std::invalid_argument("Annotation offset must be finite.");
        requireCircularEdge(circularEdge);
        BRepAdaptor_Curve curve(circularEdge);
        const gp_Circ circle = curve.Circle();
        const gp_Pnt center = circle.Location();
        const gp_Pnt circlePoint = curve.Value(curve.FirstParameter());
        const gp_Vec radiusVector(center, circlePoint);
        const double radius = radiusVector.Magnitude();
        const gp_Dir diameterDirection(radiusVector);
        const gp_Dir normal = circle.Axis().Direction();
        const gp_Dir offsetDirection = normal.Crossed(diameterDirection);
        const gp_Pnt firstCirclePoint = center.Translated(gp_Vec(diameterDirection) * -radius);
        const gp_Pnt secondCirclePoint = center.Translated(gp_Vec(diameterDirection) * radius);
        const gp_Vec offsetVector = gp_Vec(offsetDirection) * offset;
        const gp_Pnt firstDimension = firstCirclePoint.Translated(offsetVector);
        const gp_Pnt secondDimension = secondCirclePoint.Translated(offsetVector);

        ShapeCollector collector;
        collector.add(line(firstCirclePoint, firstDimension));
        collector.add(line(secondCirclePoint, secondDimension));
        collector.add(line(firstDimension, secondDimension));
        addArrow(collector, firstDimension, diameterDirection, offsetDirection, arrowSize);
        addArrow(collector, secondDimension, diameterDirection.Reversed(), offsetDirection, arrowSize);

        const gp_Pnt midpoint(
            (firstDimension.X() + secondDimension.X()) * 0.5,
            (firstDimension.Y() + secondDimension.Y()) * 0.5,
            (firstDimension.Z() + secondDimension.Z()) * 0.5);
        const double textSide = offset < 0.0 ? -1.0 : 1.0;
        const gp_Pnt textPosition = midpoint.Translated(gp_Vec(offsetDirection) * (textHeight * 0.35 * textSide));
        const std::string label = std::string("\xC3\x98") + formatValue(radius * 2.0);
        collector.add(buildBRepText(
            label.c_str(), fontName,
            { textPosition.X(), textPosition.Y(), textPosition.Z() },
            { normal.X(), normal.Y(), normal.Z() },
            { diameterDirection.X(), diameterDirection.Y(), diameterDirection.Z() },
            textHeight, 0.0, false, false,
            Graphic3d_HTA_CENTER, Graphic3d_VTA_CENTER));
        return collector.shape();
    }
}
