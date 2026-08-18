using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Artifacts;
using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Data;
using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Data.IntegrationTests.Executions;

public sealed class PipelineExecutionQueriesTests(
    PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task GetProjectsCompleteReadModelWithDeterministicCollectionsAndNoTracking()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);
        var clock = new ExecutionTestData.IncrementingClock(
            ExecutionTestData.InitialTime.AddHours(2));
        PipelineExecution execution = ExecutionTestData.CreateExecution(
            organizationId,
            pipeline,
            clock);
        StepExecution researchStep = execution.Steps.Single(
            step => step.Type == PipelineStepType.Research);
        StepExecution scriptStep = execution.Steps.Single(
            step => step.Type == PipelineStepType.Script);
        Assert.True(execution.Start(clock).IsSuccess);
        Assert.True(execution.StartStep(researchStep.Id, clock).IsSuccess);
        Artifact research = Artifact.Create(
            organizationId,
            pipeline.ProjectId,
            execution.Id,
            researchStep.Id,
            ArtifactType.Research,
            "Research content",
            clock).Value;
        Assert.True(execution.CompleteStep(researchStep.Id, clock).IsSuccess);
        Assert.True(execution.StartStep(scriptStep.Id, clock).IsSuccess);
        Artifact script = Artifact.Create(
            organizationId,
            pipeline.ProjectId,
            execution.Id,
            scriptStep.Id,
            ArtifactType.Script,
            "Script content",
            clock).Value;
        Assert.True(execution.CompleteStep(scriptStep.Id, clock).IsSuccess);
        Assert.True(execution.Complete(clock).IsSuccess);
        await PersistAsync(execution, research, script);

        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IPipelineExecutionQueries queries =
            scope.ServiceProvider.GetRequiredService<IPipelineExecutionQueries>();
        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        PipelineExecutionDetails? details = await queries.GetAsync(
            organizationId,
            execution.Id,
            CancellationToken.None);
        PipelineExecutionDetails? crossTenant = await queries.GetAsync(
            new OrganizationId(Guid.CreateVersion7()),
            execution.Id,
            CancellationToken.None);

        Assert.NotNull(details);
        Assert.Equal(execution.Id.Value, details.ExecutionId);
        Assert.Equal(pipeline.ProjectId.Value, details.ProjectId);
        Assert.Equal(pipeline.Id.Value, details.PipelineId);
        Assert.Equal(1, details.PipelineVersion);
        Assert.Equal("Pipeline execution topic", details.Topic);
        Assert.Equal("completed", details.Status);
        Assert.NotNull(details.StartedAt);
        Assert.NotNull(details.CompletedAt);
        Assert.Null(details.FailedAt);
        Assert.Null(details.FailureCode);
        Assert.Equal([1, 2], details.Steps.Select(step => step.Position));
        Assert.Equal(["research", "script"], details.Steps.Select(step => step.Type));
        Assert.Equal(["Research content", "Script content"], details.Artifacts.Select(a => a.Content));
        Assert.Equal(
            [researchStep.Id.Value, scriptStep.Id.Value],
            details.Artifacts.Select(artifact => artifact.StepExecutionId));
        Assert.Null(crossTenant);
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    private async Task PersistAsync(
        PipelineExecution execution,
        params Artifact[] artifacts)
    {
        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IPipelineExecutionRepository executionRepository =
            scope.ServiceProvider.GetRequiredService<IPipelineExecutionRepository>();
        IArtifactRepository artifactRepository =
            scope.ServiceProvider.GetRequiredService<IArtifactRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await executionRepository.AddAsync(execution, CancellationToken.None);
        foreach (Artifact artifact in artifacts)
        {
            await artifactRepository.AddAsync(artifact, CancellationToken.None);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}
