#include "OcctInternal.hxx"

#include <BRep_Builder.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <Font_FontAspect.hxx>
#include <Font_StrictLevel.hxx>
#include <GeomAbs_CurveType.hxx>
#include <Graphic3d_HorizontalTextAlignment.hxx>
#include <Graphic3d_VerticalTextAlignment.hxx>
#include <NCollection_String.hxx>
#include <Precision.hxx>
#include <StdPrs_BRepFont.hxx>
#include <StdPrs_BRepTextBuilder.hxx>
#include <TCollection_AsciiString.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Edge.hxx>
#include <gp_Ax1.hxx>
#include <gp_Ax2.hxx>
#include <gp_Ax3.hxx>
#include <gp_Circ.hxx>

#include <algorithm>
#include <array>
#include <cmath>
#include <iomanip>
#include <limits>
#include <sstream>
#include <string>
#include <vector>

using namespace OcctBridge;

namespace
{
    class ShapeCollector
    {
    public:
        ShapeCollector()
        {
            myBuilder.MakeCompound(myCompound);
        }

        void Add(const TopoDS_Shape& shape)
        {
            if (!shape.IsNull()) myBuilder.Add(myCompound, shape);
        }

        const TopoDS_Compound& Shape() const
        {
            return myCompound;
        }

    private:
        BRep_Builder myBuilder;
        TopoDS_Compound myCompound;
    };

    Font_FontAspect fontAspect(int bold, int italic)
    {
        if (bold != 0 && italic != 0) return Font_FA_BoldItalic;
        if (bold != 0) return Font_FA_Bold;
        if (italic != 0) return Font_FA_Italic;
        return Font_FA_Regular;
    }

    bool initializeFont(
        StdPrs_BRepFont& font,
        const char* fontName,
        Font_FontAspect aspect,
        double height)
    {
        const std::string requested = fontName == nullptr ? std::string() : std::string(fontName);
        std::vector<std::string> candidates;
        if (!requested.empty()) candidates.push_back(requested);
        candidates.emplace_back("Microsoft YaHei UI");
        candidates.emplace_back("Microsoft YaHei");
        candidates.emplace_back("Arial");
        candidates.emplace_back("DejaVu Sans");

        std::vector<std::string> attempted;
        for (const std::string& candidate : candidates)
        {
            if (candidate.empty()
                || std::find(attempted.begin(), attempted.end(), candidate) != attempted.end())
            {
                continue;
            }

            attempted.push_back(candidate);
            if (font.FindAndInit(
                    TCollection_AsciiString(candidate.c_str()),
                    aspect,
                    height,
                    Font_StrictLevel_Any))
            {
                return true;
            }
        }
        return false;
    }

    TopoDS_Shape buildTextShape(
        const char* text,
        OcctPoint3d position,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double height,
        double extrusionDepth,
        const char* fontName,
        int bold,
        int italic,
        Graphic3d_HorizontalTextAlignment horizontalAlignment,
        Graphic3d_VerticalTextAlignment verticalAlignment)
    {
        if (text == nullptr || text[0] == '\0')
        {
            throw std::invalid_argument("Text is empty.");
        }
        requirePositive(height, "Text height");
        if (!std::isfinite(extrusionDepth) || extrusionDepth < 0.0)
        {
            throw std::invalid_argument("Text extrusion depth must be non-negative.");
        }

        StdPrs_BRepFont font;
        if (!initializeFont(font, fontName, fontAspect(bold, italic), height))
        {
            throw std::runtime_error("No usable system font was found for BRep text generation.");
        }

        StdPrs_BRepTextBuilder builder;
        const gp_Ax3 placement(point(position), direction(normal), direction(xDirection));
        TopoDS_Shape result = builder.Perform(
            font,
            NCollection_String(text),
            placement,
            horizontalAlignment,
            verticalAlignment);
        if (result.IsNull())
        {
            throw std::runtime_error("BRep text generation returned an empty shape.");
        }

        if (extrusionDepth > Precision::Confusion())
        {
            BRepPrimAPI_MakePrism prism(
                result,
                gp_Vec(direction(normal)) * extrusionDepth,
                Standard_True,
                Standard_True);
            if (!prism.IsDone() || prism.Shape().IsNull())
            {
                throw std::runtime_error("BRep text extrusion failed.");
            }
            result = prism.Shape();
        }
        return result;
    }

    TopoDS_Shape makeLine(const gp_Pnt& start, const gp_Pnt& end)
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
        requirePositive(size, "Arrow size");
        const gp_Pnt base = tip.Translated(gp_Vec(inwardDirection) * size);
        const gp_Vec side = gp_Vec(sideDirection) * (size * 0.38);
        const gp_Pnt first = base.Translated(side);
        const gp_Pnt second = base.Translated(side.Reversed());

