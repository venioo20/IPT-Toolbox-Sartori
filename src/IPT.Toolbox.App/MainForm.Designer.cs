#nullable enable
namespace IPT.Toolbox.App;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private Label titleLabel = null!;
    private Label subtitleLabel = null!;
    private GroupBox machineGroupBox = null!;
    private Label machineInfoLabel = null!;
    private GroupBox profileGroupBox = null!;
    private ComboBox profileComboBox = null!;
    private Label profileDescriptionLabel = null!;
    private GroupBox packagesGroupBox = null!;
    private CheckedListBox packagesCheckedListBox = null!;
    private GroupBox optionsGroupBox = null!;
    private CheckBox createRestorePointCheckBox = null!;
    private CheckBox privacyCheckBox = null!;
    private CheckBox windowsUpdateCheckBox = null!;
    private CheckBox executeCheckBox = null!;
    private Button prepareButton = null!;
    private Button cancelButton = null!;
    private ProgressBar progressBar = null!;
    private Label statusLabel = null!;
    private TextBox logTextBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        titleLabel = new Label();
        subtitleLabel = new Label();
        machineGroupBox = new GroupBox();
        machineInfoLabel = new Label();
        profileGroupBox = new GroupBox();
        profileComboBox = new ComboBox();
        profileDescriptionLabel = new Label();
        packagesGroupBox = new GroupBox();
        packagesCheckedListBox = new CheckedListBox();
        optionsGroupBox = new GroupBox();
        createRestorePointCheckBox = new CheckBox();
        privacyCheckBox = new CheckBox();
        windowsUpdateCheckBox = new CheckBox();
        executeCheckBox = new CheckBox();
        prepareButton = new Button();
        cancelButton = new Button();
        progressBar = new ProgressBar();
        statusLabel = new Label();
        logTextBox = new TextBox();
        machineGroupBox.SuspendLayout();
        profileGroupBox.SuspendLayout();
        packagesGroupBox.SuspendLayout();
        optionsGroupBox.SuspendLayout();
        SuspendLayout();
        // titleLabel
        titleLabel.AutoSize = true;
        titleLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        titleLabel.Location = new Point(24, 18);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(232, 45);
        titleLabel.Text = "IPT TOOLBOX SARTORI";
        // subtitleLabel
        subtitleLabel.AutoSize = true;
        subtitleLabel.Font = new Font("Segoe UI", 10F);
        subtitleLabel.Location = new Point(29, 65);
        subtitleLabel.Name = "subtitleLabel";
        subtitleLabel.Size = new Size(151, 19);
        subtitleLabel.Text = "Informatique Pour Tous";
        // machineGroupBox
        machineGroupBox.Controls.Add(machineInfoLabel);
        machineGroupBox.Location = new Point(24, 100);
        machineGroupBox.Name = "machineGroupBox";
        machineGroupBox.Size = new Size(952, 70);
        machineGroupBox.Text = "Analyse du PC";
        // machineInfoLabel
        machineInfoLabel.AutoEllipsis = true;
        machineInfoLabel.Location = new Point(16, 29);
        machineInfoLabel.Name = "machineInfoLabel";
        machineInfoLabel.Size = new Size(920, 23);
        machineInfoLabel.Text = "Analyse en cours…";
        // profileGroupBox
        profileGroupBox.Controls.Add(profileComboBox);
        profileGroupBox.Controls.Add(profileDescriptionLabel);
        profileGroupBox.Location = new Point(24, 184);
        profileGroupBox.Name = "profileGroupBox";
        profileGroupBox.Size = new Size(454, 110);
        profileGroupBox.Text = "Profil";
        // profileComboBox
        profileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        profileComboBox.Location = new Point(16, 28);
        profileComboBox.Name = "profileComboBox";
        profileComboBox.Size = new Size(410, 23);
        profileComboBox.SelectedIndexChanged += profileComboBox_SelectedIndexChanged;
        // profileDescriptionLabel
        profileDescriptionLabel.Location = new Point(16, 59);
        profileDescriptionLabel.Name = "profileDescriptionLabel";
        profileDescriptionLabel.Size = new Size(410, 42);
        // packagesGroupBox
        packagesGroupBox.Controls.Add(packagesCheckedListBox);
        packagesGroupBox.Location = new Point(24, 308);
        packagesGroupBox.Name = "packagesGroupBox";
        packagesGroupBox.Size = new Size(454, 392);
        packagesGroupBox.Text = "Logiciels";
        // packagesCheckedListBox
        packagesCheckedListBox.HorizontalScrollbar = true;
        packagesCheckedListBox.CheckOnClick = true;
        packagesCheckedListBox.Dock = DockStyle.Fill;
        packagesCheckedListBox.FormattingEnabled = true;
        packagesCheckedListBox.IntegralHeight = false;
        packagesCheckedListBox.Name = "packagesCheckedListBox";
        packagesCheckedListBox.Padding = new Padding(8);
        // optionsGroupBox
        optionsGroupBox.Controls.Add(createRestorePointCheckBox);
        optionsGroupBox.Controls.Add(privacyCheckBox);
        optionsGroupBox.Controls.Add(windowsUpdateCheckBox);
        optionsGroupBox.Controls.Add(executeCheckBox);
        optionsGroupBox.Location = new Point(500, 184);
        optionsGroupBox.Name = "optionsGroupBox";
        optionsGroupBox.Size = new Size(476, 182);
        optionsGroupBox.Text = "Options";
        // createRestorePointCheckBox
        createRestorePointCheckBox.AutoSize = true;
        createRestorePointCheckBox.Checked = false;
        createRestorePointCheckBox.Enabled = false;
        createRestorePointCheckBox.CheckState = CheckState.Unchecked;
        createRestorePointCheckBox.Location = new Point(20, 31);
        createRestorePointCheckBox.Text = "Point de restauration (bientôt)";
        // privacyCheckBox
        privacyCheckBox.AutoSize = true;
        privacyCheckBox.Checked = false;
        privacyCheckBox.Enabled = false;
        privacyCheckBox.CheckState = CheckState.Unchecked;
        privacyCheckBox.Location = new Point(20, 62);
        privacyCheckBox.Text = "Confidentialité Windows (bientôt)";
        // windowsUpdateCheckBox
        windowsUpdateCheckBox.AutoSize = true;
        windowsUpdateCheckBox.Checked = false;
        windowsUpdateCheckBox.Enabled = false;
        windowsUpdateCheckBox.CheckState = CheckState.Unchecked;
        windowsUpdateCheckBox.Location = new Point(20, 93);
        windowsUpdateCheckBox.Text = "Windows Update (bientôt)";
        // executeCheckBox
        executeCheckBox.Checked = false;
        executeCheckBox.AutoSize = true;
        executeCheckBox.Location = new Point(20, 136);
        executeCheckBox.Text = "MODE EXÉCUTION : autoriser les installations réelles";
        // prepareButton
        prepareButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        prepareButton.Location = new Point(500, 382);
        prepareButton.Name = "prepareButton";
        prepareButton.Size = new Size(350, 52);
        prepareButton.Text = "PRÉPARER CE PC";
        prepareButton.UseVisualStyleBackColor = true;
        prepareButton.Click += prepareButton_Click;
        // cancelButton
        cancelButton.Enabled = false;
        cancelButton.Location = new Point(862, 382);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(114, 52);
        cancelButton.Text = "Annuler";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += cancelButton_Click;
        // progressBar
        progressBar.Location = new Point(500, 451);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(476, 23);
        // statusLabel
        statusLabel.Location = new Point(500, 484);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(476, 24);
        statusLabel.Text = "Prêt";
        // logTextBox
        logTextBox.Font = new Font("Consolas", 9F);
        logTextBox.Location = new Point(500, 520);
        logTextBox.Multiline = true;
        logTextBox.Name = "logTextBox";
        logTextBox.ReadOnly = true;
        logTextBox.ScrollBars = ScrollBars.Vertical;
        logTextBox.Size = new Size(476, 180);
        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 724);
        Controls.Add(logTextBox);
        Controls.Add(statusLabel);
        Controls.Add(progressBar);
        Controls.Add(cancelButton);
        Controls.Add(prepareButton);
        Controls.Add(optionsGroupBox);
        Controls.Add(packagesGroupBox);
        Controls.Add(profileGroupBox);
        Controls.Add(machineGroupBox);
        Controls.Add(subtitleLabel);
        Controls.Add(titleLabel);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimumSize = new Size(1016, 763);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "IPT Toolbox Sartori — Informatique Pour Tous";
        Load += MainForm_Load;
        machineGroupBox.ResumeLayout(false);
        profileGroupBox.ResumeLayout(false);
        packagesGroupBox.ResumeLayout(false);
        optionsGroupBox.ResumeLayout(false);
        optionsGroupBox.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}

