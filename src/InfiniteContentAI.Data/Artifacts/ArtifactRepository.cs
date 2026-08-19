using InfiniteContentAI.Application.Artifacts;
using InfiniteContentAI.Domain.Artifacts;

namespace InfiniteContentAI.Data.Artifacts;

internal sealed class ArtifactRepository(ApplicationDbContext dbContext)
    : IArtifactRepository
{
    public async Task AddAsync(
        Artifact artifact,
        CancellationToken cancellationToken)
    {
        await dbContext.Artifacts.AddAsync(artifact, cancellationToken);
    }
}
