using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcafDocument
{
    public void SetName(OcafLabel label, string value) => SetText(OcafNativeMethods.occt_ocaf_set_name, label, value, "set name");
    public bool TryGetName(OcafLabel label, out string value) => TryGetText(OcafNativeMethods.occt_ocaf_get_name, label, out value, "get name");
    public string GetName(OcafLabel label) => GetRequiredText(TryGetName, label, "TDataStd_Name");

    public void SetComment(OcafLabel label, string value) => SetText(OcafNativeMethods.occt_ocaf_set_comment, label, value, "set comment");
    public bool TryGetComment(OcafLabel label, out string value) => TryGetText(OcafNativeMethods.occt_ocaf_get_comment, label, out value, "get comment");
    public string GetComment(OcafLabel label) => GetRequiredText(TryGetComment, label, "TDataStd_Comment");

    public void SetAsciiString(OcafLabel label, string value) => SetText(OcafNativeMethods.occt_ocaf_set_ascii_string, label, value, "set ASCII string");
    public bool TryGetAsciiString(OcafLabel label, out string value) => TryGetText(OcafNativeMethods.occt_ocaf_get_ascii_string, label, out value, "get ASCII string");
    public string GetAsciiString(OcafLabel label) => GetRequiredText(TryGetAsciiString, label, "TDataStd_AsciiString");

    public void SetInteger(OcafLabel label, int value) => Check(OcafNativeMethods.occt_ocaf_set_integer(NativeHandle, Entry(label), value), "set integer");
    public bool TryGetInteger(OcafLabel label, out int value)
    {
        var result = OcafNativeMethods.occt_ocaf_get_integer(NativeHandle, Entry(label), out value);
        return ResultOrThrow(result, "get integer");
    }
    public int GetInteger(OcafLabel label) => TryGetInteger(label, out var value) ? value : throw new KeyNotFoundException("Label has no TDataStd_Integer attribute.");

    public void SetReal(OcafLabel label, double value) => Check(OcafNativeMethods.occt_ocaf_set_real(NativeHandle, Entry(label), value), "set real");
    public bool TryGetReal(OcafLabel label, out double value)
    {
        var result = OcafNativeMethods.occt_ocaf_get_real(NativeHandle, Entry(label), out value);
        return ResultOrThrow(result, "get real");
    }
    public double GetReal(OcafLabel label) => TryGetReal(label, out var value) ? value : throw new KeyNotFoundException("Label has no TDataStd_Real attribute.");

    public void SetUAttribute(OcafLabel label, Guid id) => Check(OcafNativeMethods.occt_ocaf_set_uattribute(NativeHandle, Entry(label), id.ToString("D")), "set UAttribute");
    public bool HasUAttribute(OcafLabel label, Guid id) => OcafNativeMethods.occt_ocaf_has_uattribute(NativeHandle, Entry(label), id.ToString("D")) != 0;

    public void SetReference(OcafLabel label, OcafLabel target) =>
        Check(OcafNativeMethods.occt_ocaf_set_reference(NativeHandle, Entry(label), Entry(target)), "set label reference");
    public bool TryGetReference(OcafLabel label, out OcafLabel target)
    {
        var result = OcafNativeMethods.occt_ocaf_get_reference(NativeHandle, Entry(label), out var pointer);
        if (!ResultOrThrow(result, "get label reference")) { target = default; return false; }
        target = new OcafLabel(ReadUtf8(pointer));
        return target.IsValid;
    }
    public OcafLabel GetReference(OcafLabel label) => TryGetReference(label, out var target) ? target : throw new KeyNotFoundException("Label has no TDF_Reference attribute.");

    public void SetIntegerArray(OcafLabel label, IEnumerable<int> values, int lower = 1)
    {
        var array = RequiredArray(values, nameof(values));
        Check(OcafNativeMethods.occt_ocaf_set_integer_array(NativeHandle, Entry(label), array, array.Length, lower), "set integer array");
    }
    public OcafArray<int> GetIntegerArray(OcafLabel label) => ReadIntArray(OcafNativeMethods.occt_ocaf_get_integer_array, label, "integer array");

    public void SetRealArray(OcafLabel label, IEnumerable<double> values, int lower = 1)
    {
        var array = RequiredArray(values, nameof(values));
        Check(OcafNativeMethods.occt_ocaf_set_real_array(NativeHandle, Entry(label), array, array.Length, lower), "set real array");
    }
    public OcafArray<double> GetRealArray(OcafLabel label)
    {
        var count = OcafNativeMethods.occt_ocaf_get_real_array(NativeHandle, Entry(label));
        EnsureSnapshot(count, "get real array");
        return new OcafArray<double>(OcafNativeMethods.occt_ocaf_array_lower(NativeHandle), Enumerable.Range(0, count).Select(i => OcafNativeMethods.occt_ocaf_array_real_at(NativeHandle, i)).ToArray());
    }

    public void SetBooleanArray(OcafLabel label, IEnumerable<bool> values, int lower = 1)
    {
        var array = RequiredArray(values, nameof(values)).Select(value => value ? 1 : 0).ToArray();
        Check(OcafNativeMethods.occt_ocaf_set_boolean_array(NativeHandle, Entry(label), array, array.Length, lower), "set boolean array");
    }
    public OcafArray<bool> GetBooleanArray(OcafLabel label)
    {
        var raw = ReadIntArray(OcafNativeMethods.occt_ocaf_get_boolean_array, label, "boolean array");
        return new OcafArray<bool>(raw.Lower, raw.Values.Select(value => value != 0).ToArray());
    }

    public void SetByteArray(OcafLabel label, IEnumerable<byte> values, int lower = 1)
    {
        var array = RequiredArray(values, nameof(values));
        Check(OcafNativeMethods.occt_ocaf_set_byte_array(NativeHandle, Entry(label), array, array.Length, lower), "set byte array");
    }
    public OcafArray<byte> GetByteArray(OcafLabel label)
    {
        var raw = ReadIntArray(OcafNativeMethods.occt_ocaf_get_byte_array, label, "byte array");
        return new OcafArray<byte>(raw.Lower, raw.Values.Select(value => checked((byte)value)).ToArray());
    }

    public void SetStringArray(OcafLabel label, IEnumerable<string> values, int lower = 1)
    {
        var array = RequiredArray(values, nameof(values));
        var pointers = new IntPtr[array.Length];
        try
        {
            for (var i = 0; i < array.Length; i++) pointers[i] = Marshal.StringToCoTaskMemUTF8(array[i] ?? string.Empty);
            Check(OcafNativeMethods.occt_ocaf_set_string_array(NativeHandle, Entry(label), pointers, pointers.Length, lower), "set string array");
        }
        finally
        {
            foreach (var pointer in pointers) if (pointer != IntPtr.Zero) Marshal.FreeCoTaskMem(pointer);
        }
    }
    public OcafArray<string> GetStringArray(OcafLabel label)
    {
        var count = OcafNativeMethods.occt_ocaf_get_string_array(NativeHandle, Entry(label));
        EnsureSnapshot(count, "get string array");
        return new OcafArray<string>(OcafNativeMethods.occt_ocaf_array_lower(NativeHandle), Enumerable.Range(0, count).Select(i => ReadUtf8(OcafNativeMethods.occt_ocaf_array_string_at(NativeHandle, i))).ToArray());
    }

    public void SetPosition(OcafLabel label, OcctPoint3d point) => Check(OcafNativeMethods.occt_ocaf_set_position(NativeHandle, Entry(label), point), "set position");
    public bool TryGetPosition(OcafLabel label, out OcctPoint3d point)
    {
        var result = OcafNativeMethods.occt_ocaf_get_position(NativeHandle, Entry(label), out point);
        return ResultOrThrow(result, "get position");
    }

    public void SetShapeAttribute(OcafLabel label, OcctModelingSession model, OcctModelShape shape)
    {
        var native = Shape(model, shape);
        Check(OcafNativeMethods.occt_ocaf_set_shape_attribute(NativeHandle, native.Handle, Entry(label), native.Id), "set TDataXtd shape");
    }
    public OcctModelShape GetShapeAttribute(OcafLabel label, OcctModelingSession model) =>
        RequiredShape(OcafNativeMethods.occt_ocaf_get_shape_attribute(NativeHandle, Model(model), Entry(label)), "get TDataXtd shape");

    private delegate int NativeSetText(IntPtr handle, string entry, string value);
    private delegate int NativeGetText(IntPtr handle, string entry, out IntPtr value);
    private delegate bool TryGetTextDelegate(OcafLabel label, out string value);
    private delegate int NativeGetArray(IntPtr handle, string entry);

    private void SetText(NativeSetText call, OcafLabel label, string value, string operation)
    {
        ArgumentNullException.ThrowIfNull(value);
        Check(call(NativeHandle, Entry(label), value), operation);
    }

    private bool TryGetText(NativeGetText call, OcafLabel label, out string value, string operation)
    {
        var result = call(NativeHandle, Entry(label), out var pointer);
        if (!ResultOrThrow(result, operation)) { value = string.Empty; return false; }
        value = ReadUtf8(pointer);
        return true;
    }

    private string GetRequiredText(TryGetTextDelegate call, OcafLabel label, string attributeName) =>
        call(label, out var value) ? value : throw new KeyNotFoundException($"Label has no {attributeName} attribute.");

    private OcafArray<int> ReadIntArray(NativeGetArray call, OcafLabel label, string name)
    {
        var count = call(NativeHandle, Entry(label));
        EnsureSnapshot(count, $"get {name}");
        return new OcafArray<int>(OcafNativeMethods.occt_ocaf_array_lower(NativeHandle), Enumerable.Range(0, count).Select(i => OcafNativeMethods.occt_ocaf_array_int_at(NativeHandle, i)).ToArray());
    }

    private void EnsureSnapshot(int count, string operation)
    {
        if (count == 0 && LastError.Length != 0) throw CreateException(operation);
    }

    private bool ResultOrThrow(int result, string operation)
    {
        if (result != 0) return true;
        if (LastError.Length != 0) throw CreateException(operation);
        return false;
    }

    private static T[] RequiredArray<T>(IEnumerable<T> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var result = values.ToArray();
        if (result.Length == 0) throw new ArgumentException("Collection must not be empty.", parameterName);
        return result;
    }
}
