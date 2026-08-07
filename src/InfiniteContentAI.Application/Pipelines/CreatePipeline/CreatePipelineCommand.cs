namespace InfiniteContentAI.Application.Pipelines.CreatePipeline;

public sealed record CreatePipelineCommand(
    Guid ProjectId,
    string? Name,
    string? Description);
