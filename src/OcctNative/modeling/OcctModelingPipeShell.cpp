#include "modeling/OcctModelingPipeShell.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

#include <BRepOffsetAPI_MakePipeShell.hxx>
#include <GeomFill_Trihedron.hxx>
#include <gp_Dir.hxx>

#include <cmath>

using namespace OcctModelingInternal;

namespace {
    GeomFill_Trihedron trihedronMode(int mode) {
        switch (mode) {
            case 1: return GeomFill_IsFrenet;
            case 2: return GeomFill_IsCorrectedFrenet;
            case 3: return GeomFill_IsDiscreteTrihedron;
            default: return GeomFill_IsCorrectedFrenet;
        }
    }
}

extern "C" {
    OcctStatus occt_model_feature_pipe_shell_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId spineWireId,
        const OcctObjectId* profileIds, int profileCount,
        const OcctPipeShellOptions* options,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            constexpr uint32_t kApiVersion = 1;
            if (options == nullptr) throw std::invalid_argument("PipeShell options are null.");
            if (options->structSize < sizeof(OcctPipeShellOptions))
                throw std::invalid_argument("Unsupported PipeShell options size.");
            if (options->apiVersion != kApiVersion)
                throw std::invalid_argument("Unsupported PipeShell options API version.");
            requireCount(profileCount, 1, "PipeShell profile list");
            if (profileIds == nullptr) throw std::invalid_argument("Profile ID array is null.");

            const TopoDS_Shape& spineShape = model->requireShape(spineWireId);
            if (spineShape.ShapeType() != TopAbs_WIRE)
                throw std::invalid_argument("PipeShell spine must be a wire.");

            BRepOffsetAPI_MakePipeShell algorithm(TopoDS::Wire(spineShape));

            if (options->mode == 4) {
                // Fixed normal mode
                const gp_Dir fixedNorm = toDirection(options->fixedNormal);
                algorithm.SetMode(fixedNorm);
            } else {
                algorithm.SetMode(trihedronMode(options->mode));
            }

            if (options->forceC1 != 0)
                algorithm.SetForceApproxC1(Standard_True);

            TopTools_ListOfShape arguments;
            arguments.Append(spineShape);

            for (int i = 0; i < profileCount; ++i) {
                const TopoDS_Shape& profile = model->requireShape(profileIds[i]);
                algorithm.Add(profile);
                arguments.Append(profile);
            }

            if (!algorithm.IsReady())
                throw std::runtime_error("PipeShell algorithm is not ready (check spine and profiles).");

            algorithm.Build();
            if (!algorithm.IsDone() || algorithm.Shape().IsNull())
                throw std::runtime_error("PipeShell sweep failed.");

            if (options->makeSolid != 0)
                algorithm.MakeSolid();

            Handle(BRepTools_History) history = new BRepTools_History(arguments, algorithm);
            const OcctObjectId outputId = model->addShape(algorithm.Shape());
            const OcctOperationId opId = model->addOperation(history, {}, false, false);
            return {outputId, opId, 1, 0, 0};
        });
    }
}
