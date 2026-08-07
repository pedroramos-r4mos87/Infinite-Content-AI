using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Domain;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.Pipelines;

public sealed class Pipeline : AggregateRoot<PipelineId>
{
    public const int MaximumDescriptionLength = 2000;
    public const int MaximumCreatedByLength = 200;

    private readonly List<PipelineStep> _steps = [];

    private Pipeline(
        PipelineId id,
        OrganizationId organizationId,
        ProjectId projectId,
        PipelineName name,
        string? description,
        PipelineStatus status,
        int version,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id)
    {
        OrganizationId = organizationId;
        ProjectId = projectId;
        Name = name;
        Description = description;
        Status = status;
        Version = version;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    private Pipeline()
        : base(default)
    {
        Name = null!;
        CreatedBy = null!;
    }

    public OrganizationId OrganizationId { get; private init; }

    public ProjectId ProjectId { get; private init; }

    public PipelineName Name { get; private init; }

    public string? Description { get; private init; }

    public PipelineStatus Status { get; private set; }

    public int Version { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }

    public string CreatedBy { get; private init; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public IReadOnlyCollection<PipelineStep> Steps => _steps.AsReadOnly();

    public static Result<Pipeline> Create(
        OrganizationId organizationId,
        ProjectId projectId,
        string? name,
        string? description,
        string? createdBy,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (organizationId == OrganizationId.Empty)
        {
            return Result.Failure<Pipeline>(PipelineErrors.OrganizationRequired);
        }

        if (projectId.Value == Guid.Empty)
        {
            return Result.Failure<Pipeline>(PipelineErrors.ProjectRequired);
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            return Result.Failure<Pipeline>(PipelineErrors.CreatedByRequired);
        }

        string normalizedCreatedBy = createdBy.Trim();
        if (normalizedCreatedBy.Length > MaximumCreatedByLength)
        {
            return Result.Failure<Pipeline>(PipelineErrors.CreatedByTooLong);
        }

        string? normalizedDescription =
            string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription?.Length > MaximumDescriptionLength)
        {
            return Result.Failure<Pipeline>(PipelineErrors.DescriptionTooLong);
        }

        Result<PipelineName> pipelineName = PipelineName.Create(name);
        if (pipelineName.IsFailure)
        {
            return Result.Failure<Pipeline>(pipelineName.Error);
        }

        var pipeline = new Pipeline(
            PipelineId.New(),
            organizationId,
            projectId,
            pipelineName.Value,
            normalizedDescription,
            PipelineStatus.Draft,
            1,
            clock.UtcNow,
            normalizedCreatedBy);

        pipeline.RaiseDomainEvent(
            new PipelineCreatedDomainEvent(
                Guid.CreateVersion7(),
                pipeline.Id,
                organizationId,
                projectId,
                pipeline.CreatedAt));

        return Result.Success(pipeline);
    }

    public Result<PipelineStepId> AddResearchStep(int position)
    {
        Result validation = ValidateStepCanBeAdded(position);
        if (validation.IsFailure)
        {
            return Result.Failure<PipelineStepId>(validation.Error);
        }

        if (_steps.Exists(step => step.Type == PipelineStepType.Research))
        {
            return Result.Failure<PipelineStepId>(PipelineErrors.ResearchStepAlreadyExists);
        }

        return AddStep(PipelineStepType.Research, position);
    }

    public Result<PipelineStepId> AddScriptStep(int position)
    {
        Result validation = ValidateStepCanBeAdded(position);
        if (validation.IsFailure)
        {
            return Result.Failure<PipelineStepId>(validation.Error);
        }

        if (_steps.Exists(step => step.Type == PipelineStepType.Script))
        {
            return Result.Failure<PipelineStepId>(PipelineErrors.ScriptStepAlreadyExists);
        }

        PipelineStep? researchStep =
            _steps.Find(step => step.Type == PipelineStepType.Research);
        if (researchStep is null)
        {
            return Result.Failure<PipelineStepId>(PipelineErrors.ResearchStepRequired);
        }

        if (position <= researchStep.Position)
        {
            return Result.Failure<PipelineStepId>(PipelineErrors.InvalidStepOrder);
        }

        return AddStep(PipelineStepType.Script, position);
    }

    public Result Publish(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status == PipelineStatus.Published)
        {
            return Result.Failure(PipelineErrors.AlreadyPublished);
        }

        PipelineStep? researchStep =
            _steps.Find(step => step.Type == PipelineStepType.Research);
        if (researchStep is null)
        {
            return Result.Failure(PipelineErrors.ResearchStepRequired);
        }

        PipelineStep? scriptStep =
            _steps.Find(step => step.Type == PipelineStepType.Script);
        if (scriptStep is null)
        {
            return Result.Failure(PipelineErrors.ScriptStepRequired);
        }

        if (_steps.Count != 2 ||
            researchStep.Position != 1 ||
            scriptStep.Position != 2)
        {
            return Result.Failure(PipelineErrors.InvalidStepOrder);
        }

        DateTimeOffset publishedAt = clock.UtcNow;
        PublishedAt = publishedAt;
        Status = PipelineStatus.Published;

        RaiseDomainEvent(
            new PipelinePublishedDomainEvent(
                Guid.CreateVersion7(),
                Id,
                OrganizationId,
                Version,
                publishedAt));

        return Result.Success();
    }

    private Result ValidateStepCanBeAdded(int position)
    {
        if (Status != PipelineStatus.Draft)
        {
            return Result.Failure(PipelineErrors.NotDraft);
        }

        if (position <= 0)
        {
            return Result.Failure(PipelineErrors.StepPositionInvalid);
        }

        if (_steps.Exists(step => step.Position == position))
        {
            return Result.Failure(PipelineErrors.StepPositionAlreadyExists);
        }

        return Result.Success();
    }

    private Result<PipelineStepId> AddStep(
        PipelineStepType type,
        int position)
    {
        PipelineStepId stepId = PipelineStepId.New();
        _steps.Add(new PipelineStep(stepId, type, position));

        return Result.Success(stepId);
    }
}
