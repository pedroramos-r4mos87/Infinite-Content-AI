namespace InfiniteContentAI.Application.Executions.ExecutePipeline;

public sealed record ExecutePipelineResult(
    Guid ExecutionId,
    Guid PipelineId,
    int PipelineVersion,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? FailedAt,
    string? FailureCode,
    Guid? ResearchArtifactId,
    Guid? ScriptArtifactId);
