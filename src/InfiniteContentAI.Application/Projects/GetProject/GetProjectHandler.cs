using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Projects.GetProject;

public sealed class GetProjectHandler(
    ICurrentOrganization currentOrganization,
    IProjectQueries projectQueries)
{
    public async Task<Result<ProjectDetails>> HandleAsync(
        GetProjectQuery query,
        CancellationToken cancellationToken)
    {
        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<ProjectDetails>(organization.Error);
        }

        ProjectDetails? project = await projectQueries.GetAsync(
            organization.Value,
            new ProjectId(query.ProjectId),
            cancellationToken);

        return project is null
            ? Result.Failure<ProjectDetails>(ProjectErrors.NotFound)
            : Result.Success(project);
    }
}
