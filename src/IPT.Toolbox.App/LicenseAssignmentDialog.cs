using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.App;

internal sealed class LicenseAssignmentDialog : Form
{
    private readonly TextBox _client = new() { Width = 280 };
    private readonly TextBox _device = new() { Width = 280 };
    private readonly TextBox _notes = new() { Width = 280, Height = 60, Multiline = true };
    public string ClientName => _client.Text.Trim();
    public string DeviceName => _device.Text.Trim();
    public string? Notes => string.IsNullOrWhiteSpace(_notes.Text) ? null : _notes.Text.Trim();

    public LicenseAssignmentDialog(string productName)
    {
        Text = $"Attribuer — {productName}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(450, 250);
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(table, "Client", _client);
        AddRow(table, "Nom du PC", _device);
        AddRow(table, "Notes", _notes);
        var save = new Button { Text = "Attribuer", AutoSize = true };
        var cancel = new Button { Text = "Annuler", AutoSize = true, DialogResult = DialogResult.Cancel };
        save.Click += (_, _) =>
        {
            if (ClientName.Length == 0 || DeviceName.Length == 0)
            {
                MessageBox.Show(this, "Le client et le nom du PC sont obligatoires.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        AddRow(table, string.Empty, new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Controls = { cancel, save } });
        Controls.Add(table);
        CancelButton = cancel;
    }

    private static void AddRow(TableLayoutPanel table, string label, Control control)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 8, 3, 3) }, 0, row);
        table.Controls.Add(control, 1, row);
    }
}
