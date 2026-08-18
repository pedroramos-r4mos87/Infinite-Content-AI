namespace InfiniteContentAI.Application.Executions.ExecutePipeline;

public sealed record ExecutePipelineCommand(
    Guid PipelineId,
    string? Topic);
