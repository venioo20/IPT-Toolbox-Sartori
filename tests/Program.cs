using IPT.Toolbox.Core.Services;

var root = Path.GetFullPath(args[0]);
var catalog = new CatalogService();
var packages = await catalog.LoadPackagesAsync(Path.Combine(root, "config", "packages.json"));
var profiles = await catalog.LoadProfilesAsync(Path.Combine(root, "config", "profiles.json"));
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    Console.WriteLine("OK: " + name);
}
Check(packages.Count == 50, "50 logiciels");
Check(packages.Select(p => p.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 50, "Identifiants uniques");
Check(packages.Select(p => p.WingetId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 50, "Identifiants WinGet uniques");
Check(packages.All(p => !p.EnabledByDefault), "Aucune présélection");
foreach (var id in new[] { "X.Y\" --override bad", "--help", "X Y", "X.Y\n", "", "X/Y" })
{
    try { WingetArguments.Create(id, true); throw new Exception("Identifiant dangereux accepté"); }
    catch (ArgumentException) { }
}
Check(packages.All(p => WingetArguments.Create(p.WingetId!, true).ArgumentList[2] == p.WingetId), "Arguments WinGet séparés et identifiants malformés refusés");
Check(profiles.Count == 8 && profiles[0].PackageIds.Count == 0, "8 profils, premier profil vide");
Check(profiles.SelectMany(p => p.PackageIds).All(id => packages.Any(p => p.Id == id)), "Profils cohérents");
// Un PATH vide rend WinGet inaccessible : la simulation doit rester fonctionnelle.
var path = Environment.GetEnvironmentVariable("PATH");
try
{
    Environment.SetEnvironmentVariable("PATH", "");
    var service = new InstallationQueueService(new PackageDetector(), new LogService(Path.Combine(root, "test-results")));
    var results = await service.InstallAsync(packages, false);
    Check(results.Count == 50 && results.All(r => r.Success && r.Skipped && r.Message.StartsWith("Simulation")), "Simulation des 50 logiciels sans WinGet");
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    try { await service.InstallAsync(packages, false, cancellationToken: cts.Token); throw new Exception("Annulation ignorée"); }
    catch (OperationCanceledException) { Console.WriteLine("OK: annulation de la simulation"); }
}
finally { Environment.SetEnvironmentVariable("PATH", path); }

var vaultPath = Path.Combine(root, "test-results", "license-vault-test.dat");
Directory.CreateDirectory(Path.GetDirectoryName(vaultPath)!);
var vaultService = new LicenseVaultService(vaultPath, new TestProtector());
var product = new IPT.Toolbox.Core.Models.LicensedProduct
{
    Name = "Produit de test", Edition = "Technicien",
    Type = IPT.Toolbox.Core.Models.LicenseType.IndividualKeys,
    SeatsOwned = 2, WarningThreshold = 1,
    Secrets = [new() { Value = "CLE-SECRETE-001" }, new() { Value = "CLE-SECRETE-002" }]
};
var vault = new IPT.Toolbox.Core.Models.LicenseVault { Products = [product] };
vault.Assignments.Add(new IPT.Toolbox.Core.Models.LicenseAssignment
{
    ProductId = product.Id, SecretId = product.Secrets[0].Id,
    ClientName = "Client test", DeviceName = "PC-TEST"
});
await vaultService.SaveAsync(vault);
var rawVault = await File.ReadAllBytesAsync(vaultPath);
Check(!System.Text.Encoding.UTF8.GetString(rawVault).Contains("CLE-SECRETE"), "Le coffre ne contient pas les clés en clair");
var loadedVault = await vaultService.LoadAsync();
Check(loadedVault.Products[0].Secrets[0].Value == "CLE-SECRETE-001", "Lecture du coffre chiffré");
var licenseSummary = LicenseVaultService.GetSummaries(loadedVault).Single();
Check(licenseSummary.Total == 2 && licenseSummary.Used == 1 && licenseSummary.Available == 1 && licenseSummary.Status == "Stock faible", "Compteurs et alerte de stock");
loadedVault.Assignments[0].ReleasedAtUtc = DateTime.UtcNow;
Check(LicenseVaultService.GetSummaries(loadedVault).Single().Available == 2, "Restitution d'une licence");
if (OperatingSystem.IsWindows())
{
    var dpapi = new WindowsDpapiProtector();
    var clearSecret = System.Text.Encoding.UTF8.GetBytes("secret-dpapi-test");
    var protectedSecret = dpapi.Protect(clearSecret);
    Check(!protectedSecret.SequenceEqual(clearSecret), "DPAPI chiffre les données");
    Check(dpapi.Unprotect(protectedSecret).SequenceEqual(clearSecret), "DPAPI déchiffre pour l'utilisateur Windows courant");
    Array.Clear(clearSecret);
    Array.Clear(protectedSecret);
}

var proService = new ProLicenseService(Path.Combine(root, "test-results", "pro-test.license"));
Check(!proService.Verify("not-a-license").IsValid, "Une fausse licence Pro est refusée");
if (args.Length > 1)
{
    var signedToken = await File.ReadAllTextAsync(args[1]);
    var proStatus = proService.Verify(signedToken);
    Check(proStatus.IsValid && proStatus.HasFeature("license-vault") && proStatus.HasFeature("custom-packages"), "Licence Pro signée et fonctions autorisées");
}

sealed class TestProtector : IDataProtector
{
    public byte[] Protect(byte[] clearData) => clearData.Select(x => (byte)(x ^ 0xA5)).ToArray();
    public byte[] Unprotect(byte[] protectedData) => Protect(protectedData);
}
