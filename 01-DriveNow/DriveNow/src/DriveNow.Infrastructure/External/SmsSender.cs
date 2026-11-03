using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class SmsSender : ISmsSender
{
    private readonly IDelayProvider _delayProvider;
    private readonly IConsoleWriter _consoleWriter;

    public SmsSender(
        IDelayProvider delayProvider,
        IConsoleWriter consoleWriter)
    {
        _delayProvider = delayProvider;
        _consoleWriter = consoleWriter;
    }

    public void Send(string phone, string text)
    {
        _delayProvider.Delay(500);

        _consoleWriter.WriteLine($"[SMS to {phone}] {text}");
    }
}