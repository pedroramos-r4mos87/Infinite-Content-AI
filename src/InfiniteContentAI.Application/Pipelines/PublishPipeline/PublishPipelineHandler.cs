using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Application.Pipelines.PublishPipeline;

public sealed class PublishPipelineHandler(
    ICurrentOrganization currentOrganization,
    IPipelineRepository pipelineRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<PublishPipelineResult>> HandleAsync(
        PublishPipelineCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = PublishPipelineValidator.Validate(command);
        if (validation.IsFailure)
        {
            return Result.Failure<PublishPipelineResult>(validation.Error);
        }

        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<PublishPipelineResult>(organization.Error);
        }

        Pipeline? pipeline = await pipelineRepository.GetForUpdateAsync(
            organization.Value,
            new PipelineId(command.PipelineId),
            cancellationToken);
        if (pipeline is null)
        {
            return Result.Failure<PublishPipelineResult>(
                PipelineApplicationErrors.NotFound);
        }

        Result publication = pipeline.Publish(clock);
        if (publication.IsFailure)
        {
            return Result.Failure<PublishPipelineResult>(publication.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new PublishPipelineResult(
                pipeline.Id.Value,
                pipeline.Status.ToString().ToLowerInvariant(),
                pipeline.Version,
                pipeline.PublishedAt!.Value));
    }
}
