using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.App;

internal sealed class LicenseProductDialog : Form
{
    private readonly TextBox _name = new() { Width = 330 };
    private readonly TextBox _edition = new() { Width = 330 };
    private readonly ComboBox _type = new() { Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _seats = new() { Width = 120, Minimum = 0, Maximum = 100000, Value = 1 };
    private readonly NumericUpDown _threshold = new() { Width = 120, Minimum = 0, Maximum = 10000, Value = 2 };
    private readonly CheckBox _hasExpiry = new() { Text = "Date d'expiration", AutoSize = true };
    private readonly DateTimePicker _expiry = new() { Width = 160, Enabled = false };
    private readonly TextBox _keys = new() { Width = 330, Height = 90, Multiline = true, ScrollBars = ScrollBars.Vertical, UseSystemPasswordChar = true };
    private readonly TextBox _portal = new() { Width = 330 };

    public LicensedProduct? Product { get; private set; }

    public LicenseProductDialog()
    {
        Text = "Ajouter un produit sous licence";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 470);

        _type.DataSource = new[]
        {
            new TypeChoice(LicenseType.IndividualKeys, "Clés individuelles"),
            new TypeChoice(LicenseType.MultiSeatKey, "Clé multi-postes"),
            new TypeChoice(LicenseType.VendorManaged, "Abonnement géré par l'éditeur"),
            new TypeChoice(LicenseType.OpenSourceSupport, "Logiciel libre / contrat de support")
        };
        _hasExpiry.CheckedChanged += (_, _) => _expiry.Enabled = _hasExpiry.Checked;
        _type.SelectedIndexChanged += (_, _) => UpdateTypeHelp();

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 0 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(table, "Produit", _name);
        AddRow(table, "Édition", _edition);
        AddRow(table, "Type", _type);
        AddRow(table, "Licences achetées", _seats);
        AddRow(table, "Alerte à", _threshold);
        AddRow(table, string.Empty, new FlowLayoutPanel { AutoSize = true, Controls = { _hasExpiry, _expiry } });
        AddRow(table, "Clés (une par ligne)", _keys);
        AddRow(table, "Portail éditeur", _portal);

        var help = new Label
        {
            Name = "typeHelp",
            AutoSize = false,
            Height = 42,
            Dock = DockStyle.Fill,
            ForeColor = Color.DimGray
        };
        AddRow(table, string.Empty, help);

        var save = new Button { Text = "Enregistrer", AutoSize = true, DialogResult = DialogResult.None };
        var cancel = new Button { Text = "Annuler", AutoSize = true, DialogResult = DialogResult.Cancel };
        save.Click += Save_Click;
        AddRow(table, string.Empty, new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Controls = { cancel, save } });
        Controls.Add(table);
        CancelButton = cancel;
        UpdateTypeHelp();
    }

    private static void AddRow(TableLayoutPanel table, string label, Control control)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 8, 3, 3) }, 0, row);
        control.Margin = new Padding(3, 4, 3, 4);
        table.Controls.Add(control, 1, row);
    }

    private void UpdateTypeHelp()
    {
        if (Controls.Count == 0) return;
        var help = Controls.Find("typeHelp", true).OfType<Label>().FirstOrDefault();
        if (help is null || _type.SelectedItem is not TypeChoice choice) return;
        _keys.Enabled = choice.Value is LicenseType.IndividualKeys or LicenseType.MultiSeatKey;
        _seats.Enabled = choice.Value != LicenseType.OpenSourceSupport;
        help.Text = choice.Value switch
        {
            LicenseType.VendorManaged => "Conservez seulement le nombre de places et le lien du portail. Ne copiez pas le mot de passe du compte éditeur.",
            LicenseType.OpenSourceSupport => "Aucune clé requise. Cette fiche suit un contrat de support ou une échéance.",
            _ => "Les clés sont chiffrées pour le compte Windows actuel et ne sont jamais écrites dans les journaux."
        };
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            MessageBox.Show(this, "Indiquez le nom du produit.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var choice = (TypeChoice)_type.SelectedItem!;
        var secrets = _keys.Lines.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct().ToList();
        var seats = choice.Value == LicenseType.OpenSourceSupport ? 0 : (int)_seats.Value;
        if (choice.Value == LicenseType.IndividualKeys && secrets.Count > seats)
        {
            MessageBox.Show(this, "Le nombre de clés dépasse le nombre de licences achetées.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Product = new LicensedProduct
        {
            Name = _name.Text.Trim(), Edition = _edition.Text.Trim(), Type = choice.Value,
            SeatsOwned = seats, WarningThreshold = (int)_threshold.Value,
            ExpiresOn = _hasExpiry.Checked ? _expiry.Value.Date : null,
            VendorPortalUrl = string.IsNullOrWhiteSpace(_portal.Text) ? null : _portal.Text.Trim(),
            Secrets = secrets.Select((value, i) => new LicenseSecret { Value = value, Label = $"Clé {i + 1}" }).ToList()
        };
        DialogResult = DialogResult.OK;
        Close();
    }

    private sealed record TypeChoice(LicenseType Value, string Label)
    {
        public override string ToString() => Label;
    }
}
