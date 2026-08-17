using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Application.Pipelines.CreatePipeline;

public sealed class CreatePipelineHandler(
    ICurrentOrganization currentOrganization,
    ICurrentUser currentUser,
    IProjectQueries projectQueries,
    IPipelineRepository pipelineRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<CreatePipelineResult>> HandleAsync(
        CreatePipelineCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = CreatePipelineValidator.Validate(command);
        if (validation.IsFailure)
        {
            return Result.Failure<CreatePipelineResult>(validation.Error);
        }

        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<CreatePipelineResult>(organization.Error);
        }

        if (string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            return Result.Failure<CreatePipelineResult>(IdentityErrors.UserRequired);
        }

        var projectId = new ProjectId(command.ProjectId);
        ProjectDetails? project = await projectQueries.GetAsync(
            organization.Value,
            projectId,
            cancellationToken);
        if (project is null)
        {
            return Result.Failure<CreatePipelineResult>(
                PipelineApplicationErrors.ProjectNotFound);
        }

        Result<Pipeline> creation = Pipeline.Create(
            organization.Value,
            projectId,
            command.Name,
            command.Description,
            currentUser.UserId,
            clock);
        if (creation.IsFailure)
        {
            return Result.Failure<CreatePipelineResult>(creation.Error);
        }

        Pipeline pipeline = creation.Value;
        await pipelineRepository.AddAsync(pipeline, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new CreatePipelineResult(
                pipeline.Id.Value,
                pipeline.ProjectId.Value,
                pipeline.Name.Value,
                pipeline.Description,
                pipeline.Status.ToString().ToLowerInvariant(),
                pipeline.Version,
                pipeline.CreatedAt));
    }
}
