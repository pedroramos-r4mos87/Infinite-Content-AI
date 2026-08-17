using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.GetPipeline;

public sealed class GetPipelineHandler(
    ICurrentOrganization currentOrganization,
    IPipelineQueries pipelineQueries)
{
    public async Task<Result<PipelineDetails>> HandleAsync(
        GetPipelineQuery query,
        CancellationToken cancellationToken)
    {
        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<PipelineDetails>(organization.Error);
        }

        Result validation = GetPipelineValidator.Validate(query);
        if (validation.IsFailure)
        {
            return Result.Failure<PipelineDetails>(validation.Error);
        }

        PipelineDetails? pipeline = await pipelineQueries.GetAsync(
            organization.Value,
            new PipelineId(query.PipelineId),
            cancellationToken);

        return pipeline is null
            ? Result.Failure<PipelineDetails>(PipelineApplicationErrors.NotFound)
            : Result.Success(pipeline);
    }
}
