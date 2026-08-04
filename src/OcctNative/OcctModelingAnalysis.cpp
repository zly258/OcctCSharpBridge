#include "OcctModelingInternal.hxx"

using namespace OcctModelingInternal;

extern "C"
{
    int occt_model_project_point_on_edge(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d pointValue, OcctModelProjectionResult* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            Standard_Real first = 0.0;
            Standard_Real last = 0.0;
            Handle(Geom_Curve) curve = BRep_Tool::Curve(TopoDS::Edge(shape), first, last);
            if (curve.IsNull()) throw std::runtime_error("Edge has no 3D curve.");
            GeomAPI_ProjectPointOnCurve projection(toPoint(pointValue), curve, first, last);
            if (projection.NbPoints() < 1) throw std::runtime_error("Point projection on edge failed.");
            const gp_Pnt projected = projection.NearestPoint();
            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->distance = projection.LowerDistance();
            result->parameter = projection.LowerDistanceParameter();
            result->u = 0.0;
            result->v = 0.0;
        });
    }

    int occt_model_project_point_on_face(OcctModelHandle handle, OcctObjectId faceId, OcctPoint3d pointValue, OcctModelProjectionResult* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            const TopoDS_Face face = TopoDS::Face(shape);
            Handle(Geom_Surface) surface = BRep_Tool::Surface(face);
            if (surface.IsNull()) throw std::runtime_error("Face has no surface.");
            Standard_Real uMin = 0.0;
            Standard_Real uMax = 0.0;
            Standard_Real vMin = 0.0;
            Standard_Real vMax = 0.0;
            BRepTools::UVBounds(face, uMin, uMax, vMin, vMax);
            GeomAPI_ProjectPointOnSurf projection;
            projection.Init(toPoint(pointValue), surface, uMin, uMax, vMin, vMax);
            if (projection.NbPoints() < 1) throw std::runtime_error("Point projection on face failed.");
            const gp_Pnt projected = projection.NearestPoint();
            Standard_Real u = 0.0;
            Standard_Real v = 0.0;
            projection.LowerDistanceParameters(u, v);
            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->distance = projection.LowerDistance();
            result->parameter = 0.0;
            result->u = u;
            result->v = v;
        });
    }

    int occt_model_ray_intersections(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d origin, OcctVector3d directionValue, double minimumParameter, double maximumParameter, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        int count = 0;
        const int succeeded = execute(model, [&]
        {
            if (maximumParameter < minimumParameter) throw std::invalid_argument("Ray parameter range is invalid.");
            requirePositive(tolerance, "Tolerance");
            IntCurvesFace_ShapeIntersector intersector;
            intersector.Load(model->requireShape(shapeId), tolerance);
            intersector.Perform(gp_Lin(toPoint(origin), toDirection(directionValue)), minimumParameter, maximumParameter);
            if (!intersector.IsDone()) throw std::runtime_error("Ray intersection failed.");
            intersector.SortResult();
            model->rayHits.clear();
            for (int index = 1; index <= intersector.NbPnt(); ++index)
            {
                const gp_Pnt point = intersector.Pnt(index);
                const OcctObjectId faceId = model->addShape(intersector.Face(index));
                model->rayHits.push_back({
                    {point.X(), point.Y(), point.Z()},
                    faceId,
                    intersector.WParameter(index),
                    intersector.UParameter(index),
                    intersector.VParameter(index),
                    toModelState(intersector.State(index))});
            }
            count = static_cast<int>(model->rayHits.size());
        });
        return succeeded == 0 ? -1 : count;
    }

    int occt_model_ray_hit_count(OcctModelHandle handle)
    {
        ModelSession* model = modelOf(handle);
        return model == nullptr ? 0 : static_cast<int>(model->rayHits.size());
    }

    int occt_model_ray_hit_at(OcctModelHandle handle, int index, OcctModelRayHit* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            if (index < 0 || index >= static_cast<int>(model->rayHits.size())) throw std::out_of_range("Ray hit index is out of range.");
            *result = model->rayHits[static_cast<std::size_t>(index)];
        });
    }

    int occt_model_classify_point(OcctModelHandle handle, OcctObjectId solidId, OcctPoint3d pointValue, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return OcctModelState_Unknown;
        try
        {
            const TopoDS_Shape& shape = model->requireShape(solidId);
            if (shape.ShapeType() != TopAbs_SOLID) throw std::invalid_argument("Input must be a solid.");
            BRepClass3d_SolidClassifier classifier(TopoDS::Solid(shape), toPoint(pointValue), tolerance);
            return toModelState(classifier.State());
        }
        catch (const Standard_Failure& failure)
        {
            const char* message = failure.GetMessageString();
            model->lastError = message == nullptr ? "Open CASCADE classification failed." : message;
            return OcctModelState_Unknown;
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
            return OcctModelState_Unknown;
        }
    }

    int occt_model_mesh(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelMeshParameters* parameters)
    {
        ModelSession* model = modelOf(handle);
        if (parameters == nullptr) return 0;
        return execute(model, [&]
        {
            requirePositive(parameters->linearDeflection, "Linear deflection");
            requirePositive(parameters->angularDeflection, "Angular deflection");
            IMeshTools_Parameters nativeParameters;
            nativeParameters.Deflection = parameters->linearDeflection;
            nativeParameters.DeflectionInterior = parameters->linearDeflection;
            nativeParameters.Angle = parameters->angularDeflection;
            nativeParameters.AngleInterior = parameters->angularDeflection;
            nativeParameters.MinSize = parameters->minSize > 0.0
                ? parameters->minSize
                : parameters->linearDeflection * IMeshTools_Parameters::RelMinSize();
            nativeParameters.Relative = parameters->relative != 0;
            nativeParameters.InParallel = parameters->parallel != 0;
            nativeParameters.InternalVerticesMode = parameters->internalVertices != 0;
            nativeParameters.ControlSurfaceDeflection = parameters->controlSurfaceDeflection != 0;
            BRepMesh_IncrementalMesh mesh(model->requireShape(shapeId), nativeParameters);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("Shape meshing failed.");
        });
    }

    int occt_model_clear_mesh(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&] { BRepTools::Clean(model->requireShape(shapeId)); });
    }

    int occt_model_face_mesh_counts(OcctModelHandle handle, OcctObjectId faceId, int* nodeCount, int* triangleCount)
    {
        ModelSession* model = modelOf(handle);
        if (nodeCount == nullptr || triangleCount == nullptr) return 0;
        return execute(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            *nodeCount = triangulation->NbNodes();
            *triangleCount = triangulation->NbTriangles();
        });
    }

    int occt_model_face_mesh_node(OcctModelHandle handle, OcctObjectId faceId, int index, OcctModelMeshNode* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int oneBased = index + 1;
            if (index < 0 || oneBased > triangulation->NbNodes()) throw std::out_of_range("Mesh node index is out of range.");
            if (!triangulation->HasNormals()) BRepLib_ToolTriangulatedShape::ComputeNormals(face, triangulation);
            const gp_Pnt point = triangulation->Node(oneBased).Transformed(location.Transformation());
            result->point = {point.X(), point.Y(), point.Z()};
            result->hasUv = triangulation->HasUVNodes() ? 1 : 0;
            result->hasNormal = triangulation->HasNormals() ? 1 : 0;
            if (result->hasUv != 0)
            {
                const gp_Pnt2d uv = triangulation->UVNode(oneBased);
                result->u = uv.X();
                result->v = uv.Y();
            }
            else
            {
                result->u = 0.0;
                result->v = 0.0;
            }
            if (result->hasNormal != 0)
            {
                gp_Dir normal = triangulation->Normal(oneBased);
                normal.Transform(location.Transformation());
                result->normal = {normal.X(), normal.Y(), normal.Z()};
            }
            else result->normal = {0.0, 0.0, 0.0};
        });
    }

    int occt_model_face_mesh_triangle(OcctModelHandle handle, OcctObjectId faceId, int index, OcctModelMeshTriangle* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            TopoDS_Face face;
            TopLoc_Location location;
            Handle(Poly_Triangulation) triangulation = faceTriangulation(model, faceId, face, location);
            const int oneBased = index + 1;
            if (index < 0 || oneBased > triangulation->NbTriangles()) throw std::out_of_range("Mesh triangle index is out of range.");
            int node1 = 0;
            int node2 = 0;
            int node3 = 0;
            triangulation->Triangle(oneBased).Get(node1, node2, node3);
            if (face.Orientation() == TopAbs_REVERSED) std::swap(node2, node3);
            result->node1 = node1 - 1;
            result->node2 = node2 - 1;
            result->node3 = node3 - 1;
        });
    }

    OcctObjectId occt_model_import_step(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelStep(path);
        });
    }

    OcctObjectId occt_model_import_iges(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelIges(path);
        });
    }

    OcctObjectId occt_model_import_brep(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelBrep(path);
        });
    }

    OcctObjectId occt_model_import_stl(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readModelStl(path);
        });
    }

    OcctObjectId occt_model_import_file(OcctModelHandle handle, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        const auto path = OcctBridge::pathFromUtf8(utf8Path);
        const std::string extension = OcctBridge::lowerExtension(path);
        if (extension == ".step" || extension == ".stp") return occt_model_import_step(handle, utf8Path);
        if (extension == ".iges" || extension == ".igs") return occt_model_import_iges(handle, utf8Path);
        if (extension == ".brep" || extension == ".rle") return occt_model_import_brep(handle, utf8Path);
        if (extension == ".stl") return occt_model_import_stl(handle, utf8Path);
        model->lastError = "Unsupported file extension. Supported: STEP, IGES, BREP and STL.";
        return 0;
    }

    int occt_model_export_step(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            writeModelStep(model->requireShape(shapeId), OcctBridge::pathFromUtf8(utf8Path));
        });
    }

    int occt_model_export_iges(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            writeModelIges(model->requireShape(shapeId), OcctBridge::pathFromUtf8(utf8Path));
        });
    }

    int occt_model_export_brep(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            auto stream = modelOutputStream(OcctBridge::pathFromUtf8(utf8Path));
            BRepTools::Write(model->requireShape(shapeId), stream);
            if (!stream) throw std::runtime_error("BREP file could not be written.");
        });
    }

    int occt_model_export_stl(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            requirePositive(linearDeflection, "Linear deflection");
            requirePositive(angularDeflection, "Angular deflection");
            const TopoDS_Shape& shape = model->requireShape(shapeId);
            BRepMesh_IncrementalMesh mesh(shape, linearDeflection, Standard_False, angularDeflection, Standard_True);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("STL meshing failed.");
            const auto path = OcctBridge::pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
            StlAPI_Writer writer;
            writer.ASCIIMode() = asciiMode != 0;
            if (!writer.Write(shape, path.string().c_str()))
                throw std::runtime_error("STL file could not be written. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        });
    }
}
