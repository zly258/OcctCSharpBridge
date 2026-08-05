from pathlib import Path


def read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8-sig")


def write(path: str, text: str) -> None:
    Path(path).write_text(text, encoding="utf-8", newline="\n")


def replace_once(text: str, old: str, new: str, label: str) -> str:
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected exactly one match, found {count}")
    return text.replace(old, new, 1)


# Native engine state and helpers.
path = "src/OcctNative/OcctInternal.hxx"
text = read(path)
text = replace_once(
    text,
    "        int displayMode = AIS_Shaded;\n        int selectionMode = OcctSelection_Object;\n",
    "        int displayMode = AIS_Shaded;\n        int selectionMode = OcctSelection_Object;\n        int updateDepth = 0;\n        bool redrawPending = false;\n        bool fitAllPending = false;\n",
    "OcctInternal fields")
text = replace_once(
    text,
    "        void applySelectionMode(const Handle(AIS_InteractiveObject)& presentation);\n",
    "        void applySelectionMode(const Handle(AIS_InteractiveObject)& presentation);\n        void beginUpdate();\n        void endUpdate(bool fitAll);\n        void requestRedraw();\n        void requestFitAll();\n        bool isUpdating() const;\n",
    "OcctInternal methods")
write(path, text)

# Public C ABI.
path = "src/OcctNative/OcctNative.h"
text = read(path)
text = replace_once(
    text,
    "    OCCTBRIDGE_API int occt_redraw(OcctHandle handle);\n    OCCTBRIDGE_API int occt_fit_all(OcctHandle handle);\n",
    "    OCCTBRIDGE_API int occt_redraw(OcctHandle handle);\n    OCCTBRIDGE_API int occt_begin_update(OcctHandle handle);\n    OCCTBRIDGE_API int occt_end_update(OcctHandle handle, int fitAll);\n    OCCTBRIDGE_API int occt_is_updating(OcctHandle handle);\n    OCCTBRIDGE_API int occt_fit_all(OcctHandle handle);\n",
    "OcctNative batch declarations")
write(path, text)

# Native batching implementation and redraw suppression.
path = "src/OcctNative/OcctEngine.cpp"
text = read(path)
text = replace_once(
    text,
    "    void Engine::setError(const std::string& message) { lastError = message; }\n\n",
    "    void Engine::setError(const std::string& message) { lastError = message; }\n\n"
    "    bool Engine::isUpdating() const { return updateDepth > 0; }\n\n"
    "    void Engine::beginUpdate()\n"
    "    {\n"
    "        ++updateDepth;\n"
    "    }\n\n"
    "    void Engine::requestRedraw()\n"
    "    {\n"
    "        if (isUpdating())\n"
    "        {\n"
    "            redrawPending = true;\n"
    "            return;\n"
    "        }\n"
    "        view->Redraw();\n"
    "    }\n\n"
    "    void Engine::requestFitAll()\n"
    "    {\n"
    "        if (isUpdating())\n"
    "        {\n"
    "            fitAllPending = true;\n"
    "            redrawPending = true;\n"
    "            return;\n"
    "        }\n"
    "        view->FitAll(0.01, Standard_False);\n"
    "        view->ZFitAll();\n"
    "        view->Redraw();\n"
    "    }\n\n"
    "    void Engine::endUpdate(bool fitAll)\n"
    "    {\n"
    "        if (updateDepth <= 0) throw std::logic_error(\"No OCCT display batch is active.\");\n"
    "        if (fitAll)\n"
    "        {\n"
    "            fitAllPending = true;\n"
    "            redrawPending = true;\n"
    "        }\n"
    "        --updateDepth;\n"
    "        if (updateDepth > 0) return;\n\n"
    "        if (fitAllPending)\n"
    "        {\n"
    "            view->FitAll(0.01, Standard_False);\n"
    "            view->ZFitAll();\n"
    "        }\n"
    "        if (fitAllPending || redrawPending) view->Redraw();\n"
    "        fitAllPending = false;\n"
    "        redrawPending = false;\n"
    "    }\n\n",
    "OcctEngine batch helper definitions")
