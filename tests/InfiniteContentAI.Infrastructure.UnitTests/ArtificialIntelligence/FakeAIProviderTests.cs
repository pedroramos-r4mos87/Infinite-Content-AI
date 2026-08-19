using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Infrastructure.ArtificialIntelligence;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Infrastructure.UnitTests.ArtificialIntelligence;

public sealed class FakeAIProviderTests
{
    private readonly FakeAIProvider _provider = new();

    [Fact]
    public async Task ResearchReturnsDeterministicContentContainingNormalizedTopic()
    {
        Result<AIResearchResult> first = await _provider.ResearchAsync(
            "  Agentes de IA  ",
            CancellationToken.None);
        Result<AIResearchResult> second = await _provider.ResearchAsync(
            "  Agentes de IA  ",
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(first.Value, second.Value);
        Assert.Contains("# Research", first.Value.Content, StringComparison.Ordinal);
        Assert.Contains("Topic: Agentes de IA", first.Value.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Topic:   Agentes", first.Value.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResearchHonorsCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _provider.ResearchAsync("Tema", cancellation.Token));
    }

    [Fact]
    public async Task ScriptReturnsDeterministicContentContainingTopicAndResearch()
    {
        const string research = "# Research\n\nResultado preservado.";

        Result<AIScriptResult> first = await _provider.GenerateScriptAsync(
            "  Agentes de IA  ",
            research,
            CancellationToken.None);
        Result<AIScriptResult> second = await _provider.GenerateScriptAsync(
            "  Agentes de IA  ",
            research,
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(first.Value, second.Value);
        Assert.Contains("# Script", first.Value.Content, StringComparison.Ordinal);
        Assert.Contains("Topic: Agentes de IA", first.Value.Content, StringComparison.Ordinal);
        Assert.Contains(research, first.Value.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DifferentResearchChangesScriptOutput()
    {
        Result<AIScriptResult> first = await _provider.GenerateScriptAsync(
            "Tema",
            "Research A",
            CancellationToken.None);
        Result<AIScriptResult> second = await _provider.GenerateScriptAsync(
            "Tema",
            "Research B",
            CancellationToken.None);

        Assert.NotEqual(first.Value.Content, second.Value.Content);
    }

    [Fact]
    public async Task ScriptHonorsCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _provider.GenerateScriptAsync(
                "Tema",
                "Research",
                cancellation.Token));
    }
}
