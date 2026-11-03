using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class ConsoleWriter : IConsoleWriter
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
}