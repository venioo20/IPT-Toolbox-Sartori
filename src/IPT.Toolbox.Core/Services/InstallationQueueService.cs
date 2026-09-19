using System.Diagnostics;
using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.Core.Services;

public sealed class InstallationQueueService
{
    private readonly PackageDetector _detector;
    private readonly LogService _logger;

    public InstallationQueueService(PackageDetector detector, LogService logger)
    {
        _detector = detector;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InstallResult>> InstallAsync(
        IEnumerable<PackageDefinition> packages,
        bool execute,
        IProgress<(int Current, int Total, string Message)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var queue = packages.ToList();
        var results = new List<InstallResult>(queue.Count);

        for (var index = 0; index < queue.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var package = queue[index];
            progress?.Report((index + 1, queue.Count, $"Vérification : {package.Name}"));

            if (execute && await _detector.IsInstalledAsync(package, cancellationToken))
            {
                var already = new InstallResult(package.Id, package.Name, true, true, 0, "Déjà installé");
                results.Add(already);
                await _logger.WriteAsync($"{package.Name}: déjà installé", cancellationToken);
                continue;
            }

            if (!execute)
            {
                var simulated = new InstallResult(package.Id, package.Name, true, true, 0, "Simulation : installation non exécutée");
                results.Add(simulated);
                await _logger.WriteAsync($"{package.Name}: simulation", cancellationToken);
                continue;
            }

            if (string.IsNullOrWhiteSpace(package.WingetId))
            {
                var missing = new InstallResult(package.Id, package.Name, false, false, -1, "Aucune méthode d'installation configurée");
                results.Add(missing);
                await _logger.WriteAsync($"{package.Name}: aucune méthode d'installation", cancellationToken);
                continue;
            }

            progress?.Report((index + 1, queue.Count, $"Installation : {package.Name}"));
            var result = await InstallWithWingetAsync(package, cancellationToken);
            results.Add(result);
            await _logger.WriteAsync($"{package.Name}: {result.Message} (code {result.ExitCode})", cancellationToken);
        }

        return results;
    }

    private static async Task<InstallResult> InstallWithWingetAsync(PackageDefinition package, CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = WingetArguments.Create(package.WingetId!, install: true)
            };

            process.Start();
            var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var stdOut = await stdOutTask;
            var stdErr = await stdErrTask;

            var success = process.ExitCode == 0;
            var message = success ? "Installation terminée" : (string.IsNullOrWhiteSpace(stdErr) ? stdOut.Trim() : stdErr.Trim());
            return new InstallResult(package.Id, package.Name, success, false, process.ExitCode, message);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new InstallResult(package.Id, package.Name, false, false, -1, ex.Message);
        }
    }
}
