namespace InfiniteContentAI.Application.Pipelines;

public sealed record PipelineListItem(
    Guid Id,
    Guid ProjectId,
    string Name,
    string Status,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);
