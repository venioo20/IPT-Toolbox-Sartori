namespace IPT.Toolbox.Core.Models;

public sealed record InstallResult(
    string PackageId,
    string PackageName,
    bool Success,
    bool Skipped,
    int ExitCode,
    string Message);
