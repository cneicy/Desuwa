namespace Desuwa;

public sealed class AppSettings
{
    public string Suffix { get; set; } = "desuwa";

    public SuffixInjectionMode InjectionMode { get; set; } = SuffixInjectionMode.ClipboardPaste;

    public HotkeyGesture ToggleEnabledHotkey { get; set; } = HotkeyGesture.CreateDefault(Keys.F10, Keys.Alt);

    public HotkeyGesture ToggleModeHotkey { get; set; } = HotkeyGesture.CreateDefault(Keys.F11, Keys.Alt);

    public AppSettings Clone()
    {
        return new AppSettings
        {
            Suffix = Suffix,
            InjectionMode = InjectionMode,
            ToggleEnabledHotkey = ToggleEnabledHotkey.Clone(),
            ToggleModeHotkey = ToggleModeHotkey.Clone()
        };
    }
}