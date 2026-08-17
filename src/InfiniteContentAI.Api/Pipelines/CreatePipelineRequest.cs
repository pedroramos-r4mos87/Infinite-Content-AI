namespace InfiniteContentAI.Api.Pipelines;

public sealed record CreatePipelineRequest(
    string? Name,
    string? Description);
