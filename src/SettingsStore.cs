using System.Text.Json;
using System.Text.Json.Serialization;

namespace Desuwa;

internal sealed class SettingsStore(string? settingsPath = null)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public string SettingsPath { get; } = settingsPath ?? Path.Combine(AppContext.BaseDirectory, "settings.json");

    public AppSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            return Normalize(settings);
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var normalized = Normalize(settings.Clone());
        var json = JsonSerializer.Serialize(normalized, _jsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        settings.Suffix ??= "desuwa";

        if (!Enum.IsDefined(settings.InjectionMode))
        {
            settings.InjectionMode = SuffixInjectionMode.ClipboardPaste;
        }

        settings.ToggleEnabledHotkey =
            NormalizeHotkey(settings.ToggleEnabledHotkey, HotkeyGesture.CreateDefault(Keys.F10, Keys.Alt));
        settings.ToggleModeHotkey =
            NormalizeHotkey(settings.ToggleModeHotkey, HotkeyGesture.CreateDefault(Keys.F11, Keys.Alt));

        return settings;
    }

    private static HotkeyGesture NormalizeHotkey(HotkeyGesture? gesture, HotkeyGesture fallback)
    {
        if (gesture is null)
        {
            return fallback.Clone();
        }

        if (!gesture.Enabled || gesture.Key == Keys.None)
        {
            return HotkeyGesture.Disabled();
        }

        return gesture.IsValid() ? gesture.Clone() : fallback.Clone();
    }
}