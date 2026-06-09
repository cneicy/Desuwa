namespace Desuwa;

internal sealed class SettingsForm : Form
{
    private static readonly HotkeyKeyOption[] HotkeyOptions = CreateHotkeyOptions();
    private readonly Func<AppSettings, string?>? _applySettings;

    private readonly TextBox _suffixTextBox;
    private readonly RadioButton _clipboardModeRadioButton;
    private readonly RadioButton _simulatedModeRadioButton;
    private readonly ComboBox _toggleEnabledKeyComboBox;
    private readonly CheckBox _toggleEnabledCtrlCheckBox;
    private readonly CheckBox _toggleEnabledAltCheckBox;
    private readonly CheckBox _toggleEnabledShiftCheckBox;
    private readonly ComboBox _toggleModeKeyComboBox;
    private readonly CheckBox _toggleModeCtrlCheckBox;
    private readonly CheckBox _toggleModeAltCheckBox;
    private readonly CheckBox _toggleModeShiftCheckBox;
    private readonly Label _validationLabel;

    public SettingsForm(AppSettings settings, Func<AppSettings, string?>? applySettings = null)
    {
        _applySettings = applySettings;
        UpdatedSettings = settings.Clone();

        Text = "Desuwa 设置";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(540, 360);

        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 5
        };
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(rootLayout);

