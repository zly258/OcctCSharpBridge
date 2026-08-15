#include "topology/OcctModelingFaceAnalysis.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Surface.hxx>
#include <BRepBndLib.hxx>
#include <BRepGProp.hxx>
#include <BRepTools.hxx>
#include <BRep_Tool.hxx>
#include <Bnd_Box.hxx>
#include <GProp_GProps.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_shape_face_analysis_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelFaceAnalysis* items,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& root = model->requireShape(shapeId);
            TopTools_IndexedMapOfShape faces;
            TopExp::MapShapes(root, TopAbs_FACE, faces);
            *required = faces.Extent();

            if (items == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null face-analysis buffer requires zero capacity.");
                return;
            }
            if (capacity < *required)
                throw std::out_of_range("Face-analysis buffer capacity is too small.");

            for (int index = 1; index <= faces.Extent(); ++index)
            {
                const TopoDS_Face face = TopoDS::Face(faces(index));
                OcctModelFaceAnalysis& item = items[index - 1];
                item = {};
                item.faceId = model->addShape(face);
                item.surfaceType = toOcctSurfaceType(BRepAdaptor_Surface(face, Standard_False).GetType());
                item.orientation = toModelOrientation(face.Orientation());

                TopTools_IndexedMapOfShape edges;
                TopTools_IndexedMapOfShape wires;
                TopExp::MapShapes(face, TopAbs_EDGE, edges);
                TopExp::MapShapes(face, TopAbs_WIRE, wires);
                item.edgeCount = edges.Extent();
                item.wireCount = wires.Extent();

                GProp_GProps properties;
                BRepGProp::SurfaceProperties(face, properties);
                item.area = properties.Mass();
                item.maximumTolerance = BRep_Tool::Tolerance(face);

                BRepTools::UVBounds(
                    face,
                    item.uvBounds.uMin,
                    item.uvBounds.uMax,
                    item.uvBounds.vMin,
                    item.uvBounds.vMax);

                Bnd_Box box;
                BRepBndLib::Add(face, box);
                if (box.IsVoid())
                    throw std::runtime_error("Face bounding box is empty.");
                box.Get(
                    item.bounds.minX,
                    item.bounds.minY,
                    item.bounds.minZ,
                    item.bounds.maxX,
                    item.bounds.maxY,
                    item.bounds.maxZ);
            }
        });
    }
}
