using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class RandomProvider : IRandomProvider
{
    private readonly Random _random = new Random();

    public int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }
}