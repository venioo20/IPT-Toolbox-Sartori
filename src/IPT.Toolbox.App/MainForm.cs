using IPT.Toolbox.Core.Models;
using IPT.Toolbox.Core.Services;

namespace IPT.Toolbox.App;

public partial class MainForm : Form
{
    private readonly CatalogService _catalogService = new();
    private readonly MachineInspector _machineInspector = new();
    private readonly PackageDetector _packageDetector = new();
    private readonly ProLicenseService _proLicenseService = new(AppPaths.ProLicenseFile);
    private readonly CustomPackageService _customPackageService = new(AppPaths.CustomPackagesFile);
    private readonly LogService _logger;
    private IReadOnlyList<PackageDefinition> _packages = [];
    private IReadOnlyList<ProfileDefinition> _profiles = [];
    private CancellationTokenSource? _runCts;
    private ProLicenseStatus _proStatus = new(false, "IPT Toolbox Pro n’est pas activé.");

    public MainForm()
    {
        InitializeComponent();
        _logger = new LogService(AppPaths.LogDirectory);
        Text = "IPT Toolbox Sartori — Informatique Pour Tous";
        var version = typeof(MainForm).Assembly.GetName().Version;
        versionLabel.Text = $"Version {version?.ToString(3) ?? "0.3.0"}";
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            AppendLog("Chargement d'IPT Toolbox Sartori…");

            var builtInPackages = await _catalogService.LoadPackagesAsync(AppPaths.PackagesFile);
            var customPackages = await _customPackageService.LoadAsync();
            _packages = builtInPackages.Concat(customPackages).OrderBy(p => p.Category).ThenBy(p => p.Name).ToList();
            _profiles = await _catalogService.LoadProfilesAsync(AppPaths.ProfilesFile);


            packagesCheckedListBox.Items.Clear();
            foreach (var package in _packages)
                packagesCheckedListBox.Items.Add(package, package.EnabledByDefault);

            profileComboBox.DataSource = _profiles.ToList();
            packagesGroupBox.Text = $"Logiciels ({_packages.Count}) — par catégorie";

            var machine = await _machineInspector.InspectAsync();
            machineInfoLabel.Text = $"{machine.WindowsDescription} | {machine.Architecture} | Disque libre : {machine.FreeDiskGb} Go | WinGet : {(machine.WingetAvailable ? "OK" : "absent")}";
            AppendLog("Analyse du PC terminée.");
            AppendLog($"Journal : {_logger.LogFile}");
            await RefreshProStateAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"Erreur au démarrage : {ex.Message}");
            MessageBox.Show(this, ex.Message, "IPT Toolbox Sartori", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void profileComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (profileComboBox.SelectedItem is not ProfileDefinition profile)
            return;

        for (var i = 0; i < packagesCheckedListBox.Items.Count; i++)
        {
            if (packagesCheckedListBox.Items[i] is PackageDefinition package)
                packagesCheckedListBox.SetItemChecked(i, profile.PackageIds.Contains(package.Id, StringComparer.OrdinalIgnoreCase));
        }

        profileDescriptionLabel.Text = profile.Description;
    }

