using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using InfiniteContentAI.Api.IntegrationTests.Projects;
using InfiniteContentAI.Application.Pipelines.ListPipelines;
using InfiniteContentAI.Domain.Pipelines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Api.IntegrationTests.Pipelines;

public sealed class PipelineEndpointTests(
    ProjectApiFixture fixture) : IClassFixture<ProjectApiFixture>
{
    [Fact]
    public async Task CompletePipelineFlowPersistsAndReturnsExpectedHttpContracts()
    {
        Guid projectId = await CreateProjectAsync(fixture.Client, "Projeto E2E");

        using HttpResponseMessage created = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            new
            {
                name = "Main Content Pipeline",
                description = "Research and script workflow",
                organizationId = Guid.CreateVersion7(),
                createdBy = "attacker-controlled",
                status = "published",
                version = 99,
            });
        CreatePipelinePayload? pipeline =
            await created.Content.ReadFromJsonAsync<CreatePipelinePayload>();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(pipeline);
        Assert.NotEqual(Guid.Empty, pipeline.PipelineId);
        Assert.Equal(projectId, pipeline.ProjectId);
        Assert.Equal("Main Content Pipeline", pipeline.Name);
        Assert.Equal("draft", pipeline.Status);
        Assert.Equal(1, pipeline.Version);
        Assert.Equal(
            $"/api/v1/pipelines/{pipeline.PipelineId}",
            created.Headers.Location?.OriginalString);

        AddStepPayload research = await AddStepAsync(
            fixture.Client,
            pipeline.PipelineId,
            "research",
            1);
        AddStepPayload script = await AddStepAsync(
            fixture.Client,
            pipeline.PipelineId,
            "script",
            2);

        Assert.NotEqual(Guid.Empty, research.StepId);
        Assert.Equal("research", research.Type);
        Assert.Equal(1, research.Position);
        Assert.NotEqual(Guid.Empty, script.StepId);
        Assert.Equal("script", script.Type);
        Assert.Equal(2, script.Position);

        using HttpResponseMessage publishedResponse = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/publish",
            content: null);
        PublishPipelinePayload? published =
            await publishedResponse.Content.ReadFromJsonAsync<PublishPipelinePayload>();

        Assert.Equal(HttpStatusCode.OK, publishedResponse.StatusCode);
        Assert.NotNull(published);
        Assert.Equal(pipeline.PipelineId, published.PipelineId);
        Assert.Equal("published", published.Status);
        Assert.Equal(1, published.Version);
        Assert.NotEqual(default, published.PublishedAt);

        using HttpResponseMessage getResponse = await fixture.Client.GetAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}");
        PipelinePayload? details =
            await getResponse.Content.ReadFromJsonAsync<PipelinePayload>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(details);
        Assert.Equal(pipeline.PipelineId, details.PipelineId);
        Assert.Equal(projectId, details.ProjectId);
        Assert.Equal("Main Content Pipeline", details.Name);
        Assert.Equal("Research and script workflow", details.Description);
        Assert.Equal("published", details.Status);
        Assert.Equal(1, details.Version);
        Assert.Equal(
            published.PublishedAt.ToUnixTimeMilliseconds(),
            details.PublishedAt?.ToUnixTimeMilliseconds());
        Assert.Equal(
            "019c0000-0000-7000-8000-000000000001",
            details.CreatedBy);
        Assert.Equal([1, 2], details.Steps.Select(step => step.Position));
        Assert.Equal(["research", "script"], details.Steps.Select(step => step.Type));

        using HttpResponseMessage listResponse = await fixture.Client.GetAsync(
            $"/api/v1/projects/{projectId}/pipelines?page=1&pageSize=20");
        PipelinePagePayload? page =
            await listResponse.Content.ReadFromJsonAsync<PipelinePagePayload>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(page);
        Assert.Equal(1, page.Page);
        Assert.Equal(20, page.PageSize);
        Assert.Equal(1, page.TotalCount);
        PipelineListItemPayload listed = Assert.Single(page.Items);
        Assert.Equal(pipeline.PipelineId, listed.PipelineId);
        Assert.Equal(projectId, listed.ProjectId);
        Assert.Equal("published", listed.Status);
        Assert.Equal(1, listed.Version);
        Assert.Equal(
            published.PublishedAt.ToUnixTimeMilliseconds(),
            listed.PublishedAt?.ToUnixTimeMilliseconds());
    }

    [Fact]
    public async Task PipelineEndpointsHideCrossTenantResourcesAsNotFound()
    {
        Guid organizationA = Guid.CreateVersion7();
        Guid organizationB = Guid.CreateVersion7();
        using HttpClient clientA = fixture.CreateClient(organizationA);
        using HttpClient clientB = fixture.CreateClient(organizationB);
        Guid projectId = await CreateProjectAsync(clientA, "Projeto Organization A");
        CreatePipelinePayload pipeline = await CreatePipelineAsync(
            clientA,
            projectId,
            "Pipeline A");

        using HttpResponseMessage create = await clientB.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            new { name = "Pipeline invasor" });
        using HttpResponseMessage get = await clientB.GetAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}");
        using HttpResponseMessage addStep = await clientB.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "research", position = 1 });
        using HttpResponseMessage publish = await clientB.PostAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/publish",
            content: null);
        using HttpResponseMessage list = await clientB.GetAsync(
            $"/api/v1/projects/{projectId}/pipelines");

        await AssertProblemAsync(create, HttpStatusCode.NotFound, "Pipeline.ProjectNotFound");
        await AssertProblemAsync(get, HttpStatusCode.NotFound, "Pipeline.NotFound");
        await AssertProblemAsync(addStep, HttpStatusCode.NotFound, "Pipeline.NotFound");
        await AssertProblemAsync(publish, HttpStatusCode.NotFound, "Pipeline.NotFound");
        await AssertProblemAsync(list, HttpStatusCode.NotFound, "Pipeline.ProjectNotFound");
    }

    [Fact]
    public async Task CreatePipelineReturnsExpectedValidationAndNotFoundProblems()
    {
        Guid projectId = await CreateProjectAsync(fixture.Client, "Projeto de validação");

        using HttpResponseMessage emptyName = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            new { name = "   " });
        using HttpResponseMessage longDescription = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            new
            {
                name = "Pipeline",
                description = new string('a', Pipeline.MaximumDescriptionLength + 1),
            });
        using HttpResponseMessage missingProject = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/projects/{Guid.CreateVersion7()}/pipelines",
            new { name = "Pipeline" });
        using HttpResponseMessage emptyProjectId = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/projects/{Guid.Empty}/pipelines",
            new { name = "Pipeline" });
        using HttpResponseMessage missingBody = await fixture.Client.PostAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            content: null);
        using var malformedContent = new StringContent(
            "{",
            Encoding.UTF8,
            "application/json");
        using HttpResponseMessage malformedBody = await fixture.Client.PostAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            malformedContent);

        await AssertProblemAsync(emptyName, HttpStatusCode.BadRequest, "Pipeline.NameRequired");
        await AssertProblemAsync(
            longDescription,
            HttpStatusCode.BadRequest,
            "Pipeline.DescriptionTooLong");
        await AssertProblemAsync(
            missingProject,
            HttpStatusCode.NotFound,
            "Pipeline.ProjectNotFound");
        await AssertProblemAsync(
            emptyProjectId,
            HttpStatusCode.BadRequest,
            "Pipeline.ProjectRequired");
        Assert.Equal(HttpStatusCode.BadRequest, missingBody.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, malformedBody.StatusCode);
    }

    [Fact]
    public async Task AddStepReturnsExpectedValidationConflictAndStateProblems()
    {
        Guid projectId = await CreateProjectAsync(fixture.Client, "Projeto Steps");
        CreatePipelinePayload pipeline = await CreatePipelineAsync(
            fixture.Client,
            projectId,
            "Pipeline Steps");

        using HttpResponseMessage emptyId = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{Guid.Empty}/steps",
            new { type = "research", position = 1 });
        using HttpResponseMessage emptyType = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "", position = 1 });
        using HttpResponseMessage unknownType = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "seo", position = 1 });
        using HttpResponseMessage zeroPosition = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "research", position = 0 });
        using HttpResponseMessage negativePosition = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "research", position = -1 });
        using HttpResponseMessage scriptBeforeResearch = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "script", position = 2 });

        await AssertProblemAsync(emptyId, HttpStatusCode.BadRequest, "Pipeline.IdRequired");
        await AssertProblemAsync(emptyType, HttpStatusCode.BadRequest, "Pipeline.StepTypeInvalid");
        await AssertProblemAsync(unknownType, HttpStatusCode.BadRequest, "Pipeline.StepTypeInvalid");
        await AssertProblemAsync(
            zeroPosition,
            HttpStatusCode.BadRequest,
            "Pipeline.StepPositionInvalid");
        await AssertProblemAsync(
            negativePosition,
            HttpStatusCode.BadRequest,
            "Pipeline.StepPositionInvalid");
        await AssertProblemAsync(
            scriptBeforeResearch,
            HttpStatusCode.BadRequest,
            "Pipeline.ResearchStepRequired");

        await AddStepAsync(fixture.Client, pipeline.PipelineId, "research", 1);
        using HttpResponseMessage duplicateResearch = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "research", position = 2 });
        using HttpResponseMessage duplicatePosition = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "script", position = 1 });

        await AssertProblemAsync(
            duplicateResearch,
            HttpStatusCode.Conflict,
            "Pipeline.ResearchStepAlreadyExists");
        await AssertProblemAsync(
            duplicatePosition,
            HttpStatusCode.Conflict,
            "Pipeline.StepPositionAlreadyExists");

        await AddStepAsync(fixture.Client, pipeline.PipelineId, "script", 2);
        using HttpResponseMessage duplicateScript = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "script", position = 3 });
        await AssertProblemAsync(
            duplicateScript,
            HttpStatusCode.Conflict,
            "Pipeline.ScriptStepAlreadyExists");

        using HttpResponseMessage publish = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/publish",
            content: null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        using HttpResponseMessage publishedMutation = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipeline.PipelineId}/steps",
            new { type = "research", position = 3 });
        await AssertProblemAsync(
            publishedMutation,
            HttpStatusCode.Conflict,
            "Pipeline.NotDraft");
    }

    [Fact]
    public async Task PublishReturnsExpectedValidationNotFoundAndConflictProblems()
    {
        Guid projectId = await CreateProjectAsync(fixture.Client, "Projeto Publish");
        CreatePipelinePayload empty = await CreatePipelineAsync(
            fixture.Client,
            projectId,
            "Pipeline vazia");
        CreatePipelinePayload researchOnly = await CreatePipelineAsync(
            fixture.Client,
            projectId,
            "Pipeline Research");
        CreatePipelinePayload valid = await CreatePipelineAsync(
            fixture.Client,
            projectId,
            "Pipeline válida");
        await AddStepAsync(fixture.Client, researchOnly.PipelineId, "research", 1);
        await AddStepAsync(fixture.Client, valid.PipelineId, "research", 1);
        await AddStepAsync(fixture.Client, valid.PipelineId, "script", 2);

        using HttpResponseMessage emptyId = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{Guid.Empty}/publish",
            content: null);
        using HttpResponseMessage missing = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{Guid.CreateVersion7()}/publish",
            content: null);
        using HttpResponseMessage emptyPipeline = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{empty.PipelineId}/publish",
            content: null);
        using HttpResponseMessage onlyResearch = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{researchOnly.PipelineId}/publish",
            content: null);
        using HttpResponseMessage validPublish = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{valid.PipelineId}/publish",
            content: null);
        using HttpResponseMessage secondPublish = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{valid.PipelineId}/publish",
            content: null);

        await AssertProblemAsync(emptyId, HttpStatusCode.BadRequest, "Pipeline.IdRequired");
        await AssertProblemAsync(missing, HttpStatusCode.NotFound, "Pipeline.NotFound");
        await AssertProblemAsync(
            emptyPipeline,
            HttpStatusCode.BadRequest,
            "Pipeline.ResearchStepRequired");
        await AssertProblemAsync(
            onlyResearch,
            HttpStatusCode.BadRequest,
            "Pipeline.ScriptStepRequired");
        Assert.Equal(HttpStatusCode.OK, validPublish.StatusCode);
        await AssertProblemAsync(
            secondPublish,
            HttpStatusCode.Conflict,
            "Pipeline.AlreadyPublished");
    }

    [Fact]
    public async Task ReadEndpointsValidateIdsAndPagination()
    {
        Guid projectId = await CreateProjectAsync(fixture.Client, "Projeto Read");

        using HttpResponseMessage emptyPipelineId = await fixture.Client.GetAsync(
            $"/api/v1/pipelines/{Guid.Empty}");
        using HttpResponseMessage missingPipeline = await fixture.Client.GetAsync(
            $"/api/v1/pipelines/{Guid.CreateVersion7()}");
        using HttpResponseMessage emptyProjectId = await fixture.Client.GetAsync(
            $"/api/v1/projects/{Guid.Empty}/pipelines");
        using HttpResponseMessage invalidPage = await fixture.Client.GetAsync(
            $"/api/v1/projects/{projectId}/pipelines?page=0&pageSize=20");
        using HttpResponseMessage invalidPageSize = await fixture.Client.GetAsync(
            $"/api/v1/projects/{projectId}/pipelines?page=1&pageSize=" +
            (ListPipelinesValidator.MaximumPageSize + 1));

        await AssertProblemAsync(
            emptyPipelineId,
            HttpStatusCode.BadRequest,
            "Pipeline.IdRequired");
        await AssertProblemAsync(
            missingPipeline,
            HttpStatusCode.NotFound,
            "Pipeline.NotFound");
        await AssertProblemAsync(
            emptyProjectId,
            HttpStatusCode.BadRequest,
            "Pipeline.ProjectRequired");
        await AssertProblemAsync(
            invalidPage,
            HttpStatusCode.BadRequest,
            "Pipeline.InvalidPagination");
        await AssertProblemAsync(
            invalidPageSize,
            HttpStatusCode.BadRequest,
            "Pipeline.InvalidPagination");
    }

    [Fact]
    public async Task OpenApiContainsExactlyFiveAuthorizedPipelineOperations()
    {
        using HttpResponseMessage response = await fixture.Client.GetAsync("/openapi/v1.json");
        JsonElement document = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement paths = document.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/projects/{projectId}/pipelines", out _));
        Assert.True(paths.TryGetProperty("/api/v1/pipelines/{pipelineId}", out _));
        Assert.True(paths.TryGetProperty("/api/v1/pipelines/{pipelineId}/steps", out _));
        Assert.True(paths.TryGetProperty("/api/v1/pipelines/{pipelineId}/publish", out _));

        string[] expectedNames =
        [
            "CreatePipeline",
            "ListPipelinesByProject",
            "GetPipeline",
            "AddPipelineStep",
            "PublishPipeline",
        ];
        RouteEndpoint[] pipelineEndpoints = fixture.Services
            .GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => expectedNames.Contains(
                endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName))
            .ToArray();

        Assert.Equal(5, pipelineEndpoints.Length);
        Assert.All(
            pipelineEndpoints,
            endpoint => Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()));
    }

    private static async Task<Guid> CreateProjectAsync(
        HttpClient client,
        string name)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/v1/projects",
            new { name });
        ProjectPayload? project =
            await response.Content.ReadFromJsonAsync<ProjectPayload>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(project);
        return project.Id;
    }

    private static async Task<CreatePipelinePayload> CreatePipelineAsync(
        HttpClient client,
        Guid projectId,
        string name)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            new { name });
        CreatePipelinePayload? pipeline =
            await response.Content.ReadFromJsonAsync<CreatePipelinePayload>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(pipeline);
        return pipeline;
    }

    private static async Task<AddStepPayload> AddStepAsync(
        HttpClient client,
        Guid pipelineId,
        string type,
        int position)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/steps",
            new { type, position });
        AddStepPayload? step =
            await response.Content.ReadFromJsonAsync<AddStepPayload>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(step);
        return step;
    }

    private static async Task AssertProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode status,
        string code)
    {
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(status, response.StatusCode);
        Assert.Equal((int)status, problem.GetProperty("status").GetInt32());
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.False(problem.TryGetProperty("stackTrace", out _));
    }

    private sealed record ProjectPayload(Guid Id);

    private sealed record CreatePipelinePayload(
        Guid PipelineId,
        Guid ProjectId,
        string Name,
        string? Description,
        string Status,
        int Version,
        DateTimeOffset CreatedAt);

    private sealed record AddStepPayload(
        Guid StepId,
        string Type,
        int Position);

    private sealed record PublishPipelinePayload(
        Guid PipelineId,
        string Status,
        int Version,
        DateTimeOffset PublishedAt);

    private sealed record PipelinePayload(
        Guid PipelineId,
        Guid ProjectId,
        string Name,
        string? Description,
        string Status,
        int Version,
        DateTimeOffset CreatedAt,
        string CreatedBy,
        DateTimeOffset? PublishedAt,
        IReadOnlyList<PipelineStepPayload> Steps);

    private sealed record PipelineStepPayload(
        Guid StepId,
        string Type,
        int Position);

    private sealed record PipelinePagePayload(
        IReadOnlyList<PipelineListItemPayload> Items,
        int Page,
        int PageSize,
        long TotalCount,
        int TotalPages);

    private sealed record PipelineListItemPayload(
        Guid PipelineId,
        Guid ProjectId,
        string Name,
        string Status,
        int Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset? PublishedAt);
}
