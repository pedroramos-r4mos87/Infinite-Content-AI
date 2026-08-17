using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.GetPipeline;

public static class GetPipelineValidator
{
    public static Result Validate(GetPipelineQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.PipelineId == Guid.Empty
            ? Result.Failure(PipelineApplicationErrors.IdRequired)
            : Result.Success();
    }
}
