using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.Logging;

public class AuditLog : IAuditLog
{
    private readonly IFileSystem _fileSystem;
    private readonly TimeProvider _timeProvider;

    public AuditLog(
        IFileSystem fileSystem,
        TimeProvider timeProvider)
    {
        _fileSystem = fileSystem;
        _timeProvider = timeProvider;
    }

    public void Write(string message)
    {
        string path = _fileSystem.Combine(
            _fileSystem.BaseDirectory,
            "audit.log");

        DateTime now = _timeProvider.GetLocalNow().DateTime;

        string content = $"[{now:O}] {message}{Environment.NewLine}";

        _fileSystem.AppendAllText(path, content);
    }
}