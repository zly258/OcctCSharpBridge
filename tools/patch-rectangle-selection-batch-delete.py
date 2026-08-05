from pathlib import Path
import re


def read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8-sig")


def write(path: str, text: str) -> None:
    Path(path).write_text(text, encoding="utf-8", newline="\n")


def replace_once(text: str, old: str, new: str, label: str) -> str:
    count = text.count(old)
    if count != 1:
        raise SystemExit(f"Expected exactly one {label}, found {count}.")
    return text.replace(old, new, 1)


# Native public C ABI.
path = "src/OcctNative/OcctNative.h"
text = read(path)
text = replace_once(
    text,
    "    OCCTBRIDGE_API int occt_select_rectangle(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection);\n",
    "    OCCTBRIDGE_API int occt_select_rectangle(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection);\n"
    "    OCCTBRIDGE_API int occt_select_rectangle_ex(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection, int allowOverlap);\n",
    "rectangle selection declaration",
)
text = replace_once(
    text,
    "    OCCTBRIDGE_API int occt_delete_object(OcctHandle handle, OcctObjectId objectId);\n",
    "    OCCTBRIDGE_API int occt_delete_object(OcctHandle handle, OcctObjectId objectId);\n"
    "    OCCTBRIDGE_API int occt_delete_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);\n",
    "batch delete declaration",
)
write(path, text)

# Native implementation.
path = "src/OcctNative/OcctEngine.cpp"
text = read(path)
if "#include <StdSelect_ViewerSelector3d.hxx>" not in text:
    text = replace_once(
        text,
        "#include <Standard_Version.hxx>\n",
        "#include <Standard_Version.hxx>\n#include <StdSelect_ViewerSelector3d.hxx>\n",
        "viewer selector include anchor",
    )

selection_pattern = re.compile(
    r"    int occt_select_rectangle\(OcctHandle h, int x1, int y1, int x2, int y2, int append\)\n"
    r"    \{\n"
    r".*?"
    r"    \}\n\n"
    r"    int occt_select_object",
    re.DOTALL,
)
selection_replacement = """    int occt_select_rectangle_ex(OcctHandle h, int x1, int y1, int x2, int y2, int append, int allowOverlap)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            // OCCT uses full inclusion for rectangle selection by default. Configure the
            // selector explicitly for every gesture so callers can request crossing selection.
            const Handle(StdSelect_ViewerSelector3d)& selector = e->context->MainSelector();
            selector->AllowOverlapDetection(allowOverlap != 0);
            Graphic3d_Vec2i minPoint(std::min(x1,x2), std::min(y1,y2));
            Graphic3d_Vec2i maxPoint(std::max(x1,x2), std::max(y1,y2));
            e->context->SelectRectangle(
                minPoint,
                maxPoint,
                e->view,
                append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            e->view->Redraw();
        });
    }

    int occt_select_rectangle(OcctHandle h, int x1, int y1, int x2, int y2, int append)
    {
        return occt_select_rectangle_ex(h, x1, y1, x2, y2, append, 0);
    }

    int occt_select_object"""
text, count = selection_pattern.subn(selection_replacement, text, count=1)
if count != 1:
    raise SystemExit(f"Expected one native rectangle selection function, found {count}.")

old_delete = "    int occt_delete_object(OcctHandle h, OcctObjectId id) { Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{e->erase(id);e->requestRedraw();}); }"
new_delete = """    int occt_delete_objects(OcctHandle h, const OcctObjectId* ids, int count)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count < 0) throw std::invalid_argument("Object count must not be negative.");
            if (count > 0 && ids == nullptr) throw std::invalid_argument("Object ID array is null.");

            std::vector<OcctObjectId> uniqueIds;
            uniqueIds.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
            {
                const OcctObjectId id = ids[index];
                if (e->findObject(id) == nullptr) throw std::invalid_argument("Object ID does not exist.");
                if (std::find(uniqueIds.begin(), uniqueIds.end(), id) == uniqueIds.end())
                    uniqueIds.push_back(id);
            }

            // Validate the complete request before mutating the registry. Removal is then
            // performed without viewer updates and flushed exactly once for the whole batch.
            for (const OcctObjectId id : uniqueIds) e->erase(id);
            if (!uniqueIds.empty()) e->requestRedraw();
        });
    }

    int occt_delete_object(OcctHandle h, OcctObjectId id)
    {
        return occt_delete_objects(h, &id, 1);
    }"""
text = replace_once(text, old_delete, new_delete, "single native delete implementation")
write(path, text)

