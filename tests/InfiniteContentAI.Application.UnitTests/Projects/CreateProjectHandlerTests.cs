using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Application.Projects.CreateProject;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Application.UnitTests.Projects;

public sealed class CreateProjectHandlerTests
{
    [Fact]
    public async Task HandleUsesCurrentOrganizationAndCommits()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        var repository = new RepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = new CreateProjectHandler(
            new CurrentOrganizationStub(organizationId),
            new CurrentUserStub("user-123"),
            repository,
            unitOfWork,
            new ClockStub());

        Result<CreateProjectResult> result = await handler.HandleAsync(
            new CreateProjectCommand("Projeto", "Descrição"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.Project);
        Assert.Equal(organizationId, repository.Project.OrganizationId);
        Assert.Equal(1, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePropagatesMissingOrganization()
    {
        var handler = new CreateProjectHandler(
            new CurrentOrganizationStub(null),
            new CurrentUserStub("user-123"),
            new RepositorySpy(),
            new UnitOfWorkSpy(),
            new ClockStub());

        Result<CreateProjectResult> result = await handler.HandleAsync(
            new CreateProjectCommand("Projeto", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Identity.OrganizationRequired", result.Error.Code);
    }

    private sealed class CurrentOrganizationStub(OrganizationId? organizationId)
        : ICurrentOrganization
    {
        public OrganizationId? OrganizationId { get; } = organizationId;
        public bool IsAvailable => OrganizationId.HasValue;

        public Result<OrganizationId> Require() =>
            OrganizationId.HasValue
                ? Result.Success(OrganizationId.Value)
                : Result.Failure<OrganizationId>(IdentityErrors.OrganizationRequired);
    }

    private sealed class CurrentUserStub(string userId) : ICurrentUser
    {
        public string? UserId { get; } = userId;
    }

    private sealed class RepositorySpy : IProjectRepository
    {
        public Project? Project { get; private set; }

        public Task AddAsync(Project project, CancellationToken cancellationToken)
        {
            Project = project;
            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkSpy : IUnitOfWork
    {
        public int CallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class ClockStub : IClock
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 7, 29, 12, 0, 0, TimeSpan.Zero);
    }
}
