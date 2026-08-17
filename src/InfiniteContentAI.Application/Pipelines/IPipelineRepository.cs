using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;

namespace InfiniteContentAI.Application.Pipelines;

public interface IPipelineRepository
{
    Task AddAsync(
        Pipeline pipeline,
        CancellationToken cancellationToken);

    Task<Pipeline?> GetForUpdateAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        CancellationToken cancellationToken);
}
