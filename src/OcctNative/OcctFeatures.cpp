#include "core/OcctInternal.hxx"

#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepOffsetAPI_MakeThickSolid.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <GeomAbs_JoinType.hxx>
#include <Precision.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <TopTools_ListOfShape.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS_Wire.hxx>

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }


    TopoDS_Edge indexedEdge(const TopoDS_Shape& shape, int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0) throw std::out_of_range("Edge index must not be negative.");
        TopTools_IndexedMapOfShape edges;
        TopExp::MapShapes(shape, TopAbs_EDGE, edges);
        const int oneBasedIndex = zeroBasedIndex + 1;
        if (oneBasedIndex > edges.Extent()) throw std::out_of_range("Edge index is out of range.");
        return TopoDS::Edge(edges(oneBasedIndex));
    }

    OcctObjectId addFeatureResult(Engine* engine, const TopoDS_Shape& shape, const char* name, const OcctObjectId* inputs, int inputCount, int hideInputs)
    {
        if (shape.IsNull()) throw std::runtime_error(std::string(name) + " returned a null shape.");
        const OcctObjectId result = engine->addShape(shape, false, name);
        if (hideInputs)
        {
            for (int index = 0; index < inputCount; ++index) engine->hide(inputs[index]);
        }
        return result;
    }
}

