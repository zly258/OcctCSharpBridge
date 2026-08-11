using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using DrawingColor = System.Drawing.Color;

namespace OcctDemo.Wpf;

/// <summary>
/// Thin WPF owner adapter for the native Windows color chooser. WPF does not
/// provide its own ColorDialog, so use the system common dialog directly
/// instead of adding a Windows Forms dependency or maintaining a custom RGB UI.
/// </summary>
internal static class WpfColorDialog
{
    private const uint CcRgbInit = 0x00000001;
    private const uint CcFullOpen = 0x00000002;
    private const uint CcEnableHook = 0x00000010;
    private const uint CcAnyColor = 0x00000100;
    private const uint WmInitDialog = 0x0110;

    private static readonly uint[] CustomColors = new uint[16];

    public static bool TryPick(Window owner, string title, DrawingColor initial, out DrawingColor color)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var ownerHandle = new WindowInteropHelper(owner).Handle;
        if (ownerHandle == IntPtr.Zero)
        {
            ownerHandle = new WindowInteropHelper(owner).EnsureHandle();
        }

        ColorHookProc hook = (dialogHandle, message, _, _) =>
        {
            if (message == WmInitDialog && !string.IsNullOrWhiteSpace(title))
            {
                SetWindowTextW(dialogHandle, title);
            }

            return IntPtr.Zero;
        };

        var customColorsHandle = GCHandle.Alloc(CustomColors, GCHandleType.Pinned);
        try
        {
            var options = new ChooseColorOptions
            {
                StructureSize = (uint)Marshal.SizeOf<ChooseColorOptions>(),
                Owner = ownerHandle,
                ResultColor = ToColorRef(initial),
                CustomColors = customColorsHandle.AddrOfPinnedObject(),
                Flags = CcRgbInit | CcFullOpen | CcAnyColor | CcEnableHook,
                Hook = Marshal.GetFunctionPointerForDelegate(hook)
            };

            if (!ChooseColorW(ref options))
            {
                color = initial;
                GC.KeepAlive(hook);
                return false;
            }

            color = FromColorRef(options.ResultColor);
            GC.KeepAlive(hook);
            return true;
        }
        finally
        {
            customColorsHandle.Free();
        }
    }

    private static uint ToColorRef(DrawingColor color) =>
        color.R | ((uint)color.G << 8) | ((uint)color.B << 16);

    private static DrawingColor FromColorRef(uint colorRef) => DrawingColor.FromArgb(
        255,
        (byte)(colorRef & 0xFF),
        (byte)((colorRef >> 8) & 0xFF),
        (byte)((colorRef >> 16) & 0xFF));

    [StructLayout(LayoutKind.Sequential)]
    private struct ChooseColorOptions
    {
        public uint StructureSize;
        public IntPtr Owner;
        public IntPtr Instance;
        public uint ResultColor;
        public IntPtr CustomColors;
        public uint Flags;
        public IntPtr CustomData;
        public IntPtr Hook;
        public IntPtr TemplateName;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr ColorHookProc(IntPtr dialogHandle, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("comdlg32.dll", EntryPoint = "ChooseColorW", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ChooseColorW(ref ChooseColorOptions options);

    [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowTextW(IntPtr windowHandle, string text);
}
