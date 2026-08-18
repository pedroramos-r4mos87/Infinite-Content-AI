using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Artifacts;
using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Application.UnitTests.Executions;

internal sealed class ExecutionCurrentOrganizationStub(OrganizationId? organizationId)
    : ICurrentOrganization
{
    public OrganizationId? OrganizationId { get; } = organizationId;

    public bool IsAvailable => OrganizationId.HasValue;

    public Result<OrganizationId> Require()
    {
        return OrganizationId.HasValue
            ? Result.Success(OrganizationId.Value)
            : Result.Failure<OrganizationId>(IdentityErrors.OrganizationRequired);
    }
}

internal sealed class ExecutionCurrentUserStub(string? userId) : ICurrentUser
{
    public string? UserId { get; } = userId;
}

internal sealed class ExecutionPipelineRepositoryStub : IPipelineRepository
{
    public Pipeline? PipelineToReturn { get; init; }

    public int GetCallCount { get; private set; }

    public OrganizationId RequestedOrganizationId { get; private set; }

    public PipelineId RequestedPipelineId { get; private set; }

    public CancellationToken CancellationToken { get; private set; }

    public Task AddAsync(Pipeline pipeline, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<Pipeline?> GetForUpdateAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        CancellationToken cancellationToken)
    {
        GetCallCount++;
        RequestedOrganizationId = organizationId;
        RequestedPipelineId = pipelineId;
        CancellationToken = cancellationToken;
        return Task.FromResult(PipelineToReturn);
    }
}

internal sealed class PipelineExecutionRepositorySpy(List<string> operations)
    : IPipelineExecutionRepository
{
    public PipelineExecution? AddedExecution { get; private set; }

    public int CallCount { get; private set; }

    public CancellationToken CancellationToken { get; private set; }

    public Task AddAsync(
        PipelineExecution execution,
        CancellationToken cancellationToken)
    {
        CallCount++;
        AddedExecution = execution;
        CancellationToken = cancellationToken;
        operations.Add("execution:add");
        return Task.CompletedTask;
    }
}

internal sealed class ArtifactRepositorySpy(List<string> operations)
    : IArtifactRepository
{
    private readonly List<Artifact> _artifacts = [];
    private readonly List<CancellationToken> _cancellationTokens = [];

    public IReadOnlyCollection<Artifact> Artifacts => _artifacts.AsReadOnly();

    public IReadOnlyCollection<CancellationToken> CancellationTokens =>
        _cancellationTokens.AsReadOnly();

    public Task AddAsync(
        Artifact artifact,
        CancellationToken cancellationToken)
    {
        _artifacts.Add(artifact);
        _cancellationTokens.Add(cancellationToken);
        operations.Add($"artifact:{artifact.Type.ToString().ToLowerInvariant()}");
        return Task.CompletedTask;
    }
}

internal sealed class AIProviderStub(List<string> operations) : IAIProvider
{
    public Func<string, CancellationToken, Task<Result<AIResearchResult>>>
        ResearchImplementation
    { get; init; } =
            static (topic, _) => Task.FromResult(
                Result.Success(new AIResearchResult($"Research for {topic}")));

    public Func<string, string, CancellationToken, Task<Result<AIScriptResult>>>
        ScriptImplementation
    { get; init; } =
            static (topic, research, _) => Task.FromResult(
                Result.Success(new AIScriptResult($"Script for {topic}: {research}")));

    public int ResearchCallCount { get; private set; }

    public int ScriptCallCount { get; private set; }

    public string? ResearchTopic { get; private set; }

    public string? ScriptTopic { get; private set; }

    public string? ScriptResearchContent { get; private set; }

    public CancellationToken ResearchCancellationToken { get; private set; }

    public CancellationToken ScriptCancellationToken { get; private set; }

    public Task<Result<AIResearchResult>> ResearchAsync(
        string topic,
        CancellationToken cancellationToken)
    {
        ResearchCallCount++;
        ResearchTopic = topic;
        ResearchCancellationToken = cancellationToken;
        operations.Add("ai:research");
        return ResearchImplementation(topic, cancellationToken);
    }

    public Task<Result<AIScriptResult>> GenerateScriptAsync(
        string topic,
        string researchContent,
        CancellationToken cancellationToken)
    {
        ScriptCallCount++;
        ScriptTopic = topic;
        ScriptResearchContent = researchContent;
        ScriptCancellationToken = cancellationToken;
        operations.Add("ai:script");
        return ScriptImplementation(topic, researchContent, cancellationToken);
    }
}

internal sealed class ExecutionUnitOfWorkSpy(List<string> operations) : IUnitOfWork
{
    private readonly List<CancellationToken> _cancellationTokens = [];

    public int CallCount => _cancellationTokens.Count;

    public IReadOnlyCollection<CancellationToken> CancellationTokens =>
        _cancellationTokens.AsReadOnly();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        _cancellationTokens.Add(cancellationToken);
        operations.Add("save");
        return Task.FromResult(1);
    }
}

internal sealed class IncrementingClock(DateTimeOffset initialValue) : IClock
{
    private DateTimeOffset _nextValue = initialValue;

    public DateTimeOffset UtcNow
    {
        get
        {
            DateTimeOffset currentValue = _nextValue;
            _nextValue = _nextValue.AddMinutes(1);
            return currentValue;
        }
    }
}

internal static class ExecutionTestPipelineFactory
{
    public static readonly OrganizationId OrganizationId =
        new(Guid.CreateVersion7());

    public static readonly ProjectId ProjectId = ProjectId.New();

    public static readonly DateTimeOffset InitialTime =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    public static Pipeline CreateDraft()
    {
        return Pipeline.Create(
            OrganizationId,
            ProjectId,
            "Pipeline",
            null,
            "owner-123",
            new FixedClock(InitialTime)).Value;
    }

    public static Pipeline CreatePublished()
    {
        Pipeline pipeline = CreateDraft();
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);
        pipeline.Publish(new FixedClock(InitialTime.AddMinutes(1)));
        return pipeline;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
