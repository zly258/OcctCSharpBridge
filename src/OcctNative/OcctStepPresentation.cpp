#include "OcctInternal.hxx"

#include <AIS_ColoredShape.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <TopExp.hxx>
#include <TopLoc_Location.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFPrs.hxx>
#include <XCAFPrs_IndexedDataMapOfShapeStyle.hxx>

using namespace OcctBridge;

namespace
{
    Standard_Integer findPartnerIndex(
        const TopTools_IndexedMapOfShape& definitionSubShapes,
        const TopoDS_Shape& occurrenceSubShape)
    {
        for (Standard_Integer index = 1; index <= definitionSubShapes.Extent(); ++index)
        {
            if (definitionSubShapes(index).IsPartner(occurrenceSubShape)) return index;
        }
        return 0;
    }

    void applyCustomStyle(
        const Handle(AIS_ColoredShape)& presentation,
        const TopoDS_Shape& definitionSubShape,
        const XCAFPrs_Style& style)
    {
        if (presentation.IsNull() || definitionSubShape.IsNull()) return;

        const Handle(AIS_ColoredDrawer) drawer = presentation->CustomAspects(definitionSubShape);
        if (style.IsSetColorSurf())
        {
            const Quantity_ColorRGBA& rgba = style.GetColorSurfRGBA();
            presentation->SetCustomColor(definitionSubShape, rgba.GetRGB());
            drawer->SetTransparency(static_cast<float>(1.0 - rgba.Alpha()));
        }
        else if (style.IsSetColorCurv())
        {
            presentation->SetCustomColor(definitionSubShape, style.GetColorCurv());
        }

        drawer->SetHidden(style.IsVisible() ? Standard_False : Standard_True);
    }
}

namespace OcctBridge
{
    void applyStepOccurrenceStyles(
        const TDF_Label& occurrenceLabel,
        const TopoDS_Shape& definitionShape,
        const Handle(AIS_InteractiveObject)& interactivePresentation)
    {
        if (occurrenceLabel.IsNull() || definitionShape.IsNull() || interactivePresentation.IsNull()) return;
        if (!XCAFDoc_ShapeTool::IsComponent(occurrenceLabel)) return;

        const Handle(AIS_ColoredShape) presentation =
            Handle(AIS_ColoredShape)::DownCast(interactivePresentation);
        if (presentation.IsNull()) return;

        XCAFPrs_IndexedDataMapOfShapeStyle settings;
        XCAFPrs::CollectStyleSettings(occurrenceLabel, TopLoc_Location(), settings);
        if (settings.IsEmpty()) return;

        TopTools_IndexedMapOfShape definitionSubShapes;
        TopExp::MapShapes(definitionShape, definitionSubShapes);

        bool changed = false;
        for (XCAFPrs_DataMapIteratorOfIndexedDataMapOfShapeStyle iterator(settings);
             iterator.More();
             iterator.Next())
        {
            const TopoDS_Shape& styledShape = iterator.Key();
            if (styledShape.IsNull() || styledShape.IsPartner(definitionShape)) continue;

            const Standard_Integer partnerIndex = findPartnerIndex(definitionSubShapes, styledShape);
            if (partnerIndex <= 0) continue;

            applyCustomStyle(presentation, definitionSubShapes(partnerIndex), iterator.Value());
            changed = true;
        }

        if (changed) presentation->SetToUpdate();
    }
}
