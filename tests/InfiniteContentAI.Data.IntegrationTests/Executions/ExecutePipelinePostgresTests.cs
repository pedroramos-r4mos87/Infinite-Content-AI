using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Artifacts;
using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Application.Executions.ExecutePipeline;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Data;
using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Infrastructure.ArtificialIntelligence;
using InfiniteContentAI.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Data.IntegrationTests.Executions;

public sealed class ExecutePipelinePostgresTests(
    PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task HandlerWithFakeAiCompletesExecutionAndPersistsTwoArtifacts()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);

        Result<ExecutePipelineResult> result = await ExecuteAsync(
            organizationId,
            pipeline.Id,
            new FakeAIProvider());

        Assert.True(result.IsSuccess);
        Assert.Equal("completed", result.Value.Status);
        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PipelineExecution execution = await dbContext.PipelineExecutions
            .AsNoTracking()
            .Include(candidate => candidate.Steps)
            .SingleAsync(candidate => candidate.Id == new PipelineExecutionId(result.Value.ExecutionId));
        List<Artifact> artifacts = await dbContext.Artifacts
            .AsNoTracking()
            .Where(artifact => artifact.PipelineExecutionId == execution.Id)
            .OrderBy(artifact => artifact.CreatedAt)
            .ThenBy(artifact => artifact.Id)
            .ToListAsync();

        Assert.Equal(PipelineExecutionStatus.Completed, execution.Status);
        Assert.Equal(2, execution.Steps.Count);
        Assert.All(execution.Steps, step => Assert.Equal(StepExecutionStatus.Completed, step.Status));
        Assert.Equal(2, artifacts.Count);
        Artifact research = artifacts.Single(artifact => artifact.Type == ArtifactType.Research);
        Artifact script = artifacts.Single(artifact => artifact.Type == ArtifactType.Script);
        Assert.Contains("PostgreSQL integration", research.Content, StringComparison.Ordinal);
        Assert.Contains(research.Content, script.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResearchFailurePersistsFailedExecutionAndNoArtifacts()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);
        var provider = new ConfigurableAIProvider
        {
            ResearchResult = Result.Failure<AIResearchResult>(AIProviderErrors.ResearchFailed),
        };

        Result<ExecutePipelineResult> result = await ExecuteAsync(
            organizationId,
            pipeline.Id,
            provider);

        await AssertFailureStateAsync(
            result.Value.ExecutionId,
            researchStatus: StepExecutionStatus.Failed,
            scriptStatus: StepExecutionStatus.Pending,
            expectedArtifacts: 0);
        Assert.Equal(0, provider.ScriptCalls);
    }

    [Fact]
    public async Task ScriptFailurePreservesResearchArtifactAndPersistsFailedStates()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);
        var provider = new ConfigurableAIProvider
        {
            ScriptResult = Result.Failure<AIScriptResult>(AIProviderErrors.ScriptFailed),
        };

        Result<ExecutePipelineResult> result = await ExecuteAsync(
            organizationId,
            pipeline.Id,
            provider);

        await AssertFailureStateAsync(
            result.Value.ExecutionId,
            researchStatus: StepExecutionStatus.Completed,
            scriptStatus: StepExecutionStatus.Failed,
            expectedArtifacts: 1);
    }

    private async Task<Result<ExecutePipelineResult>> ExecuteAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        IAIProvider provider)
    {
        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        var handler = new ExecutePipelineHandler(
            new CurrentOrganizationStub(organizationId),
            new CurrentUserStub("integration-user"),
            scope.ServiceProvider.GetRequiredService<IPipelineRepository>(),
            scope.ServiceProvider.GetRequiredService<IPipelineExecutionRepository>(),
            scope.ServiceProvider.GetRequiredService<IArtifactRepository>(),
            provider,
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            new ExecutionTestData.IncrementingClock(
                ExecutionTestData.InitialTime.AddHours(1)));
        return await handler.HandleAsync(
            new ExecutePipelineCommand(pipelineId.Value, "PostgreSQL integration"),
            CancellationToken.None);
    }

    private async Task AssertFailureStateAsync(
        Guid executionId,
        StepExecutionStatus researchStatus,
        StepExecutionStatus scriptStatus,
        int expectedArtifacts)
    {
        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PipelineExecution execution = await dbContext.PipelineExecutions
            .AsNoTracking()
            .Include(candidate => candidate.Steps)
            .SingleAsync(candidate => candidate.Id == new PipelineExecutionId(executionId));
        int artifactCount = await dbContext.Artifacts.CountAsync(
            artifact => artifact.PipelineExecutionId == execution.Id);

        Assert.Equal(PipelineExecutionStatus.Failed, execution.Status);
        Assert.NotNull(execution.FailedAt);
        Assert.NotNull(execution.FailureCode);
        Assert.Equal(
            researchStatus,
            execution.Steps.Single(step => step.Type == PipelineStepType.Research).Status);
        Assert.Equal(
            scriptStatus,
            execution.Steps.Single(step => step.Type == PipelineStepType.Script).Status);
        Assert.Equal(expectedArtifacts, artifactCount);
    }

    private sealed class CurrentOrganizationStub(OrganizationId organizationId)
        : ICurrentOrganization
    {
        public OrganizationId? OrganizationId { get; } = organizationId;

        public bool IsAvailable => true;

        public Result<OrganizationId> Require() => Result.Success(organizationId);
    }

    private sealed class CurrentUserStub(string userId) : ICurrentUser
    {
        public string? UserId { get; } = userId;
    }

    private sealed class ConfigurableAIProvider : IAIProvider
    {
        public Result<AIResearchResult> ResearchResult { get; init; } =
            Result.Success(new AIResearchResult("Research persisted before Script"));

        public Result<AIScriptResult> ScriptResult { get; init; } =
            Result.Success(new AIScriptResult("Script"));

        public int ScriptCalls { get; private set; }

        public Task<Result<AIResearchResult>> ResearchAsync(
            string topic,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ResearchResult);
        }

        public Task<Result<AIScriptResult>> GenerateScriptAsync(
            string topic,
            string researchContent,
            CancellationToken cancellationToken)
        {
            ScriptCalls++;
            return Task.FromResult(ScriptResult);
        }
    }
}
