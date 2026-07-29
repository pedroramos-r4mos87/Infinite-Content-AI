using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.UnitTests.Projects;

public sealed class ProjectTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.CreateVersion7());
    private static readonly DateTimeOffset Now = new(2026, 7, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateReturnsActiveProjectWithCreationEvent()
    {
        var clock = new StubClock(Now);

        var result = Project.Create(
            OrganizationId,
            "  Canal de Tecnologia  ",
            "  Conteúdo sobre IA.  ",
            "user-123",
            clock);

        Assert.True(result.IsSuccess);
        Project project = result.Value;
        Assert.Equal(OrganizationId, project.OrganizationId);
        Assert.Equal("Canal de Tecnologia", project.Name.Value);
        Assert.Equal("Conteúdo sobre IA.", project.Description);
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.Equal(Now, project.CreatedAt);
        Assert.Equal("user-123", project.CreatedBy);
        Assert.Equal(7, project.Id.Value.Version);
        ProjectCreatedDomainEvent domainEvent =
            Assert.IsType<ProjectCreatedDomainEvent>(Assert.Single(project.DomainEvents));
        Assert.Equal(project.Id, domainEvent.ProjectId);
        Assert.Equal(OrganizationId, domainEvent.OrganizationId);
        Assert.Equal(Now, domainEvent.OccurredAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRejectsMissingName(string? name)
    {
        var result = Project.Create(
            OrganizationId,
            name,
            null,
            "user-123",
            new StubClock(Now));

        Assert.True(result.IsFailure);
        Assert.Equal(ProjectErrors.NameRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsNameAboveMaximumLength()
    {
        string name = new('a', ProjectName.MaximumLength + 1);

        var result = Project.Create(
            OrganizationId,
            name,
            null,
            "user-123",
            new StubClock(Now));

        Assert.True(result.IsFailure);
        Assert.Equal(ProjectErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingOrganization()
    {
        var result = Project.Create(
            OrganizationId.Empty,
            "Projeto",
            null,
            "user-123",
            new StubClock(Now));

        Assert.True(result.IsFailure);
        Assert.Equal(ProjectErrors.OrganizationRequired, result.Error);
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
