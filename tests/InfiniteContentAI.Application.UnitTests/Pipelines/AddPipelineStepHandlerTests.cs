using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Pipelines.AddPipelineStep;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.UnitTests.Pipelines;

public sealed class AddPipelineStepHandlerTests
{
    [Fact]
    public async Task HandleAddsResearchStep()
    {
        Pipeline pipeline = TestPipelineFactory.CreateDraft();
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<AddPipelineStepResult> result = await handler.HandleAsync(
            new AddPipelineStepCommand(pipeline.Id.Value, "research", 1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("research", result.Value.Type);
        Assert.Equal(1, result.Value.Position);
        PipelineStep step = Assert.Single(pipeline.Steps);
        Assert.Equal(result.Value.StepId, step.Id.Value);
        Assert.Equal(PipelineStepType.Research, step.Type);
        Assert.Equal(1, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleAddsScriptAfterResearch()
    {
        Pipeline pipeline = TestPipelineFactory.CreateDraft();
        pipeline.AddResearchStep(1);
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<AddPipelineStepResult> result = await handler.HandleAsync(
            new AddPipelineStepCommand(pipeline.Id.Value, "SCRIPT", 2),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("script", result.Value.Type);
        Assert.Equal(2, pipeline.Steps.Count);
        Assert.Contains(
            pipeline.Steps,
            step => step.Type == PipelineStepType.Script && step.Position == 2);
        Assert.Equal(1, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandleReturnsNotFoundWhenPipelineDoesNotExist()
    {
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<AddPipelineStepResult> result = await handler.HandleAsync(
            new AddPipelineStepCommand(Guid.CreateVersion7(), "research", 1),
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
            currentOrganization);

        Result<AddPipelineStepResult> result = await handler.HandleAsync(
            new AddPipelineStepCommand(Guid.CreateVersion7(), "research", 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.NotFound, result.Error);
        Assert.Equal(currentOrganization, repository.RequestedOrganizationId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("ResearchStep")]
    public async Task HandleRejectsUnknownStepType(string? type)
    {
        var repository = new PipelineRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<AddPipelineStepResult> result = await handler.HandleAsync(
            new AddPipelineStepCommand(Guid.CreateVersion7(), type, 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineApplicationErrors.StepTypeInvalid, result.Error);
        Assert.Equal(0, repository.GetCallCount);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePropagatesDomainFailureWithoutSaving()
    {
        Pipeline pipeline = TestPipelineFactory.CreateDraft();
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);

        Result<AddPipelineStepResult> result = await handler.HandleAsync(
            new AddPipelineStepCommand(pipeline.Id.Value, "research", 0),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.StepPositionInvalid, result.Error);
        Assert.Empty(pipeline.Steps);
        Assert.Equal(0, unitOfWork.CallCount);
    }

    [Fact]
    public async Task HandlePropagatesCancellationTokenToLoadAndSave()
    {
        Pipeline pipeline = TestPipelineFactory.CreateDraft();
        var repository = new PipelineRepositorySpy { PipelineToReturn = pipeline };
        var unitOfWork = new UnitOfWorkSpy();
        var handler = CreateHandler(repository, unitOfWork);
        using var cancellation = new CancellationTokenSource();

        await handler.HandleAsync(
            new AddPipelineStepCommand(pipeline.Id.Value, "research", 1),
            cancellation.Token);

        Assert.Equal(cancellation.Token, repository.GetCancellationToken);
        Assert.Equal(cancellation.Token, unitOfWork.CancellationToken);
    }

    private static AddPipelineStepHandler CreateHandler(
        PipelineRepositorySpy repository,
        UnitOfWorkSpy unitOfWork,
        OrganizationId? organizationId = null)
    {
        return new AddPipelineStepHandler(
            new CurrentOrganizationStub(
                organizationId ?? TestPipelineFactory.OrganizationId),
            repository,
            unitOfWork);
    }
}
