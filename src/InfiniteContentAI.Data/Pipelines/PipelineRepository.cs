using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using Microsoft.EntityFrameworkCore;

namespace InfiniteContentAI.Data.Pipelines;

internal sealed class PipelineRepository(ApplicationDbContext dbContext)
    : IPipelineRepository
{
    public async Task AddAsync(
        Pipeline pipeline,
        CancellationToken cancellationToken)
    {
        await dbContext.Pipelines.AddAsync(pipeline, cancellationToken);
    }

    public Task<Pipeline?> GetForUpdateAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        CancellationToken cancellationToken)
    {
        return dbContext.Pipelines
            .Include(pipeline => pipeline.Steps)
            .SingleOrDefaultAsync(
                pipeline =>
                    pipeline.OrganizationId == organizationId &&
                    pipeline.Id == pipelineId,
                cancellationToken);
    }
}
