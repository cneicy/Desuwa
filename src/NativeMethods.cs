using System.Runtime.InteropServices;

namespace Desuwa;

internal static class NativeMethods
{
    public const int WhKeyboardLl = 13;
    public const int WmKeydown = 0x0100;
    public const int WmHotkey = 0x0312;
    public const byte VkReturn = 0x0D;
    public const byte VkEnd = 0x23;
    public const uint KeyeventfKeyup = 0x0002;
    public const uint GcsCompStr = 0x0008;
    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;
    public const uint ModShift = 0x0004;
    public const uint ModNorepeat = 0x4000;

    internal delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct KbdLlHookStruct
    {
        public uint VkCode;
        public uint ScanCode;
        public KbdLlHookFlags Flags;
        public uint Time;
        public nint DwExtraInfo;
    }

    [Flags]
    internal enum KbdLlHookFlags : uint
    {
        Injected = 0x10
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GuiThreadInfo
    {
        public uint CbSize;
        public uint Flags;
        public nint HwndActive;
        public nint HwndFocus;
        public nint HwndCapture;
        public nint HwndMenuOwner;
        public nint HwndMoveSize;
        public nint HwndCaret;
        public Rect RcCaret;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProcDelegate lpfn,
        IntPtr hMod,
        uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    internal static extern void KeybdEvent(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

    [DllImport("imm32.dll")]
    internal static extern nint ImmGetContext(nint hWnd);

    [DllImport("imm32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ImmReleaseContext(nint hWnd, nint hIMC);

    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    internal static extern int ImmGetCompositionStringW(nint hIMC, uint dwIndex, nint lpBuf, uint dwBufLen);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DestroyIcon(IntPtr hIcon);
}