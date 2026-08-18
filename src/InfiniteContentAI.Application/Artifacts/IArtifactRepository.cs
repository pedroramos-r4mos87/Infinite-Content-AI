using InfiniteContentAI.Domain.Artifacts;

namespace InfiniteContentAI.Application.Artifacts;

public interface IArtifactRepository
{
    Task AddAsync(
        Artifact artifact,
        CancellationToken cancellationToken);
}
