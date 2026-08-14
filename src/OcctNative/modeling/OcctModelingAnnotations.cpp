#include "modeling/OcctModelingAnnotations.h"
#include "geometry/OcctBRepTextBuilder.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRep_Builder.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <GeomAbs_CurveType.hxx>
#include <Graphic3d_HorizontalTextAlignment.hxx>
#include <Graphic3d_VerticalTextAlignment.hxx>
#include <Precision.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Edge.hxx>
#include <gp_Ax1.hxx>
#include <gp_Ax2.hxx>
#include <gp_Circ.hxx>

#include <array>
#include <cmath>
#include <iomanip>
#include <limits>
#include <sstream>
#include <string>

using namespace OcctModelingInternal;

namespace
{
    constexpr std::uint32_t TextOptionsApiVersion = 1;
    constexpr std::uint32_t AnnotationOptionsApiVersion = 1;

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

    Graphic3d_HorizontalTextAlignment horizontalAlignment(int value)
    {
        switch (value)
        {
        case OcctTextHorizontal_Left: return Graphic3d_HTA_LEFT;
        case OcctTextHorizontal_Center: return Graphic3d_HTA_CENTER;
        case OcctTextHorizontal_Right: return Graphic3d_HTA_RIGHT;
        default: throw std::invalid_argument("Unsupported horizontal text alignment.");
        }
    }

    Graphic3d_VerticalTextAlignment verticalAlignment(int value)
    {
        switch (value)
        {
        case OcctTextVertical_Bottom: return Graphic3d_VTA_BOTTOM;
        case OcctTextVertical_Center: return Graphic3d_VTA_CENTER;
        case OcctTextVertical_Top: return Graphic3d_VTA_TOP;
        default: throw std::invalid_argument("Unsupported vertical text alignment.");
        }
    }

    void validateTextOptions(const OcctBRepTextOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("BRep text options are null.");
        if (options->structSize < sizeof(OcctBRepTextOptions) || options->apiVersion != TextOptionsApiVersion)
            throw std::invalid_argument("Unsupported BRep text options size or version.");
    }

    void validateAnnotationOptions(const OcctBRepAnnotationOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("BRep annotation options are null.");
        if (options->structSize < sizeof(OcctBRepAnnotationOptions) || options->apiVersion != AnnotationOptionsApiVersion)
            throw std::invalid_argument("Unsupported BRep annotation options size or version.");
        OcctBridge::requirePositive(options->textHeight, "Text height");
        OcctBridge::requirePositive(options->arrowSize, "Arrow size");
        if (!std::isfinite(options->offset)) throw std::invalid_argument("Annotation offset must be finite.");
    }

    TopoDS_Shape line(const gp_Pnt& start, const gp_Pnt& end)
    {
        if (start.Distance(end) <= Precision::Confusion()) return TopoDS_Shape();
        BRepBuilderAPI_MakeEdge edge(start, end);
        if (!edge.IsDone()) throw std::runtime_error("Annotation line creation failed.");
        return edge.Shape();
    }

