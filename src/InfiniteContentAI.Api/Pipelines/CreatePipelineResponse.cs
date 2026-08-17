namespace InfiniteContentAI.Api.Pipelines;

public sealed record CreatePipelineResponse(
    Guid PipelineId,
    Guid ProjectId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset CreatedAt);
