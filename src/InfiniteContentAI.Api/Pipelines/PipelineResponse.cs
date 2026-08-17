namespace InfiniteContentAI.Api.Pipelines;

public sealed record PipelineResponse(
    Guid PipelineId,
    Guid ProjectId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? PublishedAt,
    IReadOnlyCollection<PipelineStepResponse> Steps);

public sealed record PipelineStepResponse(
    Guid StepId,
    string Type,
    int Position);
