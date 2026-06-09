using System.Text.Json.Serialization;

namespace Desuwa;

public sealed class HotkeyGesture
{
    private const Keys AllowedModifiers = Keys.Control | Keys.Alt | Keys.Shift;

    public bool Enabled { get; set; }

    public Keys Key { get; set; }

    public Keys Modifiers { get; set; }

    [JsonIgnore] public Keys NormalizedModifiers => NormalizeModifiers(Modifiers);

    public static HotkeyGesture Disabled()
    {
        return new HotkeyGesture
        {
            Enabled = false,
            Key = Keys.None,
            Modifiers = Keys.None
        };
    }

    public static HotkeyGesture CreateDefault(Keys key, Keys modifiers = Keys.Control | Keys.Alt)
    {
        return new HotkeyGesture
        {
            Enabled = true,
            Key = key,
            Modifiers = NormalizeModifiers(modifiers)
        };
    }

    public HotkeyGesture Clone()
    {
        return new HotkeyGesture
        {
            Enabled = Enabled,
            Key = Key,
            Modifiers = NormalizeModifiers(Modifiers)
        };
    }

    public bool IsValid()
    {
        return !Enabled || (Key != Keys.None && NormalizeModifiers(Modifiers) != Keys.None);
    }

    public bool ConflictsWith(HotkeyGesture? other)
    {
        if (other is null || !Enabled || !other.Enabled)
        {
            return false;
        }

        return Key == other.Key && NormalizeModifiers(Modifiers) == NormalizeModifiers(other.Modifiers);
    }

    public uint ToNativeModifiers()
    {
        var modifiers = NormalizeModifiers(Modifiers);
        uint nativeModifiers = NativeMethods.ModNorepeat;

        if ((modifiers & Keys.Control) != 0)
        {
            nativeModifiers |= NativeMethods.ModControl;
        }

        if ((modifiers & Keys.Alt) != 0)
        {
            nativeModifiers |= NativeMethods.ModAlt;
        }

        if ((modifiers & Keys.Shift) != 0)
        {
            nativeModifiers |= NativeMethods.ModShift;
        }

        return nativeModifiers;
    }

    public string ToDisplayString()
    {
        if (!Enabled || Key == Keys.None)
        {
            return "(未设置)";
        }

        var parts = new List<string>();
        var modifiers = NormalizeModifiers(Modifiers);

        if ((modifiers & Keys.Control) != 0)
        {
            parts.Add("Ctrl");
        }

        if ((modifiers & Keys.Alt) != 0)
        {
            parts.Add("Alt");
        }

        if ((modifiers & Keys.Shift) != 0)
        {
            parts.Add("Shift");
        }

        parts.Add(GetKeyDisplayName(Key));
        return string.Join("+", parts);
    }

    private static Keys NormalizeModifiers(Keys modifiers)
    {
        return modifiers & AllowedModifiers;
    }

    private static string GetKeyDisplayName(Keys key)
    {
        if (key is >= Keys.D0 and <= Keys.D9)
        {
            return ((char)('0' + (key - Keys.D0))).ToString();
        }

        return key.ToString();
    }
}