namespace OcctNet;

public sealed partial class OcctEngine
{
    public int ObjectCount
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.occt_object_count(_handle);
        }
    }

    public int ShapeCount
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.occt_shape_count(_handle);
        }
    }

    public IReadOnlyList<IOcctObject> Objects
    {
        get
        {
            EnsureNotDisposed();
            return Enumerable.Range(0, ObjectCount)
                .Select(index => NativeMethods.occt_object_id_at(_handle, index))
                .Where(id => id > 0)
                .Select(id => CreateBoundObject(id, GetObjectKind(id)))
                .ToArray();
        }
    }

    public IReadOnlyList<OcctShape> Shapes
    {
        get
        {
            EnsureNotDisposed();
            return Enumerable.Range(0, ShapeCount)
                .Select(index => NativeMethods.occt_shape_id_at(_handle, index))
                .Where(id => id > 0)
                .Select(id => new OcctShape(id, _ownerId))
                .ToArray();
        }
    }

    public bool Exists(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureNotDisposed();
        return value is OcctObject legacy
            ? MatchesLegacyObject(legacy)
            : value.Id > 0 &&
              GetOwnerId(value) == _ownerId &&
              NativeMethods.occt_object_exists(_handle, value.Id) != 0;
    }

    public bool Owns(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureNotDisposed();
        return value is OcctObject legacy
            ? MatchesLegacyObject(legacy)
            : GetOwnerId(value) == _ownerId;
    }

    public IOcctObject GetObject(long id)
    {
        EnsureNotDisposed();
        if (id <= 0 || NativeMethods.occt_object_exists(_handle, id) == 0)
            throw new ArgumentOutOfRangeException(nameof(id), id, "The object ID does not exist in this OCCT engine.");
        return CreateBoundObject(id, GetObjectKind(id));
    }

    public bool TryGetObject(long id, out IOcctObject? value)
    {
        EnsureNotDisposed();
        if (id > 0 && NativeMethods.occt_object_exists(_handle, id) != 0)
        {
            value = CreateBoundObject(id, GetObjectKind(id));
            return true;
        }

        value = null;
        return false;
    }

    public OcctShape GetShape(long id)
    {
        EnsureNotDisposed();
        if (id <= 0 ||
            NativeMethods.occt_object_exists(_handle, id) == 0 ||
            NativeMethods.occt_object_kind(_handle, id) != (int)OcctObjectKind.Shape)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "The shape ID does not exist in this OCCT engine.");
        }

        return new OcctShape(id, _ownerId);
    }

    public bool TryGetShape(long id, out OcctShape shape)
    {
        EnsureNotDisposed();
        if (id > 0 &&
            NativeMethods.occt_object_exists(_handle, id) != 0 &&
            NativeMethods.occt_object_kind(_handle, id) == (int)OcctObjectKind.Shape)
        {
            shape = new OcctShape(id, _ownerId);
            return true;
        }

        shape = default;
        return false;
    }

    public OcctObjectKind GetObjectKind(long id)
    {
        EnsureNotDisposed();
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        return (OcctObjectKind)NativeMethods.occt_object_kind(_handle, id);
    }

    public void Delete(IOcctObject value)
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
            EnsureObject(value);
            ids.Add(value.Id);
        }

        if (ids.Count == 0) return;
        var objectIds = ids.ToArray();
        Check(NativeMethods.occt_delete_objects(_handle, objectIds, objectIds.Length));
    }

    public void Clear() => CheckInitialized(() => NativeMethods.occt_clear(_handle));

    private IOcctObject CreateBoundObject(long id, OcctObjectKind kind) => kind switch
    {
        OcctObjectKind.Shape => new OcctShape(id, _ownerId),
        OcctObjectKind.Text => new OcctText(id, _ownerId),
        OcctObjectKind.Dimension => new OcctDimension(id, _ownerId),
        _ => throw new InvalidOperationException($"Unsupported OCCT object kind: {kind}.")
    };
}