# P/Invoke declarations.
path = "src/OcctNet/NativeMethods.cs"
text = read(path)
text = replace_once(
    text,
    "    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_select_rectangle(IntPtr handle, int x1, int y1, int x2, int y2, int appendSelection);\n",
    "    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_select_rectangle(IntPtr handle, int x1, int y1, int x2, int y2, int appendSelection);\n"
    "    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_select_rectangle_ex(IntPtr handle, int x1, int y1, int x2, int y2, int appendSelection, int allowOverlap);\n",
    "rectangle selection P/Invoke",
)
text = replace_once(
    text,
    "    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_delete_object(IntPtr handle, long objectId);\n",
    "    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_delete_object(IntPtr handle, long objectId);\n"
    "    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_delete_objects(IntPtr handle, [In] long[] objectIds, int count);\n",
    "batch delete P/Invoke",
)
write(path, text)

# Managed public selection behavior enum.
path = "src/OcctNet/OcctTypes.cs"
text = read(path)
text = replace_once(
    text,
    "public enum OcctSelectionMode { Object = 0, Vertex = 1, Edge = 2, Wire = 3, Face = 4, Shell = 5, Solid = 6 }\n",
    "public enum OcctSelectionMode { Object = 0, Vertex = 1, Edge = 2, Wire = 3, Face = 4, Shell = 5, Solid = 6 }\n"
    "public enum OcctRectangleSelectionBehavior { Inclusive = 0, Overlap = 1, Directional = 2 }\n",
    "rectangle selection behavior enum",
)
write(path, text)

# Managed engine APIs.
path = "src/OcctNet/OcctEngine.cs"
text = read(path)
text = replace_once(
    text,
    "    public void SelectRectangle(int x1, int y1, int x2, int y2, bool appendSelection = false) => CheckInitialized(() => NativeMethods.occt_select_rectangle(_handle, x1, y1, x2, y2, appendSelection ? 1 : 0));\n",
    "    public void SelectRectangle(int x1, int y1, int x2, int y2, bool appendSelection = false, bool allowOverlap = false) => CheckInitialized(() => NativeMethods.occt_select_rectangle_ex(_handle, x1, y1, x2, y2, appendSelection ? 1 : 0, allowOverlap ? 1 : 0));\n",
    "managed rectangle selection method",
)
text = replace_once(
    text,
    "    public void Delete(IOcctObject value) => CheckInitialized(() => NativeMethods.occt_delete_object(_handle, value.Id));\n",
    """    public void Delete(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Delete(new[] { value });
    }

    public void Delete(IEnumerable<IOcctObject> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        EnsureInitialized();

        var ids = new HashSet<long>();
        foreach (var value in values)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Id <= 0) throw new ArgumentException("Object IDs must be greater than zero.", nameof(values));
            ids.Add(value.Id);
        }

        if (ids.Count == 0) return;
        var objectIds = ids.ToArray();
        Check(NativeMethods.occt_delete_objects(_handle, objectIds, objectIds.Length));
    }
""",
    "managed delete method",
)
write(path, text)

