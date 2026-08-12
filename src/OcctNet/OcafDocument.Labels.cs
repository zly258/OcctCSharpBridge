namespace OcctNet;

public sealed partial class OcafDocument
{
    public OcafLabel Root => new(RequiredString(OcafNativeMethods.occt_ocaf_root_entry(NativeHandle), "read root label"));
    public OcafLabel Main => new(RequiredString(OcafNativeMethods.occt_ocaf_main_entry(NativeHandle), "read main label"));

    public bool LabelExists(string entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        return OcafNativeMethods.occt_ocaf_label_exists(NativeHandle, entry) != 0;
    }

    public OcafLabel GetLabel(string entry, bool create = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        if (create) Check(OcafNativeMethods.occt_ocaf_create_label(NativeHandle, entry), "create OCAF label");
        else if (!LabelExists(entry)) throw new ArgumentException($"OCAF label does not exist: {entry}", nameof(entry));
        return new OcafLabel(entry);
    }

    public OcafLabel NewChild(OcafLabel parent) =>
        new(RequiredString(OcafNativeMethods.occt_ocaf_new_child(NativeHandle, Entry(parent)), "create child label"));

    public OcafLabel FindChild(OcafLabel parent, int tag, bool create = false) =>
        new(RequiredString(OcafNativeMethods.occt_ocaf_find_child(NativeHandle, Entry(parent), tag, create ? 1 : 0), "find child label"));

    public OcafLabel Father(OcafLabel label) =>
        new(RequiredString(OcafNativeMethods.occt_ocaf_father(NativeHandle, Entry(label)), "read father label"));

    public int Tag(OcafLabel label) => OcafNativeMethods.occt_ocaf_label_tag(NativeHandle, Entry(label));
    public int Depth(OcafLabel label) => OcafNativeMethods.occt_ocaf_label_depth(NativeHandle, Entry(label));
    public bool IsRoot(OcafLabel label) => OcafNativeMethods.occt_ocaf_label_is_root(NativeHandle, Entry(label)) != 0;
    public bool IsImported(OcafLabel label) => OcafNativeMethods.occt_ocaf_label_is_imported(NativeHandle, Entry(label)) != 0;
    public void SetImported(OcafLabel label, bool imported) =>
        Check(OcafNativeMethods.occt_ocaf_set_label_imported(NativeHandle, Entry(label), imported ? 1 : 0), "set imported label state");

    public IReadOnlyList<OcafLabel> GetChildren(OcafLabel label, bool recursive = false)
    {
        var count = OcafNativeMethods.occt_ocaf_child_snapshot(NativeHandle, Entry(label), recursive ? 1 : 0);
        if (count == 0 && LastError.Length != 0) throw CreateException("enumerate child labels");
        return Enumerable.Range(0, count)
            .Select(index => new OcafLabel(RequiredString(OcafNativeMethods.occt_ocaf_child_at(NativeHandle, index), "read child label")))
            .ToArray();
    }

    public IReadOnlyList<OcafAttributeInfo> GetAttributes(OcafLabel label, bool includeForgotten = false, int jsonDepth = -1)
    {
        var count = OcafNativeMethods.occt_ocaf_attribute_snapshot(NativeHandle, Entry(label), includeForgotten ? 1 : 0);
        if (count == 0 && LastError.Length != 0) throw CreateException("enumerate label attributes");
        return Enumerable.Range(0, count).Select(index => new OcafAttributeInfo(
            RequiredString(OcafNativeMethods.occt_ocaf_attribute_type_at(NativeHandle, index), "read attribute type"),
            RequiredString(OcafNativeMethods.occt_ocaf_attribute_guid_at(NativeHandle, index), "read attribute GUID"),
            RequiredString(OcafNativeMethods.occt_ocaf_attribute_json_at(NativeHandle, index, jsonDepth), "dump attribute JSON")))
            .ToArray();
    }

    public bool ForgetAttribute(OcafLabel label, Guid attributeId) =>
        CallBoolean(OcafNativeMethods.occt_ocaf_forget_attribute(NativeHandle, Entry(label), attributeId.ToString("D")), "forget OCAF attribute");

    public void ForgetAllAttributes(OcafLabel label, bool clearChildren = false) =>
        Check(OcafNativeMethods.occt_ocaf_forget_all_attributes(NativeHandle, Entry(label), clearChildren ? 1 : 0), "forget OCAF attributes");
}
