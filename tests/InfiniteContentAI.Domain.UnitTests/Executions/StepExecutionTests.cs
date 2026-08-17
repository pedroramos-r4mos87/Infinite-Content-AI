using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.UnitTests.Executions;

public sealed class StepExecutionTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NewStepIsPendingWithVersionSevenIdAndEmptyLifecycleState()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();

        StepExecution step = Assert.Single(execution.Steps);
        Assert.Equal(stepId, step.Id);
        Assert.Equal(7, step.Id.Value.Version);
        Assert.Equal(StepExecutionStatus.Pending, step.Status);
        Assert.Null(step.StartedAt);
        Assert.Null(step.CompletedAt);
        Assert.Null(step.FailedAt);
        Assert.Null(step.FailureCode);
        Assert.Null(step.FailureMessage);
    }

    [Fact]
    public void StartChangesPendingStepToRunning()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();
        DateTimeOffset startedAt = CreatedAt.AddMinutes(2);

        var result = execution.StartStep(stepId, new StubClock(startedAt));

        Assert.True(result.IsSuccess);
        StepExecution step = Assert.Single(execution.Steps);
        Assert.Equal(StepExecutionStatus.Running, step.Status);
        Assert.Equal(startedAt, step.StartedAt);
    }

    [Fact]
    public void StartRejectsSecondCall()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();
        execution.StartStep(stepId, new StubClock(CreatedAt.AddMinutes(2)));

        var result = execution.StartStep(
            stepId,
            new StubClock(CreatedAt.AddMinutes(3)));

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.CannotStart, result.Error);
        Assert.Equal(CreatedAt.AddMinutes(2), Assert.Single(execution.Steps).StartedAt);
    }

    [Fact]
    public void CompleteChangesRunningStepToCompleted()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();
        execution.StartStep(stepId, new StubClock(CreatedAt.AddMinutes(2)));
        DateTimeOffset completedAt = CreatedAt.AddMinutes(3);

        var result = execution.CompleteStep(stepId, new StubClock(completedAt));

        Assert.True(result.IsSuccess);
        StepExecution step = Assert.Single(execution.Steps);
        Assert.Equal(StepExecutionStatus.Completed, step.Status);
        Assert.Equal(completedAt, step.CompletedAt);
        Assert.Null(step.FailedAt);
    }

    [Fact]
    public void CompleteRejectsPendingStep()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();

        var result = execution.CompleteStep(
            stepId,
            new StubClock(CreatedAt.AddMinutes(2)));

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.CannotComplete, result.Error);
    }

    [Fact]
    public void CompleteRejectsCompletedStep()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();
        execution.StartStep(stepId, new StubClock(CreatedAt.AddMinutes(2)));
        execution.CompleteStep(stepId, new StubClock(CreatedAt.AddMinutes(3)));

        var result = execution.CompleteStep(
            stepId,
            new StubClock(CreatedAt.AddMinutes(4)));

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.CannotComplete, result.Error);
    }

    [Fact]
    public void FailChangesRunningStepToFailedAndSanitizesDetails()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();
        execution.StartStep(stepId, new StubClock(CreatedAt.AddMinutes(2)));
        DateTimeOffset failedAt = CreatedAt.AddMinutes(3);

        var result = execution.FailStep(
            stepId,
            "  AI.ResearchFailed  ",
            "  Falha segura.  ",
            new StubClock(failedAt));

        Assert.True(result.IsSuccess);
        StepExecution step = Assert.Single(execution.Steps);
        Assert.Equal(StepExecutionStatus.Failed, step.Status);
        Assert.Equal(failedAt, step.FailedAt);
        Assert.Null(step.CompletedAt);
        Assert.Equal("AI.ResearchFailed", step.FailureCode);
        Assert.Equal("Falha segura.", step.FailureMessage);
    }

    [Fact]
    public void FailAllowsPendingStep()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();

        var result = execution.FailStep(
            stepId,
            "AI.NotStarted",
            null,
            new StubClock(CreatedAt.AddMinutes(2)));

        Assert.True(result.IsSuccess);
        Assert.Equal(StepExecutionStatus.Failed, Assert.Single(execution.Steps).Status);
        Assert.Null(Assert.Single(execution.Steps).StartedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FailRejectsMissingCode(string? failureCode)
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();

        var result = execution.FailStep(
            stepId,
            failureCode,
            null,
            new StubClock(CreatedAt.AddMinutes(2)));

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.FailureCodeRequired, result.Error);
    }

    [Fact]
    public void FailRejectsCodeAboveMaximumLength()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();

        var result = execution.FailStep(
            stepId,
            new string('a', StepExecution.MaximumFailureCodeLength + 1),
            null,
            new StubClock(CreatedAt.AddMinutes(2)));

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.FailureCodeTooLong, result.Error);
    }

    [Fact]
    public void FailRejectsMessageAboveMaximumLength()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();

        var result = execution.FailStep(
            stepId,
            "AI.Failed",
            new string('a', StepExecution.MaximumFailureMessageLength + 1),
            new StubClock(CreatedAt.AddMinutes(2)));

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.FailureMessageTooLong, result.Error);
    }

    [Theory]
    [InlineData(StepExecutionStatus.Completed)]
    [InlineData(StepExecutionStatus.Failed)]
    public void FailRejectsFinalStep(StepExecutionStatus finalStatus)
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();
        execution.StartStep(stepId, new StubClock(CreatedAt.AddMinutes(2)));
        if (finalStatus == StepExecutionStatus.Completed)
        {
            execution.CompleteStep(stepId, new StubClock(CreatedAt.AddMinutes(3)));
        }
        else
        {
            execution.FailStep(
                stepId,
                "AI.Failed",
                null,
                new StubClock(CreatedAt.AddMinutes(3)));
        }

        var result = execution.FailStep(
            stepId,
            "AI.FailedAgain",
            null,
            new StubClock(CreatedAt.AddMinutes(4)));

        Assert.True(result.IsFailure);
        Assert.Equal(StepExecutionErrors.CannotFail, result.Error);
    }

    [Fact]
    public void StepOperationsRejectUnknownStep()
    {
        (PipelineExecution execution, _) = CreateRunningExecution();
        StepExecutionId unknownId = StepExecutionId.New();

        var start = execution.StartStep(unknownId, new StubClock(CreatedAt));
        var complete = execution.CompleteStep(unknownId, new StubClock(CreatedAt));
        var fail = execution.FailStep(unknownId, "AI.Failed", null, new StubClock(CreatedAt));

        Assert.Equal(PipelineExecutionErrors.StepNotFound, start.Error);
        Assert.Equal(PipelineExecutionErrors.StepNotFound, complete.Error);
        Assert.Equal(PipelineExecutionErrors.StepNotFound, fail.Error);
    }

    [Fact]
    public void FinalExecutionLocksStepLifecycle()
    {
        (PipelineExecution execution, StepExecutionId stepId) = CreateRunningExecution();
        execution.Fail("Execution.Failed", null, new StubClock(CreatedAt.AddMinutes(2)));

        var start = execution.StartStep(stepId, new StubClock(CreatedAt.AddMinutes(3)));
        var complete = execution.CompleteStep(stepId, new StubClock(CreatedAt.AddMinutes(3)));
        var fail = execution.FailStep(
            stepId,
            "AI.Failed",
            null,
            new StubClock(CreatedAt.AddMinutes(3)));

        Assert.Equal(StepExecutionErrors.CannotStart, start.Error);
        Assert.Equal(StepExecutionErrors.CannotComplete, complete.Error);
        Assert.Equal(StepExecutionErrors.CannotFail, fail.Error);
        Assert.Equal(StepExecutionStatus.Pending, Assert.Single(execution.Steps).Status);
    }

    private static (PipelineExecution Execution, StepExecutionId StepId) CreateRunningExecution()
    {
        PipelineExecution execution = PipelineExecution.Create(
            new OrganizationId(Guid.CreateVersion7()),
            ProjectId.New(),
            PipelineId.New(),
            1,
            "Tema",
            "user-123",
            new StubClock(CreatedAt)).Value;
        StepExecutionId stepId = execution.AddStep(
            PipelineStepId.New(),
            PipelineStepType.Research,
            1).Value;
        execution.Start(new StubClock(CreatedAt.AddMinutes(1)));

        return (execution, stepId);
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
