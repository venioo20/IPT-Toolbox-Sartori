using System.Diagnostics;
using IPT.Toolbox.Core.Services;

namespace IPT.Toolbox.App;

internal sealed class ProAccessForm : Form
{
    private const string MonthlyUrl = "https://www.paypal.me/RaphaelSartori596/9.90EUR";
    private const string LifetimeUrl = "https://www.paypal.me/RaphaelSartori596/79.90EUR";
    private readonly ProLicenseService _service = new(AppPaths.ProLicenseFile);
    private readonly Label _status = new() { AutoSize = false, Height = 42, Dock = DockStyle.Top };
    private readonly TextBox _token = new() { Multiline = true, Height = 90, Dock = DockStyle.Top, ScrollBars = ScrollBars.Vertical };

    public ProAccessForm()
    {
        Text = "IPT Toolbox Pro — Acheter ou activer";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(590, 410);

        var intro = new Label
        {
            Text = "Choisissez une offre, effectuez le paiement sur PayPal puis demandez votre clé d’activation. Le paiement seul ne déverrouille pas automatiquement l’application.",
            AutoSize = false, Height = 58, Dock = DockStyle.Top
        };
        var monthly = new Button { Text = "MENSUEL — 9,90 €", Width = 250, Height = 46 };
        var lifetime = new Button { Text = "DÉFINITIF — 79,90 €", Width = 250, Height = 46 };
        monthly.Click += (_, _) => OpenPayment(MonthlyUrl);
        lifetime.Click += (_, _) => OpenPayment(LifetimeUrl);
        var offers = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 62, Controls = { monthly, lifetime } };
        var tokenLabel = new Label { Text = "Clé d’activation reçue après validation du paiement", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 10, 0, 5) };
        var activate = new Button { Text = "ACTIVER IPT TOOLBOX PRO", Height = 42, Dock = DockStyle.Top };
        activate.Click += Activate_Click;

        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
        panel.Controls.Add(_status);
        panel.Controls.Add(activate);
        panel.Controls.Add(_token);
        panel.Controls.Add(tokenLabel);
        panel.Controls.Add(offers);
        panel.Controls.Add(intro);
        Controls.Add(panel);
        Shown += async (_, _) => _status.Text = (await _service.LoadStatusAsync()).Message;
    }

    private static void OpenPayment(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private async void Activate_Click(object? sender, EventArgs e)
    {
        var result = await _service.ActivateAsync(_token.Text);
        _status.Text = result.Message;
        _status.ForeColor = result.IsValid ? Color.DarkGreen : Color.DarkRed;
        if (result.IsValid)
            MessageBox.Show(this, "IPT Toolbox Pro est maintenant activé.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
