namespace IPT.Toolbox.Core.Models;

public sealed class MachineInfo
{
    public string ComputerName { get; init; } = Environment.MachineName;
    public string WindowsDescription { get; init; } = string.Empty;
    public string Architecture { get; init; } = string.Empty;
    public long TotalMemoryMb { get; init; }
    public long FreeDiskGb { get; init; }
    public bool WingetAvailable { get; init; }
}
