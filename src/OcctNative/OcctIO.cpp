#include "OcctInternal.hxx"

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
#include <TDF_LabelSequence.hxx>
#include <TDocStd_Document.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS_Compound.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>

#include <fstream>
#include <map>

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
        Handle(TDocStd_Document) document = new TDocStd_Document(TCollection_ExtendedString("BinXCAF"));
        XCAFDoc_DocumentTool::Set(document->Main());
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

    bool tryGetLabelColor(
        const Handle(XCAFDoc_ColorTool)& colorTool,
        const TDF_Label& occurrence,
        const TDF_Label& definition,
        const TopoDS_Shape& occurrenceShape,
        Quantity_Color& result)
    {
        const XCAFDoc_ColorType colorTypes[] = { XCAFDoc_ColorSurf, XCAFDoc_ColorGen, XCAFDoc_ColorCurv };
        if (!occurrenceShape.IsNull())
        {
            for (const XCAFDoc_ColorType type : colorTypes)
            {
                if (colorTool->GetInstanceColor(occurrenceShape, type, result)) return true;
            }
        }

        for (const XCAFDoc_ColorType type : colorTypes)
        {
            if (!occurrence.IsNull() && XCAFDoc_ColorTool::GetColor(occurrence, type, result)) return true;
            if (!definition.IsNull() && definition != occurrence && XCAFDoc_ColorTool::GetColor(definition, type, result)) return true;
        }
        return false;
    }

    void rememberAndApplyColor(Engine* engine, ObjectEntry& entry, const Quantity_Color& value)
    {
        entry.hasStoredColor = true;
        entry.storedColorR = value.Red();
        entry.storedColorG = value.Green();
        entry.storedColorB = value.Blue();
        engine->context->SetColor(entry.presentation, value, Standard_False);
    }

    void importXdeLabel(
        Engine* engine,
        const Handle(XCAFDoc_ShapeTool)& shapeTool,
        const Handle(XCAFDoc_ColorTool)& colorTool,
        const TDF_Label& occurrence,
        const TopLoc_Location& ancestorLocation,
        std::vector<std::string> path,
        OcctObjectId& firstImportedId)
    {
        TDF_Label definition = occurrence;
        const bool isReference = shapeTool->IsReference(occurrence);
        if (isReference && !shapeTool->GetReferredShape(occurrence, definition))
            throw std::runtime_error("STEP assembly contains an unresolved component reference.");

        std::string currentName = labelName(occurrence);
        if (currentName.empty()) currentName = labelName(definition);
        const bool isAssembly = shapeTool->IsAssembly(definition);
        if (currentName.empty()) currentName = isAssembly ? "Assembly" : "Part";
        path.push_back(currentName);

        if (isAssembly)
        {
            TopLoc_Location nextAncestor = ancestorLocation;
            if (isReference)
                nextAncestor = ancestorLocation.Multiplied(shapeTool->GetLocation(occurrence));

            TDF_LabelSequence components;
            shapeTool->GetComponents(definition, components, false);
            for (Standard_Integer index = 1; index <= components.Length(); ++index)
                importXdeLabel(engine, shapeTool, colorTool, components.Value(index), nextAncestor, path, firstImportedId);
            return;
        }

        TopoDS_Shape occurrenceShape = shapeTool->GetShape(occurrence);
        if (occurrenceShape.IsNull() && definition != occurrence)
            occurrenceShape = shapeTool->GetShape(definition);
        if (occurrenceShape.IsNull()) return;

        Quantity_Color importedColor;
        const bool hasColor = tryGetLabelColor(colorTool, occurrence, definition, occurrenceShape, importedColor);

        TopoDS_Shape displayShape = occurrenceShape;
        if (!ancestorLocation.IsIdentity())
            displayShape.Location(ancestorLocation.Multiplied(displayShape.Location()));

        const OcctObjectId objectId = engine->addShape(displayShape, false, currentName);
        ObjectEntry* entry = engine->findShape(objectId);
        if (entry == nullptr) throw std::runtime_error("Imported STEP shape could not be registered.");
        entry->stepHierarchyPath = path;
        entry->applicationTag = hierarchyTag(path);
        if (hasColor) rememberAndApplyColor(engine, *entry, importedColor);
        if (firstImportedId == 0) firstImportedId = objectId;
    }

    OcctObjectId readStepXde(Engine* engine, const std::filesystem::path& path)
    {
        auto stream = inputStream(path);
        const Handle(TDocStd_Document) document = newXdeDocument();
        STEPCAFControl_Reader reader;
        reader.SetColorMode(true);
        reader.SetNameMode(true);
        reader.SetLayerMode(true);
        if (reader.ReadStream(path.filename().string().c_str(), stream) != IFSelect_RetDone)
            throw std::runtime_error("STEP file could not be read.");
        if (!reader.Transfer(document))
            throw std::runtime_error("STEP document could not be transferred to XDE.");

        const Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(document->Main());
        const Handle(XCAFDoc_ColorTool) colorTool = XCAFDoc_DocumentTool::ColorTool(document->Main());
        TDF_LabelSequence roots;
        shapeTool->GetFreeShapes(roots);
        if (roots.IsEmpty()) throw std::runtime_error("STEP file contains no transferable shapes.");

        OcctObjectId firstImportedId = 0;
        engine->beginUpdate();
        try
        {
            for (Standard_Integer index = 1; index <= roots.Length(); ++index)
                importXdeLabel(engine, shapeTool, colorTool, roots.Value(index), TopLoc_Location(), {}, firstImportedId);
            engine->endUpdate(false);
        }
        catch (...)
        {
            engine->endUpdate(false);
            throw;
        }

        if (firstImportedId == 0) throw std::runtime_error("STEP file contains no displayable leaf shapes.");
        engine->requestRedraw();
        return firstImportedId;
    }

    TopoDS_Shape allShapes(Engine* engine)
    {
        BRep_Builder builder;
        TopoDS_Compound compound;
        builder.MakeCompound(compound);
        int count = 0;
        for (const auto& pair : engine->objects)
        {
            if (pair.second.kind == OcctObject_Shape && !pair.second.shape.IsNull())
            {
                builder.Add(compound, pair.second.shape);
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
        shapes.reserve(engine->objects.size());
        for (const auto& pair : engine->objects)
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
}

extern "C"
{
    OcctObjectId occt_import_step(OcctHandle h, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            const auto path = pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return readStepXde(e, path);
        });
    }

    OcctObjectId occt_import_iges(OcctHandle h, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            const auto path = pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return e->addShape(readIges(path), true, path.stem().u8string());
        });
    }

    OcctObjectId occt_import_brep(OcctHandle h, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            const auto path = pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return e->addShape(readBrep(path), true, path.stem().u8string());
        });
    }

    OcctObjectId occt_import_stl(OcctHandle h, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            const auto path = pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            return e->addShape(readStl(path), true, path.stem().u8string());
        });
    }

    OcctObjectId occt_import_file(OcctHandle h, const char* utf8Path)
    {
        const auto path = pathFromUtf8(utf8Path);
        const std::string extension = lowerExtension(path);
        if (extension == ".step" || extension == ".stp") return occt_import_step(h, utf8Path);
        if (extension == ".iges" || extension == ".igs") return occt_import_iges(h, utf8Path);
        if (extension == ".brep" || extension == ".rle") return occt_import_brep(h, utf8Path);
        if (extension == ".stl") return occt_import_stl(h, utf8Path);
        Engine* e = engineOf(h);
        if (e) e->setError("Unsupported file extension. Supported: STEP, IGES, BREP and STL.");
        return 0;
    }

    int occt_export_step(OcctHandle h, OcctObjectId id, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { writeStepObject(requiredShape(e, id), pathFromUtf8(utf8Path)); });
    }

    int occt_export_all_step(OcctHandle h, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { writeStepAll(e, pathFromUtf8(utf8Path)); });
    }

    int occt_export_iges(OcctHandle h, OcctObjectId id, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { writeIges(requiredShape(e, id).shape, pathFromUtf8(utf8Path)); });
    }

    int occt_export_all_iges(OcctHandle h, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { writeIges(allShapes(e), pathFromUtf8(utf8Path)); });
    }

    int occt_export_brep(OcctHandle h, OcctObjectId id, const char* utf8Path)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            auto stream = outputStream(pathFromUtf8(utf8Path));
            BRepTools::Write(requiredShape(e, id).shape, stream);
            if (!stream) throw std::runtime_error("BREP file could not be written.");
        });
    }

    int occt_export_stl(OcctHandle h, OcctObjectId id, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(linearDeflection, "Linear deflection");
            requirePositive(angularDeflection, "Angular deflection");
            ObjectEntry& shape = requiredShape(e, id);
            BRepMesh_IncrementalMesh mesh(shape.shape, linearDeflection, Standard_False, angularDeflection, Standard_True);
            mesh.Perform();
            if (!mesh.IsDone()) throw std::runtime_error("STL meshing failed.");
            const auto path = pathFromUtf8(utf8Path);
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
            StlAPI_Writer writer;
            writer.ASCIIMode() = asciiMode != 0;
            if (!writer.Write(shape.shape, path.string().c_str()))
                throw std::runtime_error("STL file could not be written. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        });
    }
}
