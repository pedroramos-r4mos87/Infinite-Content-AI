using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Infrastructure.ArtificialIntelligence;

public sealed class FakeAIProvider : IAIProvider
{
    public Task<Result<AIResearchResult>> ResearchAsync(
        string topic,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(topic);
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedTopic = topic.Trim();
        string content = $"""
            # Research

            Topic: {normalizedTopic}

            - Finding 1 for {normalizedTopic}
            - Finding 2 for {normalizedTopic}
            """;

        return Task.FromResult(
            Result.Success(new AIResearchResult(content)));
    }

    public Task<Result<AIScriptResult>> GenerateScriptAsync(
        string topic,
        string researchContent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(researchContent);
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedTopic = topic.Trim();
        string content = $"""
            # Script

            Topic: {normalizedTopic}

            Based on research:

            {researchContent}
            """;

        return Task.FromResult(
            Result.Success(new AIScriptResult(content)));
    }
}
