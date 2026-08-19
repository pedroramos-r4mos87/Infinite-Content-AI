using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InfiniteContentAI.Api.IntegrationTests.Projects;
using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Data;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.SharedKernel.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Api.IntegrationTests.Executions;

public sealed class ExecutionEndpointTests(
    ProjectApiFixture fixture) : IClassFixture<ProjectApiFixture>
{
    [Fact]
    public async Task CompleteExecutionFlowReturnsOrderedStepsAndChainedArtifacts()
    {
        Guid pipelineId = await CreatePublishedPipelineAsync(
            fixture.Client,
            "Projeto Execution E2E");
        const string topic = "Arquitetura limpa para plataformas de conteudo";

        using HttpResponseMessage created = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            new
            {
                topic,
                organizationId = Guid.CreateVersion7(),
                createdBy = "attacker-controlled",
                status = "failed",
            });
        ExecutePayload? summary =
            await created.Content.ReadFromJsonAsync<ExecutePayload>();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(summary);
        Assert.NotEqual(Guid.Empty, summary.ExecutionId);
        Assert.Equal(pipelineId, summary.PipelineId);
        Assert.Equal(1, summary.PipelineVersion);
        Assert.Equal("completed", summary.Status);
        Assert.NotEqual(default, summary.CreatedAt);
        Assert.NotNull(summary.StartedAt);
        Assert.NotNull(summary.CompletedAt);
        Assert.Null(summary.FailedAt);
        Assert.Null(summary.FailureCode);
        Assert.Equal(
            $"/api/v1/executions/{summary.ExecutionId}",
            created.Headers.Location?.OriginalString);

        using HttpResponseMessage get = await fixture.Client.GetAsync(
            $"/api/v1/executions/{summary.ExecutionId}");
        ExecutionPayload? execution =
            await get.Content.ReadFromJsonAsync<ExecutionPayload>();

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.NotNull(execution);
        Assert.Equal(summary.ExecutionId, execution.ExecutionId);
        Assert.Equal(pipelineId, execution.PipelineId);
        Assert.Equal(topic, execution.Topic);
        Assert.Equal("completed", execution.Status);
        Assert.Equal([1, 2], execution.Steps.Select(step => step.Position));
        Assert.Equal(["research", "script"], execution.Steps.Select(step => step.Type));
        Assert.All(execution.Steps, step => Assert.Equal("completed", step.Status));
        Assert.All(execution.Steps, step => Assert.NotNull(step.StartedAt));
        Assert.All(execution.Steps, step => Assert.NotNull(step.CompletedAt));

        Assert.Equal(2, execution.Artifacts.Count);
        ArtifactPayload research = execution.Artifacts.Single(
            artifact => artifact.Type == "research");
        ArtifactPayload script = execution.Artifacts.Single(
            artifact => artifact.Type == "script");
        Assert.Contains(topic, research.Content, StringComparison.Ordinal);
        Assert.Contains(topic, script.Content, StringComparison.Ordinal);
        Assert.Contains(research.Content, script.Content, StringComparison.Ordinal);
        Assert.Equal(
            execution.Steps.Single(step => step.Type == "research").StepExecutionId,
            research.StepExecutionId);
        Assert.Equal(
            execution.Steps.Single(step => step.Type == "script").StepExecutionId,
            script.StepExecutionId);
    }

    [Fact]
    public async Task DraftPipelineReturnsConflictWithoutPersistingExecution()
    {
        Guid projectId = await CreateProjectAsync(fixture.Client, "Projeto Draft");
        Guid pipelineId = await CreatePipelineAsync(fixture.Client, projectId);
        await AddStepAsync(fixture.Client, pipelineId, "research", 1);
        await AddStepAsync(fixture.Client, pipelineId, "script", 2);
        int before = await CountExecutionsAsync();

        using HttpResponseMessage response = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            new { topic = "Nao deve executar" });

        await AssertProblemAsync(
            response,
            HttpStatusCode.Conflict,
            "PipelineExecution.PipelineNotPublished");
        Assert.Equal(before, await CountExecutionsAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteValidatesRequiredTopic(string topic)
    {
        Guid pipelineId = await CreatePublishedPipelineAsync(
            fixture.Client,
            $"Projeto Topic {Guid.NewGuid():N}");

        using HttpResponseMessage response = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            new { topic });

        await AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "PipelineExecution.TopicRequired");
    }

    [Fact]
    public async Task ExecuteValidatesLongTopicMissingBodyAndPipelineIds()
    {
        Guid pipelineId = await CreatePublishedPipelineAsync(
            fixture.Client,
            "Projeto Validation");
        using HttpResponseMessage tooLong = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            new { topic = new string('a', PipelineExecution.MaximumTopicLength + 1) });
        using HttpResponseMessage missingBody = await fixture.Client.PostAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            content: null);
        using HttpResponseMessage emptyId = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{Guid.Empty}/executions",
            new { topic = "Topic" });
        using HttpResponseMessage missing = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{Guid.CreateVersion7()}/executions",
            new { topic = "Topic" });

        await AssertProblemAsync(
            tooLong,
            HttpStatusCode.BadRequest,
            "PipelineExecution.TopicTooLong");
        Assert.Equal(HttpStatusCode.BadRequest, missingBody.StatusCode);
        await AssertProblemAsync(
            emptyId,
            HttpStatusCode.BadRequest,
            "PipelineExecution.PipelineRequired");
        await AssertProblemAsync(
            missing,
            HttpStatusCode.NotFound,
            "PipelineExecution.PipelineNotFound");
    }

    [Fact]
    public async Task GetValidatesExecutionIdAndMissingExecution()
    {
        using HttpResponseMessage emptyId = await fixture.Client.GetAsync(
            $"/api/v1/executions/{Guid.Empty}");
        using HttpResponseMessage missing = await fixture.Client.GetAsync(
            $"/api/v1/executions/{Guid.CreateVersion7()}");

        await AssertProblemAsync(
            emptyId,
            HttpStatusCode.BadRequest,
            "PipelineExecution.ExecutionRequired");
        await AssertProblemAsync(
            missing,
            HttpStatusCode.NotFound,
            "PipelineExecution.NotFound");
    }

    [Fact]
    public async Task ExecutionEndpointsHideCrossTenantResourcesAsNotFound()
    {
        using HttpClient owner = fixture.CreateClient(Guid.CreateVersion7());
        using HttpClient other = fixture.CreateClient(Guid.CreateVersion7());
        Guid pipelineId = await CreatePublishedPipelineAsync(owner, "Projeto Tenant");

        using HttpResponseMessage hiddenPipeline = await other.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            new { topic = "Topic" });
        await AssertProblemAsync(
            hiddenPipeline,
            HttpStatusCode.NotFound,
            "PipelineExecution.PipelineNotFound");

        ExecutePayload execution = await ExecuteAsync(owner, pipelineId, "Topic Tenant");
        using HttpResponseMessage hiddenExecution = await other.GetAsync(
            $"/api/v1/executions/{execution.ExecutionId}");
        await AssertProblemAsync(
            hiddenExecution,
            HttpStatusCode.NotFound,
            "PipelineExecution.NotFound");
    }

    [Fact]
    public async Task ResearchFailureIsPersistedAndDoesNotRunScript()
    {
        var provider = new ControlledAIProvider(failResearch: true, failScript: false);
        await using ProjectApiFixture.TestApiApplication application =
            await fixture.CreateApplicationAsync(provider);
        Guid pipelineId = await CreatePublishedPipelineAsync(
            application.Client,
            "Projeto Research Failure");

        ExecutePayload summary = await ExecuteAsync(
            application.Client,
            pipelineId,
            "Falha na pesquisa");
        ExecutionPayload execution = await GetExecutionAsync(
            application.Client,
            summary.ExecutionId);

        Assert.Equal("failed", summary.Status);
        Assert.Equal("AI.ResearchFailed", summary.FailureCode);
        Assert.NotNull(summary.FailedAt);
        Assert.Equal("failed", execution.Status);
        Assert.Equal(["failed", "pending"], execution.Steps.Select(step => step.Status));
        Assert.Empty(execution.Artifacts);
        Assert.Equal(0, provider.ScriptCalls);
    }

    [Fact]
    public async Task ScriptFailurePreservesResearchArtifactAndExecutionId()
    {
        var provider = new ControlledAIProvider(failResearch: false, failScript: true);
        await using ProjectApiFixture.TestApiApplication application =
            await fixture.CreateApplicationAsync(provider);
        Guid pipelineId = await CreatePublishedPipelineAsync(
            application.Client,
            "Projeto Script Failure");

        using HttpResponseMessage response = await application.Client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            new { topic = "Falha no roteiro" });
        ExecutePayload? summary =
            await response.Content.ReadFromJsonAsync<ExecutePayload>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(summary);
        Assert.Equal(
            $"/api/v1/executions/{summary.ExecutionId}",
            response.Headers.Location?.OriginalString);
        Assert.Equal("failed", summary.Status);
        Assert.Equal("AI.ScriptFailed", summary.FailureCode);

        ExecutionPayload execution = await GetExecutionAsync(
            application.Client,
            summary.ExecutionId);
        Assert.Equal(summary.ExecutionId, execution.ExecutionId);
        Assert.Equal(["completed", "failed"], execution.Steps.Select(step => step.Status));
        ArtifactPayload artifact = Assert.Single(execution.Artifacts);
        Assert.Equal("research", artifact.Type);
        Assert.DoesNotContain("failureMessage", await response.Content.ReadAsStringAsync());
        Assert.Equal(1, provider.ScriptCalls);
    }

    [Fact]
    public async Task OpenApiContainsExactlyTwoAuthorizedExecutionOperations()
    {
        using HttpResponseMessage response = await fixture.Client.GetAsync("/openapi/v1.json");
        JsonElement document = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement paths = document.GetProperty("paths");
        JsonElement execute = paths.GetProperty(
            "/api/v1/pipelines/{pipelineId}/executions");
        JsonElement get = paths.GetProperty("/api/v1/executions/{executionId}");
        Assert.True(execute.TryGetProperty("post", out JsonElement post));
        Assert.True(get.TryGetProperty("get", out JsonElement getOperation));
        Assert.True(post.TryGetProperty("requestBody", out _));
        Assert.True(post.GetProperty("responses").TryGetProperty("201", out _));
        Assert.True(getOperation.GetProperty("responses").TryGetProperty("200", out _));
        JsonElement executionIdParameter = Assert.Single(
            getOperation.GetProperty("parameters").EnumerateArray());
        Assert.Equal("executionId", executionIdParameter.GetProperty("name").GetString());
        Assert.Equal("path", executionIdParameter.GetProperty("in").GetString());
        Assert.True(executionIdParameter.GetProperty("required").GetBoolean());

        string[] expectedNames = ["ExecutePipeline", "GetPipelineExecution"];
        RouteEndpoint[] executionEndpoints = fixture.Services
            .GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => expectedNames.Contains(
                endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName))
            .ToArray();

        Assert.Equal(2, executionEndpoints.Length);
        Assert.All(
            executionEndpoints,
            endpoint => Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()));
    }

    private async Task<int> CountExecutionsAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.PipelineExecutions.CountAsync();
    }

    private static async Task<Guid> CreatePublishedPipelineAsync(
        HttpClient client,
        string projectName)
    {
        Guid projectId = await CreateProjectAsync(client, projectName);
        Guid pipelineId = await CreatePipelineAsync(client, projectId);
        await AddStepAsync(client, pipelineId, "research", 1);
        await AddStepAsync(client, pipelineId, "script", 2);
        using HttpResponseMessage response = await client.PostAsync(
            $"/api/v1/pipelines/{pipelineId}/publish",
            content: null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return pipelineId;
    }

    private static async Task<Guid> CreateProjectAsync(HttpClient client, string name)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/v1/projects",
            new { name });
        ProjectPayload? project = await response.Content.ReadFromJsonAsync<ProjectPayload>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(project);
        return project.Id;
    }

    private static async Task<Guid> CreatePipelineAsync(HttpClient client, Guid projectId)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/pipelines",
            new { name = "Pipeline E2E" });
        PipelinePayload? pipeline = await response.Content.ReadFromJsonAsync<PipelinePayload>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(pipeline);
        return pipeline.PipelineId;
    }

    private static async Task AddStepAsync(
        HttpClient client,
        Guid pipelineId,
        string type,
        int position)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/steps",
            new { type, position });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<ExecutePayload> ExecuteAsync(
        HttpClient client,
        Guid pipelineId,
        string topic)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/v1/pipelines/{pipelineId}/executions",
            new { topic });
        ExecutePayload? execution = await response.Content.ReadFromJsonAsync<ExecutePayload>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(execution);
        return execution;
    }

    private static async Task<ExecutionPayload> GetExecutionAsync(
        HttpClient client,
        Guid executionId)
    {
        using HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/executions/{executionId}");
        ExecutionPayload? execution =
            await response.Content.ReadFromJsonAsync<ExecutionPayload>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(execution);
        return execution;
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

    private sealed class ControlledAIProvider(
        bool failResearch,
        bool failScript) : IAIProvider
    {
        public int ScriptCalls { get; private set; }

        public Task<Result<AIResearchResult>> ResearchAsync(
            string topic,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<AIResearchResult> result = failResearch
                ? Result.Failure<AIResearchResult>(AIProviderErrors.ResearchFailed)
                : Result.Success(new AIResearchResult($"Research: {topic}"));
            return Task.FromResult(result);
        }

        public Task<Result<AIScriptResult>> GenerateScriptAsync(
            string topic,
            string researchContent,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ScriptCalls++;
            Result<AIScriptResult> result = failScript
                ? Result.Failure<AIScriptResult>(AIProviderErrors.ScriptFailed)
                : Result.Success(new AIScriptResult($"Script: {topic}\n{researchContent}"));
            return Task.FromResult(result);
        }
    }

    private sealed record ProjectPayload(Guid Id);

    private sealed record PipelinePayload(Guid PipelineId);

    private sealed record ExecutePayload(
        Guid ExecutionId,
        Guid PipelineId,
        int PipelineVersion,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? StartedAt,
        DateTimeOffset? CompletedAt,
        DateTimeOffset? FailedAt,
        string? FailureCode);

    private sealed record ExecutionPayload(
        Guid ExecutionId,
        Guid ProjectId,
        Guid PipelineId,
        int PipelineVersion,
        string Topic,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? StartedAt,
        DateTimeOffset? CompletedAt,
        DateTimeOffset? FailedAt,
        string? FailureCode,
        IReadOnlyList<StepPayload> Steps,
        IReadOnlyList<ArtifactPayload> Artifacts);

    private sealed record StepPayload(
        Guid StepExecutionId,
        Guid PipelineStepId,
        string Type,
        int Position,
        string Status,
        DateTimeOffset? StartedAt,
        DateTimeOffset? CompletedAt,
        DateTimeOffset? FailedAt,
        string? FailureCode);

    private sealed record ArtifactPayload(
        Guid ArtifactId,
        Guid StepExecutionId,
        string Type,
        string Content,
        DateTimeOffset CreatedAt);
}
