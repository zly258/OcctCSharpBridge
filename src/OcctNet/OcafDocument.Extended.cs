using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcafDocument
{
    public OcafStorageFormatVersion StorageFormatVersion
    {
        get => (OcafStorageFormatVersion)IntegerResult(
            OcafNativeMethods.occt_ocaf_storage_format_version(NativeHandle),
            "read OCAF storage format version");
        set => Check(OcafNativeMethods.occt_ocaf_set_storage_format_version(NativeHandle, (int)value),
            "set OCAF storage format version");
    }

    public void MarkModified(OcafLabel label) =>
        Check(OcafNativeMethods.occt_ocaf_mark_modified(NativeHandle, Entry(label)), "mark OCAF label modified");

    public void PurgeModified() =>
        Check(OcafNativeMethods.occt_ocaf_purge_modified(NativeHandle), "purge modified OCAF labels");

    public IReadOnlyList<OcafLabel> GetModifiedLabels() => ReadLabelSnapshot(
        () => OcafNativeMethods.occt_ocaf_modified_snapshot(NativeHandle),
        index => OcafNativeMethods.occt_ocaf_modified_at(NativeHandle, index),
        "enumerate modified OCAF labels");

    public bool InitializeDeltaCompaction() =>
        CallBoolean(OcafNativeMethods.occt_ocaf_init_delta_compaction(NativeHandle), "initialize OCAF delta compaction");

    public bool PerformDeltaCompaction() =>
        CallBoolean(OcafNativeMethods.occt_ocaf_perform_delta_compaction(NativeHandle), "perform OCAF delta compaction");

    public bool RemoveOldestUndo() =>
        CallBoolean(OcafNativeMethods.occt_ocaf_remove_first_undo(NativeHandle), "remove oldest OCAF undo delta");

    public int GetChildCount(OcafLabel label) => IntegerResult(
        OcafNativeMethods.occt_ocaf_label_child_count(NativeHandle, Entry(label)), "read OCAF child count");

    public int GetAttributeCount(OcafLabel label) => IntegerResult(
        OcafNativeMethods.occt_ocaf_label_attribute_count(NativeHandle, Entry(label)), "read OCAF attribute count");

    public int GetTransaction(OcafLabel label) => IntegerResult(
        OcafNativeMethods.occt_ocaf_label_transaction(NativeHandle, Entry(label)), "read OCAF label transaction");

    public bool MayBeModified(OcafLabel label) => BooleanResult(
        OcafNativeMethods.occt_ocaf_label_may_be_modified(NativeHandle, Entry(label)), "test OCAF subtree modification");

    public bool AttributesModified(OcafLabel label) => BooleanResult(
        OcafNativeMethods.occt_ocaf_label_attributes_modified(NativeHandle, Entry(label)), "test OCAF label modification");

    public bool IsDescendant(OcafLabel label, OcafLabel ancestor) => BooleanResult(
        OcafNativeMethods.occt_ocaf_label_is_descendant(NativeHandle, Entry(label), Entry(ancestor)),
        "test OCAF label ancestry");

    public void SetVariable(OcafLabel label, string name, double value, string unit = "", bool isConstant = false)
    {
        ArgumentNullException.ThrowIfNull(name);
        Check(OcafNativeMethods.occt_ocaf_set_variable(
            NativeHandle, Entry(label), name, value, unit ?? string.Empty, isConstant ? 1 : 0),
            "set TDataStd variable");
    }

    public bool TryGetVariable(OcafLabel label, out OcafVariableInfo? variable)
    {
        var result = OcafNativeMethods.occt_ocaf_get_variable(
            NativeHandle, Entry(label), out var name, out var value, out var unit,
            out var isConstant, out var isValued, out var isAssigned);
        if (!ResultOrThrow(result, "get TDataStd variable"))
        {
            variable = null;
            return false;
        }

        variable = new OcafVariableInfo(
            label,
            ReadUtf8(name),
            isValued != 0 ? value : null,
            ReadUtf8(unit),
            isConstant != 0,
            isAssigned != 0);
        return true;
    }

    public OcafVariableInfo GetVariable(OcafLabel label) =>
        TryGetVariable(label, out var variable)
            ? variable!
            : throw new KeyNotFoundException("Label has no TDataStd_Variable attribute.");

    public void AssignVariableExpression(OcafLabel variable, string expression,
        IEnumerable<OcafLabel>? variables = null) =>
        WithVariableEntries(variables, pointers => OcafNativeMethods.occt_ocaf_assign_variable_expression(
            NativeHandle, Entry(variable), expression, pointers, pointers.Length),
            "assign TDataStd variable expression", expression);

    public void DesassignVariable(OcafLabel variable) =>
        Check(OcafNativeMethods.occt_ocaf_desassign_variable(NativeHandle, Entry(variable)),
            "desassign TDataStd variable expression");

    public void SetExpression(OcafLabel label, string expression, IEnumerable<OcafLabel>? variables = null) =>
        WithVariableEntries(variables, pointers => OcafNativeMethods.occt_ocaf_set_expression(
            NativeHandle, Entry(label), expression, pointers, pointers.Length),
            "set TDataStd expression", expression);

    public bool TryGetExpression(OcafLabel label, out string expression)
    {
        var result = OcafNativeMethods.occt_ocaf_get_expression(NativeHandle, Entry(label), out var pointer);
        if (!ResultOrThrow(result, "get TDataStd expression"))
        {
            expression = string.Empty;
            return false;
        }
        expression = ReadUtf8(pointer);
        return true;
    }

    public string GetExpression(OcafLabel label) =>
        TryGetExpression(label, out var expression)
            ? expression
            : throw new KeyNotFoundException("Label has no TDataStd_Expression attribute.");

    public void SetRelation(OcafLabel label, string relation, IEnumerable<OcafLabel>? variables = null) =>
        WithVariableEntries(variables, pointers => OcafNativeMethods.occt_ocaf_set_relation(
            NativeHandle, Entry(label), relation, pointers, pointers.Length),
            "set TDataStd relation", relation);

    public bool TryGetRelation(OcafLabel label, out string relation)
    {
        var result = OcafNativeMethods.occt_ocaf_get_relation(NativeHandle, Entry(label), out var pointer);
        if (!ResultOrThrow(result, "get TDataStd relation"))
        {
            relation = string.Empty;
            return false;
        }
        relation = ReadUtf8(pointer);
        return true;
    }

    public string GetRelation(OcafLabel label) =>
        TryGetRelation(label, out var relation)
            ? relation
            : throw new KeyNotFoundException("Label has no TDataStd_Relation attribute.");

    public IReadOnlyList<OcafLabel> GetExpressionVariables(OcafLabel label, bool relation = false) =>
        ReadLabelSnapshot(
            () => OcafNativeMethods.occt_ocaf_expression_variable_snapshot(
                NativeHandle, Entry(label), relation ? 1 : 0),
            index => OcafNativeMethods.occt_ocaf_expression_variable_at(NativeHandle, index),
            relation ? "enumerate TDataStd relation variables" : "enumerate TDataStd expression variables");

    public OcafLabel NewShapeLabel() => new(RequiredString(
        OcafNativeMethods.occt_ocaf_xde_new_shape(NativeHandle), "create empty XDE shape label"));

    public bool IsTopLevelShape(OcafLabel label) => BooleanResult(
        OcafNativeMethods.occt_ocaf_xde_is_top_level(NativeHandle, Entry(label)), "test top-level XDE shape");

    public bool IsCompoundShape(OcafLabel label) => BooleanResult(
        OcafNativeMethods.occt_ocaf_xde_is_compound(NativeHandle, Entry(label)), "test compound XDE shape");

    public int GetComponentCount(OcafLabel label, bool recursive = false) => IntegerResult(
        OcafNativeMethods.occt_ocaf_xde_component_count(NativeHandle, Entry(label), recursive ? 1 : 0),
        "read XDE component count");

    public IReadOnlyList<OcafLabel> GetUsers(OcafLabel shape, bool recursive = false) => ReadLabelSnapshot(
        () => OcafNativeMethods.occt_ocaf_xde_user_snapshot(NativeHandle, Entry(shape), recursive ? 1 : 0),
        index => OcafNativeMethods.occt_ocaf_xde_user_at(NativeHandle, index),
        "enumerate XDE shape users");

    public OcafLabel? SearchShape(OcctModelingSession model, OcctModelShape shape,
        bool findInstance = true, bool findComponent = true, bool findSubshape = true)
    {
        var native = Shape(model, shape);
        var entry = RequiredString(OcafNativeMethods.occt_ocaf_xde_search_shape(
            NativeHandle, native.Handle, native.Id,
            findInstance ? 1 : 0, findComponent ? 1 : 0, findSubshape ? 1 : 0),
            "search XDE shape");
        return entry.Length == 0 ? null : new OcafLabel(entry);
    }

    public OcafLabel? FindSubshape(OcafLabel parentShape, OcctModelingSession model, OcctModelShape subshape)
    {
        var native = Shape(model, subshape);
        var entry = RequiredString(OcafNativeMethods.occt_ocaf_xde_find_subshape(
            NativeHandle, native.Handle, Entry(parentShape), native.Id), "find XDE subshape");
        return entry.Length == 0 ? null : new OcafLabel(entry);
    }

    public OcafLabel AddSubshape(OcafLabel parentShape, OcctModelingSession model, OcctModelShape subshape)
    {
        var native = Shape(model, subshape);
        return new OcafLabel(RequiredString(OcafNativeMethods.occt_ocaf_xde_add_subshape(
            NativeHandle, native.Handle, Entry(parentShape), native.Id), "add XDE subshape"));
    }

    public IReadOnlyList<OcafLabel> GetSubshapes(OcafLabel shape) => ReadLabelSnapshot(
        () => OcafNativeMethods.occt_ocaf_xde_subshape_snapshot(NativeHandle, Entry(shape)),
        index => OcafNativeMethods.occt_ocaf_xde_subshape_at(NativeHandle, index),
        "enumerate XDE subshapes");

    public OcafLabel AddColorDefinition(OcafColor color) => new(RequiredString(
        OcafNativeMethods.occt_ocaf_xde_add_color(NativeHandle, color), "add XDE color definition"));

    public OcafLabel? FindColorDefinition(OcafColor color)
    {
        var entry = RequiredString(OcafNativeMethods.occt_ocaf_xde_find_color(NativeHandle, color),
            "find XDE color definition");
        return entry.Length == 0 ? null : new OcafLabel(entry);
    }

    public bool IsColorDefinition(OcafLabel label) => BooleanResult(
        OcafNativeMethods.occt_ocaf_xde_is_color(NativeHandle, Entry(label)), "test XDE color definition");

    public bool HasColor(OcafLabel label, OcafColorType type) => BooleanResult(
        OcafNativeMethods.occt_ocaf_xde_color_is_set(NativeHandle, Entry(label), (int)type),
        "test XDE color assignment");

    public OcafLabel? GetColorDefinitionLabel(OcafLabel label, OcafColorType type)
    {
        var entry = RequiredString(OcafNativeMethods.occt_ocaf_xde_color_label(
            NativeHandle, Entry(label), (int)type), "get assigned XDE color label");
        return entry.Length == 0 ? null : new OcafLabel(entry);
    }

    public void SetColor(OcafLabel label, OcafColorType type, OcafLabel colorDefinition) =>
        Check(OcafNativeMethods.occt_ocaf_xde_set_color_label(
            NativeHandle, Entry(label), Entry(colorDefinition), (int)type), "assign XDE color definition");

    public bool SetInstanceColor(OcctModelingSession model, OcctModelShape instance,
        OcafColorType type, OcafColor color, bool createShuo = true)
    {
        var native = Shape(model, instance);
        return CallBoolean(OcafNativeMethods.occt_ocaf_xde_set_instance_color(
            NativeHandle, native.Handle, native.Id, (int)type, color, createShuo ? 1 : 0),
            "set XDE instance color");
    }

    public bool TryGetInstanceColor(OcctModelingSession model, OcctModelShape instance,
        OcafColorType type, out OcafColor color)
    {
        var native = Shape(model, instance);
        return ResultOrThrow(OcafNativeMethods.occt_ocaf_xde_get_instance_color(
            NativeHandle, native.Handle, native.Id, (int)type, out color), "get XDE instance color");
    }

    public bool IsInstanceVisible(OcctModelingSession model, OcctModelShape instance)
    {
        var native = Shape(model, instance);
        return BooleanResult(OcafNativeMethods.occt_ocaf_xde_is_instance_visible(
            NativeHandle, native.Handle, native.Id), "test XDE instance visibility");
    }

    public OcafLabel? FindLayer(string name, bool matchVisibility = false, bool visible = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var entry = RequiredString(OcafNativeMethods.occt_ocaf_xde_find_layer(
            NativeHandle, name, matchVisibility ? 1 : 0, visible ? 1 : 0), "find XDE layer");
        return entry.Length == 0 ? null : new OcafLabel(entry);
    }

    public bool IsLayerDefinition(OcafLabel label) => BooleanResult(
        OcafNativeMethods.occt_ocaf_xde_is_layer(NativeHandle, Entry(label)), "test XDE layer definition");

    public bool HasLayer(OcafLabel shape, OcafLabel layer) => BooleanResult(
        OcafNativeMethods.occt_ocaf_xde_layer_is_set(NativeHandle, Entry(shape), Entry(layer)),
        "test XDE layer assignment");

    public IReadOnlyList<OcafLabel> GetShapesOnLayer(OcafLabel layer) => ReadLabelSnapshot(
        () => OcafNativeMethods.occt_ocaf_xde_layer_shape_snapshot(NativeHandle, Entry(layer)),
        index => OcafNativeMethods.occt_ocaf_xde_layer_shape_at(NativeHandle, index),
        "enumerate shapes on XDE layer");

    public OcafLabel AddMaterialDefinition(string name, string description, double density,
        string densityName = "density", string densityValueType = "mass/volume")
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        return new OcafLabel(RequiredString(OcafNativeMethods.occt_ocaf_xde_add_material(
            NativeHandle, name, description, density,
            densityName ?? string.Empty, densityValueType ?? string.Empty),
            "add XDE material definition"));
    }

    public bool IsMaterialDefinition(OcafLabel label) => BooleanResult(
        OcafNativeMethods.occt_ocaf_xde_is_material(NativeHandle, Entry(label)),
        "test XDE material definition");

    public void SetMaterial(OcafLabel shape, OcafLabel materialDefinition) =>
        Check(OcafNativeMethods.occt_ocaf_xde_assign_material(
            NativeHandle, Entry(shape), Entry(materialDefinition)), "assign XDE material definition");

    private int IntegerResult(int value, string operation)
    {
        if (LastError.Length != 0) throw CreateException(operation);
        return value;
    }

    private bool BooleanResult(int value, string operation)
    {
        if (value != 0) return true;
        if (LastError.Length != 0) throw CreateException(operation);
        return false;
    }

    private void WithVariableEntries(IEnumerable<OcafLabel>? variables,
        Func<IntPtr[], int> call, string operation, string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        var entries = (variables ?? Array.Empty<OcafLabel>()).Select(Entry).ToArray();
        var pointers = new IntPtr[entries.Length];
        try
        {
            for (var index = 0; index < entries.Length; ++index)
                pointers[index] = Marshal.StringToCoTaskMemUTF8(entries[index]);
            Check(call(pointers), operation);
        }
        finally
        {
            foreach (var pointer in pointers)
                if (pointer != IntPtr.Zero) Marshal.FreeCoTaskMem(pointer);
        }
    }
}
