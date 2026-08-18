using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Executions.GetPipelineExecution;

public static class GetPipelineExecutionValidator
{
    public static Result Validate(GetPipelineExecutionQuery query)
    {
        return query.ExecutionId == Guid.Empty
            ? Result.Failure(PipelineExecutionApplicationErrors.ExecutionRequired)
            : Result.Success();
    }
}
