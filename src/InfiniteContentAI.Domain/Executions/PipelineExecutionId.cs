namespace InfiniteContentAI.Domain.Executions;

public readonly record struct PipelineExecutionId(Guid Value)
{
    public static PipelineExecutionId New()
    {
        return new PipelineExecutionId(Guid.CreateVersion7());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
