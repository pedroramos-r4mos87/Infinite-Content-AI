using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Data;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace InfiniteContentAI.Data.IntegrationTests.Pipelines;

public sealed class PipelinePersistenceTests(
    PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task MigrationCreatesAdditivePipelineSchemaInEmptyDatabase()
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(CancellationToken.None);

        await using NpgsqlCommand columnsCommand = connection.CreateCommand();
        columnsCommand.CommandText =
            """
            SELECT table_name, column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name IN ('pipelines', 'pipeline_steps')
            ORDER BY table_name, ordinal_position;
            """;

        var columns = new List<(string Table, string Name, string Type, string Nullable)>();
        await using (NpgsqlDataReader reader =
                     await columnsCommand.ExecuteReaderAsync(CancellationToken.None))
        {
            while (await reader.ReadAsync(CancellationToken.None))
            {
                columns.Add(
                    (reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }
        }

        Assert.Equal(
            ["id", "type", "position", "pipeline_id"],
            columns.Where(column => column.Table == "pipeline_steps")
                .Select(column => column.Name));
        Assert.Equal(
            [
                "id",
                "organization_id",
                "project_id",
                "name",
                "description",
                "status",
                "version",
                "created_at",
                "created_by",
                "published_at",
            ],
            columns.Where(column => column.Table == "pipelines")
                .Select(column => column.Name));
        Assert.Contains(
            columns,
            column =>
                column.Table == "pipelines" &&
                column.Name == "published_at" &&
                column.Type == "timestamp with time zone" &&
                column.Nullable == "YES");
        Assert.Contains(
            columns,
            column =>
                column.Table == "pipelines" &&
                column.Name == "version" &&
                column.Type == "integer" &&
                column.Nullable == "NO");

        await using NpgsqlCommand metadataCommand = connection.CreateCommand();
        metadataCommand.CommandText =
            """
            SELECT conname FROM pg_constraint
            WHERE conname IN (
                'FK_pipelines_projects_project_id',
                'FK_pipeline_steps_pipelines_pipeline_id',
                'ck_pipeline_steps_position_positive')
            UNION ALL
            SELECT indexname FROM pg_indexes
            WHERE schemaname = 'public' AND indexname IN (
                'ix_pipelines_organization_project_created_at_id',
                'ix_pipelines_project_id',
                'ux_pipeline_steps_pipeline_position',
                'ux_pipeline_steps_pipeline_type')
            UNION ALL
            SELECT "MigrationId" FROM "__EFMigrationsHistory";
            """;

        var names = new List<string>();
        await using NpgsqlDataReader metadataReader =
            await metadataCommand.ExecuteReaderAsync(CancellationToken.None);
        while (await metadataReader.ReadAsync(CancellationToken.None))
        {
            names.Add(metadataReader.GetString(0));
        }

        Assert.Contains("FK_pipelines_projects_project_id", names);
        Assert.Contains("FK_pipeline_steps_pipelines_pipeline_id", names);
        Assert.Contains("ck_pipeline_steps_position_positive", names);
        Assert.Contains("ix_pipelines_organization_project_created_at_id", names);
        Assert.Contains("ix_pipelines_project_id", names);
        Assert.Contains("ux_pipeline_steps_pipeline_position", names);
        Assert.Contains("ux_pipeline_steps_pipeline_type", names);
        Assert.Contains(names, name => name.EndsWith("_InitialCreate", StringComparison.Ordinal));
        Assert.Contains(names, name => name.EndsWith("_AddPipelines", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RepositoryPersistsAndMaterializesDraftPipelineWithSteps()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        Project project = CreateProject(organizationId);
        Pipeline pipeline = CreatePipeline(
            organizationId,
            project.Id,
            "  Pesquisa e Roteiro  ",
            "  Fluxo persistido.  ");
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);
        await PersistAsync(project, pipeline);

        await using AsyncServiceScope scope = database.Services.CreateAsyncScope();
        IPipelineRepository repository =
            scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Pipeline? loaded = await repository.GetForUpdateAsync(
            organizationId,
            pipeline.Id,
            CancellationToken.None);
        Pipeline? crossTenant = await repository.GetForUpdateAsync(
            new OrganizationId(Guid.CreateVersion7()),
            pipeline.Id,
            CancellationToken.None);
        Pipeline? missing = await repository.GetForUpdateAsync(
            organizationId,
            new PipelineId(Guid.CreateVersion7()),
            CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(pipeline.Id, loaded.Id);
        Assert.Equal(organizationId, loaded.OrganizationId);
        Assert.Equal(project.Id, loaded.ProjectId);
        Assert.Equal("Pesquisa e Roteiro", loaded.Name.Value);
        Assert.Equal("Fluxo persistido.", loaded.Description);
        Assert.Equal(PipelineStatus.Draft, loaded.Status);
        Assert.Equal(1, loaded.Version);
        Assert.Equal(CreatedAt, loaded.CreatedAt);
        Assert.Equal("integration-test", loaded.CreatedBy);
        Assert.Null(loaded.PublishedAt);
        Assert.Equal(2, loaded.Steps.Count);
        Assert.Contains(
            loaded.Steps,
            step => step.Type == PipelineStepType.Research && step.Position == 1);
        Assert.Contains(
            loaded.Steps,
            step => step.Type == PipelineStepType.Script && step.Position == 2);
        Assert.Equal(EntityState.Unchanged, dbContext.Entry(loaded).State);
        Assert.Null(crossTenant);
        Assert.Null(missing);
    }

    [Fact]
    public async Task TrackedAggregatePersistsNewStepThroughNormalChangeTracking()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        Project project = CreateProject(organizationId);
        Pipeline pipeline = CreatePipeline(organizationId, project.Id);
        pipeline.AddResearchStep(1);
        await PersistAsync(project, pipeline);

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IPipelineRepository repository =
                scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
            IUnitOfWork unitOfWork =
                scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            Pipeline loaded = (await repository.GetForUpdateAsync(
                organizationId,
                pipeline.Id,
                CancellationToken.None))!;

            loaded.AddScriptStep(2);
            Assert.Equal(1, await unitOfWork.SaveChangesAsync(CancellationToken.None));
        }

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IPipelineRepository repository =
                scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
            Pipeline reloaded = (await repository.GetForUpdateAsync(
                organizationId,
                pipeline.Id,
                CancellationToken.None))!;

            Assert.Equal(2, reloaded.Steps.Count);
            Assert.Contains(
                reloaded.Steps,
                step => step.Type == PipelineStepType.Script && step.Position == 2);
        }
    }

    [Fact]
    public async Task PublishRoundTripPersistsStatusTimestampVersionAndSteps()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        Project project = CreateProject(organizationId);
        Pipeline pipeline = CreatePipeline(organizationId, project.Id);
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);
        await PersistAsync(project, pipeline);
        DateTimeOffset publishedAt = CreatedAt.AddMinutes(5);

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IPipelineRepository repository =
                scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
            IUnitOfWork unitOfWork =
                scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            Pipeline loaded = (await repository.GetForUpdateAsync(
                organizationId,
                pipeline.Id,
                CancellationToken.None))!;

            Assert.True(loaded.Publish(new StubClock(publishedAt)).IsSuccess);
            Assert.Equal(1, await unitOfWork.SaveChangesAsync(CancellationToken.None));
        }

        await using (AsyncServiceScope scope = database.Services.CreateAsyncScope())
        {
            IPipelineRepository repository =
                scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
            Pipeline reloaded = (await repository.GetForUpdateAsync(
                organizationId,
                pipeline.Id,
                CancellationToken.None))!;

            Assert.Equal(PipelineStatus.Published, reloaded.Status);
            Assert.Equal(publishedAt, reloaded.PublishedAt);
            Assert.Equal(1, reloaded.Version);
            Assert.Equal(2, reloaded.Steps.Count);
            Assert.Contains(reloaded.Steps, step => step.Type == PipelineStepType.Research);
            Assert.Contains(reloaded.Steps, step => step.Type == PipelineStepType.Script);
        }
    }

    [Fact]
    public async Task PostgreSqlRejectsInvalidForeignKeysStepsAndStringLengths()
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(CancellationToken.None);

        Guid missingProjectPipelineId = Guid.CreateVersion7();
        await AssertPostgresFailureAsync(
            connection,
            CreatePipelineInsert(missingProjectPipelineId, Guid.CreateVersion7(), "Pipeline"),
            PostgresErrorCodes.ForeignKeyViolation);

        OrganizationId organizationId = new(Guid.CreateVersion7());
        Project project = CreateProject(organizationId);
        Pipeline pipeline = CreatePipeline(organizationId, project.Id);
        await PersistAsync(project, pipeline);

        await AssertPostgresFailureAsync(
            connection,
            CreateStepInsert(Guid.CreateVersion7(), Guid.CreateVersion7(), "Research", 1),
            PostgresErrorCodes.ForeignKeyViolation);
        await AssertPostgresFailureAsync(
            connection,
            CreateStepInsert(Guid.CreateVersion7(), pipeline.Id.Value, "Research", 0),
            PostgresErrorCodes.CheckViolation);

        await ExecuteAsync(
            connection,
            CreateStepInsert(Guid.CreateVersion7(), pipeline.Id.Value, "Research", 1));
        await AssertPostgresFailureAsync(
            connection,
            CreateStepInsert(Guid.CreateVersion7(), pipeline.Id.Value, "Script", 1),
            PostgresErrorCodes.UniqueViolation);
        await AssertPostgresFailureAsync(
            connection,
            CreateStepInsert(Guid.CreateVersion7(), pipeline.Id.Value, "Research", 2),
            PostgresErrorCodes.UniqueViolation);

        await AssertPostgresFailureAsync(
            connection,
            CreatePipelineInsert(
                Guid.CreateVersion7(),
                project.Id.Value,
                new string('a', PipelineName.MaximumLength + 1)),
            PostgresErrorCodes.StringDataRightTruncation);
    }

    private async Task PersistAsync(Project project, Pipeline pipeline)
    {
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
    }

    private static Project CreateProject(OrganizationId organizationId)
    {
        return Project.Create(
            organizationId,
            "Projeto",
            null,
            "integration-test",
            new StubClock(CreatedAt)).Value;
    }

    private static Pipeline CreatePipeline(
        OrganizationId organizationId,
        ProjectId projectId,
        string name = "Pipeline",
        string? description = null)
    {
        return Pipeline.Create(
            organizationId,
            projectId,
            name,
            description,
            "integration-test",
            new StubClock(CreatedAt)).Value;
    }

    private static NpgsqlCommand CreatePipelineInsert(
        Guid pipelineId,
        Guid projectId,
        string name)
    {
        var command = new NpgsqlCommand(
            """
            INSERT INTO pipelines
                (id, organization_id, project_id, name, status, version, created_at, created_by)
            VALUES
                (@id, @organization_id, @project_id, @name, 'Draft', 1, @created_at, 'test');
            """);
        command.Parameters.AddWithValue("id", pipelineId);
        command.Parameters.AddWithValue("organization_id", Guid.CreateVersion7());
        command.Parameters.AddWithValue("project_id", projectId);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("created_at", CreatedAt);
        return command;
    }

    private static NpgsqlCommand CreateStepInsert(
        Guid stepId,
        Guid pipelineId,
        string type,
        int position)
    {
        var command = new NpgsqlCommand(
            """
            INSERT INTO pipeline_steps (id, pipeline_id, type, position)
            VALUES (@id, @pipeline_id, @type, @position);
            """);
        command.Parameters.AddWithValue("id", stepId);
        command.Parameters.AddWithValue("pipeline_id", pipelineId);
        command.Parameters.AddWithValue("type", type);
        command.Parameters.AddWithValue("position", position);
        return command;
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        NpgsqlCommand command)
    {
        await using (command)
        {
            command.Connection = connection;
            await command.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    private static async Task AssertPostgresFailureAsync(
        NpgsqlConnection connection,
        NpgsqlCommand command,
        string expectedSqlState)
    {
        await using (command)
        {
            command.Connection = connection;
            PostgresException exception = await Assert.ThrowsAsync<PostgresException>(
                () => command.ExecuteNonQueryAsync(CancellationToken.None));
            Assert.Equal(expectedSqlState, exception.SqlState);
        }
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
