using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Api.IntegrationTests.Projects;

public sealed class ProjectEndpointTests(
    ProjectApiFixture fixture) : IClassFixture<ProjectApiFixture>
{
    [Fact]
    public async Task CreateGetAndListPersistProjectForCurrentOrganization()
    {
        using HttpResponseMessage created = await fixture.Client.PostAsJsonAsync(
            "/api/v1/projects",
            new
            {
                name = "Projeto HTTP",
                description = "Persistido no PostgreSQL.",
                organizationId = Guid.CreateVersion7(),
            });
        CreateProjectPayload? payload =
            await created.Content.ReadFromJsonAsync<CreateProjectPayload>();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload.Id);
        Assert.Equal("active", payload.Status);
        Assert.Equal($"/api/v1/projects/{payload.Id}", created.Headers.Location?.OriginalString);

        using HttpResponseMessage get =
            await fixture.Client.GetAsync($"/api/v1/projects/{payload.Id}");
        ProjectDetails? details =
            await get.Content.ReadFromJsonAsync<ProjectDetails>();

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.NotNull(details);
        Assert.Equal(payload.Id, details.Id);

        using HttpResponseMessage list =
            await fixture.Client.GetAsync("/api/v1/projects?page=1&pageSize=100");
        PaginatedPayload? page =
            await list.Content.ReadFromJsonAsync<PaginatedPayload>();

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.NotNull(page);
        Assert.Equal(1, page.Page);
        Assert.Equal(100, page.PageSize);
        Assert.Contains(page.Items, item => item.Id == payload.Id);

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<InfiniteContentAI.Data.ApplicationDbContext>();
        Project persisted = await dbContext.Projects.SingleAsync(
            project => project.Id == new ProjectId(payload.Id),
            CancellationToken.None);
        Assert.Equal(ProjectApiFixture.CurrentOrganizationId, persisted.OrganizationId.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateWithMissingNameReturnsValidationProblem(string name)
    {
        using HttpResponseMessage response = await fixture.Client.PostAsJsonAsync(
            "/api/v1/projects",
            new { name });

        await AssertValidationProblemAsync(response, "Project.NameRequired");
    }

    [Fact]
    public async Task CreateWithLongNameReturnsValidationProblem()
    {
        using HttpResponseMessage response = await fixture.Client.PostAsJsonAsync(
            "/api/v1/projects",
            new { name = new string('a', ProjectName.MaximumLength + 1) });

        await AssertValidationProblemAsync(response, "Project.NameTooLong");
    }

    [Fact]
    public async Task GetMissingAndCrossTenantProjectsReturnSameNotFoundProblem()
    {
        Project otherTenant = Project.Create(
            new OrganizationId(Guid.CreateVersion7()),
            "Projeto isolado",
            null,
            "integration-test",
            new StubClock()).Value;
        await PersistAsync(otherTenant);

        using HttpResponseMessage missing =
            await fixture.Client.GetAsync($"/api/v1/projects/{Guid.CreateVersion7()}");
        using HttpResponseMessage crossTenant =
            await fixture.Client.GetAsync($"/api/v1/projects/{otherTenant.Id.Value}");

        await AssertNotFoundProblemAsync(missing);
        await AssertNotFoundProblemAsync(crossTenant);
    }

    [Fact]
    public async Task ListExcludesOtherTenantAndUsesDeterministicOrdering()
    {
        DateTimeOffset sameInstant = new(2026, 7, 29, 18, 0, 0, TimeSpan.Zero);
        Project first = CreateCurrentProject("Lista A", sameInstant);
        Project second = CreateCurrentProject("Lista B", sameInstant);
        Project otherTenant = Project.Create(
            new OrganizationId(Guid.CreateVersion7()),
            "Lista secreta",
            null,
            "integration-test",
            new StubClock(sameInstant)).Value;
        await PersistAsync(first, second, otherTenant);

        using HttpResponseMessage response =
            await fixture.Client.GetAsync("/api/v1/projects?page=1&pageSize=100");
        PaginatedPayload? page =
            await response.Content.ReadFromJsonAsync<PaginatedPayload>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(page);
        Assert.DoesNotContain(page.Items, item => item.Id == otherTenant.Id.Value);
        Guid[] targetIds = [first.Id.Value, second.Id.Value];
        IEnumerable<Guid> returnedTargets = page.Items
            .Where(item => targetIds.Contains(item.Id))
            .Select(item => item.Id);
        Assert.Equal(
            targetIds.OrderDescending(),
            returnedTargets);
    }

    private async Task PersistAsync(params Project[] projects)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IProjectRepository repository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IUnitOfWork unitOfWork =
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        foreach (Project project in projects)
        {
            await repository.AddAsync(project, CancellationToken.None);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    private static Project CreateCurrentProject(string name, DateTimeOffset createdAt)
    {
        return Project.Create(
            new OrganizationId(ProjectApiFixture.CurrentOrganizationId),
            name,
            null,
            "integration-test",
            new StubClock(createdAt)).Value;
    }

    private static async Task AssertValidationProblemAsync(
        HttpResponseMessage response,
        string code)
    {
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("https://errors.infinitecontent.ai/validation", problem.GetProperty("type").GetString());
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.False(problem.TryGetProperty("stackTrace", out _));
    }

    private static async Task AssertNotFoundProblemAsync(HttpResponseMessage response)
    {
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Project.NotFound", problem.GetProperty("code").GetString());
        Assert.Equal("https://errors.infinitecontent.ai/not-found", problem.GetProperty("type").GetString());
    }

    private sealed record CreateProjectPayload(
        Guid Id,
        string Name,
        string? Description,
        string Status,
        DateTimeOffset CreatedAt);

    private sealed record PaginatedPayload(
        IReadOnlyList<ProjectListItem> Items,
        int Page,
        int PageSize,
        long TotalCount,
        int TotalPages);

    private sealed class StubClock : IClock
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 7, 29, 17, 0, 0, TimeSpan.Zero);

        public StubClock()
        {
        }

        public StubClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }
    }
}
