#pragma once

#include "OcctModeling.h"
#include "OcctInternal.hxx"

#include <BOPAlgo_GlueEnum.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepAlgoAPI_Splitter.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepBndLib.hxx>
#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeSolid.hxx>
#include <BRepBuilderAPI_MakeVertex.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_Sewing.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepClass3d_SolidClassifier.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepGProp.hxx>
#include <BRepLib_ToolTriangulatedShape.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepOffsetAPI_MakeThickSolid.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepPrimAPI_MakeWedge.hxx>
#include <BRepTools.hxx>
#include <BRepTools_History.hxx>
#include <BRep_Tool.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <GeomAPI_ProjectPointOnCurve.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_Circle.hxx>
#include <Geom_Ellipse.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <Geom_Curve.hxx>
#include <Geom_Surface.hxx>
#include <GeomAbs_JoinType.hxx>
#include <IGESControl_Reader.hxx>
#include <IGESControl_Writer.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IMeshTools_Parameters.hxx>
#include <IntCurvesFace_ShapeIntersector.hxx>
#include <Poly_Triangle.hxx>
#include <Poly_Triangulation.hxx>
#include <ShapeFix_Shape.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_Writer.hxx>
#include <StlAPI_Reader.hxx>
#include <StlAPI_Writer.hxx>
#include <TColgp_Array1OfPnt.hxx>
#include <TColgp_HArray1OfPnt.hxx>
#include <TopAbs_State.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopLoc_Location.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <TopTools_ListIteratorOfListOfShape.hxx>
#include <TopTools_ListOfShape.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS_Shell.hxx>
#include <TopoDS_Solid.hxx>
#include <TopoDS_Vertex.hxx>
#include <TopoDS_Wire.hxx>
#include <gp_Lin.hxx>
#include <gp_Pnt2d.hxx>
#include <gp_Pln.hxx>

#include <algorithm>
#include <filesystem>
#include <fstream>
#include <iterator>
#include <cmath>
#include <limits>
#include <sstream>
#include <stdexcept>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

namespace OcctModelingInternal
{
    struct OperationRecord
    {
        Handle(BRepTools_History) history;
        std::string report;
        bool hasWarnings = false;
        bool hasErrors = false;
    };

    struct ModelSession
    {
        std::string lastError;
        std::string scratchString;
        std::unordered_map<OcctObjectId, TopoDS_Shape> shapes;
        std::unordered_map<OcctOperationId, OperationRecord> operations;
        std::vector<OcctModelRayHit> rayHits;
        OcctObjectId nextShapeId = 1;
        OcctOperationId nextOperationId = 1;

        TopoDS_Shape& requireShape(OcctObjectId id)
        {
            const auto iterator = shapes.find(id);
            if (iterator == shapes.end() || iterator->second.IsNull())
            {
                throw std::invalid_argument("Shape ID does not exist.");
            }
            return iterator->second;
        }

        const TopoDS_Shape& requireShape(OcctObjectId id) const
        {
            const auto iterator = shapes.find(id);
            if (iterator == shapes.end() || iterator->second.IsNull())
            {
                throw std::invalid_argument("Shape ID does not exist.");
            }
            return iterator->second;
        }

        OcctObjectId addShape(const TopoDS_Shape& shape)
        {
            if (shape.IsNull()) throw std::runtime_error("OCCT returned a null shape.");
            const OcctObjectId id = nextShapeId++;
            shapes.emplace(id, shape);
            return id;
        }

        OcctOperationId addOperation(
            const Handle(BRepTools_History)& history,
            std::string report,
            bool hasWarnings,
            bool hasErrors)
        {
            const OcctOperationId id = nextOperationId++;
            operations.emplace(id, OperationRecord{history, std::move(report), hasWarnings, hasErrors});
            return id;
        }
    };

    inline ModelSession* modelOf(OcctModelHandle handle)
    {
        return static_cast<ModelSession*>(handle);
    }

    template<typename Function>
    inline int execute(ModelSession* model, Function&& function)
    {
        if (model == nullptr) return 0;
        model->lastError.clear();
        try
        {
            function();
            return 1;
        }
        catch (const Standard_Failure& failure)
        {
            const char* message = failure.GetMessageString();
            model->lastError = message == nullptr ? "Open CASCADE operation failed." : message;
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
        }
        catch (...)
        {
            model->lastError = "Unknown native modeling error.";
        }
        return 0;
    }

