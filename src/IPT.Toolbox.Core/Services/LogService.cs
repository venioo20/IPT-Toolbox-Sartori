namespace IPT.Toolbox.Core.Services;

public sealed class LogService
{
    private readonly string _logFile;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public LogService(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        _logFile = Path.Combine(logDirectory, $"ipt-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    }

    public string LogFile => _logFile;

    public async Task WriteAsync(string message, CancellationToken cancellationToken = default)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(_logFile, line, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