text = replace_once(
    text,
    "        context->Activate(presentation, mode, Standard_True);\n",
    "        context->Activate(presentation, mode, Standard_False);\n",
    "deferred selection activation")
text = replace_once(
    text,
    "        if (fit)\n        {\n            view->FitAll(0.01, Standard_True);\n            view->ZFitAll();\n        }\n        else\n        {\n            view->Redraw();\n        }\n",
    "        if (fit) requestFitAll();\n        else requestRedraw();\n",
    "batched addShape")
text = replace_once(
    text,
    "        objects.emplace(id, ObjectEntry{kind, TopoDS_Shape(), presentation, name});\n        view->Redraw();\n",
    "        objects.emplace(id, ObjectEntry{kind, TopoDS_Shape(), presentation, name});\n        requestRedraw();\n",
    "batched addPresentation")
text = replace_once(
    text,
    "    int occt_redraw(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->Redraw(); }); }\n    int occt_fit_all(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->view->FitAll(0.01, Standard_True); e->view->ZFitAll(); }); }\n",
    "    int occt_redraw(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->requestRedraw(); }); }\n"
    "    int occt_begin_update(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->beginUpdate(); }); }\n"
    "    int occt_end_update(OcctHandle h, int fitAll) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->endUpdate(fitAll != 0); }); }\n"
    "    int occt_is_updating(OcctHandle h) { Engine* e = engineOf(h); return e != nullptr && e->isUpdating() ? 1 : 0; }\n"
    "    int occt_fit_all(OcctHandle h) { Engine* e = engineOf(h); if (!validateInitialized(e)) return 0; return execute(e, [&] { e->requestFitAll(); }); }\n",
    "batch C functions")

replacements = [
    (
        '    int occt_set_object_color(OcctHandle h, OcctObjectId id, double r, double g, double b) { Engine* e=engineOf(h); if(!validateInitialized(e))return 0; return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetColor(o->presentation,color(r,g,b),Standard_True);}); }',
        '    int occt_set_object_color(OcctHandle h, OcctObjectId id, double r, double g, double b) { Engine* e=engineOf(h); if(!validateInitialized(e))return 0; return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetColor(o->presentation,color(r,g,b),Standard_False);e->requestRedraw();}); }',
        "batched color"),
    (
        '    int occt_set_object_transparency(OcctHandle h, OcctObjectId id, double value) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetTransparency(o->presentation,std::clamp(value,0.0,1.0),Standard_True);}); }',
        '    int occt_set_object_transparency(OcctHandle h, OcctObjectId id, double value) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetTransparency(o->presentation,std::clamp(value,0.0,1.0),Standard_False);e->requestRedraw();}); }',
        "batched transparency"),
    (
        '    int occt_set_object_visible(OcctHandle h, OcctObjectId id, int visible) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");if(visible)e->context->Display(o->presentation,Standard_True);else e->context->Erase(o->presentation,Standard_True);}); }',
        '    int occt_set_object_visible(OcctHandle h, OcctObjectId id, int visible) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");if(visible)e->context->Display(o->presentation,Standard_False);else e->context->Erase(o->presentation,Standard_False);e->requestRedraw();}); }',
        "batched visibility"),
    (
        '    int occt_set_object_display_mode(OcctHandle h, OcctObjectId id, int mode) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetDisplayMode(o->presentation,mode==OcctDisplay_Wireframe?AIS_WireFrame:AIS_Shaded,Standard_True);}); }',
        '    int occt_set_object_display_mode(OcctHandle h, OcctObjectId id, int mode) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetDisplayMode(o->presentation,mode==OcctDisplay_Wireframe?AIS_WireFrame:AIS_Shaded,Standard_False);e->requestRedraw();}); }',
        "batched object display mode"),
    (
        '    int occt_set_object_line_width(OcctHandle h, OcctObjectId id, double width) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{requirePositive(width,"Line width");ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetWidth(o->presentation,width,Standard_True);}); }',
        '    int occt_set_object_line_width(OcctHandle h, OcctObjectId id, double width) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{requirePositive(width,"Line width");ObjectEntry* o=e->findObject(id);if(!o)throw std::invalid_argument("Object ID does not exist.");e->context->SetWidth(o->presentation,width,Standard_False);e->requestRedraw();}); }',
        "batched line width"),
    (
        '            e->context->SetMaterial(entry->presentation, Graphic3d_MaterialAspect(materialName(material)), Standard_True);',
        '            e->context->SetMaterial(entry->presentation, Graphic3d_MaterialAspect(materialName(material)), Standard_False);\n            e->requestRedraw();',
        "batched material"),
    (
        '    int occt_delete_object(OcctHandle h, OcctObjectId id) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{e->erase(id);e->view->Redraw();}); }',
        '    int occt_delete_object(OcctHandle h, OcctObjectId id) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{e->erase(id);e->requestRedraw();}); }',
        "batched delete"),
    (
        '            e->context->ClearSelected(Standard_False);\n            e->view->Redraw();',
        '            e->context->ClearSelected(Standard_False);\n            e->requestRedraw();',
        "batched clear")
]
for old, new, label in replacements:
    text = replace_once(text, old, new, label)
