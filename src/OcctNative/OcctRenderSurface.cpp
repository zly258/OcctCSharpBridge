#include "OcctRenderSurface.h"
#include "core/OcctInternal.hxx"

using namespace OcctBridge;

extern "C" OCCTBRIDGE_API int occt_resize_surface(OcctHandle h)
{
    Engine* e = engineOf(h);
    if (!validateInitialized(e)) return 0;
    return execute(e, [&]
    {
        // Keep native-window sizing independent from presentation. UI hosts such
        // as WPF can receive many WM_SIZE messages during one layout/drag cycle;
        // rendering is deliberately scheduled by the host once per UI frame.
        e->viewerContext.view->MustBeResized();
    });
}