# Robust rectangle gesture state and overlap selection behavior.
path = "src/OcctNet/OcctViewportControl.cs"
text = read(path)
text = replace_once(
    text,
    "    private Point _selectionStart;\n    private bool _rotating;\n",
    "    private Point _selectionStart;\n    private Point _selectionCurrent;\n    private bool _rectangleDragStarted;\n    private bool _rotating;\n",
    "rectangle gesture fields",
)
text = replace_once(
    text,
    "    public int RectangleSelectionThreshold { get; set; } = 5;\n",
    "    public int RectangleSelectionThreshold { get; set; } = 5;\n"
    "    public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; } = OcctRectangleSelectionBehavior.Overlap;\n",
    "rectangle selection behavior property",
)
text = replace_once(
    text,
    "            _selectionStart = e.Location;\n            _selectingRectangle = EnableRectangleSelection;\n            Capture = true;\n",
    "            _selectionStart = e.Location;\n            _selectionCurrent = e.Location;\n            _rectangleDragStarted = false;\n            _selectingRectangle = EnableRectangleSelection;\n            Capture = true;\n",
    "left mouse-down rectangle initialization",
)
text = replace_once(
    text,
    "        else if (_selectingRectangle && e.Button.HasFlag(MouseButtons.Left))\n        {\n            UpdateSelectionFrame(e.Location);\n        }\n",
    "        else if (_selectingRectangle\n                 && (Capture || Control.MouseButtons.HasFlag(MouseButtons.Left)))\n        {\n            _selectionCurrent = e.Location;\n            UpdateSelectionFrame(e.Location);\n        }\n",
    "rectangle mouse move handling",
)
old_mouse_up = """        var dx = Math.Abs(e.X - _selectionStart.X);
        var dy = Math.Abs(e.Y - _selectionStart.Y);
        var useRectangle = _selectingRectangle
                           && (dx >= RectangleSelectionThreshold || dy >= RectangleSelectionThreshold);
        var append = ModifierKeys.HasFlag(Keys.Control);

        // Preserve the gesture result before releasing capture. CaptureChanged is raised synchronously
        // by WinForms; the previous implementation cleared _selectingRectangle here and therefore
        // every box gesture incorrectly fell back to point selection.
        _selectingRectangle = false;
        HideSelectionFrame();
        ReleaseMouseCapture();

        if (_engine?.IsInitialized != true) return;
        TryInvoke(() =>
        {
            if (useRectangle)
                _engine.SelectRectangle(_selectionStart.X, _selectionStart.Y, e.X, e.Y, append);
            else
                _engine.Select(e.X, e.Y, append);
            RaiseSelectionChanged();
        });
"""
new_mouse_up = """        var end = e.Location;
        var eventDistance = Math.Max(
            Math.Abs(end.X - _selectionStart.X),
            Math.Abs(end.Y - _selectionStart.Y));
        var trackedDistance = Math.Max(
            Math.Abs(_selectionCurrent.X - _selectionStart.X),
            Math.Abs(_selectionCurrent.Y - _selectionStart.Y));
        if (_rectangleDragStarted && trackedDistance > eventDistance)
            end = _selectionCurrent;

        var dx = Math.Abs(end.X - _selectionStart.X);
        var dy = Math.Abs(end.Y - _selectionStart.Y);
        var useRectangle = EnableRectangleSelection
                           && (_rectangleDragStarted
                               || dx >= RectangleSelectionThreshold
                               || dy >= RectangleSelectionThreshold);
        var append = ModifierKeys.HasFlag(Keys.Control);
        var allowOverlap = RectangleSelectionBehavior switch
        {
            OcctRectangleSelectionBehavior.Overlap => true,
            OcctRectangleSelectionBehavior.Directional => end.X < _selectionStart.X,
            _ => false
        };

        // MouseCaptureChanged may be raised before MouseUp, especially when this WinForms
        // control is hosted by WPF. The recognized drag is stored independently so the
        // gesture cannot silently degrade into a point selection.
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        ReleaseMouseCapture();

        if (_engine?.IsInitialized != true) return;
        TryInvoke(() =>
        {
            if (useRectangle)
                _engine.SelectRectangle(
                    _selectionStart.X,
                    _selectionStart.Y,
                    end.X,
                    end.Y,
                    append,
                    allowOverlap);
            else
                _engine.Select(e.X, e.Y, append);
            RaiseSelectionChanged();
        });
"""
text = replace_once(text, old_mouse_up, new_mouse_up, "rectangle mouse-up handling")
text = replace_once(
    text,
    "    protected override void OnMouseCaptureChanged(EventArgs e)\n    {\n        if (!Capture && !_releasingMouseCapture)\n            CancelRectangleSelection();\n        base.OnMouseCaptureChanged(e);\n    }\n",
    """    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        if (!Capture && !_releasingMouseCapture)
        {
            // Do not clear a recognized rectangle here. WinFormsHost and DPI/layout changes
            // can deliver CaptureChanged before MouseUp; MouseUp must still finalize the box.
            HideSelectionFrame();
        }
        base.OnMouseCaptureChanged(e);
    }
""",
    "mouse capture handling",
)
text = replace_once(
    text,
    "    private void UpdateSelectionFrame(Point current)\n    {\n        if (_engine?.IsInitialized != true) return;\n\n        var dx = Math.Abs(current.X - _selectionStart.X);\n",
    "    private void UpdateSelectionFrame(Point current)\n    {\n        if (_engine?.IsInitialized != true) return;\n\n        _selectionCurrent = current;\n        var dx = Math.Abs(current.X - _selectionStart.X);\n",
    "selection frame current point tracking",
)
text = replace_once(
    text,
    "        if (dx < RectangleSelectionThreshold && dy < RectangleSelectionThreshold)\n        {\n            HideSelectionFrame();\n            return;\n        }\n\n        var rectangle = Rectangle.FromLTRB(\n",
    "        if (dx < RectangleSelectionThreshold && dy < RectangleSelectionThreshold)\n        {\n            HideSelectionFrame();\n            return;\n        }\n\n        _rectangleDragStarted = true;\n        var rectangle = Rectangle.FromLTRB(\n",
    "rectangle drag recognition",
)
text = replace_once(
    text,
    "    private void CancelRectangleSelection()\n    {\n        _selectingRectangle = false;\n        HideSelectionFrame();\n",
    "    private void CancelRectangleSelection()\n    {\n        _selectingRectangle = false;\n        _rectangleDragStarted = false;\n        _selectionCurrent = Point.Empty;\n        HideSelectionFrame();\n",
    "rectangle cancellation reset",
)
write(path, text)

print("Applied robust rectangle selection and native batch deletion.")
