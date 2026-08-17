using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Pipelines;

public sealed class PipelineStep : Entity<PipelineStepId>
{
    internal PipelineStep(
        PipelineStepId id,
        PipelineStepType type,
        int position)
        : base(id)
    {
        Type = type;
        Position = position;
    }

    private PipelineStep()
        : base(default)
    {
    }

    public PipelineStepType Type { get; private init; }

    public int Position { get; private init; }
}
