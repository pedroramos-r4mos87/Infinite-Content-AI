using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.UnitTests.Executions;

public sealed class PipelineExecutionTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.CreateVersion7());
    private static readonly ProjectId ProjectId = ProjectId.New();
    private static readonly PipelineId PipelineId = PipelineId.New();
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateReturnsPendingExecutionWithCreationEvent()
    {
        var result = PipelineExecution.Create(
            OrganizationId,
            ProjectId,
            PipelineId,
            1,
            "  Agentes de IA  ",
            "  user-123  ",
            new StubClock(CreatedAt));

        Assert.True(result.IsSuccess);
        PipelineExecution execution = result.Value;
        Assert.Equal(7, execution.Id.Value.Version);
        Assert.Equal(OrganizationId, execution.OrganizationId);
        Assert.Equal(ProjectId, execution.ProjectId);
        Assert.Equal(PipelineId, execution.PipelineId);
        Assert.Equal(1, execution.PipelineVersion);
        Assert.Equal("Agentes de IA", execution.Topic);
        Assert.Equal("user-123", execution.CreatedBy);
        Assert.Equal(PipelineExecutionStatus.Pending, execution.Status);
        Assert.Equal(CreatedAt, execution.CreatedAt);
        Assert.Null(execution.StartedAt);
        Assert.Null(execution.CompletedAt);
        Assert.Null(execution.FailedAt);
        Assert.Null(execution.FailureCode);
        Assert.Null(execution.FailureMessage);
        Assert.Empty(execution.Steps);

        PipelineExecutionCreatedDomainEvent domainEvent =
            Assert.IsType<PipelineExecutionCreatedDomainEvent>(Assert.Single(execution.DomainEvents));
        Assert.Equal(7, domainEvent.EventId.Version);
        Assert.Equal(execution.Id, domainEvent.PipelineExecutionId);
        Assert.Equal(OrganizationId, domainEvent.OrganizationId);
        Assert.Equal(ProjectId, domainEvent.ProjectId);
        Assert.Equal(PipelineId, domainEvent.PipelineId);
        Assert.Equal(CreatedAt, domainEvent.OccurredAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRejectsMissingTopic(string? topic)
    {
        var result = CreateResult(topic: topic);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.TopicRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsTopicAboveMaximumLength()
    {
        var result = CreateResult(
            topic: new string('a', PipelineExecution.MaximumTopicLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.TopicTooLong, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingOrganization()
    {
        var result = CreateResult(organizationId: OrganizationId.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.OrganizationRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingProject()
    {
        var result = CreateResult(projectId: new ProjectId(Guid.Empty));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.ProjectRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingPipeline()
    {
        var result = CreateResult(pipelineId: new PipelineId(Guid.Empty));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.PipelineRequired, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateRejectsNonPositivePipelineVersion(int pipelineVersion)
    {
        var result = CreateResult(pipelineVersion: pipelineVersion);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.PipelineVersionInvalid, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRejectsMissingCreatedBy(string? createdBy)
    {
        var result = CreateResult(createdBy: createdBy);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.CreatedByRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsCreatedByAboveMaximumLength()
    {
        var result = CreateResult(
            createdBy: new string('a', PipelineExecution.MaximumCreatedByLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.CreatedByTooLong, result.Error);
    }

    [Fact]
    public void AddStepAddsResearchAndScriptInPositionOrder()
    {
        PipelineExecution execution = CreateExecution();
        PipelineStepId scriptPipelineStepId = PipelineStepId.New();
        PipelineStepId researchPipelineStepId = PipelineStepId.New();

        var script = execution.AddStep(scriptPipelineStepId, PipelineStepType.Script, 2);
        var research = execution.AddStep(researchPipelineStepId, PipelineStepType.Research, 1);

        Assert.True(script.IsSuccess);
        Assert.True(research.IsSuccess);
        Assert.Equal(7, script.Value.Value.Version);
        Assert.Equal(7, research.Value.Value.Version);
        StepExecution[] steps = execution.Steps.ToArray();
        Assert.Equal(2, steps.Length);
        Assert.Equal(research.Value, steps[0].Id);
        Assert.Equal(researchPipelineStepId, steps[0].PipelineStepId);
        Assert.Equal(PipelineStepType.Research, steps[0].Type);
        Assert.Equal(1, steps[0].Position);
        Assert.Equal(execution.Id, steps[0].PipelineExecutionId);
        Assert.Equal(StepExecutionStatus.Pending, steps[0].Status);
        Assert.Equal(script.Value, steps[1].Id);
        Assert.Equal(2, steps[1].Position);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddStepRejectsNonPositivePosition(int position)
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Research,
            position);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.StepPositionInvalid, result.Error);
    }

    [Fact]
    public void AddStepRejectsDuplicatePosition()
    {
        PipelineExecution execution = CreateExecution();
        execution.AddStep(PipelineStepId.New(), PipelineStepType.Research, 1);

        var result = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Script,
            1);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.StepPositionAlreadyExists, result.Error);
    }

    [Fact]
    public void AddStepRejectsDuplicateResearch()
    {
        PipelineExecution execution = CreateExecution();
        execution.AddStep(PipelineStepId.New(), PipelineStepType.Research, 1);

        var result = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Research,
            2);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.ResearchStepAlreadyExists, result.Error);
    }

    [Fact]
    public void AddStepRejectsDuplicateScript()
    {
        PipelineExecution execution = CreateExecution();
        execution.AddStep(PipelineStepId.New(), PipelineStepType.Script, 2);

        var result = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Script,
            3);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.ScriptStepAlreadyExists, result.Error);
    }

    [Theory]
    [InlineData(PipelineStepType.Research, 2)]
    [InlineData(PipelineStepType.Script, 1)]
    public void AddStepRejectsUnexpectedFlowPosition(
        PipelineStepType type,
        int position)
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.AddStep(PipelineStepId.New(), type, position);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.InvalidStepOrder, result.Error);
    }

    [Fact]
    public void AddStepRejectsMissingPipelineStep()
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.AddStep(default, PipelineStepType.Research, 1);

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.PipelineStepRequired, result.Error);
    }

    [Fact]
    public void AddStepRejectsInvalidType()
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.AddStep(PipelineStepId.New(), (PipelineStepType)999, 3);

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.TypeInvalid, result.Error);
    }

    [Fact]
    public void StepsCannotBeModifiedExternally()
    {
        PipelineExecution execution = CreateExecution();
        execution.AddStep(PipelineStepId.New(), PipelineStepType.Research, 1);
        var collection = Assert.IsAssignableFrom<ICollection<StepExecution>>(execution.Steps);

        Assert.True(collection.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => collection.Clear());
        Assert.Single(execution.Steps);
    }

    [Fact]
    public void StartChangesPendingExecutionToRunningAndRaisesEvent()
    {
        PipelineExecution execution = CreateExecution();
        execution.ClearDomainEvents();
        DateTimeOffset startedAt = CreatedAt.AddMinutes(1);

        var result = execution.Start(new StubClock(startedAt));

        Assert.True(result.IsSuccess);
        Assert.Equal(PipelineExecutionStatus.Running, execution.Status);
        Assert.Equal(startedAt, execution.StartedAt);
        PipelineExecutionStartedDomainEvent domainEvent =
            Assert.IsType<PipelineExecutionStartedDomainEvent>(Assert.Single(execution.DomainEvents));
        Assert.Equal(7, domainEvent.EventId.Version);
        Assert.Equal(execution.Id, domainEvent.PipelineExecutionId);
        Assert.Equal(startedAt, domainEvent.OccurredAt);
    }

    [Fact]
    public void StartRejectsSecondCall()
    {
        PipelineExecution execution = CreateExecution();
        execution.Start(new StubClock(CreatedAt.AddMinutes(1)));

        var result = execution.Start(new StubClock(CreatedAt.AddMinutes(2)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.CannotStart, result.Error);
        Assert.Equal(CreatedAt.AddMinutes(1), execution.StartedAt);
    }

    [Fact]
    public void AddStepRejectsRunningExecution()
    {
        PipelineExecution execution = CreateExecution();
        execution.Start(new StubClock(CreatedAt.AddMinutes(1)));

        var result = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Research,
            1);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.StepsLocked, result.Error);
    }

    [Theory]
    [InlineData(PipelineExecutionStatus.Completed)]
    [InlineData(PipelineExecutionStatus.Failed)]
    public void AddStepRejectsFinalExecution(PipelineExecutionStatus finalStatus)
    {
        PipelineExecution execution = finalStatus == PipelineExecutionStatus.Completed
            ? CreateCompletedExecution()
            : CreateFailedExecution();

        var result = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Research,
            1);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.StepsLocked, result.Error);
    }

    [Fact]
    public void CompleteChangesRunningExecutionWithCompletedFlowToCompleted()
    {
        PipelineExecution execution = CreateCompletedStepsExecution();
        execution.ClearDomainEvents();
        DateTimeOffset completedAt = CreatedAt.AddMinutes(5);

        var result = execution.Complete(new StubClock(completedAt));

        Assert.True(result.IsSuccess);
        Assert.Equal(PipelineExecutionStatus.Completed, execution.Status);
        Assert.Equal(completedAt, execution.CompletedAt);
        Assert.Null(execution.FailedAt);
        PipelineExecutionCompletedDomainEvent domainEvent =
            Assert.IsType<PipelineExecutionCompletedDomainEvent>(Assert.Single(execution.DomainEvents));
        Assert.Equal(7, domainEvent.EventId.Version);
        Assert.Equal(execution.Id, domainEvent.PipelineExecutionId);
        Assert.Equal(completedAt, domainEvent.OccurredAt);
    }

    [Fact]
    public void CompleteRejectsPendingExecution()
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.Complete(new StubClock(CreatedAt.AddMinutes(1)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.CannotComplete, result.Error);
    }

    [Theory]
    [InlineData(StepExecutionStatus.Pending)]
    [InlineData(StepExecutionStatus.Running)]
    [InlineData(StepExecutionStatus.Failed)]
    public void CompleteRejectsFlowWithNonCompletedStep(StepExecutionStatus stepStatus)
    {
        PipelineExecution execution = CreateExecution();
        (StepExecutionId researchId, StepExecutionId scriptId) = AddExpectedSteps(execution);
        execution.Start(new StubClock(CreatedAt.AddMinutes(1)));
        execution.StartStep(researchId, new StubClock(CreatedAt.AddMinutes(2)));
        execution.CompleteStep(researchId, new StubClock(CreatedAt.AddMinutes(3)));
        SetStepStatus(execution, scriptId, stepStatus);

        var result = execution.Complete(new StubClock(CreatedAt.AddMinutes(5)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.CannotComplete, result.Error);
        Assert.Null(execution.CompletedAt);
    }

    [Fact]
    public void CompleteRejectsSecondCall()
    {
        PipelineExecution execution = CreateCompletedExecution();

        var result = execution.Complete(new StubClock(CreatedAt.AddMinutes(6)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.AlreadyCompleted, result.Error);
    }

    [Fact]
    public void FailedExecutionCannotCompleteOrStart()
    {
        PipelineExecution execution = CreateExecution();
        execution.Fail("AI.Failed", "Falha segura.", new StubClock(CreatedAt.AddMinutes(1)));

        var complete = execution.Complete(new StubClock(CreatedAt.AddMinutes(2)));
        var start = execution.Start(new StubClock(CreatedAt.AddMinutes(2)));

        Assert.Equal(PipelineExecutionErrors.AlreadyFailed, complete.Error);
        Assert.Equal(PipelineExecutionErrors.CannotStart, start.Error);
    }

    [Fact]
    public void CompletedExecutionCannotStartOrFail()
    {
        PipelineExecution execution = CreateCompletedExecution();

        var start = execution.Start(new StubClock(CreatedAt.AddMinutes(6)));
        var fail = execution.Fail(
            "AI.Failed",
            null,
            new StubClock(CreatedAt.AddMinutes(6)));

        Assert.Equal(PipelineExecutionErrors.CannotStart, start.Error);
        Assert.Equal(PipelineExecutionErrors.AlreadyCompleted, fail.Error);
    }

    [Fact]
    public void FailChangesRunningExecutionToFailedAndRaisesEvent()
    {
        PipelineExecution execution = CreateExecution();
        execution.Start(new StubClock(CreatedAt.AddMinutes(1)));
        execution.ClearDomainEvents();
        DateTimeOffset failedAt = CreatedAt.AddMinutes(2);

        var result = execution.Fail(
            "  AI.ResearchFailed  ",
            "  Falha segura.  ",
            new StubClock(failedAt));

        Assert.True(result.IsSuccess);
        Assert.Equal(PipelineExecutionStatus.Failed, execution.Status);
        Assert.Equal(failedAt, execution.FailedAt);
        Assert.Null(execution.CompletedAt);
        Assert.Equal("AI.ResearchFailed", execution.FailureCode);
        Assert.Equal("Falha segura.", execution.FailureMessage);
        PipelineExecutionFailedDomainEvent domainEvent =
            Assert.IsType<PipelineExecutionFailedDomainEvent>(Assert.Single(execution.DomainEvents));
        Assert.Equal(7, domainEvent.EventId.Version);
        Assert.Equal(execution.Id, domainEvent.PipelineExecutionId);
        Assert.Equal("AI.ResearchFailed", domainEvent.FailureCode);
        Assert.Equal(failedAt, domainEvent.OccurredAt);
    }

    [Fact]
    public void FailAllowsPendingExecution()
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.Fail(
            "Execution.SetupFailed",
            null,
            new StubClock(CreatedAt.AddMinutes(1)));

        Assert.True(result.IsSuccess);
        Assert.Equal(PipelineExecutionStatus.Failed, execution.Status);
        Assert.Null(execution.StartedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FailRejectsMissingFailureCode(string? failureCode)
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.Fail(
            failureCode,
            null,
            new StubClock(CreatedAt.AddMinutes(1)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.FailureCodeRequired, result.Error);
        Assert.Equal(PipelineExecutionStatus.Pending, execution.Status);
    }

    [Fact]
    public void FailRejectsFailureCodeAboveMaximumLength()
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.Fail(
            new string('a', PipelineExecution.MaximumFailureCodeLength + 1),
            null,
            new StubClock(CreatedAt.AddMinutes(1)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.FailureCodeTooLong, result.Error);
    }

    [Fact]
    public void FailRejectsFailureMessageAboveMaximumLength()
    {
        PipelineExecution execution = CreateExecution();

        var result = execution.Fail(
            "AI.Failed",
            new string('a', PipelineExecution.MaximumFailureMessageLength + 1),
            new StubClock(CreatedAt.AddMinutes(1)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.FailureMessageTooLong, result.Error);
        Assert.Equal(PipelineExecutionStatus.Pending, execution.Status);
    }

    [Fact]
    public void FailRejectsSecondCall()
    {
        PipelineExecution execution = CreateExecution();
        execution.Fail("AI.Failed", null, new StubClock(CreatedAt.AddMinutes(1)));

        var result = execution.Fail(
            "AI.FailedAgain",
            null,
            new StubClock(CreatedAt.AddMinutes(2)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.AlreadyFailed, result.Error);
        Assert.Equal("AI.Failed", execution.FailureCode);
    }

    private static PipelineExecution CreateExecution()
    {
        return CreateResult().Value;
    }

    private static PipelineExecution CreateCompletedStepsExecution()
    {
        PipelineExecution execution = CreateExecution();
        (StepExecutionId researchId, StepExecutionId scriptId) = AddExpectedSteps(execution);
        execution.Start(new StubClock(CreatedAt.AddMinutes(1)));
        execution.StartStep(researchId, new StubClock(CreatedAt.AddMinutes(2)));
        execution.CompleteStep(researchId, new StubClock(CreatedAt.AddMinutes(3)));
        execution.StartStep(scriptId, new StubClock(CreatedAt.AddMinutes(3)));
        execution.CompleteStep(scriptId, new StubClock(CreatedAt.AddMinutes(4)));

        return execution;
    }

    private static PipelineExecution CreateCompletedExecution()
    {
        PipelineExecution execution = CreateCompletedStepsExecution();
        execution.Complete(new StubClock(CreatedAt.AddMinutes(5)));
        return execution;
    }

    private static PipelineExecution CreateFailedExecution()
    {
        PipelineExecution execution = CreateExecution();
        execution.Fail(
            "Execution.Failed",
            null,
            new StubClock(CreatedAt.AddMinutes(1)));
        return execution;
    }

    private static (StepExecutionId ResearchId, StepExecutionId ScriptId) AddExpectedSteps(
        PipelineExecution execution)
    {
        StepExecutionId researchId = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Research,
            1).Value;
        StepExecutionId scriptId = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Script,
            2).Value;

        return (researchId, scriptId);
    }

    private static void SetStepStatus(
        PipelineExecution execution,
        StepExecutionId stepId,
        StepExecutionStatus status)
    {
        if (status == StepExecutionStatus.Pending)
        {
            return;
        }

        execution.StartStep(stepId, new StubClock(CreatedAt.AddMinutes(4)));
        if (status == StepExecutionStatus.Failed)
        {
            execution.FailStep(
                stepId,
                "AI.ScriptFailed",
                null,
                new StubClock(CreatedAt.AddMinutes(4)));
        }
    }

    private static InfiniteContentAI.SharedKernel.Results.Result<PipelineExecution> CreateResult(
        OrganizationId? organizationId = null,
        ProjectId? projectId = null,
        PipelineId? pipelineId = null,
        int pipelineVersion = 1,
        string? topic = "Tema",
        string? createdBy = "user-123")
    {
        return PipelineExecution.Create(
            organizationId ?? OrganizationId,
            projectId ?? ProjectId,
            pipelineId ?? PipelineId,
            pipelineVersion,
            topic,
            createdBy,
            new StubClock(CreatedAt));
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
