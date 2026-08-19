namespace InfiniteContentAI.Domain.Executions;

public readonly record struct StepExecutionId(Guid Value)
{
    public static StepExecutionId New()
    {
        return new StepExecutionId(Guid.CreateVersion7());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
