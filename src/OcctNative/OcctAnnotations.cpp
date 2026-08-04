#include "OcctInternal.hxx"

#include <AIS_TextLabel.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <Prs3d_DimensionAspect.hxx>
#include <Precision.hxx>
#include <PrsDim_AngleDimension.hxx>
#include <PrsDim_DiameterDimension.hxx>
#include <PrsDim_Dimension.hxx>
#include <PrsDim_LengthDimension.hxx>
#include <PrsDim_RadiusDimension.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <gp_Pln.hxx>

using namespace OcctBridge;

namespace
{
    TCollection_ExtendedString extended(const char* text)
    {
        return TCollection_ExtendedString(text == nullptr ? "" : text, Standard_True);
    }

    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }

    void configureDimension(const Handle(PrsDim_Dimension)& dimension, double flyout, double r, double g, double b)
    {
        Handle(Prs3d_DimensionAspect) aspect = new Prs3d_DimensionAspect();
        aspect->SetCommonColor(color(r,g,b));
        dimension->SetDimensionAspect(aspect);
        dimension->SetFlyout(flyout);
        if (!dimension->IsValid()) throw std::runtime_error("Dimension geometry is not valid for this annotation type.");
    }

    gp_Pln dimensionPlaneForEdge(const TopoDS_Edge& edge)
    {
        BRepAdaptor_Curve curve(edge);
        const gp_Pnt first = curve.Value(curve.FirstParameter());
        const gp_Pnt last = curve.Value(curve.LastParameter());
        gp_Vec edgeVector(first,last);
        if(edgeVector.SquareMagnitude()<=Precision::SquareConfusion())throw std::runtime_error("Edge has zero length.");
        gp_Dir edgeDirection(edgeVector);
        gp_Dir reference(0.0,0.0,1.0);
        if(std::abs(edgeDirection.Dot(reference))>0.95)reference=gp_Dir(1.0,0.0,0.0);
        return gp_Pln(first,edgeDirection.Crossed(reference));
    }
}

