from pathlib import Path

path = Path("src/OcctNative/OcctEngine.cpp")
text = path.read_text(encoding="utf-8-sig")
old = '''    OcctObjectId Engine::addShape(const TopoDS_Shape& shape, bool fit, const std::string& name)
    {
        if (shape.IsNull()) throw std::runtime_error("OCCT returned a null shape.");
        if (!isInitialized()) throw std::runtime_error("The OCCT viewer has not been initialized.");
        const OcctObjectId id = nextId++;
        Handle(AIS_Shape) ais = new AIS_Shape(shape);
        ais->SetDisplayMode(displayMode);
        context->Display(ais, Standard_False);
        applySelectionMode(ais);
        objects.emplace(id, ObjectEntry{OcctObject_Shape, shape, ais, name});
        if (fit) requestFitAll();
        else requestRedraw();
        return id;
    }
'''
new = '''    OcctObjectId Engine::addShape(const TopoDS_Shape& shape, bool /*fit*/, const std::string& name)
    {
        if (shape.IsNull()) throw std::runtime_error("OCCT returned a null shape.");
        if (!isInitialized()) throw std::runtime_error("The OCCT viewer has not been initialized.");
        const OcctObjectId id = nextId++;
        Handle(AIS_Shape) ais = new AIS_Shape(shape);
        ais->SetDisplayMode(displayMode);
        context->Display(ais, Standard_False);
        applySelectionMode(ais);
        objects.emplace(id, ObjectEntry{OcctObject_Shape, shape, ais, name});
        // Shape creation changes the scene but must not change the user's camera.
        // Fit/FitAll remain explicit public view operations.
        requestRedraw();
        return id;
    }
'''
if old not in text:
    if new in text:
        print("No-auto-fit patch already applied.")
    else:
        raise SystemExit("Expected addShape implementation was not found.")
else:
    path.write_text(text.replace(old, new), encoding="utf-8", newline="\n")
    print("Disabled automatic FitAll after shape creation.")
