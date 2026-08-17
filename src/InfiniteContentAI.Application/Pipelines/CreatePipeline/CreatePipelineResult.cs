namespace InfiniteContentAI.Application.Pipelines.CreatePipeline;

public sealed record CreatePipelineResult(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset CreatedAt);
