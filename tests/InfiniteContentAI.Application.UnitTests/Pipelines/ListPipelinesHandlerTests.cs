using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Pipelines.ListPipelines;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.UnitTests.Pipelines;

public sealed class ListPipelinesHandlerTests
{
    [Fact]
    public async Task HandleListsProjectPipelinesAndPropagatesScopePaginationAndCancellation()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        ProjectId projectId = ProjectId.New();
        var projectQueries = ProjectExists(projectId);
        var expected = new PaginatedResult<PipelineListItem>(
            [CreateListItem(projectId)],
            2,
            5,
            6);
        var pipelineQueries = new PipelineQueriesStub { Page = expected };
        var handler = CreateHandler(
            organizationId,
            projectQueries,
            pipelineQueries);
        using var cancellation = new CancellationTokenSource();

        Result<PaginatedResult<PipelineListItem>> result = await handler.HandleAsync(
            new ListPipelinesQuery(projectId.Value, 2, 5),
            cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
        Assert.Single(result.Value.Items);
        Assert.Equal(organizationId, projectQueries.RequestedOrganizationId);
        Assert.Equal(organizationId, pipelineQueries.RequestedOrganizationId);
        Assert.Equal(projectId, pipelineQueries.RequestedProjectId);
        Assert.Equal(2, pipelineQueries.RequestedPage);
        Assert.Equal(5, pipelineQueries.RequestedPageSize);
        Assert.Equal(cancellation.Token, projectQueries.CancellationToken);
        Assert.Equal(cancellation.Token, pipelineQueries.ListCancellationToken);
    }

    [Fact]
    public async Task HandleReturnsEmptyPageForExistingProjectWithoutPipelines()
    {
        ProjectId projectId = ProjectId.New();
        var pipelineQueries = new PipelineQueriesStub();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            ProjectExists(projectId),
            pipelineQueries);

        Result<PaginatedResult<PipelineListItem>> result = await handler.HandleAsync(
            new ListPipelinesQuery(projectId.Value),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, pipelineQueries.ListCallCount);
    }

    [Fact]
    public async Task HandleReturnsProjectNotFoundWhenProjectDoesNotExist()
    {
        var pipelineQueries = new PipelineQueriesStub();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            new ProjectQueriesStub(),
            pipelineQueries);

        Result<PaginatedResult<PipelineListItem>> result = await handler.HandleAsync(
            new ListPipelinesQuery(ProjectId.New().Value),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.ProjectNotFound, result.Error);
        Assert.Equal(0, pipelineQueries.ListCallCount);
    }

    [Fact]
    public async Task HandleTreatsCrossTenantProjectAsNotFound()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        var projectQueries = new ProjectQueriesStub();
        var handler = CreateHandler(
            organizationId,
            projectQueries,
            new PipelineQueriesStub());

        Result<PaginatedResult<PipelineListItem>> result = await handler.HandleAsync(
            new ListPipelinesQuery(ProjectId.New().Value),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.ProjectNotFound, result.Error);
        Assert.Equal(organizationId, projectQueries.RequestedOrganizationId);
    }

    [Fact]
    public async Task HandleReturnsIdentityErrorWithoutOrganization()
    {
        var projectQueries = new ProjectQueriesStub();
        var pipelineQueries = new PipelineQueriesStub();
        var handler = CreateHandler(null, projectQueries, pipelineQueries);

        Result<PaginatedResult<PipelineListItem>> result = await handler.HandleAsync(
            new ListPipelinesQuery(ProjectId.New().Value),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrors.OrganizationRequired, result.Error);
        Assert.Equal(0, projectQueries.GetCallCount);
        Assert.Equal(0, pipelineQueries.ListCallCount);
    }

    [Fact]
    public async Task HandleRejectsEmptyProjectId()
    {
        var projectQueries = new ProjectQueriesStub();
        var pipelineQueries = new PipelineQueriesStub();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            projectQueries,
            pipelineQueries);

        Result<PaginatedResult<PipelineListItem>> result = await handler.HandleAsync(
            new ListPipelinesQuery(Guid.Empty),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ProjectRequired, result.Error);
        Assert.Equal(0, projectQueries.GetCallCount);
        Assert.Equal(0, pipelineQueries.ListCallCount);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, ListPipelinesValidator.MaximumPageSize + 1)]
    public async Task HandleRejectsInvalidPagination(int page, int pageSize)
    {
        var projectQueries = new ProjectQueriesStub();
        var pipelineQueries = new PipelineQueriesStub();
        var handler = CreateHandler(
            new OrganizationId(Guid.CreateVersion7()),
            projectQueries,
            pipelineQueries);

        Result<PaginatedResult<PipelineListItem>> result = await handler.HandleAsync(
            new ListPipelinesQuery(ProjectId.New().Value, page, pageSize),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.InvalidPagination, result.Error);
        Assert.Equal(0, projectQueries.GetCallCount);
        Assert.Equal(0, pipelineQueries.ListCallCount);
    }

    private static ListPipelinesHandler CreateHandler(
        OrganizationId? organizationId,
        ProjectQueriesStub projectQueries,
        PipelineQueriesStub pipelineQueries)
    {
        return new ListPipelinesHandler(
            new CurrentOrganizationStub(organizationId),
            projectQueries,
            pipelineQueries);
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

    private static PipelineListItem CreateListItem(ProjectId projectId)
    {
        return new PipelineListItem(
            Guid.CreateVersion7(),
            projectId.Value,
            "Pipeline",
            "draft",
            1,
            TestPipelineFactory.CreatedAt,
            null);
    }
}
