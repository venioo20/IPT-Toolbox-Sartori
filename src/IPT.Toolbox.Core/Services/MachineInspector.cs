using System.Diagnostics;
using System.Runtime.InteropServices;
using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.Core.Services;

public sealed class MachineInspector
{
    public async Task<MachineInfo> InspectAsync(CancellationToken cancellationToken = default)
    {
        var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!);

        return new MachineInfo
        {
            WindowsDescription = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            TotalMemoryMb = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024,
            FreeDiskGb = drive.AvailableFreeSpace / 1024 / 1024 / 1024,
            WingetAvailable = await CommandExistsAsync("winget", cancellationToken)
        };
    }

    private static async Task<bool> CommandExistsAsync(string command, CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
