#include "OcctViewerExchange.h"
#include "core/OcctInternal.hxx"
#include "OcctExchangePath.hxx"

#include <BRep_Builder.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESControl_Reader.hxx>
#include <IGESControl_Writer.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <StlAPI_Reader.hxx>
#include <StlAPI_Writer.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDataStd_Name.hxx>
#include <TDocStd_Document.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS_Compound.hxx>
#include <XCAFApp_Application.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFPrs_AISObject.hxx>
#include <XCAFPrs_DocumentExplorer.hxx>

#include <algorithm>
#include <fstream>
#include <map>
#include <memory>
#include <stdexcept>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    constexpr char StepPathPrefix[] = "step-path:";
    constexpr char StepPathSeparator = '\x1f';

    struct StepAssemblyNode
    {
        explicit StepAssemblyNode(std::string value) : name(std::move(value)) {}

        std::string name;
        std::map<std::string, std::unique_ptr<StepAssemblyNode>> children;
        std::vector<const ObjectEntry*> leaves;
    };

    struct StepImportLeaf
    {
        TDF_Label label;
        TDF_Label refLabel;
        XCAFPrs_Style style;
        TopLoc_Location location;
        std::vector<std::string> path;
    };

    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }

    std::ifstream inputStream(const std::filesystem::path& path)
    {
        std::ifstream stream(path, std::ios::binary);
        if (!stream) throw std::runtime_error("Unable to open input file.");
        return stream;
    }

    std::ofstream outputStream(const std::filesystem::path& path)
    {
        if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
        std::ofstream stream(path, std::ios::binary | std::ios::trunc);
        if (!stream) throw std::runtime_error("Unable to open output file.");
        return stream;
    }

    Handle(TDocStd_Document) newXdeDocument()
    {
        const Handle(XCAFApp_Application) application = XCAFApp_Application::GetApplication();
        if (application.IsNull()) throw std::runtime_error("XCAF application could not be initialized.");
        Handle(TDocStd_Document) document;
        application->NewDocument("BinXCAF", document);
        if (document.IsNull()) throw std::runtime_error("XCAF document could not be created.");
        return document;
    }

    std::string extendedStringToUtf8(const TCollection_ExtendedString& value)
    {
        const Standard_Integer byteCount = value.LengthOfCString();
        if (byteCount <= 0) return {};
        std::vector<char> buffer(static_cast<std::size_t>(byteCount) + 1U, '\0');
        Standard_PCharacter destination = buffer.data();
        value.ToUTF8CString(destination);
        return std::string(buffer.data());
    }

    std::string labelName(const TDF_Label& label)
    {
        if (label.IsNull()) return {};
        Handle(TDataStd_Name) attribute;
        if (!label.FindAttribute(TDataStd_Name::GetID(), attribute) || attribute.IsNull()) return {};
        return extendedStringToUtf8(attribute->Get());
    }

    std::string nodeName(const XCAFPrs_DocumentNode& node)
    {
        std::string result = labelName(node.Label);
        if (result.empty()) result = labelName(node.RefLabel);
        if (result.empty()) result = node.IsAssembly ? "Assembly" : "Part";
        return result;
    }

    void setLabelName(const TDF_Label& label, const std::string& name)
    {
        if (label.IsNull() || name.empty()) return;
        TDataStd_Name::Set(label, TCollection_ExtendedString(name.c_str(), true));
    }

    std::string hierarchyTag(const std::vector<std::string>& path)
    {
        std::string result = StepPathPrefix;
        for (std::size_t index = 0; index < path.size(); ++index)
        {
            if (index != 0U) result.push_back(StepPathSeparator);
            result += path[index];
        }
        return result;
    }

    bool tryStyleColor(const XCAFPrs_Style& style, Quantity_Color& result)
    {
        if (style.IsSetColorSurf())
        {
            result = style.GetColorSurf();
            return true;
        }
        if (style.IsSetColorCurv())
        {
            result = style.GetColorCurv();
            return true;
        }
        return false;
    }

    void rememberStyleColor(ObjectEntry& entry, const XCAFPrs_Style& style)
    {
        Quantity_Color value;
        if (!tryStyleColor(style, value)) return;
        entry.hasStoredColor = true;
        entry.storedColorR = value.Red();
        entry.storedColorG = value.Green();
        entry.storedColorB = value.Blue();
    }

    void applyBaseStyle(const Handle(AIS_Shape)& presentation, const XCAFPrs_Style& style)
    {
        if (presentation.IsNull()) return;
        Quantity_Color value;
        if (tryStyleColor(style, value)) presentation->SetColor(value);
    }

    std::vector<std::string> currentHierarchy(const XCAFPrs_DocumentExplorer& explorer)
    {
        const Standard_Integer depth = explorer.CurrentDepth();
        std::vector<std::string> path;
        path.reserve(static_cast<std::size_t>(depth + 1));
        for (Standard_Integer index = 0; index <= depth; ++index)
            path.push_back(nodeName(explorer.Current(index)));
        return path;
    }

    void finishImportedEntry(
        Engine* engine,
        OcctObjectId id,
        const std::vector<std::string>& path,
        const XCAFPrs_Style& style)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr) throw std::runtime_error("Imported STEP shape could not be registered.");

        // Retain structured hierarchy metadata for object identity and STEP round-trip.
        entry->stepHierarchyPath = path;
        entry->applicationTag = hierarchyTag(path);
        rememberStyleColor(*entry, style);

        Quantity_Color mergedColor;
        if (tryStyleColor(style, mergedColor))
            engine->viewerContext.context->SetColor(entry->presentation, mergedColor, Standard_False);
        if (!style.IsVisible()) engine->viewerContext.context->Erase(entry->presentation, Standard_False);
    }

    OcctObjectId addStructuredLeaf(Engine* engine, const StepImportLeaf& leaf)
    {
        TopoDS_Shape localShape = XCAFDoc_ShapeTool::GetShape(leaf.refLabel);
        if (localShape.IsNull()) localShape = XCAFDoc_ShapeTool::GetShape(leaf.label);
        if (localShape.IsNull()) return 0;

        const TDF_Label presentationLabel = leaf.refLabel.IsNull() ? leaf.label : leaf.refLabel;
        Handle(XCAFPrs_AISObject) presentation = new XCAFPrs_AISObject(presentationLabel);
        applyBaseStyle(presentation, leaf.style);
        if (!leaf.location.IsIdentity()) presentation->SetLocalTransformation(leaf.location.Transformation());

        const std::string name = leaf.path.empty() ? std::string("Part") : leaf.path.back();
        const OcctObjectId id = engine->addShapePresentation(localShape, presentation, false, name);
        finishImportedEntry(engine, id, leaf.path, leaf.style);
        return id;
    }

    OcctObjectId readStepXde(Engine* engine, const std::filesystem::path& path)
    {
        const bool sceneWasEmpty = engine->scene.objects.empty();
        auto stream = inputStream(path);
        const Handle(TDocStd_Document) document = newXdeDocument();
        STEPCAFControl_Reader reader;
        reader.SetColorMode(true);
        reader.SetNameMode(true);
        reader.SetLayerMode(true);
        reader.SetPropsMode(true);
        if (reader.ReadStream(path.filename().string().c_str(), stream) != IFSelect_RetDone)
            throw std::runtime_error("STEP file could not be read.");
        if (!reader.Transfer(document))
            throw std::runtime_error("STEP document could not be transferred to XDE.");

        std::vector<StepImportLeaf> leaves;
        XCAFPrs_DocumentExplorer explorer(document, XCAFPrs_DocumentExplorerFlags_None);
        for (; explorer.More(); explorer.Next())
        {
            const XCAFPrs_DocumentNode& node = explorer.Current();
            if (node.IsAssembly) continue;

            StepImportLeaf leaf;
            leaf.label = node.Label;
            leaf.refLabel = node.RefLabel;
            leaf.style = node.Style;
            leaf.location = node.Location;
            leaf.path = currentHierarchy(explorer);
            leaves.push_back(std::move(leaf));
        }
        if (leaves.empty()) throw std::runtime_error("STEP file contains no displayable shape nodes.");

        OcctObjectId firstImportedId = 0;
        std::vector<OcctObjectId> importedIds;
        std::vector<OcctObjectId> leafObjectIds;
        leafObjectIds.reserve(leaves.size());
        engine->beginUpdate();
        try
        {
            for (const StepImportLeaf& leaf : leaves)
            {
                const OcctObjectId id = addStructuredLeaf(engine, leaf);
                leafObjectIds.push_back(id);
                if (id == 0) continue;
                importedIds.push_back(id);
                if (firstImportedId == 0) firstImportedId = id;
            }

            if (firstImportedId == 0)
                throw std::runtime_error("STEP file contains no displayable leaf shapes.");
            engine->endUpdate(false);
        }
        catch (...)
        {
            for (const OcctObjectId id : importedIds) engine->erase(id);
            engine->endUpdate(false);
            throw;
        }

        engine->documents.stepDocuments.push_back(document);
        engine->documents.lastStepImportObjectIds = std::move(leafObjectIds);
        if (sceneWasEmpty)
        {
            engine->documents.pristineStepDocument = document;
            engine->documents.pristineStepDocumentMatchesScene = true;
        }
        else
        {
            engine->documents.pristineStepDocumentMatchesScene = false;
        }
        engine->requestRedraw();
        return firstImportedId;
    }

    TopoDS_Shape allShapes(Engine* engine)
    {
        BRep_Builder builder;
        TopoDS_Compound compound;
        builder.MakeCompound(compound);
        int count = 0;
        for (const auto& pair : engine->scene.objects)
        {
            if (pair.second.kind == OcctObject_Shape && !pair.second.shape.IsNull())
            {
                builder.Add(compound, shapeWithPresentationTransformation(pair.second));
                ++count;
            }
        }
        if (count == 0) throw std::runtime_error("There are no shapes to export.");
        return compound;
    }

    TopoDS_Shape readIges(const std::filesystem::path& path)
    {
        auto stream = inputStream(path);
        IGESControl_Reader reader;
        const IFSelect_ReturnStatus status = reader.ReadStream(path.filename().string().c_str(), stream);
        if (status != IFSelect_RetDone) throw std::runtime_error("IGES file could not be read.");
        if (reader.TransferRoots() <= 0) throw std::runtime_error("IGES roots could not be transferred.");
        TopoDS_Shape shape = reader.OneShape();
        if (shape.IsNull()) throw std::runtime_error("IGES file contains no transferable shape.");
        return shape;
    }

    TopoDS_Shape readBrep(const std::filesystem::path& path)
    {
        auto stream = inputStream(path);
        BRep_Builder builder;
        TopoDS_Shape shape;
        BRepTools::Read(shape, stream, builder);
        if (shape.IsNull()) throw std::runtime_error("BREP file contains no readable shape.");
        return shape;
    }

    TopoDS_Shape readStl(const std::filesystem::path& path)
    {
        TopoDS_Shape shape;
        StlAPI_Reader reader;
        if (!reader.Read(shape, path.string().c_str()))
            throw std::runtime_error("STL file could not be read. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        return shape;
    }

    void applyStoredColor(
        const Handle(XCAFDoc_ColorTool)& colorTool,
        const TDF_Label& label,
        const ObjectEntry& entry)
    {
        if (!entry.hasStoredColor || label.IsNull()) return;
        const Quantity_Color value = color(entry.storedColorR, entry.storedColorG, entry.storedColorB);
        colorTool->SetColor(label, value, XCAFDoc_ColorGen);
        colorTool->SetColor(label, value, XCAFDoc_ColorSurf);
    }

    TDF_Label addStepLeaf(
        const Handle(XCAFDoc_ShapeTool)& shapeTool,
        const Handle(XCAFDoc_ColorTool)& colorTool,
        const ObjectEntry& entry,
        const std::string& fallbackName)
    {
        const TopoDS_Shape shape = shapeWithPresentationTransformation(entry);
        if (shape.IsNull()) throw std::runtime_error("Shape could not be prepared for STEP export.");
        const TDF_Label label = shapeTool->AddShape(shape, false);
        const std::string name = entry.name.empty() ? fallbackName : entry.name;
        setLabelName(label, name);
        applyStoredColor(colorTool, label, entry);
        return label;
    }

    TDF_Label buildStepAssembly(
        const Handle(XCAFDoc_ShapeTool)& shapeTool,
        const Handle(XCAFDoc_ColorTool)& colorTool,
        const StepAssemblyNode& node)
    {
        const TDF_Label assemblyLabel = shapeTool->NewShape();
        setLabelName(assemblyLabel, node.name);

        for (const auto& childPair : node.children)
        {
            const StepAssemblyNode& child = *childPair.second;
            const TDF_Label childLabel = buildStepAssembly(shapeTool, colorTool, child);
            const TDF_Label occurrence = shapeTool->AddComponent(assemblyLabel, childLabel, TopLoc_Location());
            setLabelName(occurrence, child.name);
        }

        for (const ObjectEntry* entry : node.leaves)
        {
            if (entry == nullptr) continue;
            const std::string fallbackName = entry->stepHierarchyPath.empty()
                ? std::string("Part")
                : entry->stepHierarchyPath.back();
            const TDF_Label partLabel = addStepLeaf(shapeTool, colorTool, *entry, fallbackName);
            const TDF_Label occurrence = shapeTool->AddComponent(assemblyLabel, partLabel, TopLoc_Location());
            setLabelName(occurrence, entry->name.empty() ? fallbackName : entry->name);
            applyStoredColor(colorTool, occurrence, *entry);
        }
        return assemblyLabel;
    }

    void populateStepDocument(Engine* engine, const Handle(TDocStd_Document)& document)
    {
        const Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(document->Main());
        const Handle(XCAFDoc_ColorTool) colorTool = XCAFDoc_DocumentTool::ColorTool(document->Main());

        std::vector<std::pair<OcctObjectId, const ObjectEntry*>> shapes;
        shapes.reserve(engine->scene.objects.size());
        for (const auto& pair : engine->scene.objects)
        {
            if (pair.second.kind == OcctObject_Shape && !pair.second.shape.IsNull())
                shapes.emplace_back(pair.first, &pair.second);
        }
        if (shapes.empty()) throw std::runtime_error("There are no shapes to export.");
        std::sort(shapes.begin(), shapes.end(), [](const auto& left, const auto& right) { return left.first < right.first; });

        std::map<std::string, std::unique_ptr<StepAssemblyNode>> assemblyRoots;
        std::vector<const ObjectEntry*> topLevelShapes;
        for (const auto& pair : shapes)
        {
            const ObjectEntry& entry = *pair.second;
            if (entry.stepHierarchyPath.size() < 2U)
            {
                topLevelShapes.push_back(&entry);
                continue;
            }

            StepAssemblyNode* node = nullptr;
            for (std::size_t index = 0; index + 1U < entry.stepHierarchyPath.size(); ++index)
            {
                const std::string& segment = entry.stepHierarchyPath[index];
                if (node == nullptr)
                {
                    auto& root = assemblyRoots[segment];
                    if (!root) root = std::make_unique<StepAssemblyNode>(segment);
                    node = root.get();
                }
                else
                {
                    auto& child = node->children[segment];
                    if (!child) child = std::make_unique<StepAssemblyNode>(segment);
                    node = child.get();
                }
            }
            if (node != nullptr) node->leaves.push_back(&entry);
        }

        for (const auto& rootPair : assemblyRoots)
            buildStepAssembly(shapeTool, colorTool, *rootPair.second);

        for (const ObjectEntry* entry : topLevelShapes)
        {
            if (entry != nullptr) addStepLeaf(shapeTool, colorTool, *entry, "Part");
        }
        shapeTool->UpdateAssemblies();
    }

    void writeStepDocument(const Handle(TDocStd_Document)& document, const std::filesystem::path& path)
    {
        if (document.IsNull()) throw std::runtime_error("XDE document is null.");
        STEPCAFControl_Writer writer;
        writer.SetColorMode(true);
        writer.SetNameMode(true);
        writer.SetLayerMode(true);
        if (!writer.Transfer(document, STEPControl_AsIs))
            throw std::runtime_error("XDE document could not be transferred to STEP.");
        auto stream = outputStream(path);
        if (writer.WriteStream(stream) != IFSelect_RetDone)
            throw std::runtime_error("STEP file could not be written.");
    }

    void writeStepObject(const ObjectEntry& entry, const std::filesystem::path& path)
    {
        const Handle(TDocStd_Document) document = newXdeDocument();
        const Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(document->Main());
        const Handle(XCAFDoc_ColorTool) colorTool = XCAFDoc_DocumentTool::ColorTool(document->Main());
        addStepLeaf(shapeTool, colorTool, entry, "Part");
        writeStepDocument(document, path);
    }

    void writeStepAll(Engine* engine, const std::filesystem::path& path)
    {
        if (engine->documents.pristineStepDocumentMatchesScene && !engine->documents.pristineStepDocument.IsNull())
        {
            writeStepDocument(engine->documents.pristineStepDocument, path);
            return;
        }

        const Handle(TDocStd_Document) document = newXdeDocument();
        populateStepDocument(engine, document);
        writeStepDocument(document, path);
    }

    void writeIges(const TopoDS_Shape& shape, const std::filesystem::path& path)
    {
        IGESControl_Writer writer("MM", 1);
        if (!writer.AddShape(shape)) throw std::runtime_error("Shape could not be transferred to IGES.");
        writer.ComputeModel();
        auto stream = outputStream(path);
        if (!writer.Write(stream)) throw std::runtime_error("IGES file could not be written.");
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeExchange(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    template<typename Function>
    OcctStatus importExchange(Engine* engine, OcctObjectId* result, Function&& function)
    {
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = 0;
        return executeExchange(engine, [&]
        {
            *result = function();
            if (*result <= 0) throw std::runtime_error("Import did not create a viewer shape.");
        });
    }

    std::filesystem::path requiredPath(const char* utf8Path)
    {
        const auto path = pathFromUtf8(utf8Path);
        if (path.empty()) throw std::invalid_argument("Path is empty.");
        return path;
    }
}

extern "C"
{
    OcctStatus occt_engine_exchange_import_step(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return importExchange(engine, result, [&]
        {
            return readStepXde(engine, requiredPath(utf8Path));
        });
    }

    OcctStatus occt_engine_exchange_import_iges(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return importExchange(engine, result, [&]
        {
            const auto path = requiredPath(utf8Path);
            return engine->addShape(readIges(path), true, path.stem().u8string());
        });
    }

    OcctStatus occt_engine_exchange_import_brep(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return importExchange(engine, result, [&]
        {
            const auto path = requiredPath(utf8Path);
            return engine->addShape(readBrep(path), true, path.stem().u8string());
        });
    }

    OcctStatus occt_engine_exchange_import_stl(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return importExchange(engine, result, [&]
        {
            const auto path = requiredPath(utf8Path);
            return engine->addShape(readStl(path), true, path.stem().u8string());
        });
    }

    OcctStatus occt_engine_exchange_import_file(
        OcctEngineHandle handle,
        const char* utf8Path,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return importExchange(engine, result, [&]
        {
            const auto path = requiredPath(utf8Path);
            const std::string extension = lowerExtension(path);
            if (extension == ".step" || extension == ".stp") return readStepXde(engine, path);
            if (extension == ".iges" || extension == ".igs") return engine->addShape(readIges(path), true, path.stem().u8string());
            if (extension == ".brep" || extension == ".rle") return engine->addShape(readBrep(path), true, path.stem().u8string());
            if (extension == ".stl") return engine->addShape(readStl(path), true, path.stem().u8string());
            throw std::invalid_argument("Unsupported file extension. Supported: STEP, IGES, BREP and STL.");
        });
    }

    OcctStatus occt_engine_exchange_export_step(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeExchange(engine, [&]
        {
            writeStepObject(requiredShape(engine, objectId), requiredPath(utf8Path));
        });
    }

    OcctStatus occt_engine_exchange_export_all_step(
        OcctEngineHandle handle,
        const char* utf8Path)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeExchange(engine, [&]
        {
            writeStepAll(engine, requiredPath(utf8Path));
        });
    }

    OcctStatus occt_engine_exchange_export_iges(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeExchange(engine, [&]
        {
            writeIges(shapeWithPresentationTransformation(requiredShape(engine, objectId)), requiredPath(utf8Path));
        });
    }

    OcctStatus occt_engine_exchange_export_all_iges(
        OcctEngineHandle handle,
        const char* utf8Path)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeExchange(engine, [&]
        {
            writeIges(allShapes(engine), requiredPath(utf8Path));
        });
    }

    OcctStatus occt_engine_exchange_export_brep(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeExchange(engine, [&]
        {
            auto stream = outputStream(requiredPath(utf8Path));
            BRepTools::Write(shapeWithPresentationTransformation(requiredShape(engine, objectId)), stream);
            if (!stream) throw std::runtime_error("BREP file could not be written.");
        });
    }

    OcctStatus occt_engine_exchange_export_stl(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const char* utf8Path,
        double linearDeflection,
        double angularDeflection,
        OcctBool asciiMode)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeExchange(engine, [&]
        {
            requirePositive(linearDeflection, "Linear deflection");
            requirePositive(angularDeflection, "Angular deflection");
            const TopoDS_Shape shape = shapeWithPresentationTransformation(requiredShape(engine, objectId));
            BRepMesh_IncrementalMesh mesh(shape, linearDeflection, Standard_False, angularDeflection, Standard_True);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("STL meshing failed.");
            const auto path = requiredPath(utf8Path);
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
            StlAPI_Writer writer;
            writer.ASCIIMode() = asciiMode != 0;
            if (!writer.Write(shape, path.string().c_str()))
                throw std::runtime_error("STL file could not be written. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        });
    }
}
