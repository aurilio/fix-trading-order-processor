namespace FixTrading.OrderProcessing.Domain.Tests.Fixtures;

public sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

    public FakeTimeProvider() : this(DateTimeOffset.UtcNow) { }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void SetUtcNow(DateTimeOffset value) => _utcNow = value;

    public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
}