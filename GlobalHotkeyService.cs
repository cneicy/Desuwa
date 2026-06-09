using System.Runtime.InteropServices;

namespace Desuwa;

internal sealed class GlobalHotkeyService : IDisposable
{
    private readonly Action<HotkeyAction> _hotkeyHandler;
    private readonly Dictionary<int, HotkeyAction> _registeredHotkeys = new();
    private readonly MessageWindow _window;

    private int _nextHotkeyId = 1;
    private bool _disposed;

    public GlobalHotkeyService(MessageWindow window, Action<HotkeyAction> hotkeyHandler)
    {
        _window = window;
        _hotkeyHandler = hotkeyHandler;
        _window.HotkeyPressed += HandleHotkeyMessage;
    }

    public IReadOnlyList<string> Reload(AppSettings settings)
    {
        UnregisterAll();
        _nextHotkeyId = 1;

        var failures = new List<string>();
        RegisterHotkey(settings.ToggleEnabledHotkey, HotkeyAction.ToggleEnabled, "切换启用状态", failures);
        RegisterHotkey(settings.ToggleModeHotkey, HotkeyAction.ToggleMode, "切换输入方式", failures);
        return failures;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        UnregisterAll();
        _window.HotkeyPressed -= HandleHotkeyMessage;
    }

    private void RegisterHotkey(
        HotkeyGesture gesture,
        HotkeyAction action,
        string actionDisplayName,
        ICollection<string> failures)
    {
        if (!gesture.Enabled || gesture.Key == Keys.None)
        {
            return;
        }

        if (!gesture.IsValid())
        {
            failures.Add($"{actionDisplayName}：快捷键无效");
            return;
        }

        var hotkeyId = _nextHotkeyId++;
        var registered = NativeMethods.RegisterHotKey(
            _window.Handle,
            hotkeyId,
            gesture.ToNativeModifiers(),
            (uint)gesture.Key);

        if (!registered)
        {
            var errorCode = Marshal.GetLastWin32Error();
            failures.Add($"{actionDisplayName}：{gesture.ToDisplayString()} (Win32 {errorCode})");
            return;
        }

        _registeredHotkeys[hotkeyId] = action;
    }

    private void UnregisterAll()
    {
        foreach (var hotkeyId in _registeredHotkeys.Keys)
        {
            NativeMethods.UnregisterHotKey(_window.Handle, hotkeyId);
        }

        _registeredHotkeys.Clear();
    }

    private void HandleHotkeyMessage(int hotkeyId)
    {
        if (_registeredHotkeys.TryGetValue(hotkeyId, out var action))
        {
            _hotkeyHandler(action);
        }
    }
}