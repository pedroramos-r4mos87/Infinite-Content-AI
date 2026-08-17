using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;
using Microsoft.EntityFrameworkCore;

namespace InfiniteContentAI.Data.Pipelines;

internal sealed class PipelineQueries(ApplicationDbContext dbContext) : IPipelineQueries
{
    public Task<PipelineDetails?> GetAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        CancellationToken cancellationToken)
    {
        return dbContext.Pipelines
            .AsNoTracking()
            .Where(pipeline =>
                pipeline.OrganizationId == organizationId &&
                pipeline.Id == pipelineId)
            .Select(pipeline => new PipelineDetails(
                pipeline.Id.Value,
                pipeline.ProjectId.Value,
                pipeline.Name.Value,
                pipeline.Description,
                pipeline.Status.ToString().ToLowerInvariant(),
                pipeline.Version,
                pipeline.CreatedAt,
                pipeline.CreatedBy,
                pipeline.PublishedAt,
                pipeline.Steps
                    .OrderBy(step => step.Position)
                    .Select(step => new PipelineStepDetails(
                        step.Id.Value,
                        step.Type.ToString().ToLowerInvariant(),
                        step.Position))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedResult<PipelineListItem>> ListByProjectAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Pipeline> pipelines = dbContext.Pipelines
            .AsNoTracking()
            .Where(pipeline =>
                pipeline.OrganizationId == organizationId &&
                pipeline.ProjectId == projectId);
        long totalCount = await pipelines.LongCountAsync(cancellationToken);
        List<PipelineListItem> items = await pipelines
            .OrderByDescending(pipeline => pipeline.CreatedAt)
            .ThenByDescending(pipeline => pipeline.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pipeline => new PipelineListItem(
                pipeline.Id.Value,
                pipeline.ProjectId.Value,
                pipeline.Name.Value,
                pipeline.Status.ToString().ToLowerInvariant(),
                pipeline.Version,
                pipeline.CreatedAt,
                pipeline.PublishedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PipelineListItem>(items, page, pageSize, totalCount);
    }
}
