namespace OcctNet;

public sealed partial class OcafDocument
{
    public void NamingGenerated(OcafLabel label, OcctModelingSession model, OcctModelShape generated)
    {
        var shape = Shape(model, generated);
        Check(OcafNativeMethods.occt_ocaf_naming_generated(NativeHandle, shape.Handle, Entry(label), shape.Id), "record generated shape");
    }

    public void NamingGeneratedFrom(OcafLabel label, OcctModelingSession model, OcctModelShape source, OcctModelShape generated)
    {
        var oldShape = Shape(model, source); var newShape = Shape(model, generated);
        Check(OcafNativeMethods.occt_ocaf_naming_generated_from(NativeHandle, oldShape.Handle, Entry(label), oldShape.Id, newShape.Id), "record generated-from history");
    }

    public void NamingModify(OcafLabel label, OcctModelingSession model, OcctModelShape oldShape, OcctModelShape newShape)
    {
        var oldNative = Shape(model, oldShape); var newNative = Shape(model, newShape);
        Check(OcafNativeMethods.occt_ocaf_naming_modify(NativeHandle, oldNative.Handle, Entry(label), oldNative.Id, newNative.Id), "record modified shape");
    }

    public void NamingDelete(OcafLabel label, OcctModelingSession model, OcctModelShape oldShape)
    {
        var native = Shape(model, oldShape);
        Check(OcafNativeMethods.occt_ocaf_naming_delete(NativeHandle, native.Handle, Entry(label), native.Id), "record deleted shape");
    }

    public void NamingSelect(OcafLabel label, OcctModelingSession model, OcctModelShape selectedShape, OcctModelShape contextShape)
    {
        var selected = Shape(model, selectedShape); var context = Shape(model, contextShape);
        Check(OcafNativeMethods.occt_ocaf_naming_select(NativeHandle, selected.Handle, Entry(label), selected.Id, context.Id), "record selected shape");
    }

    public bool HasNamedShape(OcafLabel label) => OcafNativeMethods.occt_ocaf_named_shape_exists(NativeHandle, Entry(label)) != 0;
    public bool IsNamedShapeEmpty(OcafLabel label) => OcafNativeMethods.occt_ocaf_named_shape_is_empty(NativeHandle, Entry(label)) != 0;
    public OcafNamedShapeEvolution GetNamedShapeEvolution(OcafLabel label) => (OcafNamedShapeEvolution)OcafNativeMethods.occt_ocaf_named_shape_evolution(NativeHandle, Entry(label));
    public int GetNamedShapeVersion(OcafLabel label) => OcafNativeMethods.occt_ocaf_named_shape_version(NativeHandle, Entry(label));
    public void SetNamedShapeVersion(OcafLabel label, int version) => Check(OcafNativeMethods.occt_ocaf_set_named_shape_version(NativeHandle, Entry(label), version), "set named-shape version");
    public OcctModelShape GetNamedShape(OcafLabel label, OcctModelingSession model) => RequiredShape(OcafNativeMethods.occt_ocaf_named_shape_get(NativeHandle, Model(model), Entry(label)), "get named shape");

    public IReadOnlyList<OcafNamedShapePair> GetNamedShapeHistory(OcafLabel label, OcctModelingSession model)
    {
        var count = OcafNativeMethods.occt_ocaf_named_shape_pair_snapshot(NativeHandle, Model(model), Entry(label));
        if (count == 0 && LastError.Length != 0) throw CreateException("read named-shape history");
        return Enumerable.Range(0, count).Select(index => new OcafNamedShapePair(
            new OcctModelShape(OcafNativeMethods.occt_ocaf_named_shape_old_at(NativeHandle, index)),
            new OcctModelShape(OcafNativeMethods.occt_ocaf_named_shape_new_at(NativeHandle, index))))
            .ToArray();
    }

    public bool SelectPersistentShape(OcafLabel label, OcctModelingSession model, OcctModelShape selectedShape, OcctModelShape contextShape = default, bool geometryMode = false)
    {
        var selected = Shape(model, selectedShape);
        long contextId = 0;
        if (contextShape.IsValid) contextId = Shape(model, contextShape).Id;
        return CallBoolean(OcafNativeMethods.occt_ocaf_selector_select(NativeHandle, selected.Handle, Entry(label), selected.Id, contextId, geometryMode ? 1 : 0), "create persistent selection");
    }

    public bool SolvePersistentSelection(OcafLabel label) => CallBoolean(OcafNativeMethods.occt_ocaf_selector_solve(NativeHandle, Entry(label)), "solve persistent selection");

    public bool IsShapeIdentified(OcafLabel accessLabel, OcctModelingSession model, OcctModelShape shape)
    {
        var native = Shape(model, shape);
        return CallBoolean(OcafNativeMethods.occt_ocaf_selector_is_identified(NativeHandle, Entry(accessLabel), native.Handle, native.Id), "identify named shape");
    }
}