        BRepBuilderAPI_MakePolygon polygon;
        polygon.Add(tip);
        polygon.Add(first);
        polygon.Add(second);
        polygon.Close();
        if (!polygon.IsDone()) throw std::runtime_error("Annotation arrow creation failed.");

        BRepBuilderAPI_MakeFace face(polygon.Wire(), Standard_True);
        if (face.IsDone())
        {
            collector.Add(face.Shape());
        }
        else
        {
            collector.Add(polygon.Shape());
        }
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

    TopoDS_Edge requireLineEdge(Engine* engine, OcctObjectId id, const char* name)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr || entry->shape.ShapeType() != TopAbs_EDGE)
        {
            throw std::invalid_argument(std::string(name) + " must be an edge.");
        }
        const TopoDS_Edge edge = TopoDS::Edge(entry->shape);
        const BRepAdaptor_Curve curve(edge);
        if (curve.GetType() != GeomAbs_Line)
        {
            throw std::invalid_argument(std::string(name) + " must be a straight edge.");
        }
        return edge;
    }

    TopoDS_Edge requireCircularEdge(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr || entry->shape.ShapeType() != TopAbs_EDGE)
        {
            throw std::invalid_argument("Circular annotation input must be an edge.");
        }
        const TopoDS_Edge edge = TopoDS::Edge(entry->shape);
        const BRepAdaptor_Curve curve(edge);
        if (curve.GetType() != GeomAbs_Circle)
        {
            throw std::invalid_argument("Circular annotation input must be a circular edge.");
        }
        return edge;
    }

    std::array<gp_Pnt, 2> edgeEndpoints(const TopoDS_Edge& edge)
    {
        const BRepAdaptor_Curve curve(edge);
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
                const double distance = first[static_cast<std::size_t>(i)].Distance(
                    second[static_cast<std::size_t>(j)]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    firstIndex = i;
                    secondIndex = j;
                }
            }
        }

        const gp_Pnt vertex(
            (first[static_cast<std::size_t>(firstIndex)].X()
                + second[static_cast<std::size_t>(secondIndex)].X()) * 0.5,
            (first[static_cast<std::size_t>(firstIndex)].Y()
                + second[static_cast<std::size_t>(secondIndex)].Y()) * 0.5,
            (first[static_cast<std::size_t>(firstIndex)].Z()
                + second[static_cast<std::size_t>(secondIndex)].Z()) * 0.5);
        const gp_Pnt firstFar = first[static_cast<std::size_t>(1 - firstIndex)];
        const gp_Pnt secondFar = second[static_cast<std::size_t>(1 - secondIndex)];
        const gp_Vec firstVector(vertex, firstFar);
        const gp_Vec secondVector(vertex, secondFar);
        if (firstVector.SquareMagnitude() <= Precision::SquareConfusion()
            || secondVector.SquareMagnitude() <= Precision::SquareConfusion())
        {
            throw std::runtime_error("Angular annotation edges have invalid endpoints.");
        }
        return EdgeRay{ vertex, gp_Dir(firstVector), gp_Dir(secondVector) };
    }
}

