namespace InfiniteContentAI.Api.Executions;

public sealed record PipelineExecutionResponse(
    Guid ExecutionId,
    Guid ProjectId,
    Guid PipelineId,
    int PipelineVersion,
    string Topic,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? FailedAt,
    string? FailureCode,
    IReadOnlyCollection<StepExecutionResponse> Steps,
    IReadOnlyCollection<ArtifactResponse> Artifacts);

public sealed record StepExecutionResponse(
    Guid StepExecutionId,
    Guid PipelineStepId,
    string Type,
    int Position,
    string Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? FailedAt,
    string? FailureCode);

public sealed record ArtifactResponse(
    Guid ArtifactId,
    Guid StepExecutionId,
    string Type,
    string Content,
    DateTimeOffset CreatedAt);
