using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Domain;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.Artifacts;

public sealed class Artifact : AggregateRoot<ArtifactId>
{
    public const int MaximumContentLength = 100_000;

    private Artifact(
        ArtifactId id,
        OrganizationId organizationId,
        ProjectId projectId,
        PipelineExecutionId pipelineExecutionId,
        StepExecutionId stepExecutionId,
        ArtifactType type,
        string content,
        DateTimeOffset createdAt)
        : base(id)
    {
        OrganizationId = organizationId;
        ProjectId = projectId;
        PipelineExecutionId = pipelineExecutionId;
        StepExecutionId = stepExecutionId;
        Type = type;
        Content = content;
        CreatedAt = createdAt;
    }

    private Artifact()
        : base(default)
    {
        Content = null!;
    }

    public OrganizationId OrganizationId { get; private init; }

    public ProjectId ProjectId { get; private init; }

    public PipelineExecutionId PipelineExecutionId { get; private init; }

    public StepExecutionId StepExecutionId { get; private init; }

    public ArtifactType Type { get; private init; }

    public string Content { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }

    public static Result<Artifact> Create(
        OrganizationId organizationId,
        ProjectId projectId,
        PipelineExecutionId pipelineExecutionId,
        StepExecutionId stepExecutionId,
        ArtifactType type,
        string? content,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (organizationId == OrganizationId.Empty)
        {
            return Result.Failure<Artifact>(ArtifactErrors.OrganizationRequired);
        }

        if (projectId.Value == Guid.Empty)
        {
            return Result.Failure<Artifact>(ArtifactErrors.ProjectRequired);
        }

        if (pipelineExecutionId.Value == Guid.Empty)
        {
            return Result.Failure<Artifact>(ArtifactErrors.ExecutionRequired);
        }

        if (stepExecutionId.Value == Guid.Empty)
        {
            return Result.Failure<Artifact>(ArtifactErrors.StepExecutionRequired);
        }

        if (!Enum.IsDefined(type))
        {
            return Result.Failure<Artifact>(ArtifactErrors.TypeInvalid);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<Artifact>(ArtifactErrors.ContentRequired);
        }

        if (content.Length > MaximumContentLength)
        {
            return Result.Failure<Artifact>(ArtifactErrors.ContentTooLong);
        }

        return Result.Success(
            new Artifact(
                ArtifactId.New(),
                organizationId,
                projectId,
                pipelineExecutionId,
                stepExecutionId,
                type,
                content,
                clock.UtcNow));
    }
}