extern "C"
{
    int occt_shape_distance(OcctHandle h, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e)||result==nullptr)return 0;
        return execute(e,[&]
        {
            ObjectEntry& first=requiredShape(e,firstId);ObjectEntry& second=requiredShape(e,secondId);
            BRepExtrema_DistShapeShape distance(first.shape,second.shape);distance.Perform();
            if(!distance.IsDone()||distance.NbSolution()<1)throw std::runtime_error("Distance calculation failed.");
            const gp_Pnt p1=distance.PointOnShape1(1);const gp_Pnt p2=distance.PointOnShape2(1);
            result->distance=distance.Value();result->pointOnFirst={p1.X(),p1.Y(),p1.Z()};result->pointOnSecond={p2.X(),p2.Y(),p2.Z()};
        });
    }

    OcctObjectId occt_boolean(OcctHandle h, int operation, OcctObjectId leftId, OcctObjectId rightId, int hideInputs)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            ObjectEntry& left=requiredShape(e,leftId);ObjectEntry& right=requiredShape(e,rightId);TopoDS_Shape resultShape;
            if(operation==OcctBoolean_Cut){BRepAlgoAPI_Cut maker(left.shape,right.shape);maker.Build();if(!maker.IsDone())throw std::runtime_error("Cut failed.");resultShape=maker.Shape();}
            else if(operation==OcctBoolean_Common){BRepAlgoAPI_Common maker(left.shape,right.shape);maker.Build();if(!maker.IsDone())throw std::runtime_error("Common failed.");resultShape=maker.Shape();}
            else if(operation==OcctBoolean_Section){BRepAlgoAPI_Section maker(left.shape,right.shape,Standard_False);maker.Build();if(!maker.IsDone())throw std::runtime_error("Section failed.");resultShape=maker.Shape();}
            else{BRepAlgoAPI_Fuse maker(left.shape,right.shape);maker.Build();if(!maker.IsDone())throw std::runtime_error("Fuse failed.");resultShape=maker.Shape();}
            const OcctObjectId ids[2]={leftId,rightId};return addFeatureResult(e,resultShape,"Boolean",ids,2,hideInputs);
        });
    }

    OcctObjectId occt_extrude(OcctHandle h, OcctObjectId profileId, OcctVector3d extrusion, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            ObjectEntry& profile=requiredShape(e,profileId);const gp_Vec directionVector=vector(extrusion);
            if(directionVector.SquareMagnitude()<=Precision::SquareConfusion())throw std::invalid_argument("Extrusion vector must not be zero.");
            BRepPrimAPI_MakePrism maker(profile.shape,directionVector,Standard_False,Standard_True);if(!maker.IsDone())throw std::runtime_error("Extrusion failed.");
            return addFeatureResult(e,maker.Shape(),"Extrude",&profileId,1,hideInput);
        });
    }

    OcctObjectId occt_revolve(OcctHandle h, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            ObjectEntry& profile=requiredShape(e,profileId);if(std::abs(angleDegrees)<=Precision::Angular())throw std::invalid_argument("Revolution angle must not be zero.");
            BRepPrimAPI_MakeRevol maker(profile.shape,gp_Ax1(point(axisPoint),direction(axisDirection)),angleDegrees*3.14159265358979323846/180.0,Standard_True);if(!maker.IsDone())throw std::runtime_error("Revolution failed.");
            return addFeatureResult(e,maker.Shape(),"Revolve",&profileId,1,hideInput);
        });
    }

    OcctObjectId occt_sweep(OcctHandle h, OcctObjectId spineWireId, OcctObjectId profileId, int hideInputs)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            ObjectEntry& spine=requiredShape(e,spineWireId);ObjectEntry& profile=requiredShape(e,profileId);
            if(spine.shape.ShapeType()!=TopAbs_WIRE)throw std::invalid_argument("Sweep spine must be a wire.");
            BRepOffsetAPI_MakePipe maker(TopoDS::Wire(spine.shape),profile.shape);maker.Build();if(!maker.IsDone())throw std::runtime_error("Sweep failed.");
            const OcctObjectId ids[2]={spineWireId,profileId};return addFeatureResult(e,maker.Shape(),"Sweep",ids,2,hideInputs);
        });
    }

    OcctObjectId occt_loft(OcctHandle h, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance, int hideInputs)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            requireCount(count,2,"Loft");requirePositive(tolerance,"Tolerance");if(wireIds==nullptr)throw std::invalid_argument("Wire ID array is null.");
            BRepOffsetAPI_ThruSections maker(makeSolid!=0,ruled!=0,tolerance);
            for(int index=0;index<count;++index){ObjectEntry& wire=requiredShape(e,wireIds[index]);if(wire.shape.ShapeType()!=TopAbs_WIRE)throw std::invalid_argument("Loft inputs must be wires.");maker.AddWire(TopoDS::Wire(wire.shape));}
            maker.Build();if(!maker.IsDone())throw std::runtime_error("Loft failed.");return addFeatureResult(e,maker.Shape(),"Loft",wireIds,count,hideInputs);
        });
    }

    OcctObjectId occt_fillet_all_edges(OcctHandle h, OcctObjectId shapeId, double radius, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            requirePositive(radius,"Fillet radius");ObjectEntry& source=requiredShape(e,shapeId);BRepFilletAPI_MakeFillet maker(source.shape);int count=0;
            for(TopExp_Explorer explorer(source.shape,TopAbs_EDGE);explorer.More();explorer.Next()){maker.Add(radius,TopoDS::Edge(explorer.Current()));++count;}
            if(count==0)throw std::runtime_error("Shape has no edges to fillet.");maker.Build();if(!maker.IsDone())throw std::runtime_error("Fillet failed. Try a smaller radius or use the selected-edge API.");
            return addFeatureResult(e,maker.Shape(),"Fillet",&shapeId,1,hideInput);
        });
    }

    OcctObjectId occt_chamfer_all_edges(OcctHandle h, OcctObjectId shapeId, double distance, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            requirePositive(distance,"Chamfer distance");ObjectEntry& source=requiredShape(e,shapeId);BRepFilletAPI_MakeChamfer maker(source.shape);int count=0;
            for(TopExp_Explorer explorer(source.shape,TopAbs_EDGE);explorer.More();explorer.Next()){maker.Add(distance,TopoDS::Edge(explorer.Current()));++count;}
            if(count==0)throw std::runtime_error("Shape has no edges to chamfer.");maker.Build();if(!maker.IsDone())throw std::runtime_error("Chamfer failed. Try a smaller distance.");
            return addFeatureResult(e,maker.Shape(),"Chamfer",&shapeId,1,hideInput);
        });
    }

    OcctObjectId occt_fillet_edges(OcctHandle h, OcctObjectId shapeId, const int* edgeIndices, int count, double radius, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            requirePositive(radius,"Fillet radius");requireCount(count,1,"Fillet edge list");if(edgeIndices==nullptr)throw std::invalid_argument("Edge index array is null.");
            ObjectEntry& source=requiredShape(e,shapeId);BRepFilletAPI_MakeFillet maker(source.shape);
            for(int index=0;index<count;++index)maker.Add(radius,indexedEdge(source.shape,edgeIndices[index]));
            maker.Build();if(!maker.IsDone())throw std::runtime_error("Selected-edge fillet failed. Try a smaller radius or different edge set.");
            return addFeatureResult(e,maker.Shape(),"FilletEdges",&shapeId,1,hideInput);
        });
    }

    OcctObjectId occt_chamfer_edges(OcctHandle h, OcctObjectId shapeId, const int* edgeIndices, int count, double distance, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            requirePositive(distance,"Chamfer distance");requireCount(count,1,"Chamfer edge list");if(edgeIndices==nullptr)throw std::invalid_argument("Edge index array is null.");
            ObjectEntry& source=requiredShape(e,shapeId);BRepFilletAPI_MakeChamfer maker(source.shape);
            for(int index=0;index<count;++index)maker.Add(distance,indexedEdge(source.shape,edgeIndices[index]));
            maker.Build();if(!maker.IsDone())throw std::runtime_error("Selected-edge chamfer failed. Try a smaller distance or different edge set.");
            return addFeatureResult(e,maker.Shape(),"ChamferEdges",&shapeId,1,hideInput);
        });
    }

    OcctObjectId occt_offset_shape(OcctHandle h, OcctObjectId shapeId, double offset, double tolerance, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            if(std::abs(offset)<=Precision::Confusion())throw std::invalid_argument("Offset must not be zero.");requirePositive(tolerance,"Tolerance");ObjectEntry& source=requiredShape(e,shapeId);
            BRepOffsetAPI_MakeOffsetShape maker;maker.PerformByJoin(source.shape,offset,tolerance,BRepOffset_Skin,Standard_False,Standard_False,GeomAbs_Arc,Standard_True);
            if(!maker.IsDone())throw std::runtime_error("Offset shape failed.");return addFeatureResult(e,maker.Shape(),"Offset",&shapeId,1,hideInput);
        });
    }

    OcctObjectId occt_thick_solid(OcctHandle h, OcctObjectId solidId, int faceIndexToRemove, double thickness, double tolerance, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]
        {
            if(std::abs(thickness)<=Precision::Confusion())throw std::invalid_argument("Thickness must not be zero.");requirePositive(tolerance,"Tolerance");if(faceIndexToRemove<0)throw std::invalid_argument("Face index must not be negative.");
            ObjectEntry& source=requiredShape(e,solidId);TopTools_ListOfShape faces;int index=0;bool found=false;
            for(TopExp_Explorer explorer(source.shape,TopAbs_FACE);explorer.More();explorer.Next(),++index){if(index==faceIndexToRemove){faces.Append(explorer.Current());found=true;break;}}
            if(!found)throw std::out_of_range("Face index is out of range.");BRepOffsetAPI_MakeThickSolid maker;maker.MakeThickSolidByJoin(source.shape,faces,thickness,tolerance,BRepOffset_Skin,Standard_False,Standard_False,GeomAbs_Arc,Standard_True);
            if(!maker.IsDone())throw std::runtime_error("Thick solid operation failed.");return addFeatureResult(e,maker.Shape(),"ThickSolid",&solidId,1,hideInput);
        });
    }
}
