using OcctDemo.Common;

namespace OcctDemo.WinForms;

internal sealed class ParameterDialog : Form
{
    private readonly Dictionary<string, Control> _editors = new(StringComparer.OrdinalIgnoreCase);

    private ParameterDialog(string title, IReadOnlyList<DemoParameterDefinition> parameters)
    {
        Text = title;
        Font = new Font(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI", 9F);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        Width = 460;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            Padding = new Padding(12)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));

        var row = 0;
        foreach (var parameter in parameters)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var label = new Label { Text = parameter.Label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 8) };
            var editor = CreateEditor(parameter);
            editor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            editor.Margin = new Padding(3, 4, 3, 4);
            var unit = new Label { Text = parameter.Unit ?? string.Empty, AutoSize = true, Anchor = AnchorStyles.Left };
            table.Controls.Add(label, 0, row);
            table.Controls.Add(editor, 1, row);
            table.Controls.Add(unit, 2, row);
            _editors[parameter.Key] = editor;
            row++;
        }

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        var ok = new Button { Text = DemoLocalization.Text("Dialog.Ok"), DialogResult = DialogResult.OK, AutoSize = true, Padding = new Padding(12, 2, 12, 2) };
        var cancel = new Button { Text = DemoLocalization.Text("Dialog.Cancel"), DialogResult = DialogResult.Cancel, AutoSize = true, Padding = new Padding(12, 2, 12, 2) };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 0, row);
        table.SetColumnSpan(buttons, 3);

        Controls.Add(table);
        AcceptButton = ok;
        CancelButton = cancel;
        ClientSize = new Size(440, Math.Min(720, 90 + parameters.Count * 42));
    }

    public static bool TryGetValues(IWin32Window owner, string title, IReadOnlyList<DemoParameterDefinition> parameters, out IReadOnlyDictionary<string, string> values)
    {
        if (parameters.Count == 0)
        {
            values = new Dictionary<string, string>();
            return true;
        }

        using var dialog = new ParameterDialog(title, parameters);
        if (dialog.ShowDialog(owner) != DialogResult.OK)
        {
            values = new Dictionary<string, string>();
            return false;
        }
        values = dialog.ReadValues();
        return true;
    }

    private static Control CreateEditor(DemoParameterDefinition parameter)
    {
        return parameter.Kind switch
        {
            DemoParameterKind.Boolean => new CheckBox { Checked = string.Equals(parameter.DefaultValue, "true", StringComparison.OrdinalIgnoreCase), AutoSize = true },
            DemoParameterKind.Choice => new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = parameter.Options?.ToArray() ?? Array.Empty<string>(),
                SelectedItem = parameter.DefaultValue,
                Width = 220
            },
            _ => new TextBox { Text = parameter.DefaultValue, Width = 220 }
        };
    }

    private IReadOnlyDictionary<string, string> ReadValues()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _editors)
        {
            values[pair.Key] = pair.Value switch
            {
                CheckBox checkBox => checkBox.Checked ? "true" : "false",
                ComboBox comboBox => comboBox.SelectedItem?.ToString() ?? string.Empty,
                TextBox textBox => textBox.Text.Trim(),
                _ => string.Empty
            };
        }
        return values;
    }
}
