using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Application.Projects.CreateProject;

public sealed class CreateProjectHandler(
    ICurrentOrganization currentOrganization,
    ICurrentUser currentUser,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<CreateProjectResult>> HandleAsync(
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = CreateProjectValidator.Validate(command);
        if (validation.IsFailure)
        {
            return Result.Failure<CreateProjectResult>(validation.Error);
        }

        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<CreateProjectResult>(organization.Error);
        }

        Result<Project> creation = Project.Create(
            organization.Value,
            command.Name,
            command.Description,
            currentUser.UserId,
            clock);
        if (creation.IsFailure)
        {
            return Result.Failure<CreateProjectResult>(creation.Error);
        }

        Project project = creation.Value;
        await projectRepository.AddAsync(project, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new CreateProjectResult(
                project.Id.Value,
                project.Name.Value,
                project.Description,
                project.Status.ToString().ToLowerInvariant(),
                project.CreatedAt));
    }
}
