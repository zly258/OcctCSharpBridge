#include "OcctModelingInternal.hxx"
#include "OcctModelingFaceAnalysis.h"

#include <BRepAdaptor_Surface.hxx>
#include <BRepBndLib.hxx>
#include <BRepGProp.hxx>
#include <BRepTools.hxx>
#include <BRep_Tool.hxx>
#include <Bnd_Box.hxx>
#include <GProp_GProps.hxx>

using namespace OcctModelingInternal;

namespace
{
    int toSurfaceType(GeomAbs_SurfaceType type)
    {
        switch (type)
        {
            case GeomAbs_Plane: return OcctSurface_Plane;
            case GeomAbs_Cylinder: return OcctSurface_Cylinder;
            case GeomAbs_Cone: return OcctSurface_Cone;
            case GeomAbs_Sphere: return OcctSurface_Sphere;
            case GeomAbs_Torus: return OcctSurface_Torus;
            case GeomAbs_BezierSurface: return OcctSurface_Bezier;
            case GeomAbs_BSplineSurface: return OcctSurface_BSpline;
            case GeomAbs_SurfaceOfRevolution: return OcctSurface_Revolution;
            case GeomAbs_SurfaceOfExtrusion: return OcctSurface_Extrusion;
            case GeomAbs_OffsetSurface: return OcctSurface_Offset;
            default: return OcctSurface_Other;
        }
    }

    int toOrientation(TopAbs_Orientation orientation)
    {
        switch (orientation)
        {
            case TopAbs_FORWARD: return OcctModelOrientation_Forward;
            case TopAbs_REVERSED: return OcctModelOrientation_Reversed;
            case TopAbs_INTERNAL: return OcctModelOrientation_Internal;
            case TopAbs_EXTERNAL: return OcctModelOrientation_External;
            default: return OcctModelOrientation_Forward;
        }
    }
}

extern "C"
{
    int occt_model_shape_face_analysis(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctModelFaceAnalysis* items,
        int capacity,
        int* count)
    {
        ModelSession* model = modelOf(handle);
        if (count == nullptr) return 0;

        return execute(model, [&]
        {
            if (capacity < 0)
                throw std::invalid_argument("Face analysis capacity must not be negative.");

            const TopoDS_Shape& root = model->requireShape(shapeId);
            TopTools_IndexedMapOfShape faces;
            TopExp::MapShapes(root, TopAbs_FACE, faces);
            *count = faces.Extent();

            if (items == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Face analysis output is null but capacity is non-zero.");
                return;
            }
            if (capacity < *count)
                throw std::out_of_range("Face analysis output capacity is too small.");

            for (int index = 1; index <= faces.Extent(); ++index)
            {
                const TopoDS_Face face = TopoDS::Face(faces(index));
                OcctModelFaceAnalysis& item = items[index - 1];
                item.faceId = model->addShape(face);
                item.surfaceType = toSurfaceType(BRepAdaptor_Surface(face, Standard_False).GetType());
                item.orientation = toOrientation(face.Orientation());

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
