using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Pipelines.PublishPipeline;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.UnitTests.Pipelines;

public sealed class PublishPipelineHandlerTests
{
    [Fact]
    public async Task HandlePublishesValidPipelineUsingClock()
    {
        Pipeline pipeline = TestPipelineFactory.CreatePublishable();
        DateTimeOffset publishedAt = TestPipelineFactory.CreatedAt.AddMinutes(5);
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork, publishedAt);

        Result<PublishPipelineResult> result = await handler.HandleAsync(
            new PublishPipelineCommand(pipeline.Id.Value),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(pipeline.Id.Value, result.Value.PipelineId);
        Assert.Equal("published", result.Value.Status);
        Assert.Equal(1, result.Value.Version);
        Assert.Equal(publishedAt, result.Value.PublishedAt);
        Assert.Equal(publishedAt, pipeline.PublishedAt);
        Assert.Equal(1, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleReturnsNotFoundWhenPipelineDoesNotExist()
    {
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<PublishPipelineResult> result = await handler.HandleAsync(
            new PublishPipelineCommand(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleTreatsCrossTenantPipelineAsNotFound()
    {
        OrganizationId currentOrganization = new(Guid.CreateVersion7());
        var repository = new PipelineRepositorySpy();
        var handler = CreateHandler(
            repository,
            new UnitOfWorkSpy(),
            organizationId: currentOrganization);

        Result<PublishPipelineResult> result = await handler.HandleAsync(
            new PublishPipelineCommand(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.NotFound, result.Error);
        Assert.Equal(currentOrganization, repository.RequestedOrganizationId);
    }

    [Fact]
    public async Task HandlePropagatesInvalidConfigurationWithoutSaving()
    {
        Pipeline pipeline = TestPipelineFactory.CreateDraft();
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<PublishPipelineResult> result = await handler.HandleAsync(
            new PublishPipelineCommand(pipeline.Id.Value),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ResearchStepRequired, result.Error);
        Assert.Equal(PipelineStatus.Draft, pipeline.Status);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePropagatesSecondPublicationWithoutSaving()
    {
        Pipeline pipeline = TestPipelineFactory.CreatePublishable();
        pipeline.Publish(
            new ClockStub(TestPipelineFactory.CreatedAt.AddMinutes(5)));
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<PublishPipelineResult> result = await handler.HandleAsync(
            new PublishPipelineCommand(pipeline.Id.Value),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.AlreadyPublished, result.Error);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePropagatesCancellationTokenToLoadAndSave()
    {
        Pipeline pipeline = TestPipelineFactory.CreatePublishable();
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);
        using var cancellation = new CancellationTokenSource();

        await handler.HandleAsync(
            new PublishPipelineCommand(pipeline.Id.Value),
            cancellation.Token);

        Assert.Equal(cancellation.Token, repository.GetCancellationToken);
        Assert.Equal(cancellation.Token, unitOfWork.CancellationToken);
    }

    private static PublishPipelineHandler CreateHandler(
        PipelineRepositorySpy repository,
        UnitOfWorkSpy unitOfWork,
        DateTimeOffset? publishedAt = null,
        OrganizationId? organizationId = null)
    {
        return new PublishPipelineHandler(
            new CurrentOrganizationStub(
                organizationId ?? TestPipelineFactory.OrganizationId),
            repository,
            unitOfWork,
            new ClockStub(
                publishedAt ?? TestPipelineFactory.CreatedAt.AddMinutes(5)));
    }
}
