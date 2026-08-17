namespace InfiniteContentAI.Domain.Pipelines;

public readonly record struct PipelineStepId(Guid Value)
{
    public static PipelineStepId New()
    {
        return new PipelineStepId(Guid.CreateVersion7());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
