using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.SharedKernel.Domain;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.Projects;

public sealed class Project : AggregateRoot<ProjectId>
{
    private Project(
        ProjectId id,
        OrganizationId organizationId,
        ProjectName name,
        string? description,
        ProjectStatus status,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        Status = status;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    private Project()
        : base(default)
    {
        Name = null!;
        CreatedBy = null!;
    }

    public OrganizationId OrganizationId { get; private init; }

    public ProjectName Name { get; private init; }

    public string? Description { get; private init; }

    public ProjectStatus Status { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }

    public string CreatedBy { get; private init; }

    public static Result<Project> Create(
        OrganizationId organizationId,
        string? name,
        string? description,
        string? createdBy,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (organizationId == OrganizationId.Empty)
        {
            return Result.Failure<Project>(ProjectErrors.OrganizationRequired);
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            return Result.Failure<Project>(ProjectErrors.CreatedByRequired);
        }

        Result<ProjectName> projectName = ProjectName.Create(name);
        if (projectName.IsFailure)
        {
            return Result.Failure<Project>(projectName.Error);
        }

        var project = new Project(
            ProjectId.New(),
            organizationId,
            projectName.Value,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ProjectStatus.Active,
            clock.UtcNow,
            createdBy.Trim());

        project.RaiseDomainEvent(
            new ProjectCreatedDomainEvent(
                Guid.CreateVersion7(),
                project.Id,
                organizationId,
                project.CreatedAt));

        return Result.Success(project);
    }
}
