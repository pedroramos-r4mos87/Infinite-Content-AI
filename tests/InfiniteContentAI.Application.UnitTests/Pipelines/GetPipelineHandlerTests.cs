using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Pipelines.GetPipeline;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.UnitTests.Pipelines;

public sealed class GetPipelineHandlerTests
{
    [Fact]
    public async Task HandleReturnsProjectedPipelineAndPropagatesTenantAndCancellation()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        Guid pipelineId = Guid.CreateVersion7();
        PipelineDetails expected = CreateDetails(pipelineId);
        var queries = new PipelineQueriesStub { Pipeline = expected };
        var handler = new GetPipelineHandler(
            new CurrentOrganizationStub(organizationId),
            queries);
        using var cancellation = new CancellationTokenSource();

        Result<PipelineDetails> result = await handler.HandleAsync(
            new GetPipelineQuery(pipelineId),
            cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
        Assert.Equal(pipelineId, result.Value.Id);
        Assert.Equal("Pipeline", result.Value.Name);
        Assert.Equal("Descrição", result.Value.Description);
        Assert.Equal("published", result.Value.Status);
        Assert.Equal(1, result.Value.Version);
        Assert.Equal("user-123", result.Value.CreatedBy);
        Assert.NotNull(result.Value.PublishedAt);
        Assert.Equal([1, 2], result.Value.Steps.Select(step => step.Position));
        Assert.Equal(["research", "script"], result.Value.Steps.Select(step => step.Type));
        Assert.Equal(organizationId, queries.RequestedOrganizationId);
        Assert.Equal(new PipelineId(pipelineId), queries.RequestedPipelineId);
        Assert.Equal(cancellation.Token, queries.GetCancellationToken);
    }

    [Fact]
    public async Task HandleReturnsNotFoundWhenPipelineDoesNotExist()
    {
        var handler = new GetPipelineHandler(
            new CurrentOrganizationStub(new OrganizationId(Guid.CreateVersion7())),
            new PipelineQueriesStub());

        Result<PipelineDetails> result = await handler.HandleAsync(
            new GetPipelineQuery(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task HandleTreatsCrossTenantPipelineAsNotFound()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        var queries = new PipelineQueriesStub();
        var handler = new GetPipelineHandler(
            new CurrentOrganizationStub(organizationId),
            queries);

        Result<PipelineDetails> result = await handler.HandleAsync(
            new GetPipelineQuery(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.NotFound, result.Error);
        Assert.Equal(organizationId, queries.RequestedOrganizationId);
    }

    [Fact]
    public async Task HandleReturnsIdentityErrorWithoutOrganization()
    {
        var queries = new PipelineQueriesStub();
        var handler = new GetPipelineHandler(
            new CurrentOrganizationStub(null),
            queries);

        Result<PipelineDetails> result = await handler.HandleAsync(
            new GetPipelineQuery(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrors.OrganizationRequired, result.Error);
        Assert.Equal(0, queries.GetCallCount);
    }

    [Fact]
    public async Task HandleRejectsEmptyPipelineId()
    {
        var queries = new PipelineQueriesStub();
        var handler = new GetPipelineHandler(
            new CurrentOrganizationStub(new OrganizationId(Guid.CreateVersion7())),
            queries);

        Result<PipelineDetails> result = await handler.HandleAsync(
            new GetPipelineQuery(Guid.Empty),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.IdRequired, result.Error);
        Assert.Equal(0, queries.GetCallCount);
    }

    private static PipelineDetails CreateDetails(Guid pipelineId)
    {
        DateTimeOffset createdAt = TestPipelineFactory.CreatedAt;
        return new PipelineDetails(
            pipelineId,
            TestPipelineFactory.ProjectId.Value,
            "Pipeline",
            "Descrição",
            "published",
            1,
            createdAt,
            "user-123",
            createdAt.AddMinutes(5),
            [
                new PipelineStepDetails(Guid.CreateVersion7(), "research", 1),
                new PipelineStepDetails(Guid.CreateVersion7(), "script", 2),
            ]);
    }
}
