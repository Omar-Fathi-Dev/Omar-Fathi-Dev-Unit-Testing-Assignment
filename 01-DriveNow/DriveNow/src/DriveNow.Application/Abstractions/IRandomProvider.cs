namespace DriveNow.Application.Abstractions;

public interface IRandomProvider
{
    int Next(int maxValue);
}