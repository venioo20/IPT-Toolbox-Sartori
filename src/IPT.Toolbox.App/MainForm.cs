using IPT.Toolbox.Core.Models;
using IPT.Toolbox.Core.Services;

namespace IPT.Toolbox.App;

public partial class MainForm : Form
{
    private readonly CatalogService _catalogService = new();
    private readonly MachineInspector _machineInspector = new();
    private readonly PackageDetector _packageDetector = new();
    private readonly LogService _logger;
    private IReadOnlyList<PackageDefinition> _packages = [];
    private IReadOnlyList<ProfileDefinition> _profiles = [];
    private CancellationTokenSource? _runCts;

    public MainForm()
    {
        InitializeComponent();
        _logger = new LogService(AppPaths.LogDirectory);
        Text = "IPT Toolbox Sartori — Informatique Pour Tous";
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

            _packages = (await _catalogService.LoadPackagesAsync(AppPaths.PackagesFile)).OrderBy(p => p.Category).ThenBy(p => p.Name).ToList();
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
                AppendLog("Point de restauration : prévu pour le module Windows (non exécuté dans v0.2). ");
            if (privacyCheckBox.Checked)
                AppendLog("Confidentialité : prévue pour le module Windows (non exécutée dans v0.2). ");
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

