namespace InfiniteContentAI.Api.Pipelines;

public sealed record PipelineListItemResponse(
    Guid PipelineId,
    Guid ProjectId,
    string Name,
    string Status,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);