extern "C"
{
    OcctObjectId occt_add_text(OcctHandle h, const char* text, OcctPoint3d position, double height, double r, double g, double b, int zoomable)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            requirePositive(height,"Text height");Handle(AIS_TextLabel) label=new AIS_TextLabel();label->SetText(extended(text));label->SetPosition(point(position));label->SetHeight(height);label->SetZoomable(zoomable!=0);label->SetColor(color(r,g,b));
            return e->addPresentation(label,OcctObject_Text,"Text");
        });
    }

    int occt_set_text(OcctHandle h, OcctObjectId textId, const char* text)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]{ObjectEntry* entry=e->findObject(textId);if(!entry||entry->kind!=OcctObject_Text)throw std::invalid_argument("Text ID does not exist.");Handle(AIS_TextLabel) label=Handle(AIS_TextLabel)::DownCast(entry->presentation);label->SetText(extended(text));e->context->Redisplay(label,Standard_True);});
    }

    int occt_set_text_position(OcctHandle h, OcctObjectId textId, OcctPoint3d position)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]{ObjectEntry* entry=e->findObject(textId);if(!entry||entry->kind!=OcctObject_Text)throw std::invalid_argument("Text ID does not exist.");Handle(AIS_TextLabel) label=Handle(AIS_TextLabel)::DownCast(entry->presentation);label->SetPosition(point(position));e->context->Redisplay(label,Standard_True);});
    }

    int occt_set_text_height(OcctHandle h, OcctObjectId textId, double height)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]{requirePositive(height,"Text height");ObjectEntry* entry=e->findObject(textId);if(!entry||entry->kind!=OcctObject_Text)throw std::invalid_argument("Text ID does not exist.");Handle(AIS_TextLabel) label=Handle(AIS_TextLabel)::DownCast(entry->presentation);label->SetHeight(height);e->context->Redisplay(label,Standard_True);});
    }

    int occt_set_text_font(OcctHandle h, OcctObjectId textId, const char* fontName)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]{ObjectEntry* entry=e->findObject(textId);if(!entry||entry->kind!=OcctObject_Text)throw std::invalid_argument("Text ID does not exist.");Handle(AIS_TextLabel) label=Handle(AIS_TextLabel)::DownCast(entry->presentation);label->SetFont(fontName==nullptr?"":fontName);e->context->Redisplay(label,Standard_True);});
    }

    int occt_set_text_angle(OcctHandle h, OcctObjectId textId, double angleDegrees)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]{ObjectEntry* entry=e->findObject(textId);if(!entry||entry->kind!=OcctObject_Text)throw std::invalid_argument("Text ID does not exist.");Handle(AIS_TextLabel) label=Handle(AIS_TextLabel)::DownCast(entry->presentation);label->SetAngle(angleDegrees*3.14159265358979323846/180.0);e->context->Redisplay(label,Standard_True);});
    }

    int occt_set_text_zoomable(OcctHandle h, OcctObjectId textId, int zoomable)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]{ObjectEntry* entry=e->findObject(textId);if(!entry||entry->kind!=OcctObject_Text)throw std::invalid_argument("Text ID does not exist.");Handle(AIS_TextLabel) label=Handle(AIS_TextLabel)::DownCast(entry->presentation);label->SetZoomable(zoomable!=0);e->context->Redisplay(label,Standard_True);});
    }

    int occt_set_dimension_flyout(OcctHandle h, OcctObjectId dimensionId, double flyout)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return execute(e,[&]{ObjectEntry* entry=e->findObject(dimensionId);if(!entry||entry->kind!=OcctObject_Dimension)throw std::invalid_argument("Dimension ID does not exist.");Handle(PrsDim_Dimension) dimension=Handle(PrsDim_Dimension)::DownCast(entry->presentation);if(dimension.IsNull())throw std::runtime_error("Dimension presentation type is invalid.");dimension->SetFlyout(flyout);e->context->Redisplay(dimension,Standard_True);});
    }

    OcctObjectId occt_add_length_dimension(OcctHandle h, OcctObjectId edgeId, double flyout, double r, double g, double b)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{ObjectEntry& edge=requiredShape(e,edgeId);if(edge.shape.ShapeType()!=TopAbs_EDGE)throw std::invalid_argument("Length dimension input must be an edge.");Handle(PrsDim_LengthDimension) dimension=new PrsDim_LengthDimension(TopoDS::Edge(edge.shape),dimensionPlaneForEdge(TopoDS::Edge(edge.shape)));configureDimension(dimension,flyout,r,g,b);return e->addPresentation(dimension,OcctObject_Dimension,"LengthDimension");});
    }

    OcctObjectId occt_add_angle_dimension(OcctHandle h, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double flyout, double r, double g, double b)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{ObjectEntry& first=requiredShape(e,firstEdgeId);ObjectEntry& second=requiredShape(e,secondEdgeId);if(first.shape.ShapeType()!=TopAbs_EDGE||second.shape.ShapeType()!=TopAbs_EDGE)throw std::invalid_argument("Angle dimension inputs must be edges.");Handle(PrsDim_AngleDimension) dimension=new PrsDim_AngleDimension(TopoDS::Edge(first.shape),TopoDS::Edge(second.shape));configureDimension(dimension,flyout,r,g,b);return e->addPresentation(dimension,OcctObject_Dimension,"AngleDimension");});
    }

    OcctObjectId occt_add_radius_dimension(OcctHandle h, OcctObjectId shapeId, double flyout, double r, double g, double b)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{ObjectEntry& shape=requiredShape(e,shapeId);Handle(PrsDim_RadiusDimension) dimension=new PrsDim_RadiusDimension(shape.shape);configureDimension(dimension,flyout,r,g,b);return e->addPresentation(dimension,OcctObject_Dimension,"RadiusDimension");});
    }

    OcctObjectId occt_add_diameter_dimension(OcctHandle h, OcctObjectId shapeId, double flyout, double r, double g, double b)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{ObjectEntry& shape=requiredShape(e,shapeId);Handle(PrsDim_DiameterDimension) dimension=new PrsDim_DiameterDimension(shape.shape);configureDimension(dimension,flyout,r,g,b);return e->addPresentation(dimension,OcctObject_Dimension,"DiameterDimension");});
    }
}
