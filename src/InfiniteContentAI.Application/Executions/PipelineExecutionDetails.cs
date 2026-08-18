namespace InfiniteContentAI.Application.Executions;

public sealed record PipelineExecutionDetails(
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
    IReadOnlyCollection<StepExecutionDetails> Steps,
    IReadOnlyCollection<ArtifactDetails> Artifacts);

public sealed record StepExecutionDetails(
    Guid StepExecutionId,
    Guid PipelineStepId,
    string Type,
    int Position,
    string Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? FailedAt,
    string? FailureCode);

public sealed record ArtifactDetails(
    Guid ArtifactId,
    Guid StepExecutionId,
    string Type,
    string Content,
    DateTimeOffset CreatedAt);
