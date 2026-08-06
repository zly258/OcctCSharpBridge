using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>
/// Managed OCCT 7.9.0 OCAF/XDE document session.
/// Labels are represented by stable TDF entry strings; no native label or attribute pointer escapes the C ABI.
/// </summary>
public sealed partial class OcafDocument : IDisposable
{
    public const string RequiredOcctVersion = "7.9.0";

    private IntPtr _handle;

    public OcafDocument(string storageFormat = OcafDocumentFormats.BinaryXde)
        : this()
    {
        New(storageFormat);
    }

    private OcafDocument()
    {
        OcctRuntime.Configure();
        RuntimeHelpers.RunClassConstructor(typeof(NativeMethods).TypeHandle);
        OcctBridgeInfo.EnsureCompatible();
        _handle = OcafNativeMethods.occt_ocaf_create();
        if (_handle == IntPtr.Zero)
            throw new OcctException("Unable to create the native OCAF session.");
        if (!string.Equals(NativeVersion, RequiredOcctVersion, StringComparison.Ordinal))
        {
            Dispose();
            throw new NotSupportedException($"OcafDocument requires OCCT {RequiredOcctVersion}; loaded {NativeVersion}.");
        }
    }

    internal IntPtr NativeHandle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
    }

    public static string NativeVersion => ReadUtf8(OcafNativeMethods.occt_ocaf_version());
    public static string Capabilities => ReadUtf8(OcafNativeMethods.occt_ocaf_capabilities());

    public bool IsOpen => OcafNativeMethods.occt_ocaf_is_open(NativeHandle) != 0;
    public string FilePath => ReadUtf8(OcafNativeMethods.occt_ocaf_document_path(NativeHandle));
    public string StorageFormat => RequiredString(OcafNativeMethods.occt_ocaf_storage_format(NativeHandle), "read document format");
    public bool IsSaved => OcafNativeMethods.occt_ocaf_is_saved(NativeHandle) != 0;
    public bool IsChanged => OcafNativeMethods.occt_ocaf_is_changed(NativeHandle) != 0;
    public bool IsEmpty => OcafNativeMethods.occt_ocaf_is_empty(NativeHandle) != 0;
    public bool IsValid => OcafNativeMethods.occt_ocaf_is_valid(NativeHandle) != 0;
    public bool HasOpenCommand => OcafNativeMethods.occt_ocaf_has_open_command(NativeHandle) != 0;
    public int UndoLimit
    {
        get => OcafNativeMethods.occt_ocaf_get_undo_limit(NativeHandle);
        set => Check(OcafNativeMethods.occt_ocaf_set_undo_limit(NativeHandle, value), "set undo limit");
    }
    public int AvailableUndos => OcafNativeMethods.occt_ocaf_available_undos(NativeHandle);
    public int AvailableRedos => OcafNativeMethods.occt_ocaf_available_redos(NativeHandle);
    public bool NestedTransactionMode
    {
        get => OcafNativeMethods.occt_ocaf_nested_transaction_mode(NativeHandle) != 0;
        set => Check(OcafNativeMethods.occt_ocaf_set_nested_transaction_mode(NativeHandle, value ? 1 : 0), "set nested transaction mode");
    }
    public bool ModificationMode
    {
        get => OcafNativeMethods.occt_ocaf_modification_mode(NativeHandle) != 0;
        set => Check(OcafNativeMethods.occt_ocaf_set_modification_mode(NativeHandle, value ? 1 : 0), "set modification mode");
    }
    public bool EmptyLabelsSavingMode
    {
        get => OcafNativeMethods.occt_ocaf_empty_labels_saving_mode(NativeHandle) != 0;
        set => Check(OcafNativeMethods.occt_ocaf_set_empty_labels_saving_mode(NativeHandle, value ? 1 : 0), "set empty-label saving mode");
    }

    public static OcafDocument Open(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var document = new OcafDocument();
        try
        {
            document.Check(OcafNativeMethods.occt_ocaf_open_document(document._handle, Path.GetFullPath(filePath)), "open OCAF document");
            return document;
        }
        catch
        {
            document.Dispose();
            throw;
        }
    }

    public void New(string storageFormat = OcafDocumentFormats.BinaryXde)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageFormat);
        Check(OcafNativeMethods.occt_ocaf_new_document(NativeHandle, storageFormat), "create OCAF document");
    }

    public void Save() => Check(OcafNativeMethods.occt_ocaf_save_document(NativeHandle), "save OCAF document");

    public void SaveAs(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        Check(OcafNativeMethods.occt_ocaf_save_as(NativeHandle, Path.GetFullPath(filePath)), "save OCAF document as");
    }

    public void Close()
    {
        if (!IsOpen) return;
        Check(OcafNativeMethods.occt_ocaf_close_document(NativeHandle), "close OCAF document");
    }

    public void ChangeStorageFormat(string storageFormat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageFormat);
        Check(OcafNativeMethods.occt_ocaf_change_storage_format(NativeHandle, storageFormat), "change storage format");
    }

    public string DumpJson(int depth = -1) => RequiredString(OcafNativeMethods.occt_ocaf_document_json(NativeHandle, depth), "dump document JSON");

    public void NewCommand() => Check(OcafNativeMethods.occt_ocaf_new_command(NativeHandle), "start OCAF command");
    public void OpenCommand() => Check(OcafNativeMethods.occt_ocaf_open_command(NativeHandle), "open OCAF command");

    /// <summary>Commits the command. False means the command produced no undo delta; it is not an error.</summary>
    public bool CommitCommand() => CallBoolean(OcafNativeMethods.occt_ocaf_commit_command(NativeHandle), "commit OCAF command");

    public void AbortCommand() => Check(OcafNativeMethods.occt_ocaf_abort_command(NativeHandle), "abort OCAF command");
    public bool Undo() => CallBoolean(OcafNativeMethods.occt_ocaf_undo(NativeHandle), "undo OCAF command");
    public bool Redo() => CallBoolean(OcafNativeMethods.occt_ocaf_redo(NativeHandle), "redo OCAF command");
    public void ClearUndos() => Check(OcafNativeMethods.occt_ocaf_clear_undos(NativeHandle), "clear OCAF undos");
    public void ClearRedos() => Check(OcafNativeMethods.occt_ocaf_clear_redos(NativeHandle), "clear OCAF redos");
    public OcafCommandScope BeginCommand() => new(this);

    private static string ReadUtf8(IntPtr pointer) => pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;

    private string RequiredString(IntPtr pointer, string operation)
    {
        var value = ReadUtf8(pointer);
        if (pointer == IntPtr.Zero || (value.Length == 0 && LastError.Length != 0)) throw CreateException(operation);
        return value;
    }

    private string LastError => _handle == IntPtr.Zero ? string.Empty : ReadUtf8(OcafNativeMethods.occt_ocaf_last_error(_handle));

    private bool CallBoolean(int value, string operation)
    {
        if (value != 0) return true;
        if (LastError.Length != 0) throw CreateException(operation);
        return false;
    }

    private void Check(int value, string operation)
    {
        if (value == 0) throw CreateException(operation);
    }

    private OcctException CreateException(string operation)
    {
        var message = LastError;
        return new OcctException(message.Length == 0 ? $"Failed to {operation}." : message);
    }

    private static string Entry(OcafLabel label)
    {
        if (!label.IsValid) throw new ArgumentException("OCAF label entry must not be empty.", nameof(label));
        return label.Entry;
    }

    private static (IntPtr Handle, long Id) Shape(OcctModelingSession model, OcctModelShape shape)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (!shape.IsValid || !model.Exists(shape))
            throw new ArgumentException("Shape does not belong to the supplied modeling session.", nameof(shape));
        return (model.NativeHandle, shape.Id);
    }

    private static IntPtr Model(OcctModelingSession model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return model.NativeHandle;
    }

    private OcctModelShape RequiredShape(long id, string operation)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctModelShape(id);
    }

    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

    public void Dispose()
    {
        if (_handle == IntPtr.Zero) return;
        OcafNativeMethods.occt_ocaf_destroy(_handle);
        _handle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    ~OcafDocument() => Dispose();
}
