namespace InfiniteContentAI.Application.Pipelines.PublishPipeline;

public sealed record PublishPipelineResult(
    Guid PipelineId,
    string Status,
    int Version,
    DateTimeOffset PublishedAt);
