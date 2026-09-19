namespace IPT.Toolbox.Core.Models;

public sealed class ProLicense
{
    public string LicenseId { get; set; } = Guid.NewGuid().ToString("N");
    public string CustomerName { get; set; } = string.Empty;
    public string Plan { get; set; } = "monthly";
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string[] Features { get; set; } = ["license-vault", "custom-packages"];
}

public sealed record ProLicenseStatus(bool IsValid, string Message, ProLicense? License = null)
{
    public bool HasFeature(string feature) => IsValid &&
        License?.Features.Contains(feature, StringComparer.OrdinalIgnoreCase) == true;
}
