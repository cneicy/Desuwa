using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Desuwa;

public class Program : Form
{
    private const int WhKeyboardLl = 13;
    private const int WmKeydown = 0x0100;
    private const int VkReturn = 0x0D;
    
    private IntPtr _hookId;
    private bool _processing;
    private bool _active = true;
    
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;
    
    private static string _customSuffix = "desuwa";
    private static readonly object LockObj = new();

    private LowLevelKeyboardProcDelegate _hookDelegate;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    
    private delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

    private Program()
    {
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("启用/禁用", null, ToggleActive);
        _trayMenu.Items.Add("编辑口癖", null, EditSuffix);
        _trayMenu.Items.Add("退出", null, OnExit);
        
        _trayIcon = new NotifyIcon
        {
            Text = $"全自动口癖工具 (当前口癖: {_customSuffix.Trim()})",
            Icon = new Icon(SystemIcons.Application, 40, 40),
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        
        _trayIcon.DoubleClick += TrayIcon_DoubleClick;
        
        _hookDelegate = HookCallback;
        
        _hookId = SetHook(_hookDelegate);
    }

    private void TrayIcon_DoubleClick(object sender, EventArgs e)
    {
        ToggleActive(null, EventArgs.Empty);
    }

    private void ToggleActive(object sender, EventArgs e)
    {
        _active = !_active;
        _trayIcon.Text = $"全自动口癖工具 - {(_active ? "已启用" : "已禁用")} (口癖: {_customSuffix.Trim()})";
        
        if (_active)
        {
            _trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
        }
        else
        {
            using (var bmp = new Bitmap(16, 16))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.DrawRectangle(Pens.Red, 0, 0, 15, 15);
                g.DrawLine(Pens.Red, 0, 0, 15, 15);
                _trayIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            }
        }
    }

    private void EditSuffix(object sender, EventArgs e)
    {
        lock (LockObj)
        {
            using (var form = new SuffixEditorForm(_customSuffix.Trim()))
            {
                if (form.ShowDialog() != DialogResult.OK) return;
                _customSuffix = form.SuffixValue;
                _trayIcon.Text = $"全自动口癖工具 - by Eicy (当前口癖: {_customSuffix.Trim()})";
            }
        }
    }

    private IntPtr SetHook(LowLevelKeyboardProcDelegate proc)
    {
        using (var curProcess = Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WhKeyboardLl, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0 || wParam != (IntPtr)WmKeydown || !_active) 
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        
        var vkCode = Marshal.ReadInt32(lParam);

        if (vkCode != VkReturn || _processing) 
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        
        _processing = true;
        
        try
        {
            string currentSuffix;
            lock (LockObj)
            {
                currentSuffix = _customSuffix;
            }
            
            keybd_event(0x23, 0, 0, 0); // VK_END
            keybd_event(0x23, 0, 0x0002, 0); // KEYEVENTF_KEYUP
            
            Thread.Sleep(20);

            SendKeys.SendWait(currentSuffix);
            
            Thread.Sleep(20);
            keybd_event(0x0D, 0, 0, 0);
            keybd_event(0x0D, 0, 0x0002, 0);
            
            return (IntPtr)1;
        }
        catch
        {
            // 忽略错误
        }
        finally
        {
            _processing = false;
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void OnExit(object sender, EventArgs e)
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
        
        _trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon.Dispose();
            _trayMenu.Dispose();
        }
        
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
        
        base.Dispose(disposing);
    }

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Program());
    }
    
    private class SuffixEditorForm : Form
    {
        public string SuffixValue { get; private set; }
        
        public SuffixEditorForm(string currentSuffix)
        {
            Text = "编辑口癖文本";
            Width = 400;
            Height = 150;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            
            var label = new Label
            {
                Text = "输入你想要添加的口癖文本:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            
            var textBox = new TextBox
            {
                Text = currentSuffix,
                Location = new Point(20, 50),
                Width = 340
            };
            
            var okButton = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 80),
                Size = new Size(75, 30)
            };
            
            var cancelButton = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(285, 80),
                Size = new Size(75, 30)
            };
            
            okButton.Click += (_, _) =>
            {
                SuffixValue = textBox.Text;
            };
            
            Controls.Add(label);
            Controls.Add(textBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        [AllowNull] public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }
    }
}