namespace InfiniteContentAI.Application.Executions.GetPipelineExecution;

public sealed record GetPipelineExecutionResult(
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
