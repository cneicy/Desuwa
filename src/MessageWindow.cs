namespace Desuwa;

internal sealed class MessageWindow : Form
{
    public event Action<int>? HotkeyPressed;

    public MessageWindow()
    {
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.Manual;
        Size = Size.Empty;
        Location = new Point(-32000, -32000);
        Opacity = 0;
    }

    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(false);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WmHotkey)
        {
            HotkeyPressed?.Invoke(m.WParam.ToInt32());
        }

        base.WndProc(ref m);
    }
}