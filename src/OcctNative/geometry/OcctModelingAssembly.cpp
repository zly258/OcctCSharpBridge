#include "geometry/OcctModelingAssembly.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_MakeSolid.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_Sewing.hxx>
#include <BRep_Builder.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Shell.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_assembly_compound_create(
        OcctModelingSessionHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requireCount(count, 1, "Compound");
            if (shapeIds == nullptr) throw std::invalid_argument("Shape ID array is null.");

            BRep_Builder builder;
            TopoDS_Compound compound;
            builder.MakeCompound(compound);
            for (int index = 0; index < count; ++index)
                builder.Add(compound, model->requireShape(shapeIds[index]));
            return TopoDS_Shape(compound);
        });
    }

    OcctStatus occt_model_assembly_wire_create(
        OcctModelingSessionHandle handle,
        const OcctObjectId* edgeIds,
        int count,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requireCount(count, 1, "Wire");
            if (edgeIds == nullptr) throw std::invalid_argument("Edge ID array is null.");

            BRepBuilderAPI_MakeWire maker;
            for (int index = 0; index < count; ++index)
            {
                const TopoDS_Shape& edge = model->requireShape(edgeIds[index]);
                if (edge.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Wire inputs must be edges.");
                maker.Add(TopoDS::Edge(edge));
            }
            if (!maker.IsDone()) throw std::runtime_error("Wire creation failed.");
            return TopoDS_Shape(maker.Wire());
        });
    }

    OcctStatus occt_model_assembly_sew(
        OcctModelingSessionHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        double tolerance,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            requireCount(count, 1, "Sewing");
            requirePositive(tolerance, "Sewing tolerance");
            if (shapeIds == nullptr) throw std::invalid_argument("Shape ID array is null.");

            BRepBuilderAPI_Sewing sewing(tolerance);
            for (int index = 0; index < count; ++index)
                sewing.Add(model->requireShape(shapeIds[index]));
            sewing.Perform();
            const TopoDS_Shape sewed = sewing.SewedShape();
            if (sewed.IsNull()) throw std::runtime_error("Sewing failed.");
            return sewed;
        });
    }

    OcctStatus occt_model_assembly_solid_from_shell_create(
        OcctModelingSessionHandle handle,
        OcctObjectId shellId,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            const TopoDS_Shape& shell = model->requireShape(shellId);
            if (shell.ShapeType() != TopAbs_SHELL) throw std::invalid_argument("Input must be a shell.");
            BRepBuilderAPI_MakeSolid maker(TopoDS::Shell(shell));
            if (!maker.IsDone()) throw std::runtime_error("Solid creation failed.");
            return TopoDS_Shape(maker.Solid());
        });
    }
}
