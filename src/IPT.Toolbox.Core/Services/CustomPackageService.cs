using System.Text.Json;
using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.Core.Services;

public sealed class CustomPackageService(string filePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<IReadOnlyList<PackageDefinition>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath)) return [];
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<PackageDefinition>>(stream, JsonOptions, cancellationToken) ?? [];
    }

    public async Task AddAsync(PackageDefinition package, CancellationToken cancellationToken = default)
    {
        _ = WingetArguments.Create(package.WingetId ?? string.Empty, true);
        var packages = (await LoadAsync(cancellationToken)).ToList();
        if (packages.Any(x => string.Equals(x.WingetId, package.WingetId, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ce logiciel personnalisé existe déjà.");
        packages.Add(package);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, packages, JsonOptions, cancellationToken);
    }
}
