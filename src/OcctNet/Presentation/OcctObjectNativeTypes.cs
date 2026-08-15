using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct OcctObjectDescriptorNative
{
    internal long ObjectId;
    internal int Kind;
}
