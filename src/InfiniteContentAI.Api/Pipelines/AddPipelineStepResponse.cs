namespace InfiniteContentAI.Api.Pipelines;

public sealed record AddPipelineStepResponse(
    Guid StepId,
    string Type,
    int Position);
