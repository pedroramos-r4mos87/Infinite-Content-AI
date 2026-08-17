namespace InfiniteContentAI.Application.Pipelines;

public sealed record PipelineDetails(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? PublishedAt,
    IReadOnlyCollection<PipelineStepDetails> Steps);

public sealed record PipelineStepDetails(
    Guid Id,
    string Type,
    int Position);
