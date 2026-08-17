using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Pipelines.CreatePipeline;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.UnitTests.Pipelines;

public sealed class CreatePipelineHandlerTests
{
    [Fact]
    public async Task HandleCreatesPipelineForCurrentOrganizationAndUser()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        ProjectId projectId = ProjectId.New();
        var projectQueries = ProjectExists(projectId);
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(
            organizationId,
            "current-user",
            projectQueries,
            repository,
            unitOfWork);

        Result<CreatePipelineResult> result = await handler.HandleAsync(
            new CreatePipelineCommand(
                projectId.Value,
                "  Pesquisa e Roteiro  ",
                "  Descrição.  "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(projectId.Value, result.Value.ProjectId);
        Assert.Equal("Pesquisa e Roteiro", result.Value.Name);
        Assert.Equal("Descrição.", result.Value.Description);
        Assert.Equal("draft", result.Value.Status);
        Assert.Equal(1, result.Value.Version);
        Assert.Equal(TestPipelineFactory.CreatedAt, result.Value.CreatedAt);
        Assert.NotNull(repository.AddedPipeline);
        Assert.Equal(organizationId, repository.AddedPipeline.OrganizationId);
        Assert.Equal("current-user", repository.AddedPipeline.CreatedBy);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(1, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleUsesCurrentOrganizationToVerifyProjectOwnership()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        ProjectId projectId = ProjectId.New();
        var projectQueries = ProjectExists(projectId);
        var handler = CreateHandler(
            organizationId,
            "user-123",
            projectQueries,
            new PipelineRepositorySpy(),
            new UnitOfWorkSpy());

        await handler.HandleAsync(
            new CreatePipelineCommand(projectId.Value, "Pipeline", null),
            CancellationToken.None);

        Assert.Equal(organizationId, projectQueries.RequestedOrganizationId);
        Assert.Equal(projectId, projectQueries.RequestedProjectId);
    }

    [Fact]
    public async Task HandleReturnsNotFoundWhenProjectDoesNotExist()
    {
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            "user-123",
            new ProjectQueriesStub(),
            repository,
            unitOfWork);

        Result<CreatePipelineResult> result = await handler.HandleAsync(
            new CreatePipelineCommand(ProjectId.New().Value, "Pipeline", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.ProjectNotFound, result.Error);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleTreatsCrossTenantProjectAsNotFound()
    {
        OrganizationId currentOrganization = new(Guid.CreateVersion7());
        var projectQueries = new ProjectQueriesStub();
        var handler = CreateHandler(
            currentOrganization,
            "user-123",
            projectQueries,
            new PipelineRepositorySpy(),
            new UnitOfWorkSpy());

        Result<CreatePipelineResult> result = await handler.HandleAsync(
            new CreatePipelineCommand(ProjectId.New().Value, "Pipeline", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.ProjectNotFound, result.Error);
        Assert.Equal(currentOrganization, projectQueries.RequestedOrganizationId);
    }

    [Fact]
    public async Task HandleDoesNotPersistInvalidName()
    {
        var projectQueries = ProjectExists(ProjectId.New());
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            "user-123",
            projectQueries,
            repository,
            unitOfWork);

        Result<CreatePipelineResult> result = await handler.HandleAsync(
            new CreatePipelineCommand(ProjectId.New().Value, "   ", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.NameRequired, result.Error);
        Assert.Equal(0, projectQueries.GetCallCount);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleDoesNotPersistWithoutOrganization()
    {
        var projectQueries = new ProjectQueriesStub();
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(
            null,
            "user-123",
            projectQueries,
            repository,
            unitOfWork);

        Result<CreatePipelineResult> result = await handler.HandleAsync(
            new CreatePipelineCommand(ProjectId.New().Value, "Pipeline", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrors.OrganizationRequired, result.Error);
        Assert.Equal(0, projectQueries.GetCallCount);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleDoesNotPersistWithoutUser(string? userId)
    {
        var projectQueries = new ProjectQueriesStub();
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            userId,
            projectQueries,
            repository,
            unitOfWork);

        Result<CreatePipelineResult> result = await handler.HandleAsync(
            new CreatePipelineCommand(ProjectId.New().Value, "Pipeline", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrors.UserRequired, result.Error);
        Assert.Equal(0, projectQueries.GetCallCount);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePropagatesCancellationTokenToAllIoOperations()
    {
        ProjectId projectId = ProjectId.New();
        var projectQueries = ProjectExists(projectId);
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            "user-123",
            projectQueries,
            repository,
            unitOfWork);
        using var cancellation = new CancellationTokenSource();

        await handler.HandleAsync(
            new CreatePipelineCommand(projectId.Value, "Pipeline", null),
            cancellation.Token);

        Assert.Equal(cancellation.Token, projectQueries.CancellationToken);
        Assert.Equal(cancellation.Token, repository.AddCancellationToken);
        Assert.Equal(cancellation.Token, unitOfWork.CancellationToken);
    }

    private static CreatePipelineHandler CreateHandler(
        OrganizationId? organizationId,
        string? userId,
        ProjectQueriesStub projectQueries,
        PipelineRepositorySpy repository,
        UnitOfWorkSpy unitOfWork)
    {
        return new CreatePipelineHandler(
            new CurrentOrganizationStub(organizationId),
            new CurrentUserStub(userId),
            projectQueries,
            repository,
            unitOfWork,
            new ClockStub(TestPipelineFactory.CreatedAt));
    }

    private static ProjectQueriesStub ProjectExists(ProjectId projectId)
    {
        return new ProjectQueriesStub
        {
            Project = new ProjectDetails(
                projectId.Value,
                "Projeto",
                null,
                "active",
                TestPipelineFactory.CreatedAt),
        };
    }
}
