#pragma once

#include "OcctNative.h"

#include <AIS_InteractiveContext.hxx>
#include <AIS_RubberBand.hxx>
#include <AIS_ViewCube.hxx>
#include <Aspect_DisplayConnection.hxx>
#include <Aspect_Window.hxx>
#include <OpenGl_GraphicDriver.hxx>
#include <V3d_AmbientLight.hxx>
#include <V3d_DirectionalLight.hxx>
#include <V3d_View.hxx>
#include <V3d_Viewer.hxx>

namespace OcctBridge
{
    class ViewerContext
    {
    public:
        Handle(Aspect_DisplayConnection) displayConnection;
        Handle(OpenGl_GraphicDriver) graphicDriver;
        Handle(V3d_Viewer) viewer;
        Handle(V3d_View) view;
        Handle(AIS_InteractiveContext) context;
        Handle(AIS_ViewCube) viewCube;
        Handle(AIS_RubberBand) selectionRubberBand;
        Handle(Aspect_Window) window;
        Handle(V3d_AmbientLight) customAmbientLight;
        Handle(V3d_DirectionalLight) customDirectionalLight;
        Handle(V3d_DirectionalLight) customSunLight;
        Handle(V3d_DirectionalLight) customFillLight;
        int displayMode = AIS_Shaded;
        int selectionMode = OcctSelection_Object;
        int updateDepth = 0;
        bool redrawPending = false;
        bool fitAllPending = false;

        bool isInitialized() const;
        bool isUpdating() const;
        void beginUpdate();
        void endUpdate(bool fitAll);
        void requestRedraw();
        void requestFitAll();
        void ensureWindowMapped();
    };
}
