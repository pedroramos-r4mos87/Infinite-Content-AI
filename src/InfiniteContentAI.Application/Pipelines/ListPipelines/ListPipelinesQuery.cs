namespace InfiniteContentAI.Application.Pipelines.ListPipelines;

public sealed record ListPipelinesQuery(
    Guid ProjectId,
    int Page = 1,
    int PageSize = 20);
