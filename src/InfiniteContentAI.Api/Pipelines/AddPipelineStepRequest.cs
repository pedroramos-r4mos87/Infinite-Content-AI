namespace InfiniteContentAI.Api.Pipelines;

public sealed record AddPipelineStepRequest(
    string? Type,
    int Position);
