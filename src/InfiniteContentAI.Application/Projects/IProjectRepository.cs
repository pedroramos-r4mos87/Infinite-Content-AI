using InfiniteContentAI.Domain.Projects;

namespace InfiniteContentAI.Application.Projects;

public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken);
}
