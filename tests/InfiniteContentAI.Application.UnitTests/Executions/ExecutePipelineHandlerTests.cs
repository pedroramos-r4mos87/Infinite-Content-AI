using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Application.Executions.ExecutePipeline;
using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.UnitTests.Executions;

public sealed class ExecutePipelineHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleDoesNotPersistWhenCommandValidationFails(bool emptyPipelineId)
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var context = new HandlerContext(pipeline);
        var command = new ExecutePipelineCommand(
            emptyPipelineId ? Guid.Empty : pipeline.Id.Value,
            emptyPipelineId ? "Tema" : "   ");

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(0, context.PipelineRepository.GetCallCount);
        Assert.Null(context.ExecutionRepository.AddedExecution);
        Assert.Equal(0, context.UnitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleExecutesPublishedPipelineFromResearchThroughScript()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var context = new HandlerContext(pipeline);
        using var cancellation = new CancellationTokenSource();

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "  Agentes de IA  "),
            cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal("completed", result.Value.Status);
        Assert.Equal(pipeline.Id.Value, result.Value.PipelineId);
        Assert.Equal(pipeline.Version, result.Value.PipelineVersion);
        Assert.Equal(ExecutionTestPipelineFactory.InitialTime, result.Value.CreatedAt);
        Assert.Equal(
            ExecutionTestPipelineFactory.InitialTime.AddMinutes(1),
            result.Value.StartedAt);
        Assert.Equal(
            ExecutionTestPipelineFactory.InitialTime.AddMinutes(8),
            result.Value.CompletedAt);
        Assert.Null(result.Value.FailedAt);
        Assert.Null(result.Value.FailureCode);
        Assert.NotNull(result.Value.ResearchArtifactId);
        Assert.NotNull(result.Value.ScriptArtifactId);

        PipelineExecution execution = Assert.IsType<PipelineExecution>(
            context.ExecutionRepository.AddedExecution);
        Assert.Equal(result.Value.ExecutionId, execution.Id.Value);
        Assert.Equal(7, execution.Id.Value.Version);
        Assert.Equal(ExecutionTestPipelineFactory.OrganizationId, execution.OrganizationId);
        Assert.Equal(ExecutionTestPipelineFactory.ProjectId, execution.ProjectId);
        Assert.Equal(pipeline.Id, execution.PipelineId);
        Assert.Equal(pipeline.Version, execution.PipelineVersion);
        Assert.Equal("Agentes de IA", execution.Topic);
        Assert.Equal("user-123", execution.CreatedBy);
        Assert.Equal(PipelineExecutionStatus.Completed, execution.Status);

        StepExecution[] steps = execution.Steps.ToArray();
        Assert.Equal(2, steps.Length);
        Assert.Equal(PipelineStepType.Research, steps[0].Type);
        Assert.Equal(1, steps[0].Position);
        Assert.Equal(pipeline.Steps.Single(step => step.Type == PipelineStepType.Research).Id, steps[0].PipelineStepId);
        Assert.Equal(StepExecutionStatus.Completed, steps[0].Status);
        Assert.Equal(ExecutionTestPipelineFactory.InitialTime.AddMinutes(2), steps[0].StartedAt);
        Assert.Equal(ExecutionTestPipelineFactory.InitialTime.AddMinutes(4), steps[0].CompletedAt);
        Assert.Equal(PipelineStepType.Script, steps[1].Type);
        Assert.Equal(2, steps[1].Position);
        Assert.Equal(pipeline.Steps.Single(step => step.Type == PipelineStepType.Script).Id, steps[1].PipelineStepId);
        Assert.Equal(StepExecutionStatus.Completed, steps[1].Status);
        Assert.Equal(ExecutionTestPipelineFactory.InitialTime.AddMinutes(5), steps[1].StartedAt);
        Assert.Equal(ExecutionTestPipelineFactory.InitialTime.AddMinutes(7), steps[1].CompletedAt);

        Artifact[] artifacts = context.ArtifactRepository.Artifacts.ToArray();
        Assert.Equal(2, artifacts.Length);
        Artifact researchArtifact = artifacts[0];
        Artifact scriptArtifact = artifacts[1];
        Assert.Equal(ArtifactType.Research, researchArtifact.Type);
        Assert.Equal(ArtifactType.Script, scriptArtifact.Type);
        Assert.Equal(execution.OrganizationId, researchArtifact.OrganizationId);
        Assert.Equal(execution.ProjectId, researchArtifact.ProjectId);
        Assert.Equal(execution.Id, researchArtifact.PipelineExecutionId);
        Assert.Equal(steps[0].Id, researchArtifact.StepExecutionId);
        Assert.Equal(steps[1].Id, scriptArtifact.StepExecutionId);
        Assert.Equal(
            ExecutionTestPipelineFactory.InitialTime.AddMinutes(3),
            researchArtifact.CreatedAt);
        Assert.Equal(
            ExecutionTestPipelineFactory.InitialTime.AddMinutes(6),
            scriptArtifact.CreatedAt);

        Assert.Equal("Agentes de IA", context.AIProvider.ResearchTopic);
        Assert.Equal("Agentes de IA", context.AIProvider.ScriptTopic);
        Assert.Equal(researchArtifact.Content, context.AIProvider.ScriptResearchContent);
        Assert.Contains("Agentes de IA", researchArtifact.Content, StringComparison.Ordinal);
        Assert.Contains(researchArtifact.Content, scriptArtifact.Content, StringComparison.Ordinal);
        Assert.Equal(1, context.AIProvider.ResearchCallCount);
        Assert.Equal(1, context.AIProvider.ScriptCallCount);
        Assert.Equal(5, context.UnitOfWork.CallCount);
        Assert.Equal(
            [
                "execution:add",
                "save",
                "save",
                "ai:research",
                "artifact:research",
                "save",
                "save",
                "ai:script",
                "artifact:script",
                "save",
            ],
            context.Operations);
        Assert.Equal(cancellation.Token, context.PipelineRepository.CancellationToken);
        Assert.Equal(cancellation.Token, context.ExecutionRepository.CancellationToken);
        Assert.All(
            context.ArtifactRepository.CancellationTokens,
            token => Assert.Equal(cancellation.Token, token));
        Assert.All(
            context.UnitOfWork.CancellationTokens,
            token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(cancellation.Token, context.AIProvider.ResearchCancellationToken);
        Assert.Equal(cancellation.Token, context.AIProvider.ScriptCancellationToken);
    }

    [Fact]
    public async Task HandleReturnsOrganizationErrorBeforeLoadingPipeline()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var context = new HandlerContext(pipeline, organizationAvailable: false);

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Identity.IdentityErrors.OrganizationRequired, result.Error);
        Assert.Equal(0, context.PipelineRepository.GetCallCount);
        Assert.Null(context.ExecutionRepository.AddedExecution);
    }

    [Fact]
    public async Task HandleReturnsUserErrorBeforeLoadingPipeline()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var context = new HandlerContext(pipeline, userId: "   ");

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Identity.IdentityErrors.UserRequired, result.Error);
        Assert.Equal(0, context.PipelineRepository.GetCallCount);
        Assert.Null(context.ExecutionRepository.AddedExecution);
    }

    [Fact]
    public async Task HandleTreatsMissingOrCrossTenantPipelineAsNotFound()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        OrganizationId otherOrganization = new(Guid.CreateVersion7());
        var context = new HandlerContext(
            pipeline: null,
            organizationId: otherOrganization);

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionApplicationErrors.PipelineNotFound, result.Error);
        Assert.Equal(otherOrganization, context.PipelineRepository.RequestedOrganizationId);
        Assert.Equal(pipeline.Id, context.PipelineRepository.RequestedPipelineId);
        Assert.Null(context.ExecutionRepository.AddedExecution);
    }

    [Fact]
    public async Task HandleRejectsDraftPipelineWithoutPersistingExecution()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreateDraft();
        var context = new HandlerContext(pipeline);

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionApplicationErrors.PipelineNotPublished, result.Error);
        Assert.Null(context.ExecutionRepository.AddedExecution);
        Assert.Equal(0, context.UnitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePersistsResearchFailureAndDoesNotExecuteScript()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var operations = new List<string>();
        var aiProvider = new AIProviderStub(operations)
        {
            ResearchImplementation = static (_, _) => Task.FromResult(
                Result.Failure<AIResearchResult>(AIProviderErrors.ResearchFailed)),
        };
        var context = new HandlerContext(pipeline, operations: operations, aiProvider: aiProvider);

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("failed", result.Value.Status);
        Assert.Equal(AIProviderErrors.ResearchFailed.Code, result.Value.FailureCode);
        Assert.NotEqual(Guid.Empty, result.Value.ExecutionId);
        Assert.Null(result.Value.ResearchArtifactId);
        Assert.Null(result.Value.ScriptArtifactId);
        PipelineExecution execution = context.ExecutionRepository.AddedExecution!;
        Assert.Equal(PipelineExecutionStatus.Failed, execution.Status);
        StepExecution research = execution.Steps.Single(
            step => step.Type == PipelineStepType.Research);
        StepExecution script = execution.Steps.Single(
            step => step.Type == PipelineStepType.Script);
        Assert.Equal(StepExecutionStatus.Failed, research.Status);
        Assert.Equal(AIProviderErrors.ResearchFailed.Code, research.FailureCode);
        Assert.Equal(StepExecutionStatus.Pending, script.Status);
        Assert.Empty(context.ArtifactRepository.Artifacts);
        Assert.Equal(1, context.AIProvider.ResearchCallCount);
        Assert.Equal(0, context.AIProvider.ScriptCallCount);
        Assert.Equal(3, context.UnitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePreservesResearchArtifactWhenScriptFails()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var operations = new List<string>();
        var aiProvider = new AIProviderStub(operations)
        {
            ScriptImplementation = static (_, _, _) => Task.FromResult(
                Result.Failure<AIScriptResult>(AIProviderErrors.ScriptFailed)),
        };
        var context = new HandlerContext(pipeline, operations: operations, aiProvider: aiProvider);

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("failed", result.Value.Status);
        Assert.Equal(AIProviderErrors.ScriptFailed.Code, result.Value.FailureCode);
        Assert.NotNull(result.Value.ResearchArtifactId);
        Assert.Null(result.Value.ScriptArtifactId);
        PipelineExecution execution = context.ExecutionRepository.AddedExecution!;
        Assert.Equal(PipelineExecutionStatus.Failed, execution.Status);
        Assert.Equal(
            StepExecutionStatus.Completed,
            execution.Steps.Single(step => step.Type == PipelineStepType.Research).Status);
        Assert.Equal(
            StepExecutionStatus.Failed,
            execution.Steps.Single(step => step.Type == PipelineStepType.Script).Status);
        Artifact artifact = Assert.Single(context.ArtifactRepository.Artifacts);
        Assert.Equal(ArtifactType.Research, artifact.Type);
        Assert.Equal(1, context.AIProvider.ResearchCallCount);
        Assert.Equal(1, context.AIProvider.ScriptCallCount);
        Assert.Equal(5, context.UnitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleSanitizesUnexpectedProviderExceptionAndPersistsFailure()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var operations = new List<string>();
        var aiProvider = new AIProviderStub(operations)
        {
            ResearchImplementation = static (_, _) =>
                throw new InvalidOperationException("secret provider details"),
        };
        var context = new HandlerContext(pipeline, operations: operations, aiProvider: aiProvider);

        Result<ExecutePipelineResult> result = await context.Handler.HandleAsync(
            new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("failed", result.Value.Status);
        Assert.Equal(AIProviderErrors.UnexpectedFailure.Code, result.Value.FailureCode);
        PipelineExecution execution = context.ExecutionRepository.AddedExecution!;
        Assert.Equal(PipelineExecutionStatus.Failed, execution.Status);
        Assert.Equal(AIProviderErrors.UnexpectedFailure.Description, execution.FailureMessage);
        Assert.DoesNotContain("secret", execution.FailureMessage, StringComparison.Ordinal);
        Assert.Equal(3, context.UnitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleDoesNotConvertResearchCancellationIntoFailure()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var operations = new List<string>();
        var aiProvider = new AIProviderStub(operations)
        {
            ResearchImplementation = static (_, token) =>
                throw new OperationCanceledException(token),
        };
        var context = new HandlerContext(pipeline, operations: operations, aiProvider: aiProvider);
        using var cancellation = new CancellationTokenSource();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => context.Handler.HandleAsync(
                new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
                cancellation.Token));

        PipelineExecution execution = context.ExecutionRepository.AddedExecution!;
        Assert.Equal(PipelineExecutionStatus.Running, execution.Status);
        Assert.Null(execution.FailureCode);
        Assert.Equal(2, context.UnitOfWork.CallCount);
        Assert.Equal(cancellation.Token, context.AIProvider.ResearchCancellationToken);
    }

    [Fact]
    public async Task HandleDoesNotConvertScriptCancellationIntoFailure()
    {
        Pipeline pipeline = ExecutionTestPipelineFactory.CreatePublished();
        var operations = new List<string>();
        var aiProvider = new AIProviderStub(operations)
        {
            ScriptImplementation = static (_, _, token) =>
                throw new OperationCanceledException(token),
        };
        var context = new HandlerContext(pipeline, operations: operations, aiProvider: aiProvider);
        using var cancellation = new CancellationTokenSource();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => context.Handler.HandleAsync(
                new ExecutePipelineCommand(pipeline.Id.Value, "Tema"),
                cancellation.Token));

        PipelineExecution execution = context.ExecutionRepository.AddedExecution!;
        Assert.Equal(PipelineExecutionStatus.Running, execution.Status);
        Assert.Null(execution.FailureCode);
        Assert.Single(context.ArtifactRepository.Artifacts);
        Assert.Equal(4, context.UnitOfWork.CallCount);
        Assert.Equal(cancellation.Token, context.AIProvider.ScriptCancellationToken);
    }

    private sealed class HandlerContext
    {
        public HandlerContext(
            Pipeline? pipeline,
            OrganizationId? organizationId = default,
            bool organizationAvailable = true,
            string? userId = "user-123",
            List<string>? operations = null,
            AIProviderStub? aiProvider = null)
        {
            Operations = operations ?? [];
            PipelineRepository = new ExecutionPipelineRepositoryStub
            {
                PipelineToReturn = pipeline,
            };
            ExecutionRepository = new PipelineExecutionRepositorySpy(Operations);
            ArtifactRepository = new ArtifactRepositorySpy(Operations);
            AIProvider = aiProvider ?? new AIProviderStub(Operations);
            UnitOfWork = new ExecutionUnitOfWorkSpy(Operations);

            OrganizationId? currentOrganization = organizationAvailable
                ? organizationId ?? ExecutionTestPipelineFactory.OrganizationId
                : null;
            Handler = new ExecutePipelineHandler(
                new ExecutionCurrentOrganizationStub(currentOrganization),
                new ExecutionCurrentUserStub(userId),
                PipelineRepository,
                ExecutionRepository,
                ArtifactRepository,
                AIProvider,
                UnitOfWork,
                new IncrementingClock(ExecutionTestPipelineFactory.InitialTime));
        }

        public List<string> Operations { get; }

        public ExecutionPipelineRepositoryStub PipelineRepository { get; }

        public PipelineExecutionRepositorySpy ExecutionRepository { get; }

        public ArtifactRepositorySpy ArtifactRepository { get; }

        public AIProviderStub AIProvider { get; }

        public ExecutionUnitOfWorkSpy UnitOfWork { get; }

        public ExecutePipelineHandler Handler { get; }
    }
}
