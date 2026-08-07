namespace InfiniteContentAI.Domain.Pipelines;

public readonly record struct PipelineId(Guid Value)
{
    public static PipelineId New()
    {
        return new PipelineId(Guid.CreateVersion7());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
