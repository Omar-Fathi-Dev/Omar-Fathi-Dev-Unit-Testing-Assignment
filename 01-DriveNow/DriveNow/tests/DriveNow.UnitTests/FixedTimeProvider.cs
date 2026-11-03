public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _fixedTime;

    public FixedTimeProvider(DateTimeOffset fixedTime)
    {
        _fixedTime = fixedTime;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _fixedTime;
    }
}