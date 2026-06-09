using System.Reflection;

namespace Desuwa;

internal sealed class AppIcons : IDisposable
{
    public Icon Enabled { get; } = LoadIconFromResource("enabled.ico") ?? (Icon)SystemIcons.Application.Clone();

    public Icon Disabled { get; } = LoadIconFromResource("disabled.ico") ?? CreateFallbackDisabledIcon();

    public void Dispose()
    {
        Enabled.Dispose();
        Disabled.Dispose();
    }

    private static Icon? LoadIconFromResource(string resourceName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = $"Desuwa.Assets.{resourceName}";

            using var stream = assembly.GetManifestResourceStream(fullResourceName);
            return stream == null ? null : new Icon(stream);
        }
        catch
        {
            return null;
        }
    }

    private static Icon CreateFallbackDisabledIcon()
    {
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.DrawRectangle(Pens.Red, 0, 0, 15, 15);
        graphics.DrawLine(Pens.Red, 0, 0, 15, 15);

        var iconHandle = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(iconHandle);
            return (Icon)icon.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(iconHandle);
        }
    }
}