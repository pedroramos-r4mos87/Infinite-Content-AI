using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Application.Executions.GetPipelineExecution;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.UnitTests.Executions;

public sealed class GetPipelineExecutionHandlerTests
{
    [Fact]
    public async Task HandleReturnsTenantScopedExecutionDetails()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        Guid executionId = Guid.CreateVersion7();
        var queries = new PipelineExecutionQueriesStub
        {
            Details = CreateDetails(executionId),
        };
        var handler = new GetPipelineExecutionHandler(
            new ExecutionCurrentOrganizationStub(organizationId),
            queries);
        using var cancellation = new CancellationTokenSource();

        Result<GetPipelineExecutionResult> result = await handler.HandleAsync(
            new GetPipelineExecutionQuery(executionId),
            cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal(executionId, result.Value.ExecutionId);
        Assert.Single(result.Value.Steps);
        Assert.Single(result.Value.Artifacts);
        Assert.Equal(organizationId, queries.OrganizationId);
        Assert.Equal(new PipelineExecutionId(executionId), queries.ExecutionId);
        Assert.Equal(cancellation.Token, queries.CancellationToken);
    }

    [Fact]
    public async Task HandleReturnsNotFoundWhenQueryCannotSeeExecution()
    {
        var handler = new GetPipelineExecutionHandler(
            new ExecutionCurrentOrganizationStub(new OrganizationId(Guid.CreateVersion7())),
            new PipelineExecutionQueriesStub());

        Result<GetPipelineExecutionResult> result = await handler.HandleAsync(
            new GetPipelineExecutionQuery(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionApplicationErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task HandleStopsBeforeQueryWhenOrganizationIsMissing()
    {
        var queries = new PipelineExecutionQueriesStub();
        var handler = new GetPipelineExecutionHandler(
            new ExecutionCurrentOrganizationStub(null),
            queries);

        Result<GetPipelineExecutionResult> result = await handler.HandleAsync(
            new GetPipelineExecutionQuery(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Identity.OrganizationRequired", result.Error.Code);
        Assert.Equal(0, queries.CallCount);
    }

    [Fact]
    public async Task HandleStopsBeforeQueryWhenExecutionIdIsEmpty()
    {
        var queries = new PipelineExecutionQueriesStub();
        var handler = new GetPipelineExecutionHandler(
            new ExecutionCurrentOrganizationStub(new OrganizationId(Guid.CreateVersion7())),
            queries);

        Result<GetPipelineExecutionResult> result = await handler.HandleAsync(
            new GetPipelineExecutionQuery(Guid.Empty),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionApplicationErrors.ExecutionRequired, result.Error);
        Assert.Equal(0, queries.CallCount);
    }

    private static PipelineExecutionDetails CreateDetails(Guid executionId)
    {
        Guid stepId = Guid.CreateVersion7();
        DateTimeOffset createdAt = new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);
        return new PipelineExecutionDetails(
            executionId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1,
            "Topic",
            "completed",
            createdAt,
            createdAt,
            createdAt,
            null,
            null,
            [
                new StepExecutionDetails(
                    stepId,
                    Guid.CreateVersion7(),
                    "research",
                    1,
                    "completed",
                    createdAt,
                    createdAt,
                    null,
                    null),
            ],
            [
                new ArtifactDetails(
                    Guid.CreateVersion7(),
                    stepId,
                    "research",
                    "Content",
                    createdAt),
            ]);
    }

    private sealed class PipelineExecutionQueriesStub : IPipelineExecutionQueries
    {
        public PipelineExecutionDetails? Details { get; init; }

        public int CallCount { get; private set; }

        public OrganizationId OrganizationId { get; private set; }

        public PipelineExecutionId ExecutionId { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<PipelineExecutionDetails?> GetAsync(
            OrganizationId organizationId,
            PipelineExecutionId executionId,
            CancellationToken cancellationToken)
        {
            CallCount++;
            OrganizationId = organizationId;
            ExecutionId = executionId;
            CancellationToken = cancellationToken;
            return Task.FromResult(Details);
        }
    }
}
