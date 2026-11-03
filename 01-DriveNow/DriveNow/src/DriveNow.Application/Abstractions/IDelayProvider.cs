namespace DriveNow.Application.Abstractions;

public interface IDelayProvider
{
    void Delay(int milliseconds);
}