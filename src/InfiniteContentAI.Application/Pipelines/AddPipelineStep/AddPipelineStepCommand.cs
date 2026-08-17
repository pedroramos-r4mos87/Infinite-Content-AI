namespace InfiniteContentAI.Application.Pipelines.AddPipelineStep;

public sealed record AddPipelineStepCommand(
    Guid PipelineId,
    string? Type,
    int Position);
