namespace OcctNet;

public sealed partial class OcafDocument
{
    public OcafLabel ShapesSection => Section(OcafNativeMethods.occt_ocaf_xde_shapes_entry, "read XDE shapes section");
    public OcafLabel ColorsSection => Section(OcafNativeMethods.occt_ocaf_xde_colors_entry, "read XDE colors section");
    public OcafLabel LayersSection => Section(OcafNativeMethods.occt_ocaf_xde_layers_entry, "read XDE layers section");
    public OcafLabel MaterialsSection => Section(OcafNativeMethods.occt_ocaf_xde_materials_entry, "read XDE materials section");
    public OcafLabel GeometricTolerancesSection => Section(OcafNativeMethods.occt_ocaf_xde_dgts_entry, "read XDE geometric-tolerances section");
    public OcafLabel ViewsSection => Section(OcafNativeMethods.occt_ocaf_xde_views_entry, "read XDE views section");
    public OcafLabel ClippingPlanesSection => Section(OcafNativeMethods.occt_ocaf_xde_clipping_planes_entry, "read XDE clipping-planes section");
    public OcafLabel NotesSection => Section(OcafNativeMethods.occt_ocaf_xde_notes_entry, "read XDE notes section");
    public OcafLabel VisualMaterialsSection => Section(OcafNativeMethods.occt_ocaf_xde_visual_materials_entry, "read XDE visual-materials section");
    public double? LengthUnitMeters
    {
        get => ResultOrThrow(OcafNativeMethods.occt_ocaf_xde_get_length_unit(NativeHandle, out var value), "get XDE length unit") ? value : null;
        set
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            Check(OcafNativeMethods.occt_ocaf_xde_set_length_unit(NativeHandle, value.Value), "set XDE length unit");
        }
    }

    public OcafLabel AddShape(OcctModelingSession model, OcctModelShape shape, bool makeAssembly = true, bool prepareAssembly = true)
    {
        var native = Shape(model, shape);
        return new OcafLabel(RequiredString(OcafNativeMethods.occt_ocaf_xde_add_shape(NativeHandle, native.Handle, native.Id, makeAssembly ? 1 : 0, prepareAssembly ? 1 : 0), "add XDE shape"));
    }

    public void SetShape(OcafLabel label, OcctModelingSession model, OcctModelShape shape)
    {
        var native = Shape(model, shape);
        Check(OcafNativeMethods.occt_ocaf_xde_set_shape(NativeHandle, native.Handle, Entry(label), native.Id), "set XDE shape");
    }

    public OcctModelShape GetShape(OcafLabel label, OcctModelingSession model) => RequiredShape(OcafNativeMethods.occt_ocaf_xde_get_shape(NativeHandle, Model(model), Entry(label)), "get XDE shape");
    public bool RemoveShape(OcafLabel label, bool removeCompletely = true) => CallBoolean(OcafNativeMethods.occt_ocaf_xde_remove_shape(NativeHandle, Entry(label), removeCompletely ? 1 : 0), "remove XDE shape");

    public OcafLabel? FindShape(OcctModelingSession model, OcctModelShape shape, bool findInstance = false)
    {
        var native = Shape(model, shape);
        var entry = RequiredString(OcafNativeMethods.occt_ocaf_xde_find_shape(NativeHandle, native.Handle, native.Id, findInstance ? 1 : 0), "find XDE shape");
        return entry.Length == 0 ? null : new OcafLabel(entry);
    }

    public IReadOnlyList<OcafLabel> GetShapes(bool freeOnly = false) => ReadLabelSnapshot(
        () => OcafNativeMethods.occt_ocaf_xde_shape_snapshot(NativeHandle, freeOnly ? 1 : 0),
        index => OcafNativeMethods.occt_ocaf_xde_shape_at(NativeHandle, index), "enumerate XDE shapes");

    public IReadOnlyList<OcafLabel> GetComponents(OcafLabel assembly, bool recursive = false) => ReadLabelSnapshot(
        () => OcafNativeMethods.occt_ocaf_xde_component_snapshot(NativeHandle, Entry(assembly), recursive ? 1 : 0),
        index => OcafNativeMethods.occt_ocaf_xde_component_at(NativeHandle, index), "enumerate XDE components");

    public OcafLabel AddComponent(OcafLabel assembly, OcafLabel component, OcctModelLocation? location = null)
    {
        var nativeLocation = location ?? OcctModelLocation.Identity;
        return new OcafLabel(RequiredString(OcafNativeMethods.occt_ocaf_xde_add_component(NativeHandle, Entry(assembly), Entry(component), in nativeLocation), "add XDE component"));
    }

    public void RemoveComponent(OcafLabel component) => Check(OcafNativeMethods.occt_ocaf_xde_remove_component(NativeHandle, Entry(component)), "remove XDE component");
    public OcafLabel GetReferredShape(OcafLabel component) => new(RequiredString(OcafNativeMethods.occt_ocaf_xde_referred_shape(NativeHandle, Entry(component)), "get referred XDE shape"));

    public OcctModelLocation GetLocation(OcafLabel component)
    {
        Check(OcafNativeMethods.occt_ocaf_xde_get_location(NativeHandle, Entry(component), out var location), "get XDE component location");
        return location;
    }

    public OcafLabel SetLocation(OcafLabel shapeOrComponent, OcctModelLocation location) =>
        new(RequiredString(OcafNativeMethods.occt_ocaf_xde_set_location(NativeHandle, Entry(shapeOrComponent), in location), "set XDE location"));

    public void UpdateAssemblies() => Check(OcafNativeMethods.occt_ocaf_xde_update_assemblies(NativeHandle), "update XDE assemblies");
    public bool IsShape(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_shape(NativeHandle, Entry(label)) != 0;
    public bool IsSimpleShape(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_simple_shape(NativeHandle, Entry(label)) != 0;
    public bool IsAssembly(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_assembly(NativeHandle, Entry(label)) != 0;
    public bool IsComponent(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_component(NativeHandle, Entry(label)) != 0;
    public bool IsReference(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_reference(NativeHandle, Entry(label)) != 0;
    public bool IsFreeShape(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_free(NativeHandle, Entry(label)) != 0;
    public bool IsSubshape(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_subshape(NativeHandle, Entry(label)) != 0;

    public IReadOnlyList<(OcafLabel Label, OcafColor Color)> GetColorDefinitions()
    {
        var labels = ReadLabelSnapshot(() => OcafNativeMethods.occt_ocaf_xde_color_snapshot(NativeHandle), i => OcafNativeMethods.occt_ocaf_xde_color_at(NativeHandle, i), "enumerate XDE colors");
        return labels.Select(label => (label, GetColorDefinition(label))).ToArray();
    }

    public OcafColor GetColorDefinition(OcafLabel colorLabel)
    {
        if (!ResultOrThrow(OcafNativeMethods.occt_ocaf_xde_get_color_definition(NativeHandle, Entry(colorLabel), out var color), "get color definition"))
            throw new KeyNotFoundException("Label is not an XDE color definition.");
        return color;
    }

    public void RemoveColorDefinition(OcafLabel colorLabel) => Check(OcafNativeMethods.occt_ocaf_xde_remove_color(NativeHandle, Entry(colorLabel)), "remove color definition");
    public void SetColor(OcafLabel label, OcafColorType type, OcafColor color) => Check(OcafNativeMethods.occt_ocaf_xde_set_color(NativeHandle, Entry(label), (int)type, color), "set XDE color");
    public bool TryGetColor(OcafLabel label, OcafColorType type, out OcafColor color) => ResultOrThrow(OcafNativeMethods.occt_ocaf_xde_get_color(NativeHandle, Entry(label), (int)type, out color), "get XDE color");
    public void UnsetColor(OcafLabel label, OcafColorType type) => Check(OcafNativeMethods.occt_ocaf_xde_unset_color(NativeHandle, Entry(label), (int)type), "unset XDE color");
    public void SetVisible(OcafLabel label, bool visible) => Check(OcafNativeMethods.occt_ocaf_xde_set_visibility(NativeHandle, Entry(label), visible ? 1 : 0), "set XDE visibility");
    public bool IsVisible(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_visible(NativeHandle, Entry(label)) != 0;
    public void SetColorByLayer(OcafLabel label, bool enabled) => Check(OcafNativeMethods.occt_ocaf_xde_set_color_by_layer(NativeHandle, Entry(label), enabled ? 1 : 0), "set color-by-layer");
    public bool IsColorByLayer(OcafLabel label) => OcafNativeMethods.occt_ocaf_xde_is_color_by_layer(NativeHandle, Entry(label)) != 0;

    public OcafLabel AddLayer(string name, bool visible = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new OcafLabel(RequiredString(OcafNativeMethods.occt_ocaf_xde_add_layer(NativeHandle, name, visible ? 1 : 0), "add XDE layer"));
    }
    public void RemoveLayer(OcafLabel layer) => Check(OcafNativeMethods.occt_ocaf_xde_remove_layer(NativeHandle, Entry(layer)), "remove XDE layer");
    public string GetLayerName(OcafLabel layer) => RequiredString(OcafNativeMethods.occt_ocaf_xde_layer_name(NativeHandle, Entry(layer)), "get XDE layer name");
    public IReadOnlyList<OcafLabel> GetLayers() => ReadLabelSnapshot(() => OcafNativeMethods.occt_ocaf_xde_layer_snapshot(NativeHandle), i => OcafNativeMethods.occt_ocaf_xde_layer_at(NativeHandle, i), "enumerate XDE layers");
    public void SetLayer(OcafLabel shape, OcafLabel layer, bool oneLayerOnly = false) => Check(OcafNativeMethods.occt_ocaf_xde_set_layer(NativeHandle, Entry(shape), Entry(layer), oneLayerOnly ? 1 : 0), "assign XDE layer");
    public bool UnsetLayer(OcafLabel shape, OcafLabel layer) => CallBoolean(OcafNativeMethods.occt_ocaf_xde_unset_layer(NativeHandle, Entry(shape), Entry(layer)), "unassign XDE layer");
    public void UnsetLayers(OcafLabel shape) => Check(OcafNativeMethods.occt_ocaf_xde_unset_layers(NativeHandle, Entry(shape)), "unassign XDE layers");
    public IReadOnlyList<OcafLabel> GetLayers(OcafLabel shape) => ReadLabelSnapshot(() => OcafNativeMethods.occt_ocaf_xde_shape_layer_snapshot(NativeHandle, Entry(shape)), i => OcafNativeMethods.occt_ocaf_xde_layer_at(NativeHandle, i), "enumerate assigned XDE layers");
    public void SetLayerVisible(OcafLabel layer, bool visible) => Check(OcafNativeMethods.occt_ocaf_xde_set_layer_visibility(NativeHandle, Entry(layer), visible ? 1 : 0), "set XDE layer visibility");
    public bool IsLayerVisible(OcafLabel layer) => OcafNativeMethods.occt_ocaf_xde_is_layer_visible(NativeHandle, Entry(layer)) != 0;

    public void SetMaterial(OcafLabel shape, string name, string description, double density, string densityName = "density", string densityValueType = "mass/volume")
    {
        ArgumentNullException.ThrowIfNull(name); ArgumentNullException.ThrowIfNull(description);
        Check(OcafNativeMethods.occt_ocaf_xde_set_material(NativeHandle, Entry(shape), name, description, density, densityName ?? string.Empty, densityValueType ?? string.Empty), "set XDE material");
    }

    public OcafLabel? GetMaterialLabel(OcafLabel shape)
    {
        var entry = RequiredString(OcafNativeMethods.occt_ocaf_xde_material_for_shape(NativeHandle, Entry(shape)), "get XDE material label");
        return entry.Length == 0 ? null : new OcafLabel(entry);
    }

    public IReadOnlyList<OcafMaterial> GetMaterials()
    {
        var labels = ReadLabelSnapshot(() => OcafNativeMethods.occt_ocaf_xde_material_snapshot(NativeHandle), i => OcafNativeMethods.occt_ocaf_xde_material_at(NativeHandle, i), "enumerate XDE materials");
        return labels.Select(GetMaterial).ToArray();
    }

    public OcafMaterial GetMaterial(OcafLabel material) => new(
        material,
        RequiredString(OcafNativeMethods.occt_ocaf_xde_material_name(NativeHandle, Entry(material)), "get material name"),
        RequiredString(OcafNativeMethods.occt_ocaf_xde_material_description(NativeHandle, Entry(material)), "get material description"),
        OcafNativeMethods.occt_ocaf_xde_material_density(NativeHandle, Entry(material)),
        RequiredString(OcafNativeMethods.occt_ocaf_xde_material_density_name(NativeHandle, Entry(material)), "get density name"),
        RequiredString(OcafNativeMethods.occt_ocaf_xde_material_density_value_type(NativeHandle, Entry(material)), "get density value type"));

    public double GetDensity(OcafLabel shape) => OcafNativeMethods.occt_ocaf_xde_density_for_shape(NativeHandle, Entry(shape));
    public void SetArea(OcafLabel shape, double area) => Check(OcafNativeMethods.occt_ocaf_xde_set_area(NativeHandle, Entry(shape), area), "set XDE area");
    public bool TryGetArea(OcafLabel shape, out double area) => ResultOrThrow(OcafNativeMethods.occt_ocaf_xde_get_area(NativeHandle, Entry(shape), out area), "get XDE area");
    public void SetVolume(OcafLabel shape, double volume) => Check(OcafNativeMethods.occt_ocaf_xde_set_volume(NativeHandle, Entry(shape), volume), "set XDE volume");
    public bool TryGetVolume(OcafLabel shape, out double volume) => ResultOrThrow(OcafNativeMethods.occt_ocaf_xde_get_volume(NativeHandle, Entry(shape), out volume), "get XDE volume");
    public void SetCentroid(OcafLabel shape, OcctPoint3d centroid) => Check(OcafNativeMethods.occt_ocaf_xde_set_centroid(NativeHandle, Entry(shape), centroid), "set XDE centroid");
    public bool TryGetCentroid(OcafLabel shape, out OcctPoint3d centroid) => ResultOrThrow(OcafNativeMethods.occt_ocaf_xde_get_centroid(NativeHandle, Entry(shape), out centroid), "get XDE centroid");

    public void ImportStep(string filePath) => Exchange(filePath, OcafNativeMethods.occt_ocaf_import_step, false, "import STEPCAF file");
    public void ExportStep(string filePath) => Exchange(filePath, OcafNativeMethods.occt_ocaf_export_step, true, "export STEPCAF file");
    public void ImportIges(string filePath) => Exchange(filePath, OcafNativeMethods.occt_ocaf_import_iges, false, "import IGESCAF file");
    public void ExportIges(string filePath) => Exchange(filePath, OcafNativeMethods.occt_ocaf_export_iges, true, "export IGESCAF file");

    private delegate IntPtr SectionCall(IntPtr handle);
    private delegate int SnapshotCall();
    private delegate IntPtr SnapshotItemCall(int index);
    private delegate int ExchangeCall(IntPtr handle, string path);

    private OcafLabel Section(SectionCall call, string operation) => new(RequiredString(call(NativeHandle), operation));

    private IReadOnlyList<OcafLabel> ReadLabelSnapshot(SnapshotCall snapshot, SnapshotItemCall item, string operation)
    {
        var count = snapshot();
        if (count == 0 && LastError.Length != 0) throw CreateException(operation);
        return Enumerable.Range(0, count).Select(index => new OcafLabel(RequiredString(item(index), operation))).ToArray();
    }

    private void Exchange(string filePath, ExchangeCall call, bool allowMissing, string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var fullPath = Path.GetFullPath(filePath);
        if (!allowMissing && !File.Exists(fullPath)) throw new FileNotFoundException("Exchange file was not found.", fullPath);
        Check(call(NativeHandle, fullPath), operation);
    }
}
