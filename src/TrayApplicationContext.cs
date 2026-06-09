namespace Desuwa;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly AppIcons _icons;
    private readonly SettingsStore _settingsStore;
    private readonly ClipboardSuffixInjector _clipboardInjector;
    private readonly SimulatedTypingSuffixInjector _simulatedTypingInjector;
    private readonly MessageWindow _messageWindow;
    private readonly KeyboardHookService _keyboardHookService;
    private readonly GlobalHotkeyService _hotkeyService;
    private readonly ContextMenuStrip _trayMenu;
    private readonly NotifyIcon _trayIcon;
    private readonly ToolStripMenuItem _toggleEnabledMenuItem;
    private readonly ToolStripMenuItem _settingsMenuItem;
    private readonly ToolStripMenuItem _clipboardModeMenuItem;
    private readonly ToolStripMenuItem _simulatedModeMenuItem;

    private AppSettings _settings;
    private bool _isEnabled = true;

    public TrayApplicationContext()
    {
        _settingsStore = new SettingsStore();
        _settings = _settingsStore.Load();
        _icons = new AppIcons();
        _clipboardInjector = new ClipboardSuffixInjector();
        _simulatedTypingInjector = new SimulatedTypingSuffixInjector();
        _messageWindow = new MessageWindow();
        _ = _messageWindow.Handle;
        _keyboardHookService = new KeyboardHookService(
            () => _isEnabled,
            () => _settings.Suffix,
            GetCurrentInjector);
        _hotkeyService = new GlobalHotkeyService(_messageWindow, HandleHotkeyAction);

        _toggleEnabledMenuItem = new ToolStripMenuItem();
        _toggleEnabledMenuItem.Click += (_, _) => ToggleEnabled();

        _settingsMenuItem = new ToolStripMenuItem("设置...");
        _settingsMenuItem.Click += (_, _) => OpenSettings();

        _clipboardModeMenuItem = new ToolStripMenuItem("剪贴板粘贴");
        _clipboardModeMenuItem.Click += (_, _) => SetInjectionMode(SuffixInjectionMode.ClipboardPaste, true);

        _simulatedModeMenuItem = new ToolStripMenuItem("逐字模拟输入");
        _simulatedModeMenuItem.Click += (_, _) => SetInjectionMode(SuffixInjectionMode.SimulatedTyping, true);

        var inputModeMenuItem = new ToolStripMenuItem("输入方式");
        inputModeMenuItem.DropDownItems.Add(_clipboardModeMenuItem);
        inputModeMenuItem.DropDownItems.Add(_simulatedModeMenuItem);

        var exitMenuItem = new ToolStripMenuItem("退出");
        exitMenuItem.Click += (_, _) => ExitThread();

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.AddRange(
        [
            _toggleEnabledMenuItem,
            _settingsMenuItem,
            inputModeMenuItem,
            new ToolStripSeparator(),
            exitMenuItem
        ]);

        _trayIcon = new NotifyIcon
        {
            ContextMenuStrip = _trayMenu,
            Icon = _icons.Enabled,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ToggleEnabled();

        RefreshTrayState();

        var hotkeyFailures = _hotkeyService.Reload(_settings);
        if (hotkeyFailures.Count > 0)
        {
            ShowBalloon($"以下快捷键未注册成功：{string.Join("；", hotkeyFailures)}", ToolTipIcon.Warning);
        }
    }

    protected override void ExitThreadCore()
    {
        _trayIcon.Visible = false;
        base.ExitThreadCore();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon.Dispose();
            _trayMenu.Dispose();
            _hotkeyService.Dispose();
            _keyboardHookService.Dispose();
            _messageWindow.Dispose();
            _icons.Dispose();
        }

        base.Dispose(disposing);
    }

    private ISuffixInjector GetCurrentInjector()
    {
        return _settings.InjectionMode == SuffixInjectionMode.ClipboardPaste
            ? _clipboardInjector
            : _simulatedTypingInjector;
    }

    private void HandleHotkeyAction(HotkeyAction action)
    {
        switch (action)
        {
            case HotkeyAction.ToggleEnabled:
                ToggleEnabled();
                break;
            case HotkeyAction.ToggleMode:
                var nextMode = _settings.InjectionMode == SuffixInjectionMode.ClipboardPaste
                    ? SuffixInjectionMode.SimulatedTyping
                    : SuffixInjectionMode.ClipboardPaste;
                SetInjectionMode(nextMode, true);
                break;
        }
    }

    private void ToggleEnabled()
    {
        _isEnabled = !_isEnabled;
        RefreshTrayState();
        ShowBalloon(_isEnabled ? "口癖补全已启用" : "口癖补全已禁用");
    }

    private void SetInjectionMode(SuffixInjectionMode mode, bool showNotification)
    {
        if (_settings.InjectionMode == mode)
        {
            RefreshTrayState();
            return;
        }

        _settings.InjectionMode = mode;
        RefreshTrayState();
        SaveSettings(showInteractiveError: false);

        if (showNotification)
        {
            ShowBalloon($"输入方式已切换为：{mode.ToDisplayName()}");
        }
    }

    private void OpenSettings()
    {
        using var settingsForm = new SettingsForm(_settings, TryApplySettingsFromDialog);
        if (settingsForm.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        ShowBalloon("设置已保存");
    }

    private void RefreshTrayState()
    {
        _toggleEnabledMenuItem.Checked = _isEnabled;
        _toggleEnabledMenuItem.Text = _isEnabled ? "禁用口癖补全" : "启用口癖补全";
        _clipboardModeMenuItem.Checked = _settings.InjectionMode == SuffixInjectionMode.ClipboardPaste;
        _simulatedModeMenuItem.Checked = _settings.InjectionMode == SuffixInjectionMode.SimulatedTyping;
        _trayIcon.Icon = _isEnabled ? _icons.Enabled : _icons.Disabled;
        _trayIcon.Text = BuildTrayText();
    }

    private string BuildTrayText()
    {
        var suffixDisplay = string.IsNullOrWhiteSpace(_settings.Suffix)
            ? "(空)"
            : _settings.Suffix.Trim();

        var text =
            $"Desuwa | {(_isEnabled ? "已启用" : "已禁用")} | {_settings.InjectionMode.ToShortDisplayName()} | {suffixDisplay}";
        return text.Length <= 63 ? text : $"{text[..60]}...";
    }

    private void SaveSettings(bool showInteractiveError)
    {
        try
        {
            _settingsStore.Save(_settings);
        }
        catch (Exception ex)
        {
            if (showInteractiveError)
            {
                MessageBox.Show(
                    $"保存设置失败：{ex.Message}",
                    "保存失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                ShowBalloon($"保存设置失败：{ex.Message}", ToolTipIcon.Warning);
            }
        }
    }

    private void ShowBalloon(string message, ToolTipIcon toolTipIcon = ToolTipIcon.Info)
    {
        _trayIcon.BalloonTipTitle = "Desuwa";
        _trayIcon.BalloonTipText = message;
        _trayIcon.BalloonTipIcon = toolTipIcon;
        _trayIcon.ShowBalloonTip(1500);
    }

    private string? TryApplySettingsFromDialog(AppSettings proposedSettings)
    {
        var previousSettings = _settings.Clone();
        var hotkeyFailures = _hotkeyService.Reload(proposedSettings);
        if (hotkeyFailures.Count > 0)
        {
            _hotkeyService.Reload(previousSettings);
            return FormatHotkeyFailures(hotkeyFailures);
        }

        _settings = proposedSettings.Clone();
        RefreshTrayState();

        try
        {
            _settingsStore.Save(_settings);
            return null;
        }
        catch (Exception ex)
        {
            _settings = previousSettings;
            RefreshTrayState();
            _hotkeyService.Reload(previousSettings);
            return $"保存设置失败：{ex.Message}";
        }
    }

    private static string FormatHotkeyFailures(IReadOnlyCollection<string> hotkeyFailures)
    {
        var normalizedFailures = hotkeyFailures
            .Select(failure => failure.Contains("Win32 1409", StringComparison.Ordinal)
                ? $"{failure.Replace(" (Win32 1409)", string.Empty, StringComparison.Ordinal)} 已被系统或其他程序占用"
                : failure)
            .ToArray();

        return $"以下快捷键不可用：{string.Join("；", normalizedFailures)}";
    }
}