        var suffixLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        suffixLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suffixLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        suffixLayout.Controls.Add(new Label
        {
            Text = "口癖文本：",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 8, 6)
        }, 0, 0);

        _suffixTextBox = new TextBox
        {
            Dock = DockStyle.Top,
            Width = 380,
            Text = settings.Suffix
        };
        suffixLayout.Controls.Add(_suffixTextBox, 1, 0);
        rootLayout.Controls.Add(suffixLayout);

        var modeGroupBox = new GroupBox
        {
            Text = "输入方式",
            Dock = DockStyle.Top,
            AutoSize = true
        };
        var modeLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoSize = true,
            Padding = new Padding(8)
        };
        _clipboardModeRadioButton = new RadioButton
        {
            Text = "剪贴板粘贴",
            AutoSize = true
        };
        _simulatedModeRadioButton = new RadioButton
        {
            Text = "逐字模拟输入",
            AutoSize = true
        };
        modeLayout.Controls.Add(_clipboardModeRadioButton);
        modeLayout.Controls.Add(_simulatedModeRadioButton);
        modeGroupBox.Controls.Add(modeLayout);
        rootLayout.Controls.Add(modeGroupBox);

        var hotkeyGroupBox = new GroupBox
        {
            Text = "快捷键",
            Dock = DockStyle.Top,
            AutoSize = true
        };
        var hotkeyLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(8)
        };
        hotkeyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        hotkeyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        hotkeyLayout.Controls.Add(new Label
        {
            Text = "切换启用状态：",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 8, 8)
        }, 0, 0);
        hotkeyLayout.Controls.Add(
            CreateHotkeyEditor(
                out _toggleEnabledKeyComboBox,
                out _toggleEnabledCtrlCheckBox,
                out _toggleEnabledAltCheckBox,
                out _toggleEnabledShiftCheckBox),
            1,
            0);

        hotkeyLayout.Controls.Add(new Label
        {
            Text = "切换输入方式：",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 8, 8)
        }, 0, 1);
        hotkeyLayout.Controls.Add(
            CreateHotkeyEditor(
                out _toggleModeKeyComboBox,
                out _toggleModeCtrlCheckBox,
                out _toggleModeAltCheckBox,
                out _toggleModeShiftCheckBox),
            1,
            1);

        var hotkeyHintLabel = new Label
        {
            Text = "说明：不选按键视为禁用；快捷键至少要包含 Ctrl / Alt / Shift 之一。",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 8, 0, 0)
        };
        hotkeyLayout.Controls.Add(hotkeyHintLabel, 0, 2);
        hotkeyLayout.SetColumnSpan(hotkeyHintLabel, 2);

        hotkeyGroupBox.Controls.Add(hotkeyLayout);
        rootLayout.Controls.Add(hotkeyGroupBox);

        _validationLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.Firebrick,
            Margin = new Padding(0, 8, 0, 0)
        };
        rootLayout.Controls.Add(_validationLabel);

        var buttonLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        var saveButton = new Button
        {
            Text = "保存",
            AutoSize = true
        };
        saveButton.Click += (_, _) => SaveSettings();

        var cancelButton = new Button
        {
            Text = "取消",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };

        buttonLayout.Controls.Add(saveButton);
        buttonLayout.Controls.Add(cancelButton);
        rootLayout.Controls.Add(buttonLayout);

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        LoadSettings(settings);
    }

    public AppSettings UpdatedSettings { get; private set; }

    private static HotkeyKeyOption[] CreateHotkeyOptions()
    {
        var options = new List<HotkeyKeyOption>
        {
            new(Keys.None, "(未设置)")
        };

        for (var key = Keys.A; key <= Keys.Z; key++)
        {
            options.Add(new HotkeyKeyOption(key, key.ToString()));
        }

        for (var key = Keys.D0; key <= Keys.D9; key++)
        {
            var display = ((char)('0' + (key - Keys.D0))).ToString();
            options.Add(new HotkeyKeyOption(key, display));
        }

        for (var functionKey = Keys.F1; functionKey <= Keys.F12; functionKey++)
        {
            options.Add(new HotkeyKeyOption(functionKey, functionKey.ToString()));
        }

        return options.ToArray();
    }

    private static Control CreateHotkeyEditor(
        out ComboBox keyComboBox,
        out CheckBox ctrlCheckBox,
        out CheckBox altCheckBox,
        out CheckBox shiftCheckBox)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };

        keyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 120
        };
        keyComboBox.Items.AddRange(HotkeyOptions.Cast<object>().ToArray());
        keyComboBox.SelectedIndex = 0;

        ctrlCheckBox = new CheckBox
        {
            Text = "Ctrl",
            AutoSize = true,
            Margin = new Padding(12, 4, 0, 0)
        };

        altCheckBox = new CheckBox
        {
            Text = "Alt",
            AutoSize = true,
            Margin = new Padding(12, 4, 0, 0)
        };

        shiftCheckBox = new CheckBox
        {
            Text = "Shift",
            AutoSize = true,
            Margin = new Padding(12, 4, 0, 0)
        };

        panel.Controls.Add(keyComboBox);
        panel.Controls.Add(ctrlCheckBox);
        panel.Controls.Add(altCheckBox);
        panel.Controls.Add(shiftCheckBox);
        return panel;
    }

    private void LoadSettings(AppSettings settings)
    {
        _suffixTextBox.Text = settings.Suffix;
        _clipboardModeRadioButton.Checked = settings.InjectionMode == SuffixInjectionMode.ClipboardPaste;
        _simulatedModeRadioButton.Checked = settings.InjectionMode == SuffixInjectionMode.SimulatedTyping;

        LoadHotkey(settings.ToggleEnabledHotkey, _toggleEnabledKeyComboBox, _toggleEnabledCtrlCheckBox,
            _toggleEnabledAltCheckBox, _toggleEnabledShiftCheckBox);
        LoadHotkey(settings.ToggleModeHotkey, _toggleModeKeyComboBox, _toggleModeCtrlCheckBox, _toggleModeAltCheckBox,
            _toggleModeShiftCheckBox);
    }

    private void SaveSettings()
    {
        var toggleEnabledHotkey = ReadHotkey(_toggleEnabledKeyComboBox, _toggleEnabledCtrlCheckBox,
            _toggleEnabledAltCheckBox, _toggleEnabledShiftCheckBox);
        var toggleModeHotkey = ReadHotkey(_toggleModeKeyComboBox, _toggleModeCtrlCheckBox, _toggleModeAltCheckBox,
            _toggleModeShiftCheckBox);

        if (!toggleEnabledHotkey.IsValid())
        {
            ShowValidation("“切换启用状态”快捷键至少需要一个修饰键。");
            return;
        }

        if (!toggleModeHotkey.IsValid())
        {
            ShowValidation("“切换输入方式”快捷键至少需要一个修饰键。");
            return;
        }

        if (toggleEnabledHotkey.ConflictsWith(toggleModeHotkey))
        {
            ShowValidation("两个快捷键不能设置成同一组组合键。");
            return;
        }

        var proposedSettings = new AppSettings
        {
            Suffix = _suffixTextBox.Text,
            InjectionMode = _clipboardModeRadioButton.Checked
                ? SuffixInjectionMode.ClipboardPaste
                : SuffixInjectionMode.SimulatedTyping,
            ToggleEnabledHotkey = toggleEnabledHotkey,
            ToggleModeHotkey = toggleModeHotkey
        };

        if (_applySettings != null)
        {
            var applyError = _applySettings(proposedSettings.Clone());
            if (!string.IsNullOrEmpty(applyError))
            {
                ShowValidation(applyError);
                return;
            }
        }

        _validationLabel.Text = string.Empty;
        UpdatedSettings = proposedSettings;

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowValidation(string message)
    {
        _validationLabel.Text = message;
    }

    private static void LoadHotkey(
        HotkeyGesture gesture,
        ComboBox keyComboBox,
        CheckBox ctrlCheckBox,
        CheckBox altCheckBox,
        CheckBox shiftCheckBox)
    {
        var selectedKey = gesture.Enabled ? gesture.Key : Keys.None;
        var selectedIndex = 0;
        for (var index = 0; index < HotkeyOptions.Length; index++)
        {
            if (HotkeyOptions[index].Key == selectedKey)
            {
                selectedIndex = index;
                break;
            }
        }

        keyComboBox.SelectedIndex = selectedIndex;

        var modifiers = gesture.Enabled ? gesture.NormalizedModifiers : Keys.None;
        ctrlCheckBox.Checked = (modifiers & Keys.Control) != 0;
        altCheckBox.Checked = (modifiers & Keys.Alt) != 0;
        shiftCheckBox.Checked = (modifiers & Keys.Shift) != 0;
    }

    private static HotkeyGesture ReadHotkey(
        ComboBox keyComboBox,
        CheckBox ctrlCheckBox,
        CheckBox altCheckBox,
        CheckBox shiftCheckBox)
    {
        var selectedOption = keyComboBox.SelectedItem as HotkeyKeyOption ?? HotkeyOptions[0];
        if (selectedOption.Key == Keys.None)
        {
            return HotkeyGesture.Disabled();
        }

        var modifiers = Keys.None;
        if (ctrlCheckBox.Checked)
        {
            modifiers |= Keys.Control;
        }

        if (altCheckBox.Checked)
        {
            modifiers |= Keys.Alt;
        }

        if (shiftCheckBox.Checked)
        {
            modifiers |= Keys.Shift;
        }

        return new HotkeyGesture
        {
            Enabled = true,
            Key = selectedOption.Key,
            Modifiers = modifiers
        };
    }

    private sealed class HotkeyKeyOption
    {
        public HotkeyKeyOption(Keys key, string displayName)
        {
            Key = key;
            DisplayName = displayName;
        }

        public Keys Key { get; }

        public string DisplayName { get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}