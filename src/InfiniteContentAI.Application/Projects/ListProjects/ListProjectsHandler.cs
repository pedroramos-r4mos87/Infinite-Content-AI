using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Projects.ListProjects;

public sealed class ListProjectsHandler(
    ICurrentOrganization currentOrganization,
    IProjectQueries projectQueries)
{
    public async Task<Result<PaginatedResult<ProjectListItem>>> HandleAsync(
        ListProjectsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100)
        {
            return Result.Failure<PaginatedResult<ProjectListItem>>(
                Error.Validation(
                    "Project.InvalidPagination",
                    "Page deve ser maior que zero e pageSize deve estar entre 1 e 100."));
        }

        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<PaginatedResult<ProjectListItem>>(organization.Error);
        }

        return Result.Success(
            await projectQueries.ListAsync(
                organization.Value,
                query.Page,
                query.PageSize,
                cancellationToken));
    }
}
