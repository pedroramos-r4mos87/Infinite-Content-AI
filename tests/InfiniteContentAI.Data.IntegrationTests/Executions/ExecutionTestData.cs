using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Data.IntegrationTests.Executions;

internal static class ExecutionTestData
{
    public static readonly DateTimeOffset InitialTime =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    public static async Task<(Project Project, Pipeline Pipeline)> PersistPublishedPipelineAsync(
        PostgresDatabaseFixture database,
        OrganizationId organizationId)
    {
        Project project = Project.Create(
            organizationId,
            $"Project {Guid.NewGuid():N}",
            null,
            "integration-test",
            new FixedClock(InitialTime)).Value;
        Pipeline pipeline = Pipeline.Create(
            organizationId,
            project.Id,
            $"Pipeline {Guid.NewGuid():N}",
            null,
            "integration-test",
            new FixedClock(InitialTime)).Value;
        Assert.True(pipeline.AddResearchStep(1).IsSuccess);
        Assert.True(pipeline.AddScriptStep(2).IsSuccess);
        Assert.True(pipeline.Publish(new FixedClock(InitialTime.AddMinutes(1))).IsSuccess);

        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IPipelineRepository pipelineRepository =
            scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
        IUnitOfWork unitOfWork =
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await projectRepository.AddAsync(project, CancellationToken.None);
        await pipelineRepository.AddAsync(pipeline, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        return (project, pipeline);
    }

    public static PipelineExecution CreateExecution(
        OrganizationId organizationId,
        Pipeline pipeline,
        IClock? clock = null)
    {
        PipelineExecution execution = PipelineExecution.Create(
            organizationId,
            pipeline.ProjectId,
            pipeline.Id,
            pipeline.Version,
            "Pipeline execution topic",
            "integration-test",
            clock ?? new FixedClock(InitialTime.AddMinutes(2))).Value;
        foreach (PipelineStep step in pipeline.Steps.OrderBy(step => step.Position))
        {
            Assert.True(execution.AddStep(step.Id, step.Type, step.Position).IsSuccess);
        }

        return execution;
    }

    internal sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    internal sealed class IncrementingClock(DateTimeOffset utcNow) : IClock
    {
        private DateTimeOffset _utcNow = utcNow;

        public DateTimeOffset UtcNow
        {
            get
            {
                DateTimeOffset result = _utcNow;
                _utcNow = _utcNow.AddMinutes(1);
                return result;
            }
        }
    }
}
