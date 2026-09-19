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
