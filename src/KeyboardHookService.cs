using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Desuwa;

internal sealed class KeyboardHookService : IDisposable
{
    private const int RepeatSuppressWindowMs = 250;

    private readonly Func<bool> _isEnabledProvider;
    private readonly Func<string> _suffixProvider;
    private readonly Func<ISuffixInjector> _injectorProvider;
    private readonly Action<Exception>? _errorHandler;
    private readonly NativeMethods.LowLevelKeyboardProcDelegate _hookDelegate;
    private IntPtr _hookId;
    private bool _processing;
    private long _suppressPhysicalEnterUntilTick;

    public KeyboardHookService(
        Func<bool> isEnabledProvider,
        Func<string> suffixProvider,
        Func<ISuffixInjector> injectorProvider,
        Action<Exception>? errorHandler = null)
    {
        _isEnabledProvider = isEnabledProvider;
        _suffixProvider = suffixProvider;
        _injectorProvider = injectorProvider;
        _errorHandler = errorHandler;
        _hookDelegate = HookCallback;

        _hookId = InstallHook();
        if (_hookId == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "安装全局键盘钩子失败。");
        }
    }

    public void Dispose()
    {
        if (_hookId == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.UnhookWindowsHookEx(_hookId);
        _hookId = IntPtr.Zero;
    }

    private IntPtr InstallHook()
    {
        using var currentProcess = Process.GetCurrentProcess();
        using var currentModule = currentProcess.MainModule;

        var moduleName = currentModule?.ModuleName;
        var moduleHandle = string.IsNullOrEmpty(moduleName)
            ? IntPtr.Zero
            : NativeMethods.GetModuleHandle(moduleName);

        return NativeMethods.SetWindowsHookEx(
            NativeMethods.WhKeyboardLl,
            _hookDelegate,
            moduleHandle,
            0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0 || wParam != (IntPtr)NativeMethods.WmKeydown || !_isEnabledProvider())
        {
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var hookData = Marshal.PtrToStructure<NativeMethods.KbdLlHookStruct>(lParam);
        if (hookData.VkCode != NativeMethods.VkReturn)
        {
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var isInjected = (hookData.Flags & NativeMethods.KbdLlHookFlags.Injected) != 0;

        if (_processing)
        {
            return isInjected
                ? NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam)
                : (IntPtr)1;
        }

        if (!isInjected && IsImeComposing())
        {
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        if (!isInjected && Environment.TickCount64 < _suppressPhysicalEnterUntilTick)
        {
            return 1;
        }

        _processing = true;

        try
        {
            var suffix = _suffixProvider() ?? string.Empty;
            var injector = _injectorProvider();
            SendVirtualKey(NativeMethods.VkEnd);
            Thread.Sleep(20);

            injector.Inject(suffix);

            Thread.Sleep(20);
            _suppressPhysicalEnterUntilTick = Environment.TickCount64 + RepeatSuppressWindowMs;
            SendVirtualKey(NativeMethods.VkReturn);
            return (IntPtr)1;
        }
        catch (Exception ex)
        {
            _processing = false;
            _errorHandler?.Invoke(ex);
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
        finally
        {
            _processing = false;
        }
    }

    private static bool IsImeComposing()
    {
        var guiInfo = new NativeMethods.GuiThreadInfo
        {
            CbSize = (uint)Marshal.SizeOf<NativeMethods.GuiThreadInfo>()
        };

        if (!NativeMethods.GetGUIThreadInfo(0, ref guiInfo) || guiInfo.HwndFocus == IntPtr.Zero)
        {
            return false;
        }

        var inputContext = NativeMethods.ImmGetContext(guiInfo.HwndFocus);
        if (inputContext == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            return NativeMethods.ImmGetCompositionStringW(
                inputContext,
                NativeMethods.GcsCompStr,
                IntPtr.Zero,
                0) > 0;
        }
        finally
        {
            NativeMethods.ImmReleaseContext(guiInfo.HwndFocus, inputContext);
        }
    }

    private static void SendVirtualKey(byte virtualKey)
    {
        NativeMethods.KeybdEvent(virtualKey, 0, 0, 0);
        NativeMethods.KeybdEvent(virtualKey, 0, NativeMethods.KeyeventfKeyup, 0);
    }
}