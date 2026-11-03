namespace DriveNow.Application.Abstractions;

public interface IAuditLog
{
    void Write(string message);
}