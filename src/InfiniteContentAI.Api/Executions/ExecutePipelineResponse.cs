namespace InfiniteContentAI.Api.Executions;

public sealed record ExecutePipelineResponse(
    Guid ExecutionId,
    Guid PipelineId,
    int PipelineVersion,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? FailedAt,
    string? FailureCode);
