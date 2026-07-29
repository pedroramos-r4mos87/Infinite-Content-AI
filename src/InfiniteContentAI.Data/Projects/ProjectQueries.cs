using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;
using Microsoft.EntityFrameworkCore;

namespace InfiniteContentAI.Data.Projects;

internal sealed class ProjectQueries(ApplicationDbContext dbContext) : IProjectQueries
{
    public Task<ProjectDetails?> GetAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        CancellationToken cancellationToken)
    {
        return dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.OrganizationId == organizationId &&
                project.Id == projectId)
            .Select(project => new ProjectDetails(
                project.Id.Value,
                project.Name.Value,
                project.Description,
                project.Status.ToString().ToLowerInvariant(),
                project.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedResult<ProjectListItem>> ListAsync(
        OrganizationId organizationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Project> projects = dbContext.Projects
            .AsNoTracking()
            .Where(project => project.OrganizationId == organizationId);
        long totalCount = await projects.LongCountAsync(cancellationToken);
        List<ProjectListItem> items = await projects
            .OrderByDescending(project => project.CreatedAt)
            .ThenByDescending(project => project.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(project => new ProjectListItem(
                project.Id.Value,
                project.Name.Value,
                project.Status.ToString().ToLowerInvariant(),
                project.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ProjectListItem>(items, page, pageSize, totalCount);
    }
}
