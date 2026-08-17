using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Domain;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.Executions;

public sealed class StepExecution : Entity<StepExecutionId>
{
    public const int MaximumFailureCodeLength = 200;
    public const int MaximumFailureMessageLength = 2000;

    private StepExecution(
        StepExecutionId id,
        PipelineExecutionId pipelineExecutionId,
        PipelineStepId pipelineStepId,
        PipelineStepType type,
        int position)
        : base(id)
    {
        PipelineExecutionId = pipelineExecutionId;
        PipelineStepId = pipelineStepId;
        Type = type;
        Position = position;
        Status = StepExecutionStatus.Pending;
    }

    private StepExecution()
        : base(default)
    {
    }

    public PipelineExecutionId PipelineExecutionId { get; private init; }

    public PipelineStepId PipelineStepId { get; private init; }

    public PipelineStepType Type { get; private init; }

    public int Position { get; private init; }

    public StepExecutionStatus Status { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? FailedAt { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureMessage { get; private set; }

    internal static Result<StepExecution> Create(
        PipelineExecutionId pipelineExecutionId,
        PipelineStepId pipelineStepId,
        PipelineStepType type,
        int position)
    {
        if (pipelineStepId.Value == Guid.Empty)
        {
            return Result.Failure<StepExecution>(StepExecutionErrors.PipelineStepRequired);
        }

        if (!Enum.IsDefined(type))
        {
            return Result.Failure<StepExecution>(StepExecutionErrors.TypeInvalid);
        }

        if (position <= 0)
        {
            return Result.Failure<StepExecution>(StepExecutionErrors.PositionInvalid);
        }

        return Result.Success(
            new StepExecution(
                StepExecutionId.New(),
                pipelineExecutionId,
                pipelineStepId,
                type,
                position));
    }

    internal Result Start(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status != StepExecutionStatus.Pending)
        {
            return Result.Failure(StepExecutionErrors.CannotStart);
        }

        Status = StepExecutionStatus.Running;
        StartedAt = clock.UtcNow;

        return Result.Success();
    }

    internal Result Complete(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status != StepExecutionStatus.Running)
        {
            return Result.Failure(StepExecutionErrors.CannotComplete);
        }

        Status = StepExecutionStatus.Completed;
        CompletedAt = clock.UtcNow;

        return Result.Success();
    }

    internal Result Fail(
        string? failureCode,
        string? failureMessage,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status is StepExecutionStatus.Completed or StepExecutionStatus.Failed)
        {
            return Result.Failure(StepExecutionErrors.CannotFail);
        }

        if (string.IsNullOrWhiteSpace(failureCode))
        {
            return Result.Failure(StepExecutionErrors.FailureCodeRequired);
        }

        string normalizedFailureCode = failureCode.Trim();
        if (normalizedFailureCode.Length > MaximumFailureCodeLength)
        {
            return Result.Failure(StepExecutionErrors.FailureCodeTooLong);
        }

        string? normalizedFailureMessage = NormalizeFailureMessage(failureMessage);
        if (normalizedFailureMessage?.Length > MaximumFailureMessageLength)
        {
            return Result.Failure(StepExecutionErrors.FailureMessageTooLong);
        }

        Status = StepExecutionStatus.Failed;
        FailedAt = clock.UtcNow;
        FailureCode = normalizedFailureCode;
        FailureMessage = normalizedFailureMessage;

        return Result.Success();
    }

    private static string? NormalizeFailureMessage(string? failureMessage)
    {
        return string.IsNullOrWhiteSpace(failureMessage)
            ? null
            : failureMessage.Trim();
    }
}