extern "C"
{
    OcctObjectId occt_make_text_shape(
        OcctHandle h,
        const char* text,
        OcctPoint3d position,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double height,
        double extrusionDepth,
        const char* fontName,
        int bold,
        int italic)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            const TopoDS_Shape result = buildTextShape(
                text,
                position,
                normal,
                xDirection,
                height,
                extrusionDepth,
                fontName,
                bold,
                italic,
                Graphic3d_HTA_LEFT,
                Graphic3d_VTA_BOTTOM);
            return e->addShape(result, false, "BRepText");
        });
    }

    OcctObjectId occt_make_length_annotation_shape(
        OcctHandle h,
        OcctObjectId edgeId,
        double flyout,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            requirePositive(textHeight, "Text height");
            requirePositive(arrowSize, "Arrow size");
            const TopoDS_Edge edge = requireLineEdge(e, edgeId, "Length annotation input");
            const auto endpoints = edgeEndpoints(edge);
            const gp_Vec edgeVector(endpoints[0], endpoints[1]);
            const double length = edgeVector.Magnitude();
            if (length <= Precision::Confusion())
            {
                throw std::runtime_error("Length annotation edge has zero length.");
            }

            const gp_Dir edgeDirection(edgeVector);
            gp_Dir reference(0.0, 0.0, 1.0);
            if (std::abs(edgeDirection.Dot(reference)) > 0.95)
            {
                reference = gp_Dir(1.0, 0.0, 0.0);
            }
            const gp_Dir planeNormal = edgeDirection.Crossed(reference);
            const gp_Dir offsetDirection = planeNormal.Crossed(edgeDirection);
            const gp_Vec offset = gp_Vec(offsetDirection) * flyout;
            const gp_Pnt firstDimension = endpoints[0].Translated(offset);
            const gp_Pnt secondDimension = endpoints[1].Translated(offset);

            ShapeCollector collector;
            collector.Add(makeLine(endpoints[0], firstDimension));
            collector.Add(makeLine(endpoints[1], secondDimension));
            collector.Add(makeLine(firstDimension, secondDimension));
            addArrow(collector, firstDimension, edgeDirection, offsetDirection, arrowSize);
            addArrow(collector, secondDimension, edgeDirection.Reversed(), offsetDirection, arrowSize);

            const gp_Pnt midpoint(
                (firstDimension.X() + secondDimension.X()) * 0.5,
                (firstDimension.Y() + secondDimension.Y()) * 0.5,
                (firstDimension.Z() + secondDimension.Z()) * 0.5);
            const double textSide = flyout < 0.0 ? -1.0 : 1.0;
            const gp_Pnt textPosition = midpoint.Translated(
                gp_Vec(offsetDirection) * (textHeight * 0.35 * textSide));
            const std::string label = formatValue(length);
            collector.Add(buildTextShape(
                label.c_str(),
                OcctPoint3d{ textPosition.X(), textPosition.Y(), textPosition.Z() },
                OcctVector3d{ planeNormal.X(), planeNormal.Y(), planeNormal.Z() },
                OcctVector3d{ edgeDirection.X(), edgeDirection.Y(), edgeDirection.Z() },
                textHeight,
                0.0,
                fontName,
                0,
                0,
                Graphic3d_HTA_CENTER,
                Graphic3d_VTA_CENTER));
            return e->addShape(collector.Shape(), false, "VectorLengthDimension");
        });
    }

    OcctObjectId occt_make_angle_annotation_shape(
        OcctHandle h,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double radius,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            requirePositive(radius, "Angular annotation radius");
            requirePositive(textHeight, "Text height");
            requirePositive(arrowSize, "Arrow size");
            const TopoDS_Edge firstEdge = requireLineEdge(e, firstEdgeId, "First angular annotation input");
            const TopoDS_Edge secondEdge = requireLineEdge(e, secondEdgeId, "Second angular annotation input");
            const EdgeRay rays = angleRays(firstEdge, secondEdge);
            const gp_Vec cross = gp_Vec(rays.firstDirection).Crossed(gp_Vec(rays.secondDirection));
            if (cross.SquareMagnitude() <= Precision::SquareConfusion())
            {
                throw std::invalid_argument("Angular annotation edges must not be parallel.");
            }
            const gp_Dir normal(cross);
            const double angle = rays.firstDirection.Angle(rays.secondDirection);
            if (angle <= Precision::Angular())
            {
                throw std::invalid_argument("Angular annotation angle is too small.");
            }

            const gp_Pnt firstPoint = rays.vertex.Translated(gp_Vec(rays.firstDirection) * radius);
            const gp_Pnt secondPoint = rays.vertex.Translated(gp_Vec(rays.secondDirection) * radius);
            ShapeCollector collector;
            collector.Add(makeLine(rays.vertex, firstPoint));
            collector.Add(makeLine(rays.vertex, secondPoint));

            const gp_Circ circle(gp_Ax2(rays.vertex, normal, rays.firstDirection), radius);
            BRepBuilderAPI_MakeEdge arc(circle, 0.0, angle);
            if (!arc.IsDone()) throw std::runtime_error("Angular annotation arc creation failed.");
            collector.Add(arc.Shape());

            const gp_Dir firstTangent = normal.Crossed(rays.firstDirection);
            const gp_Dir secondTangent = normal.Crossed(rays.secondDirection);
            addArrow(collector, firstPoint, firstTangent, rays.firstDirection, arrowSize);
            addArrow(collector, secondPoint, secondTangent.Reversed(), rays.secondDirection, arrowSize);

            gp_Dir middleDirection = rays.firstDirection;
            middleDirection.Rotate(gp_Ax1(rays.vertex, normal), angle * 0.5);
            const gp_Dir textDirection = normal.Crossed(middleDirection);
            const gp_Pnt textPosition = rays.vertex.Translated(
                gp_Vec(middleDirection) * (radius + textHeight * 0.9));
            const std::string label = formatValue(angle * 180.0 / 3.14159265358979323846) + "\xC2\xB0";
            collector.Add(buildTextShape(
                label.c_str(),
                OcctPoint3d{ textPosition.X(), textPosition.Y(), textPosition.Z() },
                OcctVector3d{ normal.X(), normal.Y(), normal.Z() },
                OcctVector3d{ textDirection.X(), textDirection.Y(), textDirection.Z() },
                textHeight,
                0.0,
                fontName,
                0,
                0,
                Graphic3d_HTA_CENTER,
                Graphic3d_VTA_CENTER));
            return e->addShape(collector.Shape(), false, "VectorAngleDimension");
        });
    }

    OcctObjectId occt_make_radius_annotation_shape(
        OcctHandle h,
        OcctObjectId circularEdgeId,
        double flyout,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            requirePositive(textHeight, "Text height");
            requirePositive(arrowSize, "Arrow size");
            const TopoDS_Edge edge = requireCircularEdge(e, circularEdgeId);
            const BRepAdaptor_Curve curve(edge);
            const gp_Circ circle = curve.Circle();
            const gp_Pnt center = circle.Location();
            const gp_Pnt circlePoint = curve.Value(curve.FirstParameter());
            const gp_Vec radiusVector(center, circlePoint);
            const double radius = radiusVector.Magnitude();
            const gp_Dir radiusDirection(radiusVector);
            const gp_Dir normal = circle.Axis().Direction();
            const gp_Dir sideDirection = normal.Crossed(radiusDirection);
            const gp_Pnt leaderEnd = circlePoint.Translated(gp_Vec(radiusDirection) * flyout);

            ShapeCollector collector;
            collector.Add(makeLine(center, leaderEnd));
            addArrow(collector, circlePoint, radiusDirection, sideDirection, arrowSize);
            const gp_Pnt textPosition = leaderEnd.Translated(gp_Vec(sideDirection) * (textHeight * 0.3));
            const std::string label = std::string("R") + formatValue(radius);
            collector.Add(buildTextShape(
                label.c_str(),
                OcctPoint3d{ textPosition.X(), textPosition.Y(), textPosition.Z() },
                OcctVector3d{ normal.X(), normal.Y(), normal.Z() },
                OcctVector3d{ radiusDirection.X(), radiusDirection.Y(), radiusDirection.Z() },
                textHeight,
                0.0,
                fontName,
                0,
                0,
                Graphic3d_HTA_LEFT,
                Graphic3d_VTA_CENTER));
            return e->addShape(collector.Shape(), false, "VectorRadiusDimension");
        });
    }

    OcctObjectId occt_make_diameter_annotation_shape(
        OcctHandle h,
        OcctObjectId circularEdgeId,
        double flyout,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            requirePositive(textHeight, "Text height");
            requirePositive(arrowSize, "Arrow size");
            const TopoDS_Edge edge = requireCircularEdge(e, circularEdgeId);
            const BRepAdaptor_Curve curve(edge);
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
            const gp_Vec offset = gp_Vec(offsetDirection) * flyout;
            const gp_Pnt firstDimension = firstCirclePoint.Translated(offset);
            const gp_Pnt secondDimension = secondCirclePoint.Translated(offset);

            ShapeCollector collector;
            collector.Add(makeLine(firstCirclePoint, firstDimension));
            collector.Add(makeLine(secondCirclePoint, secondDimension));
            collector.Add(makeLine(firstDimension, secondDimension));
            addArrow(collector, firstDimension, diameterDirection, offsetDirection, arrowSize);
            addArrow(collector, secondDimension, diameterDirection.Reversed(), offsetDirection, arrowSize);

            const gp_Pnt midpoint(
                (firstDimension.X() + secondDimension.X()) * 0.5,
                (firstDimension.Y() + secondDimension.Y()) * 0.5,
                (firstDimension.Z() + secondDimension.Z()) * 0.5);
            const double textSide = flyout < 0.0 ? -1.0 : 1.0;
            const gp_Pnt textPosition = midpoint.Translated(
                gp_Vec(offsetDirection) * (textHeight * 0.35 * textSide));
            const std::string label = std::string("\xC3\x98") + formatValue(radius * 2.0);
            collector.Add(buildTextShape(
                label.c_str(),
                OcctPoint3d{ textPosition.X(), textPosition.Y(), textPosition.Z() },
                OcctVector3d{ normal.X(), normal.Y(), normal.Z() },
                OcctVector3d{ diameterDirection.X(), diameterDirection.Y(), diameterDirection.Z() },
                textHeight,
                0.0,
                fontName,
                0,
                0,
                Graphic3d_HTA_CENTER,
                Graphic3d_VTA_CENTER));
            return e->addShape(collector.Shape(), false, "VectorDiameterDimension");
        });
    }
}
