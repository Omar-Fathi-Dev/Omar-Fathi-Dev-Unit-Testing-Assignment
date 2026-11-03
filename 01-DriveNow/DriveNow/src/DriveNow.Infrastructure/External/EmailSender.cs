using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class EmailSender : IEmailSender
{
    private readonly IFileSystem _fileSystem;
    private readonly TimeProvider _timeProvider;

    public EmailSender(
        IFileSystem fileSystem,
        TimeProvider timeProvider)
    {
        _fileSystem = fileSystem;
        _timeProvider = timeProvider;
    }

    public void Send(string to, string subject, string body)
    {
        string folder = _fileSystem.Combine(
            _fileSystem.BaseDirectory,
            "outbox");

        _fileSystem.CreateDirectory(folder);

        DateTime now = _timeProvider.GetLocalNow().DateTime;

        string path = _fileSystem.Combine(
            folder,
            $"{now:yyyyMMdd_HHmmssfff}_{to}.txt");

        string content =
            $"To: {to}{Environment.NewLine}" +
            $"Subject: {subject}{Environment.NewLine}" +
            $"{Environment.NewLine}{body}";

        _fileSystem.WriteAllText(path, content);
    }
}