using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Domain;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.Executions;

public sealed class PipelineExecution : AggregateRoot<PipelineExecutionId>
{
    public const int MaximumTopicLength = 500;
    public const int MaximumCreatedByLength = 200;
    public const int MaximumFailureCodeLength = 200;
    public const int MaximumFailureMessageLength = 2000;

    private readonly List<StepExecution> _steps = [];

    private PipelineExecution(
        PipelineExecutionId id,
        OrganizationId organizationId,
        ProjectId projectId,
        PipelineId pipelineId,
        int pipelineVersion,
        string topic,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id)
    {
        OrganizationId = organizationId;
        ProjectId = projectId;
        PipelineId = pipelineId;
        PipelineVersion = pipelineVersion;
        Topic = topic;
        Status = PipelineExecutionStatus.Pending;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    private PipelineExecution()
        : base(default)
    {
        Topic = null!;
        CreatedBy = null!;
    }

    public OrganizationId OrganizationId { get; private init; }

    public ProjectId ProjectId { get; private init; }

    public PipelineId PipelineId { get; private init; }

    public int PipelineVersion { get; private init; }

    public string Topic { get; private init; }

    public PipelineExecutionStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }

    public string CreatedBy { get; private init; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? FailedAt { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureMessage { get; private set; }

    public IReadOnlyCollection<StepExecution> Steps => _steps.AsReadOnly();

    public static Result<PipelineExecution> Create(
        OrganizationId organizationId,
        ProjectId projectId,
        PipelineId pipelineId,
        int pipelineVersion,
        string? topic,
        string? createdBy,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (organizationId == OrganizationId.Empty)
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.OrganizationRequired);
        }

        if (projectId.Value == Guid.Empty)
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.ProjectRequired);
        }

        if (pipelineId.Value == Guid.Empty)
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.PipelineRequired);
        }

        if (pipelineVersion <= 0)
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.PipelineVersionInvalid);
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.TopicRequired);
        }

        string normalizedTopic = topic.Trim();
        if (normalizedTopic.Length > MaximumTopicLength)
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.TopicTooLong);
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.CreatedByRequired);
        }

        string normalizedCreatedBy = createdBy.Trim();
        if (normalizedCreatedBy.Length > MaximumCreatedByLength)
        {
            return Result.Failure<PipelineExecution>(PipelineExecutionErrors.CreatedByTooLong);
        }

        var execution = new PipelineExecution(
            PipelineExecutionId.New(),
            organizationId,
            projectId,
            pipelineId,
            pipelineVersion,
            normalizedTopic,
            clock.UtcNow,
            normalizedCreatedBy);

        execution.RaiseDomainEvent(
            new PipelineExecutionCreatedDomainEvent(
                Guid.CreateVersion7(),
                execution.Id,
                organizationId,
                projectId,
                pipelineId,
                execution.CreatedAt));

        return Result.Success(execution);
    }

    public Result<StepExecutionId> AddStep(
        PipelineStepId pipelineStepId,
        PipelineStepType type,
        int position)
    {
        if (Status != PipelineExecutionStatus.Pending)
        {
            return Result.Failure<StepExecutionId>(PipelineExecutionErrors.StepsLocked);
        }

        if (position <= 0)
        {
            return Result.Failure<StepExecutionId>(PipelineExecutionErrors.StepPositionInvalid);
        }

        if (_steps.Exists(step => step.Position == position))
        {
            return Result.Failure<StepExecutionId>(PipelineExecutionErrors.StepPositionAlreadyExists);
        }

        Error? duplicateTypeError = GetDuplicateTypeError(type);
        if (duplicateTypeError is not null)
        {
            return Result.Failure<StepExecutionId>(duplicateTypeError);
        }

        if (!HasExpectedPosition(type, position))
        {
            return Result.Failure<StepExecutionId>(PipelineExecutionErrors.InvalidStepOrder);
        }

        Result<StepExecution> step = StepExecution.Create(
            Id,
            pipelineStepId,
            type,
            position);
        if (step.IsFailure)
        {
            return Result.Failure<StepExecutionId>(step.Error);
        }

        _steps.Add(step.Value);
        _steps.Sort(static (left, right) => left.Position.CompareTo(right.Position));

        return Result.Success(step.Value.Id);
    }

    public Result Start(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status != PipelineExecutionStatus.Pending)
        {
            return Result.Failure(PipelineExecutionErrors.CannotStart);
        }

        DateTimeOffset startedAt = clock.UtcNow;
        Status = PipelineExecutionStatus.Running;
        StartedAt = startedAt;

        RaiseDomainEvent(
            new PipelineExecutionStartedDomainEvent(
                Guid.CreateVersion7(),
                Id,
                startedAt));

        return Result.Success();
    }

    public Result StartStep(
        StepExecutionId stepExecutionId,
        IClock clock)
    {
        if (Status != PipelineExecutionStatus.Running)
        {
            return Result.Failure(StepExecutionErrors.CannotStart);
        }

        StepExecution? step = FindStep(stepExecutionId);
        return step is null
            ? Result.Failure(PipelineExecutionErrors.StepNotFound)
            : step.Start(clock);
    }

    public Result CompleteStep(
        StepExecutionId stepExecutionId,
        IClock clock)
    {
        if (Status != PipelineExecutionStatus.Running)
        {
            return Result.Failure(StepExecutionErrors.CannotComplete);
        }

        StepExecution? step = FindStep(stepExecutionId);
        return step is null
            ? Result.Failure(PipelineExecutionErrors.StepNotFound)
            : step.Complete(clock);
    }

    public Result FailStep(
        StepExecutionId stepExecutionId,
        string? failureCode,
        string? failureMessage,
        IClock clock)
    {
        if (Status != PipelineExecutionStatus.Running)
        {
            return Result.Failure(StepExecutionErrors.CannotFail);
        }

        StepExecution? step = FindStep(stepExecutionId);
        return step is null
            ? Result.Failure(PipelineExecutionErrors.StepNotFound)
            : step.Fail(failureCode, failureMessage, clock);
    }

    public Result Complete(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status == PipelineExecutionStatus.Completed)
        {
            return Result.Failure(PipelineExecutionErrors.AlreadyCompleted);
        }

        if (Status == PipelineExecutionStatus.Failed)
        {
            return Result.Failure(PipelineExecutionErrors.AlreadyFailed);
        }

        if (Status != PipelineExecutionStatus.Running ||
            !HasExpectedCompletedFlow())
        {
            return Result.Failure(PipelineExecutionErrors.CannotComplete);
        }

        DateTimeOffset completedAt = clock.UtcNow;
        Status = PipelineExecutionStatus.Completed;
        CompletedAt = completedAt;

        RaiseDomainEvent(
            new PipelineExecutionCompletedDomainEvent(
                Guid.CreateVersion7(),
                Id,
                completedAt));

        return Result.Success();
    }

    public Result Fail(
        string? failureCode,
        string? failureMessage,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status == PipelineExecutionStatus.Completed)
        {
            return Result.Failure(PipelineExecutionErrors.AlreadyCompleted);
        }

        if (Status == PipelineExecutionStatus.Failed)
        {
            return Result.Failure(PipelineExecutionErrors.AlreadyFailed);
        }

        if (string.IsNullOrWhiteSpace(failureCode))
        {
            return Result.Failure(PipelineExecutionErrors.FailureCodeRequired);
        }

        string normalizedFailureCode = failureCode.Trim();
        if (normalizedFailureCode.Length > MaximumFailureCodeLength)
        {
            return Result.Failure(PipelineExecutionErrors.FailureCodeTooLong);
        }

        string? normalizedFailureMessage = NormalizeFailureMessage(failureMessage);
        if (normalizedFailureMessage?.Length > MaximumFailureMessageLength)
        {
            return Result.Failure(PipelineExecutionErrors.FailureMessageTooLong);
        }

        DateTimeOffset failedAt = clock.UtcNow;
        Status = PipelineExecutionStatus.Failed;
        FailedAt = failedAt;
        FailureCode = normalizedFailureCode;
        FailureMessage = normalizedFailureMessage;

        RaiseDomainEvent(
            new PipelineExecutionFailedDomainEvent(
                Guid.CreateVersion7(),
                Id,
                normalizedFailureCode,
                failedAt));

        return Result.Success();
    }

    private Error? GetDuplicateTypeError(PipelineStepType type)
    {
        if (type == PipelineStepType.Research &&
            _steps.Exists(step => step.Type == PipelineStepType.Research))
        {
            return PipelineExecutionErrors.ResearchStepAlreadyExists;
        }

        if (type == PipelineStepType.Script &&
            _steps.Exists(step => step.Type == PipelineStepType.Script))
        {
            return PipelineExecutionErrors.ScriptStepAlreadyExists;
        }

        return null;
    }

    private static bool HasExpectedPosition(
        PipelineStepType type,
        int position)
    {
        return type switch
        {
            PipelineStepType.Research => position == 1,
            PipelineStepType.Script => position == 2,
            _ => true,
        };
    }

    private bool HasExpectedCompletedFlow()
    {
        return _steps.Count == 2 &&
            _steps[0].Type == PipelineStepType.Research &&
            _steps[0].Position == 1 &&
            _steps[1].Type == PipelineStepType.Script &&
            _steps[1].Position == 2 &&
            _steps.TrueForAll(step => step.Status == StepExecutionStatus.Completed);
    }

    private StepExecution? FindStep(StepExecutionId stepExecutionId)
    {
        return _steps.Find(step => step.Id == stepExecutionId);
    }

    private static string? NormalizeFailureMessage(string? failureMessage)
    {
        return string.IsNullOrWhiteSpace(failureMessage)
            ? null
            : failureMessage.Trim();
    }
}
