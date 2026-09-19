using System.Text.Json;
using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.Core.Services;

public sealed class CatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public async Task<IReadOnlyList<PackageDefinition>> LoadPackagesAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        var root = await JsonSerializer.DeserializeAsync<CatalogRoot>(stream, JsonOptions, cancellationToken)
                   ?? new CatalogRoot();
        foreach (var package in root.Packages)
            WingetArguments.ValidateId(package.WingetId);
        if (root.Packages.Select(p => p.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != root.Packages.Count)
            throw new InvalidDataException("Identifiants de logiciels dupliqués.");
        return root.Packages;
    }

    public async Task<IReadOnlyList<ProfileDefinition>> LoadProfilesAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        var root = await JsonSerializer.DeserializeAsync<ProfileRoot>(stream, JsonOptions, cancellationToken)
                   ?? new ProfileRoot();
        return root.Profiles;
    }
}
