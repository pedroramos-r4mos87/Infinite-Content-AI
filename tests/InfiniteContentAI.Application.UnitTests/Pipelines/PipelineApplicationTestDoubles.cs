using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Application.UnitTests.Pipelines;

internal sealed class CurrentOrganizationStub(OrganizationId? organizationId)
    : ICurrentOrganization
{
    public OrganizationId? OrganizationId { get; } = organizationId;

    public bool IsAvailable => OrganizationId.HasValue;

    public Result<OrganizationId> Require()
    {
        return OrganizationId.HasValue
            ? Result.Success(OrganizationId.Value)
            : Result.Failure<OrganizationId>(IdentityErrors.OrganizationRequired);
    }
}

internal sealed class CurrentUserStub(string? userId) : ICurrentUser
{
    public string? UserId { get; } = userId;
}

internal sealed class ClockStub(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; } = utcNow;
}

internal sealed class ProjectQueriesStub : IProjectQueries
{
    public ProjectDetails? Project { get; init; }

    public int GetCallCount { get; private set; }

    public OrganizationId RequestedOrganizationId { get; private set; }

    public ProjectId RequestedProjectId { get; private set; }

    public CancellationToken CancellationToken { get; private set; }

    public Task<ProjectDetails?> GetAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        CancellationToken cancellationToken)
    {
        GetCallCount++;
        RequestedOrganizationId = organizationId;
        RequestedProjectId = projectId;
        CancellationToken = cancellationToken;
        return Task.FromResult(Project);
    }

    public Task<PaginatedResult<ProjectListItem>> ListAsync(
        OrganizationId organizationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            new PaginatedResult<ProjectListItem>([], page, pageSize, 0));
    }
}

internal sealed class PipelineRepositorySpy : IPipelineRepository
{
    public Pipeline? PipelineToReturn { get; init; }

    public Pipeline? AddedPipeline { get; private set; }

    public int AddCallCount { get; private set; }

    public int GetCallCount { get; private set; }

    public OrganizationId RequestedOrganizationId { get; private set; }

    public PipelineId RequestedPipelineId { get; private set; }

    public CancellationToken AddCancellationToken { get; private set; }

    public CancellationToken GetCancellationToken { get; private set; }

    public Task AddAsync(
        Pipeline pipeline,
        CancellationToken cancellationToken)
    {
        AddCallCount++;
        AddedPipeline = pipeline;
        AddCancellationToken = cancellationToken;
        return Task.CompletedTask;
    }

    public Task<Pipeline?> GetForUpdateAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        CancellationToken cancellationToken)
    {
        GetCallCount++;
        RequestedOrganizationId = organizationId;
        RequestedPipelineId = pipelineId;
        GetCancellationToken = cancellationToken;
        return Task.FromResult(PipelineToReturn);
    }
}

internal sealed class PipelineQueriesStub : IPipelineQueries
{
    public PipelineDetails? Pipeline { get; init; }

    public PaginatedResult<PipelineListItem> Page { get; init; } =
        new([], 1, 20, 0);

    public int GetCallCount { get; private set; }

    public int ListCallCount { get; private set; }

    public OrganizationId RequestedOrganizationId { get; private set; }

    public PipelineId RequestedPipelineId { get; private set; }

    public ProjectId RequestedProjectId { get; private set; }

    public int RequestedPage { get; private set; }

    public int RequestedPageSize { get; private set; }

    public CancellationToken GetCancellationToken { get; private set; }

    public CancellationToken ListCancellationToken { get; private set; }

    public Task<PipelineDetails?> GetAsync(
        OrganizationId organizationId,
        PipelineId pipelineId,
        CancellationToken cancellationToken)
    {
        GetCallCount++;
        RequestedOrganizationId = organizationId;
        RequestedPipelineId = pipelineId;
        GetCancellationToken = cancellationToken;
        return Task.FromResult(Pipeline);
    }

    public Task<PaginatedResult<PipelineListItem>> ListByProjectAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ListCallCount++;
        RequestedOrganizationId = organizationId;
        RequestedProjectId = projectId;
        RequestedPage = page;
        RequestedPageSize = pageSize;
        ListCancellationToken = cancellationToken;
        return Task.FromResult(Page);
    }
}

internal sealed class UnitOfWorkSpy : IUnitOfWork
{
    public int CallCount { get; private set; }

    public CancellationToken CancellationToken { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        CallCount++;
        CancellationToken = cancellationToken;
        return Task.FromResult(1);
    }
}

internal static class TestPipelineFactory
{
    public static readonly OrganizationId OrganizationId =
        new(Guid.CreateVersion7());

    public static readonly ProjectId ProjectId = ProjectId.New();

    public static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public static Pipeline CreateDraft()
    {
        return Pipeline.Create(
            OrganizationId,
            ProjectId,
            "Pipeline",
            null,
            "user-123",
            new ClockStub(CreatedAt)).Value;
    }

    public static Pipeline CreatePublishable()
    {
        Pipeline pipeline = CreateDraft();
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);
        return pipeline;
    }
}