write(path, text)

# Managed P/Invoke declarations.
write(
    "src/OcctNet/BatchNativeMethods.cs",
    '''using System.Runtime.InteropServices;\n\nnamespace OcctNet;\n\ninternal static class BatchNativeMethods\n{\n    private const string LibraryName = "OcctNative";\n\n    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]\n    internal static extern int occt_begin_update(IntPtr handle);\n\n    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]\n    internal static extern int occt_end_update(IntPtr handle, int fitAll);\n\n    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]\n    internal static extern int occt_is_updating(IntPtr handle);\n}\n''')

# Managed disposable batch API.
write(
    "src/OcctNet/OcctEngine.Batch.cs",
    '''using System.Threading;\n\nnamespace OcctNet;\n\n/// <summary>\n/// Defers OCCT viewer updates until the batch is disposed. Batches can be nested.\n/// </summary>\npublic sealed class OcctDisplayBatch : IDisposable\n{\n    private OcctEngine? _engine;\n\n    internal OcctDisplayBatch(OcctEngine engine, bool fitAllOnDispose)\n    {\n        _engine = engine;\n        FitAllOnDispose = fitAllOnDispose;\n    }\n\n    /// <summary>Fits all displayed objects before the final redraw when this outermost batch ends.</summary>\n    public bool FitAllOnDispose { get; set; }\n\n    public void Dispose()\n    {\n        var engine = Interlocked.Exchange(ref _engine, null);\n        if (engine is not null) engine.EndDisplayBatch(FitAllOnDispose);\n    }\n}\n\npublic sealed partial class OcctEngine\n{\n    /// <summary>Returns true while one or more display update batches are active.</summary>\n    public bool IsDisplayBatchActive\n    {\n        get\n        {\n            EnsureInitialized();\n            return BatchNativeMethods.occt_is_updating(_handle) != 0;\n        }\n    }\n\n    /// <summary>\n    /// Defers Display, Redisplay and view redraw work until the returned scope is disposed.\n    /// Use this when creating or changing several objects in one operation.\n    /// </summary>\n    public OcctDisplayBatch BeginDisplayBatch(bool fitAllOnDispose = false)\n    {\n        EnsureInitialized();\n        Check(BatchNativeMethods.occt_begin_update(_handle));\n        return new OcctDisplayBatch(this, fitAllOnDispose);\n    }\n\n    internal void EndDisplayBatch(bool fitAll)\n    {\n        if (_handle == IntPtr.Zero || !_initialized) return;\n        Check(BatchNativeMethods.occt_end_update(_handle, fitAll ? 1 : 0));\n    }\n}\n''')

print("Batched viewer update patch applied.")