    template<typename Function>
    inline OcctObjectId executeShape(ModelSession* model, Function&& function)
    {
        OcctObjectId result = 0;
        execute(model, [&] { result = model->addShape(function()); });
        return result;
    }

    inline OcctModelAlgorithmResult failedAlgorithmResult()
    {
        return {0, 0, 0, 0, 1};
    }

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
        {
            throw std::invalid_argument("Direction vector must not be zero.");
        }
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

    inline BOPAlgo_GlueEnum glueValue(int value)
    {
        switch (value)
        {
            case OcctModelGlue_Shift: return BOPAlgo_GlueShift;
            case OcctModelGlue_Full: return BOPAlgo_GlueFull;
            default: return BOPAlgo_GlueOff;
        }
    }

    template<typename Algorithm>
    inline void applyBooleanOptions(Algorithm& algorithm, const OcctModelBooleanOptions* options)
    {
        if (options == nullptr) return;
        if (options->fuzzyValue < 0.0) throw std::invalid_argument("Fuzzy value must not be negative.");
        algorithm.SetFuzzyValue(options->fuzzyValue);
        algorithm.SetRunParallel(options->runParallel != 0);
        algorithm.SetNonDestructive(options->nonDestructive != 0);
        algorithm.SetGlue(glueValue(options->glue));
        algorithm.SetCheckInverted(options->checkInverted != 0);
    }

    template<typename Algorithm>
    inline std::string algorithmReport(Algorithm& algorithm)
    {
        std::ostringstream stream;
        algorithm.DumpErrors(stream);
        algorithm.DumpWarnings(stream);
        return stream.str();
    }

    template<typename Algorithm>
    inline OcctModelAlgorithmResult finishBuilderAlgorithm(
        ModelSession* model,
        Algorithm& algorithm,
        const OcctModelBooleanOptions* options)
    {
        algorithm.Build();
        const bool hasErrors = algorithm.HasErrors();
        const bool hasWarnings = algorithm.HasWarnings();
        const std::string report = algorithmReport(algorithm);
        if (!algorithm.IsDone() || hasErrors || algorithm.Shape().IsNull())
        {
            throw std::runtime_error(report.empty() ? "OCCT modeling algorithm failed." : report);
        }
        if (options != nullptr && (options->simplifyEdges != 0 || options->simplifyFaces != 0))
        {
            const double angularTolerance = options->angularTolerance > 0.0
                ? options->angularTolerance
                : Precision::Angular();
            algorithm.SimplifyResult(options->simplifyEdges != 0, options->simplifyFaces != 0, angularTolerance);
        }
        const OcctObjectId shapeId = model->addShape(algorithm.Shape());
        const OcctOperationId operationId = model->addOperation(algorithm.History(), report, hasWarnings, hasErrors);
        return {shapeId, operationId, 1, hasWarnings ? 1 : 0, hasErrors ? 1 : 0};
    }

    template<typename Algorithm>
    inline OcctModelAlgorithmResult finishMakeShapeAlgorithm(
        ModelSession* model,
        Algorithm& algorithm,
        const TopTools_ListOfShape& arguments,
        const char* failureMessage)
    {
        algorithm.Build();
        if (!algorithm.IsDone() || algorithm.Shape().IsNull()) throw std::runtime_error(failureMessage);
        Handle(BRepTools_History) history = new BRepTools_History(arguments, algorithm);
        const OcctObjectId shapeId = model->addShape(algorithm.Shape());
        const OcctOperationId operationId = model->addOperation(history, {}, false, false);
        return {shapeId, operationId, 1, 0, 0};
    }

    inline const OperationRecord& requireOperation(ModelSession* model, OcctOperationId operationId)
    {
        const auto iterator = model->operations.find(operationId);
        if (iterator == model->operations.end()) throw std::invalid_argument("Operation ID does not exist.");
        return iterator->second;
    }

    inline OcctObjectId historyShapeAt(
        ModelSession* model,
        OcctOperationId operationId,
        OcctObjectId sourceId,
        int index,
        bool generated)
    {
        if (index < 0) throw std::out_of_range("History index must not be negative.");
        const OperationRecord& operation = requireOperation(model, operationId);
        if (operation.history.IsNull()) throw std::runtime_error("The operation has no topology history.");
        const TopoDS_Shape& source = model->requireShape(sourceId);
        const auto& list = generated ? operation.history->Generated(source) : operation.history->Modified(source);
        int current = 0;
        for (TopTools_ListIteratorOfListOfShape iterator(list); iterator.More(); iterator.Next(), ++current)
        {
            if (current == index) return model->addShape(iterator.Value());
        }
        throw std::out_of_range("History index is out of range.");
    }