    private async void prepareButton_Click(object? sender, EventArgs e)
    {
        if (_runCts is not null)
            return;

        var selected = packagesCheckedListBox.CheckedItems.Cast<PackageDefinition>().ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show(this, "Sélectionne au moins un logiciel.", "IPT Toolbox Sartori", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _runCts = new CancellationTokenSource();
        SetBusy(true);
        progressBar.Minimum = 0;
        progressBar.Maximum = selected.Count;
        progressBar.Value = 0;

        try
        {
            AppendLog(executeCheckBox.Checked
                ? "Mode EXÉCUTION activé."
                : "Mode SIMULATION : aucune installation réelle ne sera lancée.");

            if (createRestorePointCheckBox.Checked)
                AppendLog("Point de restauration : prévu pour le module Windows (non exécuté dans cette version). ");
            if (privacyCheckBox.Checked)
                AppendLog("Confidentialité : prévue pour le module Windows (non exécutée dans cette version). ");
            if (windowsUpdateCheckBox.Checked)
                AppendLog("Windows Update : vérification prévue pour une version ultérieure.");

            var service = new InstallationQueueService(_packageDetector, _logger);
            var progress = new Progress<(int Current, int Total, string Message)>(p =>
            {
                progressBar.Maximum = Math.Max(1, p.Total);
                progressBar.Value = Math.Min(p.Current, progressBar.Maximum);
                statusLabel.Text = p.Message;
                AppendLog(p.Message);
            });

            var results = await service.InstallAsync(selected, executeCheckBox.Checked, progress, _runCts.Token);

            foreach (var result in results)
            {
                var symbol = result.Success ? "✓" : "✗";
                AppendLog($"{symbol} {result.PackageName} — {result.Message}");
            }

            var success = results.Count(r => r.Success);
            statusLabel.Text = $"Terminé : {success}/{results.Count} traité(s) avec succès.";
        }
        catch (OperationCanceledException)
        {
            AppendLog("Opération annulée.");
            statusLabel.Text = "Annulé";
        }
        catch (Exception ex)
        {
            AppendLog($"Erreur : {ex.Message}");
            MessageBox.Show(this, ex.Message, "IPT Toolbox Sartori", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _runCts.Dispose();
            _runCts = null;
            SetBusy(false);
        }
    }

    private void cancelButton_Click(object? sender, EventArgs e) => _runCts?.Cancel();

    private void licensesButton_Click(object? sender, EventArgs e)
    {
        if (!_proStatus.HasFeature("license-vault"))
        {
            OpenProAccess();
            return;
        }
        using var form = new LicenseManagerForm();
        form.ShowDialog(this);
    }

    private void proAccessButton_Click(object? sender, EventArgs e) => OpenProAccess();

    private async void OpenProAccess()
    {
        using var form = new ProAccessForm();
        form.ShowDialog(this);
        await RefreshProStateAsync();
    }

    private async void customPackageButton_Click(object? sender, EventArgs e)
    {
        if (!_proStatus.HasFeature("custom-packages"))
        {
            OpenProAccess();
            return;
        }
        using var dialog = new CustomPackageDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Package is null) return;
        try
        {
            await _customPackageService.AddAsync(dialog.Package);
            var updated = _packages.Append(dialog.Package).OrderBy(p => p.Category).ThenBy(p => p.Name).ToList();
            _packages = updated;
            packagesCheckedListBox.Items.Clear();
            foreach (var package in _packages) packagesCheckedListBox.Items.Add(package, false);
            packagesGroupBox.Text = $"Logiciels ({_packages.Count}) — par catégorie";
            AppendLog($"Logiciel personnalisé ajouté : {dialog.Package.Name} ({dialog.Package.WingetId}).");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            MessageBox.Show(this, ex.Message, "Logiciel personnalisé", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task RefreshProStateAsync()
    {
        _proStatus = await _proLicenseService.LoadStatusAsync();
        licensesButton.Enabled = _proStatus.HasFeature("license-vault");
        customPackageButton.Enabled = _proStatus.HasFeature("custom-packages");
        licensesButton.Text = _proStatus.IsValid ? "LICENCES PRO" : "LICENCES PRO 🔒";
        proAccessButton.Text = _proStatus.IsValid ? _proStatus.Message.ToUpperInvariant() : "ACHETER / ACTIVER PRO";
        AppendLog(_proStatus.Message);
    }

    private void SetBusy(bool busy)
    {
        prepareButton.Enabled = !busy;
        cancelButton.Enabled = busy;
        profileComboBox.Enabled = !busy;
        packagesCheckedListBox.Enabled = !busy;
        executeCheckBox.Enabled = !busy;
    }

    private void AppendLog(string message)
    {
        logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        logTextBox.SelectionStart = logTextBox.TextLength;
        logTextBox.ScrollToCaret();
    }
}

