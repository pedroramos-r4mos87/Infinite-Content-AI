namespace InfiniteContentAI.SharedKernel.UnitTests.Time;

public sealed class FakeClockTests
{
    [Fact]
    public void AdvanceMovesUtcNowByDuration()
    {
        var initialTime = new DateTimeOffset(
            2026,
            7,
            29,
            12,
            0,
            0,
            TimeSpan.Zero);
        var clock = new FakeClock(initialTime);
        TimeSpan duration = TimeSpan.FromMinutes(30);

        clock.Advance(duration);

        Assert.Equal(initialTime.Add(duration), clock.UtcNow);
        Assert.Equal(TimeSpan.Zero, clock.UtcNow.Offset);
    }
}