    inline int historyCount(
        ModelSession* model,
        OcctOperationId operationId,
        OcctObjectId sourceId,
        bool generated)
    {
        const OperationRecord& operation = requireOperation(model, operationId);
        if (operation.history.IsNull()) return 0;
        const TopoDS_Shape& source = model->requireShape(sourceId);
        return generated
            ? operation.history->Generated(source).Size()
            : operation.history->Modified(source).Size();
    }

    inline Handle(Poly_Triangulation) faceTriangulation(
        ModelSession* model,
        OcctObjectId faceId,
        TopoDS_Face& face,
        TopLoc_Location& location)
    {
        const TopoDS_Shape& shape = model->requireShape(faceId);
        if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
        face = TopoDS::Face(shape);
        Handle(Poly_Triangulation) triangulation = BRep_Tool::Triangulation(face, location);
        if (triangulation.IsNull()) throw std::runtime_error("The face has no triangulation. Call Mesh first.");
        return triangulation;
    }

    inline std::ifstream modelInputStream(const std::filesystem::path& path)
    {
        std::ifstream stream(path, std::ios::binary);
        if (!stream) throw std::runtime_error("Unable to open input file.");
        return stream;
    }

    inline std::ofstream modelOutputStream(const std::filesystem::path& path)
    {
        if (path.empty()) throw std::invalid_argument("Path is empty.");
        if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
        std::ofstream stream(path, std::ios::binary | std::ios::trunc);
        if (!stream) throw std::runtime_error("Unable to open output file.");
        return stream;
    }

    inline TopoDS_Shape readModelStep(const std::filesystem::path& path)
    {
        auto stream = modelInputStream(path);
        STEPControl_Reader reader;
        const IFSelect_ReturnStatus status = reader.ReadStream(path.filename().string().c_str(), stream);
        if (status != IFSelect_RetDone) throw std::runtime_error("STEP file could not be read.");
        if (reader.TransferRoots() <= 0) throw std::runtime_error("STEP roots could not be transferred.");
        const TopoDS_Shape shape = reader.OneShape();
        if (shape.IsNull()) throw std::runtime_error("STEP file contains no transferable shape.");
        return shape;
    }

    inline TopoDS_Shape readModelIges(const std::filesystem::path& path)
    {
        auto stream = modelInputStream(path);
        IGESControl_Reader reader;
        const IFSelect_ReturnStatus status = reader.ReadStream(path.filename().string().c_str(), stream);
        if (status != IFSelect_RetDone) throw std::runtime_error("IGES file could not be read.");
        if (reader.TransferRoots() <= 0) throw std::runtime_error("IGES roots could not be transferred.");
        const TopoDS_Shape shape = reader.OneShape();
        if (shape.IsNull()) throw std::runtime_error("IGES file contains no transferable shape.");
        return shape;
    }

    inline TopoDS_Shape readModelBrep(const std::filesystem::path& path)
    {
        auto stream = modelInputStream(path);
        BRep_Builder builder;
        TopoDS_Shape shape;
        BRepTools::Read(shape, stream, builder);
        if (shape.IsNull()) throw std::runtime_error("BREP file contains no readable shape.");
        return shape;
    }

    inline TopoDS_Shape readModelStl(const std::filesystem::path& path)
    {
        TopoDS_Shape shape;
        StlAPI_Reader reader;
        if (!reader.Read(shape, path.string().c_str()))
            throw std::runtime_error("STL file could not be read. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        if (shape.IsNull()) throw std::runtime_error("STL file contains no readable shape.");
        return shape;
    }

    inline void writeModelStep(const TopoDS_Shape& shape, const std::filesystem::path& path)
    {
        STEPControl_Writer writer;
        if (writer.Transfer(shape, STEPControl_AsIs) != IFSelect_RetDone)
            throw std::runtime_error("Shape could not be transferred to STEP.");
        auto stream = modelOutputStream(path);
        if (writer.WriteStream(stream) != IFSelect_RetDone)
            throw std::runtime_error("STEP file could not be written.");
    }

    inline void writeModelIges(const TopoDS_Shape& shape, const std::filesystem::path& path)
    {
        IGESControl_Writer writer("MM", 1);
        if (!writer.AddShape(shape)) throw std::runtime_error("Shape could not be transferred to IGES.");
        writer.ComputeModel();
        auto stream = modelOutputStream(path);
        if (!writer.Write(stream)) throw std::runtime_error("IGES file could not be written.");
    }
}
