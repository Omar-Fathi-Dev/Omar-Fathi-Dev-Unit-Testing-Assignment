using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class PhysicalDelayProvider : IDelayProvider
{
    public void Delay(int milliseconds)
    {
        Thread.Sleep(milliseconds);
    }
}