namespace IPT.Toolbox.Core.Models;

public sealed class PackageDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Essentiel";
    public string? WingetId { get; set; }
    public string? DetectionExecutable { get; set; }
    public bool EnabledByDefault { get; set; }
    public string? Notes { get; set; }

    public override string ToString() => $"[{Category}] {Name}";
}

