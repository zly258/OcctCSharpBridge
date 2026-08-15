#include "presentation/OcctViewport.h"
#include "core/OcctInternal.hxx"

#include <Graphic3d_ClipPlane.hxx>
#include <Graphic3d_SequenceOfHClipPlane.hxx>
#include <gp_Pln.hxx>

#include <stdexcept>

using namespace OcctBridge;

extern "C"
{
    int occt_set_view_clip_planes(
        OcctHandle handle,
        const OcctViewClipPlane* planes,
        int count)
    {
        Engine* engine = engineOf(handle);
        if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
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

    int occt_get_view_clip_plane_limit(
        OcctHandle handle,
        int* limit)
    {
        Engine* engine = engineOf(handle);
        if (!validateInitialized(engine) || limit == nullptr) return 0;
        return execute(engine, [&] { *limit = engine->viewerContext.view->PlaneLimit(); });
    }
}
