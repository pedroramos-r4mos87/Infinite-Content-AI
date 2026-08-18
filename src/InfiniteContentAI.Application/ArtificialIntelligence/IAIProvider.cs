using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.ArtificialIntelligence;

public interface IAIProvider
{
    Task<Result<AIResearchResult>> ResearchAsync(
        string topic,
        CancellationToken cancellationToken);

    Task<Result<AIScriptResult>> GenerateScriptAsync(
        string topic,
        string researchContent,
        CancellationToken cancellationToken);
}
