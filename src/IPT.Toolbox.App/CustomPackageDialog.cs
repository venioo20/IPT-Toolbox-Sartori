using IPT.Toolbox.Core.Models;
using IPT.Toolbox.Core.Services;

namespace IPT.Toolbox.App;

internal sealed class CustomPackageDialog : Form
{
    private readonly TextBox _name = new() { Width = 330 };
    private readonly TextBox _wingetId = new() { Width = 330 };
    private readonly TextBox _category = new() { Width = 330, Text = "Personnalisé" };
    private readonly TextBox _detection = new() { Width = 330 };
    public PackageDefinition? Package { get; private set; }

    public CustomPackageDialog()
    {
        Text = "Ajouter un logiciel personnalisé — Pro";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(540, 300);
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 0 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(table, "Nom du logiciel", _name);
        AddRow(table, "Identifiant WinGet", _wingetId);
        AddRow(table, "Catégorie", _category);
        AddRow(table, "Exécutable de détection", _detection);
        AddRow(table, string.Empty, new Label { Text = "Exemple d’identifiant : Piriform.CCleaner. L’identifiant doit correspondre exactement au catalogue WinGet.", AutoSize = false, Height = 45, Dock = DockStyle.Fill, ForeColor = Color.DimGray });
        var save = new Button { Text = "Ajouter", AutoSize = true };
        var cancel = new Button { Text = "Annuler", AutoSize = true, DialogResult = DialogResult.Cancel };
        save.Click += Save_Click;
        AddRow(table, string.Empty, new FlowLayoutPanel { AutoSize = true, Controls = { save, cancel } });
        Controls.Add(table);
        CancelButton = cancel;
    }

    private static void AddRow(TableLayoutPanel table, string label, Control control)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 8, 3, 3) }, 0, row);
        control.Margin = new Padding(3, 4, 3, 4);
        table.Controls.Add(control, 1, row);
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) throw new ArgumentException("Indiquez le nom du logiciel.");
            var wingetId = _wingetId.Text.Trim();
            _ = WingetArguments.Create(wingetId, true);
            Package = new PackageDefinition
            {
                Id = $"custom-{Guid.NewGuid():N}", Name = _name.Text.Trim(), WingetId = wingetId,
                Category = string.IsNullOrWhiteSpace(_category.Text) ? "Personnalisé" : _category.Text.Trim(),
                DetectionExecutable = string.IsNullOrWhiteSpace(_detection.Text) ? null : _detection.Text.Trim(),
                EnabledByDefault = false, Notes = "Ajouté localement par un utilisateur Pro"
            };
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
