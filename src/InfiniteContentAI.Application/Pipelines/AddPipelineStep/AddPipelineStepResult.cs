namespace InfiniteContentAI.Application.Pipelines.AddPipelineStep;

public sealed record AddPipelineStepResult(
    Guid StepId,
    string Type,
    int Position);
