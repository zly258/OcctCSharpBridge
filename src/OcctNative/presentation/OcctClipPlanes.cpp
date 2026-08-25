#include "presentation/OcctViewport.h"
#include "core/OcctInternal.hxx"

#include <Graphic3d_ClipPlane.hxx>
#include <Graphic3d_SequenceOfHClipPlane.hxx>
#include <gp_Pln.hxx>

#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeViewportClipStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }
}

extern "C"
{
    OcctStatus occt_engine_viewport_clip_planes_set(
        OcctEngineHandle handle,
        const OcctViewClipPlane* planes,
        int count)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportClipStatus(engine, [&]
        {
            if (count < 0) throw std::invalid_argument("Clip plane count must not be negative.");
            if (count > 0 && planes == nullptr)
                throw std::invalid_argument("Clip plane array is null.");
            if (count > engine->viewerContext.view->PlaneLimit())
                throw std::invalid_argument("Clip plane count exceeds the view plane limit.");

            Handle(Graphic3d_SequenceOfHClipPlane) sequence = new Graphic3d_SequenceOfHClipPlane();
            for (int index = 0; index < count; ++index)
            {
                const OcctViewClipPlane& source = planes[index];
                Handle(Graphic3d_ClipPlane) plane =
                    new Graphic3d_ClipPlane(gp_Pln(point(source.point), direction(source.normal)));
                plane->SetOn(source.enabled != 0);
                plane->SetCapping(source.capping != 0);
                plane->SetCappingColor(color(source.cappingR, source.cappingG, source.cappingB));
                sequence->Append(plane);
            }
            engine->viewerContext.view->SetClipPlanes(sequence);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_viewport_clip_plane_limit_get(
        OcctEngineHandle handle,
        int* limit)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportClipStatus(engine, [&]
        {
            if (limit == nullptr) throw std::invalid_argument("Clip plane limit result is null.");
            *limit = engine->viewerContext.view->PlaneLimit();
        });
    }
}