    void addArrow(ShapeCollector& collector, const gp_Pnt& tip, const gp_Dir& inward, const gp_Dir& side, double size)
    {
        const gp_Pnt base = tip.Translated(gp_Vec(inward) * size);
        const gp_Vec sideVector = gp_Vec(side) * (size * 0.38);
        BRepBuilderAPI_MakePolygon polygon;
        polygon.Add(tip);
        polygon.Add(base.Translated(sideVector));
        polygon.Add(base.Translated(sideVector.Reversed()));
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

    TopoDS_Edge requireLineEdge(ModelSession* model, OcctObjectId id, const char* name)
    {
        const TopoDS_Shape& shape = model->requireShape(id);
        if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument(std::string(name) + " must be an edge.");
        const TopoDS_Edge edge = TopoDS::Edge(shape);
        BRepAdaptor_Curve curve(edge);
        if (curve.GetType() != GeomAbs_Line) throw std::invalid_argument(std::string(name) + " must be a straight edge.");
        return edge;
    }

    TopoDS_Edge requireCircularEdge(ModelSession* model, OcctObjectId id)
    {
        const TopoDS_Shape& shape = model->requireShape(id);
        if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Circular annotation input must be an edge.");
        const TopoDS_Edge edge = TopoDS::Edge(shape);
        BRepAdaptor_Curve curve(edge);
        if (curve.GetType() != GeomAbs_Circle) throw std::invalid_argument("Circular annotation input must be a circular edge.");
        return edge;
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
        double closest = std::numeric_limits<double>::max();
        int firstIndex = 0;
        int secondIndex = 0;
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                const double distance = first[static_cast<std::size_t>(i)].Distance(second[static_cast<std::size_t>(j)]);
                if (distance < closest)
                {
                    closest = distance;
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
        if (firstVector.SquareMagnitude() <= Precision::SquareConfusion() || secondVector.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::runtime_error("Angular annotation edges have invalid endpoints.");
        return { vertex, gp_Dir(firstVector), gp_Dir(secondVector) };
    }

    OcctStatus finish(ModelSession* model, OcctObjectId* output, const TopoDS_Shape& shape)
    {
        *output = model->addShape(shape);
        return OcctStatus_Ok;
    }

    OcctStatus executeStatus(ModelSession* model, OcctObjectId* output, const std::function<TopoDS_Shape()>& factory)
    {
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        model->errors.clear();
        if (output == nullptr)
        {
            model->errors.set(OcctStatus_ErrorInvalidArgument, "Result shape ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }
        *output = 0;
        if (!execute(model, [&] { finish(model, output, factory()); })) return model->errors.code;
        return OcctStatus_Ok;
    }
}

extern "C"
{
    OcctStatus occt_model_brep_text_create(
        OcctModelingSessionHandle session,
        const char* utf8Text,
        const char* fontName,
        const OcctBRepTextOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeStatus(model, resultShapeId, [&]
        {
            validateTextOptions(options);
            return buildBRepText(
                utf8Text,
                fontName,
                options->position,
                options->normal,
                options->xDirection,
                options->height,
                options->extrusionDepth,
                options->bold != 0,
                options->italic != 0,
                horizontalAlignment(options->horizontalAlignment),
                verticalAlignment(options->verticalAlignment));
        });
    }

    OcctStatus occt_model_length_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId edgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            const TopoDS_Edge edge = requireLineEdge(model, edgeId, "Length annotation input");
            const auto endpoints = edgeEndpoints(edge);
            const gp_Vec edgeVector(endpoints[0], endpoints[1]);
            const double lengthValue = edgeVector.Magnitude();
            if (lengthValue <= Precision::Confusion()) throw std::runtime_error("Length annotation edge has zero length.");
            const gp_Dir edgeDirection(edgeVector);
            gp_Dir reference(0, 0, 1);
            if (std::abs(edgeDirection.Dot(reference)) > 0.95) reference = gp_Dir(1, 0, 0);
            const gp_Dir planeNormal = edgeDirection.Crossed(reference);
            const gp_Dir offsetDirection = planeNormal.Crossed(edgeDirection);
            const gp_Vec offset = gp_Vec(offsetDirection) * options->offset;
            const gp_Pnt firstDimension = endpoints[0].Translated(offset);
            const gp_Pnt secondDimension = endpoints[1].Translated(offset);

            ShapeCollector collector;
            collector.add(line(endpoints[0], firstDimension));
            collector.add(line(endpoints[1], secondDimension));
            collector.add(line(firstDimension, secondDimension));
            addArrow(collector, firstDimension, edgeDirection, offsetDirection, options->arrowSize);
            addArrow(collector, secondDimension, edgeDirection.Reversed(), offsetDirection, options->arrowSize);

            const gp_Pnt midpoint((firstDimension.X() + secondDimension.X()) * 0.5, (firstDimension.Y() + secondDimension.Y()) * 0.5, (firstDimension.Z() + secondDimension.Z()) * 0.5);
            const double textSide = options->offset < 0.0 ? -1.0 : 1.0;
            const gp_Pnt textPosition = midpoint.Translated(gp_Vec(offsetDirection) * (options->textHeight * 0.35 * textSide));
            const std::string label = formatValue(lengthValue);
            collector.add(buildBRepText(label.c_str(), fontName,
                { textPosition.X(), textPosition.Y(), textPosition.Z() },
                { planeNormal.X(), planeNormal.Y(), planeNormal.Z() },
                { edgeDirection.X(), edgeDirection.Y(), edgeDirection.Z() },
                options->textHeight, 0.0, false, false, Graphic3d_HTA_CENTER, Graphic3d_VTA_CENTER));
            return collector.shape();
        });
    }

    OcctStatus occt_model_angle_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            OcctBridge::requirePositive(options->offset, "Angular annotation radius");
            const TopoDS_Edge firstEdge = requireLineEdge(model, firstEdgeId, "First angular annotation input");
            const TopoDS_Edge secondEdge = requireLineEdge(model, secondEdgeId, "Second angular annotation input");
            const EdgeRay rays = angleRays(firstEdge, secondEdge);
            const gp_Vec cross = gp_Vec(rays.firstDirection).Crossed(gp_Vec(rays.secondDirection));
            if (cross.SquareMagnitude() <= Precision::SquareConfusion()) throw std::invalid_argument("Angular annotation edges must not be parallel.");
            const gp_Dir normal(cross);
            const double angle = rays.firstDirection.Angle(rays.secondDirection);
            if (angle <= Precision::Angular()) throw std::invalid_argument("Angular annotation angle is too small.");

            const gp_Pnt firstPoint = rays.vertex.Translated(gp_Vec(rays.firstDirection) * options->offset);
            const gp_Pnt secondPoint = rays.vertex.Translated(gp_Vec(rays.secondDirection) * options->offset);
            ShapeCollector collector;
            collector.add(line(rays.vertex, firstPoint));
            collector.add(line(rays.vertex, secondPoint));
            BRepBuilderAPI_MakeEdge arc(gp_Circ(gp_Ax2(rays.vertex, normal, rays.firstDirection), options->offset), 0.0, angle);
            if (!arc.IsDone()) throw std::runtime_error("Angular annotation arc creation failed.");
            collector.add(arc.Shape());
            addArrow(collector, firstPoint, normal.Crossed(rays.firstDirection), rays.firstDirection, options->arrowSize);
            addArrow(collector, secondPoint, normal.Crossed(rays.secondDirection).Reversed(), rays.secondDirection, options->arrowSize);

            gp_Dir middleDirection = rays.firstDirection;
            middleDirection.Rotate(gp_Ax1(rays.vertex, normal), angle * 0.5);
            const gp_Dir textDirection = normal.Crossed(middleDirection);
            const gp_Pnt textPosition = rays.vertex.Translated(gp_Vec(middleDirection) * (options->offset + options->textHeight * 0.9));
            const std::string label = formatValue(angle * 180.0 / 3.14159265358979323846) + "\xC2\xB0";
            collector.add(buildBRepText(label.c_str(), fontName,
                { textPosition.X(), textPosition.Y(), textPosition.Z() },
                { normal.X(), normal.Y(), normal.Z() },
                { textDirection.X(), textDirection.Y(), textDirection.Z() },
                options->textHeight, 0.0, false, false, Graphic3d_HTA_CENTER, Graphic3d_VTA_CENTER));
            return collector.shape();
        });
    }

    OcctStatus occt_model_radius_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId circularEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            const TopoDS_Edge edge = requireCircularEdge(model, circularEdgeId);
            BRepAdaptor_Curve curve(edge);
            const gp_Circ circleValue = curve.Circle();
            const gp_Pnt center = circleValue.Location();
            const gp_Pnt circlePoint = curve.Value(curve.FirstParameter());
            const gp_Vec radiusVector(center, circlePoint);
            const double radius = radiusVector.Magnitude();
            const gp_Dir radiusDirection(radiusVector);
            const gp_Dir normal = circleValue.Axis().Direction();
            const gp_Dir sideDirection = normal.Crossed(radiusDirection);
            const gp_Pnt leaderEnd = circlePoint.Translated(gp_Vec(radiusDirection) * options->offset);
            ShapeCollector collector;
            collector.add(line(center, leaderEnd));
            addArrow(collector, circlePoint, radiusDirection, sideDirection, options->arrowSize);
            const gp_Pnt textPosition = leaderEnd.Translated(gp_Vec(sideDirection) * (options->textHeight * 0.3));
            const std::string label = std::string("R") + formatValue(radius);
            collector.add(buildBRepText(label.c_str(), fontName,
                { textPosition.X(), textPosition.Y(), textPosition.Z() },
                { normal.X(), normal.Y(), normal.Z() },
                { radiusDirection.X(), radiusDirection.Y(), radiusDirection.Z() },
                options->textHeight, 0.0, false, false, Graphic3d_HTA_LEFT, Graphic3d_VTA_CENTER));
            return collector.shape();
        });
    }

    OcctStatus occt_model_diameter_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId circularEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            const TopoDS_Edge edge = requireCircularEdge(model, circularEdgeId);
            BRepAdaptor_Curve curve(edge);
            const gp_Circ circleValue = curve.Circle();
            const gp_Pnt center = circleValue.Location();
            const gp_Pnt circlePoint = curve.Value(curve.FirstParameter());
            const gp_Vec radiusVector(center, circlePoint);
            const double radius = radiusVector.Magnitude();
            const gp_Dir diameterDirection(radiusVector);
            const gp_Dir normal = circleValue.Axis().Direction();
            const gp_Dir offsetDirection = normal.Crossed(diameterDirection);
            const gp_Pnt firstCirclePoint = center.Translated(gp_Vec(diameterDirection) * -radius);
            const gp_Pnt secondCirclePoint = center.Translated(gp_Vec(diameterDirection) * radius);
            const gp_Vec offset = gp_Vec(offsetDirection) * options->offset;
            const gp_Pnt firstDimension = firstCirclePoint.Translated(offset);
            const gp_Pnt secondDimension = secondCirclePoint.Translated(offset);

            ShapeCollector collector;
            collector.add(line(firstCirclePoint, firstDimension));
            collector.add(line(secondCirclePoint, secondDimension));
            collector.add(line(firstDimension, secondDimension));
            addArrow(collector, firstDimension, diameterDirection, offsetDirection, options->arrowSize);
            addArrow(collector, secondDimension, diameterDirection.Reversed(), offsetDirection, options->arrowSize);
            const gp_Pnt midpoint((firstDimension.X() + secondDimension.X()) * 0.5, (firstDimension.Y() + secondDimension.Y()) * 0.5, (firstDimension.Z() + secondDimension.Z()) * 0.5);
            const double textSide = options->offset < 0.0 ? -1.0 : 1.0;
            const gp_Pnt textPosition = midpoint.Translated(gp_Vec(offsetDirection) * (options->textHeight * 0.35 * textSide));
            const std::string label = std::string("\xC3\x98") + formatValue(radius * 2.0);
            collector.add(buildBRepText(label.c_str(), fontName,
                { textPosition.X(), textPosition.Y(), textPosition.Z() },
                { normal.X(), normal.Y(), normal.Z() },
                { diameterDirection.X(), diameterDirection.Y(), diameterDirection.Z() },
                options->textHeight, 0.0, false, false, Graphic3d_HTA_CENTER, Graphic3d_VTA_CENTER));
            return collector.shape();
        });
    }
}
