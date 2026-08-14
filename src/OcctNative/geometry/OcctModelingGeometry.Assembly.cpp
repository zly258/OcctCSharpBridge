#include "OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_MakeSolid.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRep_Builder.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Shell.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctObjectId occt_model_make_compound(OcctModelHandle handle, const OcctObjectId* shapeIds, int count)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requireCount(count, 1, "Compound");
            if (shapeIds == nullptr) throw std::invalid_argument("Shape ID array is null.");
            BRep_Builder builder;
            TopoDS_Compound compound;
            builder.MakeCompound(compound);
            for (int index = 0; index < count; ++index) builder.Add(compound, model->requireShape(shapeIds[index]));
            return compound;
        });
    }

    OcctObjectId occt_model_make_wire(OcctModelHandle handle, const OcctObjectId* edgeIds, int count)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
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
            return maker.Wire();
        });
    }

    OcctObjectId occt_model_make_solid_from_shell(OcctModelHandle handle, OcctObjectId shellId)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const TopoDS_Shape& shell = model->requireShape(shellId);
            if (shell.ShapeType() != TopAbs_SHELL) throw std::invalid_argument("Input must be a shell.");
            BRepBuilderAPI_MakeSolid maker(TopoDS::Shell(shell));
            if (!maker.IsDone()) throw std::runtime_error("Solid creation failed.");
            return maker.Solid();
        });
    }
}
