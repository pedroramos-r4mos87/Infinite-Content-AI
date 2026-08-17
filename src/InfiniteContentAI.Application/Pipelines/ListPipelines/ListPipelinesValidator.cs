using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.ListPipelines;

public static class ListPipelinesValidator
{
    public const int MaximumPageSize = 100;

    public static Result Validate(ListPipelinesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.ProjectId == Guid.Empty)
        {
            return Result.Failure(PipelineErrors.ProjectRequired);
        }

        return query.Page < 1 || query.PageSize is < 1 or > MaximumPageSize
            ? Result.Failure(PipelineApplicationErrors.InvalidPagination)
            : Result.Success();
    }
}
