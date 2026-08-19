using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;

namespace InfiniteContentAI.Application.Executions;

public interface IPipelineExecutionQueries
{
    Task<PipelineExecutionDetails?> GetAsync(
        OrganizationId organizationId,
        PipelineExecutionId executionId,
        CancellationToken cancellationToken);
}
