using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;

namespace InfiniteContentAI.Application.Projects;

public interface IProjectQueries
{
    Task<ProjectDetails?> GetAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        CancellationToken cancellationToken);

    Task<PaginatedResult<ProjectListItem>> ListAsync(
        OrganizationId organizationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
