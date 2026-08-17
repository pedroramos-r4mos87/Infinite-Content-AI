using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;

namespace InfiniteContentAI.Application.Pipelines;

public interface IPipelineQueries
{
    Task<PipelineDetails?> GetAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        CancellationToken cancellationToken);

    Task<PaginatedResult<PipelineListItem>> ListByProjectAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
