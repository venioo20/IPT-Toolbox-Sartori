namespace IPT.Toolbox.Core.Models;

public enum LicenseType
{
    IndividualKeys,
    MultiSeatKey,
    VendorManaged,
    OpenSourceSupport
}

public sealed class LicenseVault
{
    public int Version { get; set; } = 1;
    public List<LicensedProduct> Products { get; set; } = [];
    public List<LicenseAssignment> Assignments { get; set; } = [];
    public List<LicenseAuditEntry> Audit { get; set; } = [];
}

public sealed class LicensedProduct
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Edition { get; set; } = string.Empty;
    public LicenseType Type { get; set; }
    public int SeatsOwned { get; set; }
    public int WarningThreshold { get; set; } = 2;
    public DateTime? ExpiresOn { get; set; }
    public string? VendorPortalUrl { get; set; }
    public string? Notes { get; set; }
    public List<LicenseSecret> Secrets { get; set; } = [];
}

public sealed class LicenseSecret
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Value { get; set; } = string.Empty;
    public string? Label { get; set; }
}

public sealed class LicenseAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid? SecretId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class LicenseAuditEntry
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid? AssignmentId { get; set; }
    public string Details { get; set; } = string.Empty;
}

public sealed record LicenseProductSummary(
    Guid ProductId,
    string Name,
    string Edition,
    LicenseType Type,
    int Total,
    int Used,
    int Available,
    DateTime? ExpiresOn,
    string Status);
