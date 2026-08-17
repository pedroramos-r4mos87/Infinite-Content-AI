using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Data.IntegrationTests.Pipelines;

public sealed class PipelineQueriesTests(
    PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAsyncProjectsPublishedPipelineAndOrderedStepsWithoutTracking()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        Project project = CreateProject(organizationId, "Projeto");
        Pipeline pipeline = CreatePipeline(
            organizationId,
            project.Id,
            "  Pesquisa e Roteiro  ",
            "  Fluxo publicado.  ");
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);
        DateTimeOffset publishedAt = CreatedAt.AddMinutes(5);
        pipeline.Publish(new ClockStub(publishedAt));
        await PersistAsync([project], [pipeline]);

        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IPipelineQueries queries =
            scope.ServiceProvider.GetRequiredService<IPipelineQueries>();
        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        PipelineDetails? result = await queries.GetAsync(
            organizationId,
            pipeline.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(pipeline.Id.Value, result.Id);
        Assert.Equal(project.Id.Value, result.ProjectId);
        Assert.Equal("Pesquisa e Roteiro", result.Name);
        Assert.Equal("Fluxo publicado.", result.Description);
        Assert.Equal("published", result.Status);
        Assert.Equal(1, result.Version);
        Assert.Equal(CreatedAt, result.CreatedAt);
        Assert.Equal("integration-test", result.CreatedBy);
        Assert.Equal(publishedAt, result.PublishedAt);
        Assert.Equal([1, 2], result.Steps.Select(step => step.Position));
        Assert.Equal(["research", "script"], result.Steps.Select(step => step.Type));
        Assert.All(result.Steps, step => Assert.NotEqual(Guid.Empty, step.Id));
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetAndListApplyExplicitOrganizationScope()
    {
        OrganizationId organizationA = new(Guid.CreateVersion7());
        OrganizationId organizationB = new(Guid.CreateVersion7());
        Project projectA = CreateProject(organizationA, "Projeto A");
        Project projectB = CreateProject(organizationB, "Projeto B");
        Pipeline pipelineA = CreatePipeline(organizationA, projectA.Id, "Pipeline A");
        Pipeline pipelineB = CreatePipeline(organizationB, projectB.Id, "Pipeline B");
        await PersistAsync([projectA, projectB], [pipelineA, pipelineB]);

        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IPipelineQueries queries =
            scope.ServiceProvider.GetRequiredService<IPipelineQueries>();

        PipelineDetails? ownPipeline = await queries.GetAsync(
            organizationA,
            pipelineA.Id,
            CancellationToken.None);
        PipelineDetails? crossTenantPipeline = await queries.GetAsync(
            organizationA,
            pipelineB.Id,
            CancellationToken.None);
        PaginatedResult<PipelineListItem> ownList =
            await queries.ListByProjectAsync(
                organizationA,
                projectA.Id,
                1,
                20,
                CancellationToken.None);
        PaginatedResult<PipelineListItem> crossTenantList =
            await queries.ListByProjectAsync(
                organizationA,
                projectB.Id,
                1,
                20,
                CancellationToken.None);

        Assert.NotNull(ownPipeline);
        Assert.Null(crossTenantPipeline);
        Assert.Single(ownList.Items);
        Assert.Equal(pipelineA.Id.Value, ownList.Items.Single().Id);
        Assert.Empty(crossTenantList.Items);
        Assert.Equal(0, crossTenantList.TotalCount);
    }

    [Fact]
    public async Task ListByProjectAsyncPaginatesWithDeterministicOrderingAndNoDuplicates()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        OrganizationId otherOrganizationId = new(Guid.CreateVersion7());
        Project project = CreateProject(organizationId, "Projeto");
        Project otherProject = CreateProject(organizationId, "Outro Projeto");
        Project otherTenantProject = CreateProject(otherOrganizationId, "Outro Tenant");
        List<Pipeline> expectedPipelines = Enumerable.Range(1, 5)
            .Select(index => CreatePipeline(
                organizationId,
                project.Id,
                $"Pipeline {index}"))
            .ToList();
        Pipeline otherProjectPipeline = CreatePipeline(
            organizationId,
            otherProject.Id,
            "Fora do Project");
        Pipeline otherTenantPipeline = CreatePipeline(
            otherOrganizationId,
            otherTenantProject.Id,
            "Fora da Organization");
        await PersistAsync(
            [project, otherProject, otherTenantProject],
            [.. expectedPipelines, otherProjectPipeline, otherTenantPipeline]);

        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IPipelineQueries queries =
            scope.ServiceProvider.GetRequiredService<IPipelineQueries>();

        PaginatedResult<PipelineListItem> firstPage =
            await queries.ListByProjectAsync(
                organizationId,
                project.Id,
                1,
                2,
                CancellationToken.None);
        PaginatedResult<PipelineListItem> secondPage =
            await queries.ListByProjectAsync(
                organizationId,
                project.Id,
                2,
                2,
                CancellationToken.None);
        PaginatedResult<PipelineListItem> thirdPage =
            await queries.ListByProjectAsync(
                organizationId,
                project.Id,
                3,
                2,
                CancellationToken.None);

        PipelineListItem[] allItems =
            [.. firstPage.Items, .. secondPage.Items, .. thirdPage.Items];
        Guid[] expectedIds = expectedPipelines
            .Select(pipeline => pipeline.Id.Value)
            .OrderByDescending(id => id)
            .ToArray();

        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(3, firstPage.TotalPages);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(2, secondPage.Items.Count);
        Assert.Single(thirdPage.Items);
        Assert.Equal(expectedIds, allItems.Select(item => item.Id));
        Assert.Equal(5, allItems.Select(item => item.Id).Distinct().Count());
        Assert.All(allItems, item => Assert.Equal(project.Id.Value, item.ProjectId));
        Assert.All(allItems, item => Assert.Equal(CreatedAt, item.CreatedAt));
    }

    private async Task PersistAsync(
        IReadOnlyCollection<Project> projects,
        IReadOnlyCollection<Pipeline> pipelines)
    {
        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IPipelineRepository pipelineRepository =
            scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
        IUnitOfWork unitOfWork =
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        foreach (Project project in projects)
        {
            await projectRepository.AddAsync(project, CancellationToken.None);
        }

        foreach (Pipeline pipeline in pipelines)
        {
            await pipelineRepository.AddAsync(pipeline, CancellationToken.None);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    private static Project CreateProject(
        OrganizationId organizationId,
        string name)
    {
        return Project.Create(
            organizationId,
            name,
            null,
            "integration-test",
            new ClockStub(CreatedAt)).Value;
    }

    private static Pipeline CreatePipeline(
        OrganizationId organizationId,
        ProjectId projectId,
        string name,
        string? description = null)
    {
        return Pipeline.Create(
            organizationId,
            projectId,
            name,
            description,
            "integration-test",
            new ClockStub(CreatedAt)).Value;
    }

    private sealed class ClockStub(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
