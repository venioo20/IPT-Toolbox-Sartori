using System.Text.Json;
using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.Core.Services;

public sealed class LicenseVaultService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
    private readonly string _path;
    private readonly IDataProtector _protector;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public LicenseVaultService(string path, IDataProtector protector)
    {
        _path = path;
        _protector = protector;
    }

    public async Task<LicenseVault> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_path)) return new LicenseVault();
            var encrypted = await File.ReadAllBytesAsync(_path, cancellationToken);
            var clear = _protector.Unprotect(encrypted);
            try
            {
                return JsonSerializer.Deserialize<LicenseVault>(clear, JsonOptions)
                       ?? throw new InvalidDataException("Le coffre de licences est vide ou invalide.");
            }
            finally { Array.Clear(clear); }
        }
        finally { _gate.Release(); }
    }

    public async Task SaveAsync(LicenseVault vault, CancellationToken cancellationToken = default)
    {
        Validate(vault);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var directory = Path.GetDirectoryName(_path)!;
            Directory.CreateDirectory(directory);
            var clear = JsonSerializer.SerializeToUtf8Bytes(vault, JsonOptions);
            try
            {
                var encrypted = _protector.Protect(clear);
                var temporary = _path + ".tmp";
                await File.WriteAllBytesAsync(temporary, encrypted, cancellationToken);
                File.Move(temporary, _path, overwrite: true);
            }
            finally { Array.Clear(clear); }
        }
        finally { _gate.Release(); }
    }

    public static IReadOnlyList<LicenseProductSummary> GetSummaries(LicenseVault vault, DateTime? today = null)
    {
        var date = (today ?? DateTime.Today).Date;
        return vault.Products.Select(product =>
        {
            var used = vault.Assignments.Count(a => a.ProductId == product.Id && a.ReleasedAtUtc is null);
            var total = product.Type == LicenseType.OpenSourceSupport ? 0 : product.SeatsOwned;
            var available = Math.Max(0, total - used);
            var status = product.Type == LicenseType.OpenSourceSupport ? "Support suivi" :
                product.ExpiresOn is { } expiration && expiration.Date < date ? "Expirée" :
                product.ExpiresOn is { } soon && soon.Date <= date.AddDays(30) ? "Expire bientôt" :
                available == 0 ? "Épuisée" :
                available <= product.WarningThreshold ? "Stock faible" : "Disponible";
            return new LicenseProductSummary(product.Id, product.Name, product.Edition, product.Type, total, used, available, product.ExpiresOn, status);
        }).OrderBy(x => x.Name).ToList();
    }

    public static void Validate(LicenseVault vault)
    {
        if (vault.Version != 1) throw new InvalidDataException("Version de coffre non prise en charge.");
        if (vault.Products.Select(p => p.Id).Distinct().Count() != vault.Products.Count)
            throw new InvalidDataException("Produits de licence dupliqués.");
        foreach (var product in vault.Products)
        {
            if (string.IsNullOrWhiteSpace(product.Name)) throw new InvalidDataException("Le nom du produit est obligatoire.");
            if (product.SeatsOwned < 0 || product.WarningThreshold < 0) throw new InvalidDataException("Quantité de licences invalide.");
            if (product.Secrets.Select(s => s.Id).Distinct().Count() != product.Secrets.Count)
                throw new InvalidDataException("Clés de licence dupliquées.");
            if (product.Type == LicenseType.IndividualKeys && product.Secrets.Count > product.SeatsOwned)
                throw new InvalidDataException("Le nombre de clés dépasse le nombre de licences achetées.");
        }
        foreach (var assignment in vault.Assignments)
        {
            var product = vault.Products.SingleOrDefault(p => p.Id == assignment.ProductId)
                          ?? throw new InvalidDataException("Attribution associée à un produit inconnu.");
            if (string.IsNullOrWhiteSpace(assignment.ClientName) || string.IsNullOrWhiteSpace(assignment.DeviceName))
                throw new InvalidDataException("Le client et le poste sont obligatoires.");
            if (assignment.SecretId is { } secretId && product.Secrets.All(s => s.Id != secretId))
                throw new InvalidDataException("Clé attribuée inconnue.");
        }
        var active = vault.Assignments.Where(a => a.ReleasedAtUtc is null).ToList();
        foreach (var product in vault.Products.Where(p => p.Type != LicenseType.OpenSourceSupport))
            if (active.Count(a => a.ProductId == product.Id) > product.SeatsOwned)
                throw new InvalidDataException($"Le nombre d'attributions dépasse le stock pour {product.Name}.");
        if (active.Where(a => a.SecretId.HasValue).GroupBy(a => a.SecretId).Any(g => g.Count() > 1))
            throw new InvalidDataException("Une clé individuelle est attribuée à plusieurs postes.");
    }
}
