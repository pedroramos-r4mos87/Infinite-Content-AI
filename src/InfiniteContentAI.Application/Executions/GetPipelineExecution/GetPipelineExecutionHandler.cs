using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Executions.GetPipelineExecution;

public sealed class GetPipelineExecutionHandler(
    ICurrentOrganization currentOrganization,
    IPipelineExecutionQueries executionQueries)
{
    public async Task<Result<GetPipelineExecutionResult>> HandleAsync(
        GetPipelineExecutionQuery query,
        CancellationToken cancellationToken)
    {
        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<GetPipelineExecutionResult>(organization.Error);
        }

        Result validation = GetPipelineExecutionValidator.Validate(query);
        if (validation.IsFailure)
        {
            return Result.Failure<GetPipelineExecutionResult>(validation.Error);
        }

        PipelineExecutionDetails? execution = await executionQueries.GetAsync(
            organization.Value,
            new PipelineExecutionId(query.ExecutionId),
            cancellationToken);

        return execution is null
            ? Result.Failure<GetPipelineExecutionResult>(PipelineExecutionApplicationErrors.NotFound)
            : Result.Success(Map(execution));
    }

    private static GetPipelineExecutionResult Map(PipelineExecutionDetails execution)
    {
        return new GetPipelineExecutionResult(
            execution.ExecutionId,
            execution.ProjectId,
            execution.PipelineId,
            execution.PipelineVersion,
            execution.Topic,
            execution.Status,
            execution.CreatedAt,
            execution.StartedAt,
            execution.CompletedAt,
            execution.FailedAt,
            execution.FailureCode,
            execution.Steps,
            execution.Artifacts);
    }
}
