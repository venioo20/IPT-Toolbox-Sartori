using System.Diagnostics;
using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.Core.Services;

public sealed class PackageDetector
{
    public async Task<bool> IsInstalledAsync(PackageDefinition package, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(package.DetectionExecutable))
        {
            var paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (paths.Any(path => File.Exists(Path.Combine(path, package.DetectionExecutable))))
                return true;
        }

        if (!string.IsNullOrWhiteSpace(package.WingetId))
            return await IsInstalledViaWingetAsync(package.WingetId, cancellationToken);

        return false;
    }

    private static async Task<bool> IsInstalledViaWingetAsync(string packageId, CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = WingetArguments.Create(packageId, install: false)
            };

            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var output = await outputTask;
            await errorTask;
            return process.ExitCode == 0 && output.Contains(packageId, StringComparison.OrdinalIgnoreCase);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { throw new InvalidOperationException("Détection WinGet impossible ; installation interrompue.", ex); }
    }
}
