#pragma once

#include "OcctNative.h"

#include <AIS_InteractiveContext.hxx>
#include <AIS_InteractiveObject.hxx>
#include <AIS_RubberBand.hxx>
#include <AIS_Shape.hxx>
#include <AIS_ViewCube.hxx>
#include <Aspect_DisplayConnection.hxx>
#include <Aspect_Window.hxx>
#include <Graphic3d_GraphicDriver.hxx>
#include <Graphic3d_NameOfMaterial.hxx>
#include <GProp_GProps.hxx>
#include <OpenGl_GraphicDriver.hxx>
#include <Quantity_Color.hxx>
#include <Standard_Failure.hxx>
#include <TDocStd_Document.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS_Shape.hxx>
#include <V3d_AmbientLight.hxx>
#include <V3d_DirectionalLight.hxx>
#include <V3d_View.hxx>
#include <V3d_Viewer.hxx>
#include <gp_Ax1.hxx>
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>

#include <algorithm>
#include <cctype>
#include <cmath>
#include <cstdint>
#include <filesystem>
#include <functional>
#include <memory>
#include <stdexcept>
#include <string>
#include <unordered_map>
#include <vector>

namespace OcctBridge
{
    struct ObjectEntry
    {
        int kind = OcctObject_Unknown;
        TopoDS_Shape shape;
        Handle(AIS_InteractiveObject) presentation;
        std::string name;
        std::string applicationTag;
        std::vector<std::string> stepHierarchyPath;
        bool hasStoredColor = false;
        double storedColorR = 0.0;
        double storedColorG = 0.0;
        double storedColorB = 0.0;
        bool selectable = true;
        double storedColorA = 1.0;
        bool hasStoredAlpha = false;
        bool storedVisible = true;
        bool hasStoredVisibility = false;
        int stepDocumentIndex = -1;
        std::string stepNodeId;
        int presentationSubtype = 0;
    };

    class Engine
    {
    public:
        std::string lastError;
        std::string scratchString;
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
        std::unordered_map<OcctObjectId, ObjectEntry> objects;
        std::unordered_map<std::string, OcctObjectId> objectIdByApplicationTag;
        std::vector<Handle(TDocStd_Document)> stepDocuments;
        std::vector<OcctObjectId> lastStepImportObjectIds;
        Handle(TDocStd_Document) pristineStepDocument;
        bool pristineStepDocumentMatchesScene = false;
        OcctObjectId nextId = 1;
        int displayMode = AIS_Shaded;
        int selectionMode = OcctSelection_Object;
        int updateDepth = 0;
        bool redrawPending = false;
        bool fitAllPending = false;

        bool isInitialized() const;
        void clearError();
        void setError(const std::string& message);
        ObjectEntry* findObject(OcctObjectId id);
        const ObjectEntry* findObject(OcctObjectId id) const;
        ObjectEntry* findShape(OcctObjectId id);
        const ObjectEntry* findShape(OcctObjectId id) const;
        OcctObjectId findPresentation(const Handle(AIS_InteractiveObject)& presentation) const;
        OcctObjectId addShape(const TopoDS_Shape& shape, bool fit = false, const std::string& name = {});
        OcctObjectId addShapePresentation(
            const TopoDS_Shape& shape,
            const Handle(AIS_InteractiveObject)& presentation,
            bool fit = false,
            const std::string& name = {});
        OcctObjectId addPresentation(const Handle(AIS_InteractiveObject)& presentation, int kind, const std::string& name = {});
        void hide(OcctObjectId id);
        void erase(OcctObjectId id);
        void applySelectionMode(const Handle(AIS_InteractiveObject)& presentation);
        void beginUpdate();
        void endUpdate(bool fitAll);
        void requestRedraw();
        void requestFitAll();
        bool isUpdating() const;
        void invalidatePristineStepDocument();
    };

    Engine* engineOf(OcctHandle handle);
    bool validateInitialized(Engine* engine);
    void initializeViewer(Engine* engine, void* windowHandle, void* displayHandle = nullptr);
    std::string failureMessage(const Standard_Failure& failure);
    std::filesystem::path pathFromUtf8(const char* utf8Path);
    std::string lowerExtension(const std::filesystem::path& path);
    Quantity_Color color(double r, double g, double b);
    gp_Pnt point(OcctPoint3d value);
    gp_Vec vector(OcctVector3d value);
    gp_Dir direction(OcctVector3d value);
    gp_Ax2 axis2(OcctPoint3d origin, OcctVector3d normal);
    gp_Ax2 axis2(OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection);
    TopAbs_ShapeEnum shapeEnum(int shapeType);
    int shapeTypeValue(const TopoDS_Shape& shape);
    void requirePositive(double value, const char* name);
    void requireCount(int count, int minimum, const char* name);
    TopoDS_Shape transformed(const TopoDS_Shape& source, const gp_Trsf& transform);
    TopoDS_Shape shapeWithPresentationTransformation(const ObjectEntry& entry);
    void fillMassProperties(const GProp_GProps& properties, OcctMassProperties* result);
    Graphic3d_NameOfMaterial materialName(int value);

    bool syncStepObjectName(Engine* engine, ObjectEntry& entry);
    bool syncStepObjectColor(Engine* engine, ObjectEntry& entry);
    bool syncStepObjectVisibility(Engine* engine, ObjectEntry& entry);

    template<typename Function>
    int execute(Engine* engine, Function&& function)
    {
        if (engine == nullptr) return 0;
        engine->clearError();
        try
        {
            function();
            return 1;
        }
        catch (const Standard_Failure& failure)
        {
            engine->setError(failureMessage(failure));
        }
        catch (const std::exception& exception)
        {
            engine->setError(exception.what());
        }
        catch (...)
        {
            engine->setError("Unknown native error.");
        }
        return 0;
    }

    template<typename Function>
    OcctObjectId executeObject(Engine* engine, Function&& function)
    {
        if (engine == nullptr) return 0;
        engine->clearError();
        try
        {
            return function();
        }
        catch (const Standard_Failure& failure)
        {
            engine->setError(failureMessage(failure));
        }
        catch (const std::exception& exception)
        {
            engine->setError(exception.what());
        }
        catch (...)
        {
            engine->setError("Unknown native error.");
        }
        return 0;
    }
}
