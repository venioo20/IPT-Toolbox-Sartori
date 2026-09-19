using IPT.Toolbox.Core.Models;
using IPT.Toolbox.Core.Services;

namespace IPT.Toolbox.App;

internal sealed class LicenseManagerForm : Form
{
    private readonly LicenseVaultService _service;
    private LicenseVault _vault = new();
    private readonly DataGridView _products = CreateGrid();
    private readonly DataGridView _assignments = CreateGrid();
    private readonly Label _summary = new() { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
    private readonly Button _assignButton = new() { Text = "Attribuer une licence", AutoSize = true, Enabled = false };
    private readonly Button _releaseButton = new() { Text = "Libérer la sélection", AutoSize = true, Enabled = false };

    public LicenseManagerForm()
    {
        Text = "IPT Toolbox Pro — Gestion des licences";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 650);
        Size = new Size(1050, 720);
        _service = new LicenseVaultService(AppPaths.LicenseVaultFile, new WindowsDpapiProtector());

        var header = new Panel { Dock = DockStyle.Top, Height = 75, Padding = new Padding(14) };
        header.Controls.Add(new Label
        {
            Text = "LICENCES PROFESSIONNELLES",
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            Location = new Point(14, 8)
        });
        _summary.Location = new Point(17, 43);
        header.Controls.Add(_summary);

        var productButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8, 5, 8, 5) };
        var addButton = new Button { Text = "Ajouter un produit", AutoSize = true };
        var openPortalButton = new Button { Text = "Ouvrir le portail éditeur", AutoSize = true };
        addButton.Click += AddButton_Click;
        _assignButton.Click += AssignButton_Click;
        openPortalButton.Click += OpenPortalButton_Click;
        productButtons.Controls.AddRange([addButton, _assignButton, openPortalButton]);

        var assignmentButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8, 5, 8, 5) };
        _releaseButton.Click += ReleaseButton_Click;
        assignmentButtons.Controls.Add(_releaseButton);

        _products.SelectionChanged += (_, _) => RefreshAssignments();
        _assignments.SelectionChanged += (_, _) => _releaseButton.Enabled = SelectedAssignment() is not null;

        var productsPanel = new Panel { Dock = DockStyle.Fill };
        productsPanel.Controls.Add(_products);
        productsPanel.Controls.Add(productButtons);
        var assignmentsPanel = new Panel { Dock = DockStyle.Fill };
        assignmentsPanel.Controls.Add(_assignments);
        assignmentsPanel.Controls.Add(assignmentButtons);
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
        split.Panel1.Controls.Add(productsPanel);
        split.Panel2.Controls.Add(assignmentsPanel);

        var notice = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 42,
            Padding = new Padding(12, 8, 12, 4),
            Text = "Coffre local chiffré pour le compte Windows actuel. Les clés ne sont jamais affichées en clair ni inscrites dans les journaux.",
            ForeColor = Color.DimGray
        };
        Controls.Add(split);
        Controls.Add(notice);
        Controls.Add(header);
        Shown += async (_, _) => await LoadVaultAsync();
    }

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        RowHeadersVisible = false
    };

    private async Task LoadVaultAsync()
    {
        try
        {
            _vault = await _service.LoadAsync();
            RefreshProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible d'ouvrir le coffre : {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void RefreshProducts()
    {
        var summaries = LicenseVaultService.GetSummaries(_vault);
        _products.DataSource = summaries.Select(x => new ProductRow(
            x.ProductId, x.Name, x.Edition, TypeLabel(x.Type), x.Total, x.Used, x.Available,
            x.ExpiresOn?.ToString("dd/MM/yyyy") ?? "—", x.Status)).ToList();
        if (_products.Columns[nameof(ProductRow.ProductId)] is { } idColumn) idColumn.Visible = false;
        var total = summaries.Sum(x => x.Total);
        var used = summaries.Sum(x => x.Used);
        var alerts = summaries.Count(x => x.Status is "Stock faible" or "Épuisée" or "Expirée" or "Expire bientôt");
        _summary.Text = $"{summaries.Count} produit(s) • {used}/{total} licence(s) attribuée(s) • {alerts} alerte(s)";
        RefreshAssignments();
    }

    private void RefreshAssignments()
    {
        var product = SelectedProduct();
        _assignButton.Enabled = product is not null && CanAssign(product);
        if (product is null)
        {
            _assignments.DataSource = Array.Empty<AssignmentRow>();
            return;
        }
        _assignments.DataSource = _vault.Assignments.Where(a => a.ProductId == product.Id)
            .OrderByDescending(a => a.AssignedAtUtc)
            .Select(a => new AssignmentRow(a.Id, a.ClientName, a.DeviceName,
                a.AssignedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                a.ReleasedAtUtc is null ? "Attribuée" : $"Libérée le {a.ReleasedAtUtc.Value.ToLocalTime():dd/MM/yyyy}"))
            .ToList();
        if (_assignments.Columns[nameof(AssignmentRow.AssignmentId)] is { } idColumn) idColumn.Visible = false;
        _releaseButton.Enabled = SelectedAssignment() is not null;
    }

    private LicensedProduct? SelectedProduct()
    {
        if (_products.CurrentRow?.DataBoundItem is not ProductRow row) return null;
        return _vault.Products.SingleOrDefault(p => p.Id == row.ProductId);
    }

    private LicenseAssignment? SelectedAssignment()
    {
        if (_assignments.CurrentRow?.DataBoundItem is not AssignmentRow row) return null;
        return _vault.Assignments.SingleOrDefault(a => a.Id == row.AssignmentId && a.ReleasedAtUtc is null);
    }

    private bool CanAssign(LicensedProduct product)
    {
        if (product.Type == LicenseType.OpenSourceSupport) return false;
        var active = _vault.Assignments.Count(a => a.ProductId == product.Id && a.ReleasedAtUtc is null);
        return active < product.SeatsOwned && (product.ExpiresOn is null || product.ExpiresOn.Value.Date >= DateTime.Today);
    }

    private async void AddButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new LicenseProductDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Product is null) return;
        _vault.Products.Add(dialog.Product);
        _vault.Audit.Add(new LicenseAuditEntry { Action = "Produit ajouté", ProductId = dialog.Product.Id, Details = $"{dialog.Product.Name} — {dialog.Product.Edition}" });
        await SaveAndRefreshAsync();
    }

    private async void AssignButton_Click(object? sender, EventArgs e)
    {
        var product = SelectedProduct();
        if (product is null || !CanAssign(product)) return;
        using var dialog = new LicenseAssignmentDialog(product.Name);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        Guid? secretId = null;
        if (product.Type == LicenseType.IndividualKeys)
        {
            var usedSecretIds = _vault.Assignments.Where(a => a.ReleasedAtUtc is null && a.SecretId.HasValue).Select(a => a.SecretId!.Value).ToHashSet();
            secretId = product.Secrets.FirstOrDefault(s => !usedSecretIds.Contains(s.Id))?.Id;
            if (secretId is null)
            {
                MessageBox.Show(this, "Aucune clé individuelle libre n'est enregistrée.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        else if (product.Type == LicenseType.MultiSeatKey)
            secretId = product.Secrets.FirstOrDefault()?.Id;

        var assignment = new LicenseAssignment
        {
            ProductId = product.Id, SecretId = secretId,
            ClientName = dialog.ClientName, DeviceName = dialog.DeviceName, Notes = dialog.Notes
        };
        _vault.Assignments.Add(assignment);
        _vault.Audit.Add(new LicenseAuditEntry { Action = "Licence attribuée", ProductId = product.Id, AssignmentId = assignment.Id, Details = $"{dialog.ClientName} — {dialog.DeviceName}" });
        await SaveAndRefreshAsync();
    }

    private async void ReleaseButton_Click(object? sender, EventArgs e)
    {
        var assignment = SelectedAssignment();
        if (assignment is null) return;
        if (MessageBox.Show(this, $"Libérer la licence attribuée à {assignment.ClientName} / {assignment.DeviceName} ?",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        assignment.ReleasedAtUtc = DateTime.UtcNow;
        _vault.Audit.Add(new LicenseAuditEntry { Action = "Licence libérée", ProductId = assignment.ProductId, AssignmentId = assignment.Id, Details = $"{assignment.ClientName} — {assignment.DeviceName}" });
        await SaveAndRefreshAsync();
    }

    private void OpenPortalButton_Click(object? sender, EventArgs e)
    {
        var product = SelectedProduct();
        if (product?.VendorPortalUrl is not { Length: > 0 } url || !Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            MessageBox.Show(this, "Aucun portail HTTPS valide n'est configuré pour ce produit.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }

    private async Task SaveAndRefreshAsync()
    {
        try
        {
            await _service.SaveAsync(_vault);
            RefreshProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Le coffre n'a pas pu être enregistré : {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string TypeLabel(LicenseType type) => type switch
    {
        LicenseType.IndividualKeys => "Clés individuelles",
        LicenseType.MultiSeatKey => "Multi-postes",
        LicenseType.VendorManaged => "Portail éditeur",
        _ => "Libre / support"
    };

    private sealed record ProductRow(Guid ProductId, string Produit, string Edition, string Type, int Total, int Utilisées, int Disponibles, string Expiration, string État);
    private sealed record AssignmentRow(Guid AssignmentId, string Client, string Poste, string Attribution, string État);
}
