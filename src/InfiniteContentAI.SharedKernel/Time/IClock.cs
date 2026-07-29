namespace InfiniteContentAI.SharedKernel.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
