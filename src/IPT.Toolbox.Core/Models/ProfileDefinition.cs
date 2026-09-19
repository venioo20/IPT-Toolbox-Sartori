namespace IPT.Toolbox.Core.Models;

public sealed class ProfileDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> PackageIds { get; set; } = [];

    public override string ToString() => Name;
}
