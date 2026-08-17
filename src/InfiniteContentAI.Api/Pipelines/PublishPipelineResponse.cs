namespace InfiniteContentAI.Api.Pipelines;

public sealed record PublishPipelineResponse(
    Guid PipelineId,
    string Status,
    int Version,
    DateTimeOffset PublishedAt);
