using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Projects;

namespace InfiniteContentAI.Data.Projects;

internal sealed class ProjectRepository(ApplicationDbContext dbContext) : IProjectRepository
{
    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
    }
}
