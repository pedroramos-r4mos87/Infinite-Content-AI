using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Artifacts;
using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Data;
using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Data.IntegrationTests.Executions;

public sealed class PipelineExecutionPersistenceTests(
    PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task RepositoryPersistsAggregateAndIncrementalLifecycleWithTracking()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);
        var clock = new ExecutionTestData.IncrementingClock(
            ExecutionTestData.InitialTime.AddMinutes(2));
        PipelineExecution execution = ExecutionTestData.CreateExecution(
            organizationId,
            pipeline,
            clock);

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IPipelineExecutionRepository repository =
                scope.ServiceProvider.GetRequiredService<IPipelineExecutionRepository>();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await repository.AddAsync(execution, CancellationToken.None);
            Assert.Equal(3, await unitOfWork.SaveChangesAsync(CancellationToken.None));

            StepExecution research = execution.Steps.Single(
                step => step.Type == PipelineStepType.Research);
            StepExecution script = execution.Steps.Single(
                step => step.Type == PipelineStepType.Script);
            Assert.True(execution.Start(clock).IsSuccess);
            Assert.True(execution.StartStep(research.Id, clock).IsSuccess);
            Assert.Equal(2, await unitOfWork.SaveChangesAsync(CancellationToken.None));

            await using (AsyncServiceScope runningScope = database.Services.CreateAsyncScope())
            {
                ApplicationDbContext runningContext =
                    runningScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                PipelineExecution running = await runningContext.PipelineExecutions
                    .AsNoTracking()
                    .Include(candidate => candidate.Steps)
                    .SingleAsync(candidate => candidate.Id == execution.Id);
                Assert.Equal(PipelineExecutionStatus.Running, running.Status);
                Assert.Equal(
                    StepExecutionStatus.Running,
                    running.Steps.Single(step => step.Type == PipelineStepType.Research).Status);
            }

            Assert.True(execution.CompleteStep(research.Id, clock).IsSuccess);
            Assert.True(execution.StartStep(script.Id, clock).IsSuccess);
            Assert.Equal(2, await unitOfWork.SaveChangesAsync(CancellationToken.None));
            Assert.True(execution.CompleteStep(script.Id, clock).IsSuccess);
            Assert.True(execution.Complete(clock).IsSuccess);
            Assert.Equal(2, await unitOfWork.SaveChangesAsync(CancellationToken.None));
        }

        await using AsyncServiceScope readScope = database.Services.CreateAsyncScope();
        ApplicationDbContext dbContext =
            readScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PipelineExecution loaded = await dbContext.PipelineExecutions
            .AsNoTracking()
            .Include(candidate => candidate.Steps)
            .SingleAsync(candidate => candidate.Id == execution.Id);

        Assert.Equal(organizationId, loaded.OrganizationId);
        Assert.Equal(pipeline.ProjectId, loaded.ProjectId);
        Assert.Equal(pipeline.Id, loaded.PipelineId);
        Assert.Equal(1, loaded.PipelineVersion);
        Assert.Equal("Pipeline execution topic", loaded.Topic);
        Assert.Equal("integration-test", loaded.CreatedBy);
        Assert.Equal(PipelineExecutionStatus.Completed, loaded.Status);
        Assert.NotNull(loaded.StartedAt);
        Assert.NotNull(loaded.CompletedAt);
        Assert.Null(loaded.FailedAt);
        Assert.Null(loaded.FailureCode);
        Assert.Null(loaded.FailureMessage);
        Assert.Equal(2, loaded.Steps.Count);
        Assert.All(loaded.Steps, step => Assert.Equal(StepExecutionStatus.Completed, step.Status));
        Assert.Equal(
            [PipelineStepType.Research, PipelineStepType.Script],
            loaded.Steps.OrderBy(step => step.Position).Select(step => step.Type));
    }

    [Fact]
    public async Task PendingAndFailedStatesRoundTripWithNullableTimestampsAndFailureDetails()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);
        PipelineExecution pending = ExecutionTestData.CreateExecution(organizationId, pipeline);
        var clock = new ExecutionTestData.IncrementingClock(
            ExecutionTestData.InitialTime.AddMinutes(10));
        PipelineExecution failed = ExecutionTestData.CreateExecution(organizationId, pipeline, clock);
        StepExecution research = failed.Steps.Single(step => step.Type == PipelineStepType.Research);
        Assert.True(failed.Start(clock).IsSuccess);
        Assert.True(failed.StartStep(research.Id, clock).IsSuccess);
        Assert.True(failed.FailStep(research.Id, "AI.ResearchFailed", "Sanitized", clock).IsSuccess);
        Assert.True(failed.Fail("AI.ResearchFailed", "Sanitized", clock).IsSuccess);

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IPipelineExecutionRepository repository =
                scope.ServiceProvider.GetRequiredService<IPipelineExecutionRepository>();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await repository.AddAsync(pending, CancellationToken.None);
            await repository.AddAsync(failed, CancellationToken.None);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        await using AsyncServiceScope readScope = database.Services.CreateAsyncScope();
        ApplicationDbContext dbContext =
            readScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PipelineExecution loadedPending = await dbContext.PipelineExecutions
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == pending.Id);
        PipelineExecution loadedFailed = await dbContext.PipelineExecutions
            .AsNoTracking()
            .Include(candidate => candidate.Steps)
            .SingleAsync(candidate => candidate.Id == failed.Id);

        Assert.Equal(PipelineExecutionStatus.Pending, loadedPending.Status);
        Assert.Null(loadedPending.StartedAt);
        Assert.Null(loadedPending.CompletedAt);
        Assert.Null(loadedPending.FailedAt);
        Assert.Equal(PipelineExecutionStatus.Failed, loadedFailed.Status);
        Assert.NotNull(loadedFailed.StartedAt);
        Assert.Null(loadedFailed.CompletedAt);
        Assert.NotNull(loadedFailed.FailedAt);
        Assert.Equal("AI.ResearchFailed", loadedFailed.FailureCode);
        Assert.Equal("Sanitized", loadedFailed.FailureMessage);
        StepExecution loadedResearch = loadedFailed.Steps.Single(
            step => step.Type == PipelineStepType.Research);
        Assert.Equal(StepExecutionStatus.Failed, loadedResearch.Status);
        Assert.Equal("AI.ResearchFailed", loadedResearch.FailureCode);
        Assert.Equal("Sanitized", loadedResearch.FailureMessage);
    }

    [Fact]
    public async Task ArtifactRepositoryPersistsResearchAndScriptOwnershipAndContent()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);
        PipelineExecution execution = ExecutionTestData.CreateExecution(organizationId, pipeline);
        StepExecution researchStep = execution.Steps.Single(
            step => step.Type == PipelineStepType.Research);
        StepExecution scriptStep = execution.Steps.Single(
            step => step.Type == PipelineStepType.Script);
        var clock = new ExecutionTestData.FixedClock(
            ExecutionTestData.InitialTime.AddMinutes(20));
        Artifact research = Artifact.Create(
            organizationId,
            pipeline.ProjectId,
            execution.Id,
            researchStep.Id,
            ArtifactType.Research,
            "# Research content",
            clock).Value;
        Artifact script = Artifact.Create(
            organizationId,
            pipeline.ProjectId,
            execution.Id,
            scriptStep.Id,
            ArtifactType.Script,
            "# Script content",
            clock).Value;

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IPipelineExecutionRepository executionRepository =
                scope.ServiceProvider.GetRequiredService<IPipelineExecutionRepository>();
            IArtifactRepository artifactRepository =
                scope.ServiceProvider.GetRequiredService<IArtifactRepository>();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await executionRepository.AddAsync(execution, CancellationToken.None);
            await artifactRepository.AddAsync(research, CancellationToken.None);
            await artifactRepository.AddAsync(script, CancellationToken.None);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        await using AsyncServiceScope readScope = database.Services.CreateAsyncScope();
        ApplicationDbContext dbContext =
            readScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        List<Artifact> loaded = await dbContext.Artifacts
            .AsNoTracking()
            .Where(artifact => artifact.PipelineExecutionId == execution.Id)
            .OrderBy(artifact => artifact.Type)
            .ToListAsync();

        Assert.Equal(2, loaded.Count);
        Assert.Contains(loaded, artifact =>
            artifact.Type == ArtifactType.Research &&
            artifact.StepExecutionId == researchStep.Id &&
            artifact.Content == "# Research content");
        Assert.Contains(loaded, artifact =>
            artifact.Type == ArtifactType.Script &&
            artifact.StepExecutionId == scriptStep.Id &&
            artifact.Content == "# Script content");
        Assert.All(loaded, artifact =>
        {
            Assert.Equal(organizationId, artifact.OrganizationId);
            Assert.Equal(pipeline.ProjectId, artifact.ProjectId);
            Assert.Equal(execution.Id, artifact.PipelineExecutionId);
        });
    }
}
