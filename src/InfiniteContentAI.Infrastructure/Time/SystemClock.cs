using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
