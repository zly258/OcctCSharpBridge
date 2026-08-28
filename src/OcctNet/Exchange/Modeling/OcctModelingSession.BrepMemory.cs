namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public byte[] SerializeBrep(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_brep_serialize(
            _handle, shape.Id, null, 0, out var required));
        if (required == 0) return Array.Empty<byte>();

        var buffer = new byte[required];
        CheckStatus(ModelNativeMethods.occt_model_brep_serialize(
            _handle, shape.Id, buffer, buffer.Length, out var copiedRequired));
        if (copiedRequired != required)
            throw new InvalidOperationException("Native BREP size changed during serialization.");
        return buffer;
    }

    public OcctModelShape DeserializeBrep(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) throw new ArgumentException("BREP data must not be empty.", nameof(data));
        var buffer = data.ToArray();
        var status = ModelNativeMethods.occt_model_brep_deserialize(
            _handle, buffer, buffer.Length, out var result);
        return CheckShape(status, result);
    }
}
