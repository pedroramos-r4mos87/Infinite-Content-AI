using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.Data;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace InfiniteContentAI.Data.IntegrationTests.Projects;

public sealed class ProjectPersistenceTests(
    PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task MigrationCreatesExpectedSchemaInEmptyDatabase()
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(CancellationToken.None);
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'projects'
            ORDER BY ordinal_position;
            """;

        var columns = new List<(string Name, string Type, string Nullable)>();
        await using NpgsqlDataReader reader =
            await command.ExecuteReaderAsync(CancellationToken.None);
        while (await reader.ReadAsync(CancellationToken.None))
        {
            columns.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }

        Assert.Equal(
            ["id", "organization_id", "name", "description", "status", "created_at", "created_by"],
            columns.Select(column => column.Name));
        Assert.Equal("uuid", columns[0].Type);
        Assert.Equal("uuid", columns[1].Type);
        Assert.Equal("timestamp with time zone", columns[5].Type);
        Assert.Equal("NO", columns[1].Nullable);
    }

    [Fact]
    public async Task RepositoryAndQueriesPersistMaterializeFilterAndPaginateProjects()
    {
        OrganizationId organizationA = new(Guid.CreateVersion7());
        OrganizationId organizationB = new(Guid.CreateVersion7());
        DateTimeOffset createdAt = new(2026, 7, 29, 12, 0, 0, TimeSpan.Zero);

        Project first = CreateProject(organizationA, "Primeiro", createdAt);
        Project second = CreateProject(organizationA, "Segundo", createdAt);
        Project otherTenant = CreateProject(organizationB, "Outro tenant", createdAt);

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IProjectRepository repository =
                scope.ServiceProvider.GetRequiredService<IProjectRepository>();
            IUnitOfWork unitOfWork =
                scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await repository.AddAsync(first, CancellationToken.None);
            await repository.AddAsync(second, CancellationToken.None);
            await repository.AddAsync(otherTenant, CancellationToken.None);
            Assert.Equal(3, await unitOfWork.SaveChangesAsync(CancellationToken.None));
        }

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IProjectQueries queries =
                scope.ServiceProvider.GetRequiredService<IProjectQueries>();
            ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Project aggregate = await dbContext.Projects
                .AsNoTracking()
                .SingleAsync(
                    project => project.Id == first.Id,
                    CancellationToken.None);
            ProjectDetails? materialized = await queries.GetAsync(
                organizationA,
                first.Id,
                CancellationToken.None);
            ProjectDetails? crossTenant = await queries.GetAsync(
                organizationB,
                first.Id,
                CancellationToken.None);
            ProjectDetails? missing = await queries.GetAsync(
                organizationA,
                new ProjectId(Guid.CreateVersion7()),
                CancellationToken.None);
            PaginatedResult<ProjectListItem> page = await queries.ListAsync(
                organizationA,
                1,
                1,
                CancellationToken.None);

            Assert.Equal(first.Id, aggregate.Id);
            Assert.Equal(organizationA, aggregate.OrganizationId);
            Assert.Equal(first.Name, aggregate.Name);
            Assert.Equal(ProjectStatus.Active, aggregate.Status);
            Assert.NotNull(materialized);
            Assert.Equal(first.Id.Value, materialized.Id);
            Assert.Equal(first.Name.Value, materialized.Name);
            Assert.Equal("active", materialized.Status);
            Assert.Null(crossTenant);
            Assert.Null(missing);
            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(
                new[] { first.Id.Value, second.Id.Value }.Max(),
                page.Items.Single().Id);
        }
    }

    [Fact]
    public async Task DatabaseRejectsNameAboveConfiguredLimit()
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(CancellationToken.None);
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO projects
                (id, organization_id, name, status, created_at, created_by)
            VALUES
                (@id, @organization_id, @name, 'Active', @created_at, 'test');
            """;
        command.Parameters.AddWithValue("id", Guid.CreateVersion7());
        command.Parameters.AddWithValue("organization_id", Guid.CreateVersion7());
        command.Parameters.AddWithValue("name", new string('a', ProjectName.MaximumLength + 1));
        command.Parameters.AddWithValue("created_at", DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<PostgresException>(
            () => command.ExecuteNonQueryAsync(CancellationToken.None));
    }

    private static Project CreateProject(
        OrganizationId organizationId,
        string name,
        DateTimeOffset createdAt)
    {
        return Project.Create(
            organizationId,
            name,
            null,
            "integration-test",
            new StubClock(createdAt)).Value;
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
