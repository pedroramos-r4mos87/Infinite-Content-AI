using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using Npgsql;

namespace InfiniteContentAI.Data.IntegrationTests.Executions;

public sealed class ExecutionSchemaTests(
    PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task EmptyDatabaseMigrationCreatesExecutionArtifactSchemaAdditively()
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT table_name FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name IN ('pipeline_executions', 'step_executions', 'artifacts')
            UNION ALL
            SELECT conname FROM pg_constraint
            WHERE conname IN (
                'FK_pipeline_executions_pipelines_pipeline_id',
                'FK_pipeline_executions_projects_project_id',
                'FK_step_executions_pipeline_executions_pipeline_execution_id',
                'FK_step_executions_pipeline_steps_pipeline_step_id',
                'FK_artifacts_pipeline_executions_pipeline_execution_id',
                'FK_artifacts_step_executions_step_execution_id',
                'ck_pipeline_executions_pipeline_version_positive',
                'ck_step_executions_position_positive',
                'ck_artifacts_content_length')
            UNION ALL
            SELECT indexname FROM pg_indexes
            WHERE schemaname = 'public' AND indexname IN (
                'ix_pipeline_executions_organization_pipeline_created_at_id',
                'ix_pipeline_executions_organization_project_created_at_id',
                'ux_step_executions_execution_position',
                'ux_step_executions_execution_pipeline_step',
                'ux_artifacts_step_execution_id',
                'ix_artifacts_execution_created_at_id')
            UNION ALL
            SELECT "MigrationId" FROM "__EFMigrationsHistory";
            """;
        var names = new List<string>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        Assert.Contains("pipeline_executions", names);
        Assert.Contains("step_executions", names);
        Assert.Contains("artifacts", names);
        Assert.Contains("FK_pipeline_executions_pipelines_pipeline_id", names);
        Assert.Contains("FK_step_executions_pipeline_executions_pipeline_execution_id", names);
        Assert.Contains("FK_artifacts_pipeline_executions_pipeline_execution_id", names);
        Assert.Contains("FK_artifacts_step_executions_step_execution_id", names);
        Assert.Contains("ck_pipeline_executions_pipeline_version_positive", names);
        Assert.Contains("ck_step_executions_position_positive", names);
        Assert.Contains("ck_artifacts_content_length", names);
        Assert.Contains("ux_step_executions_execution_position", names);
        Assert.Contains("ux_step_executions_execution_pipeline_step", names);
        Assert.Contains("ux_artifacts_step_execution_id", names);
        Assert.Contains(names, name => name.EndsWith("_InitialCreate", StringComparison.Ordinal));
        Assert.Contains(names, name => name.EndsWith("_AddPipelines", StringComparison.Ordinal));
        Assert.Contains(
            names,
            name => name.EndsWith(
                "_AddPipelineExecutionsAndArtifacts",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task PostgreSqlRejectsInvalidExecutionStepAndArtifactRows()
    {
        OrganizationId organizationId = new(Guid.CreateVersion7());
        (_, Pipeline pipeline) = await ExecutionTestData.PersistPublishedPipelineAsync(
            database,
            organizationId);
        Guid executionId = Guid.CreateVersion7();
        Guid researchStepId = Guid.CreateVersion7();
        Guid researchDefinitionId = pipeline.Steps.Single(
            step => step.Type == PipelineStepType.Research).Id.Value;
        Guid scriptDefinitionId = pipeline.Steps.Single(
            step => step.Type == PipelineStepType.Script).Id.Value;
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync();

        await AssertFailureAsync(
            connection,
            ExecutionInsert(Guid.CreateVersion7(), pipeline.ProjectId.Value, Guid.CreateVersion7(), 1),
            PostgresErrorCodes.ForeignKeyViolation);
        await AssertFailureAsync(
            connection,
            ExecutionInsert(Guid.CreateVersion7(), Guid.CreateVersion7(), pipeline.Id.Value, 1),
            PostgresErrorCodes.ForeignKeyViolation);
        await AssertFailureAsync(
            connection,
            ExecutionInsert(Guid.CreateVersion7(), pipeline.ProjectId.Value, pipeline.Id.Value, 0),
            PostgresErrorCodes.CheckViolation);
        await ExecuteAsync(
            connection,
            ExecutionInsert(executionId, pipeline.ProjectId.Value, pipeline.Id.Value, 1));

        await AssertFailureAsync(
            connection,
            StepInsert(Guid.CreateVersion7(), Guid.CreateVersion7(), researchDefinitionId, 1),
            PostgresErrorCodes.ForeignKeyViolation);
        await AssertFailureAsync(
            connection,
            StepInsert(Guid.CreateVersion7(), executionId, researchDefinitionId, 0),
            PostgresErrorCodes.CheckViolation);
        await ExecuteAsync(
            connection,
            StepInsert(researchStepId, executionId, researchDefinitionId, 1));
        await AssertFailureAsync(
            connection,
            StepInsert(Guid.CreateVersion7(), executionId, scriptDefinitionId, 1),
            PostgresErrorCodes.UniqueViolation);
        await AssertFailureAsync(
            connection,
            StepInsert(Guid.CreateVersion7(), executionId, researchDefinitionId, 2),
            PostgresErrorCodes.UniqueViolation);

        await AssertFailureAsync(
            connection,
            ArtifactInsert(
                Guid.CreateVersion7(),
                organizationId.Value,
                pipeline.ProjectId.Value,
                Guid.CreateVersion7(),
                researchStepId,
                "content"),
            PostgresErrorCodes.ForeignKeyViolation);
        await AssertFailureAsync(
            connection,
            ArtifactInsert(
                Guid.CreateVersion7(),
                organizationId.Value,
                pipeline.ProjectId.Value,
                executionId,
                Guid.CreateVersion7(),
                "content"),
            PostgresErrorCodes.ForeignKeyViolation);
        await AssertFailureAsync(
            connection,
            ArtifactInsert(
                Guid.CreateVersion7(),
                organizationId.Value,
                pipeline.ProjectId.Value,
                executionId,
                researchStepId,
                string.Empty),
            PostgresErrorCodes.CheckViolation);
        await AssertFailureAsync(
            connection,
            ArtifactInsert(
                Guid.CreateVersion7(),
                organizationId.Value,
                pipeline.ProjectId.Value,
                executionId,
                researchStepId,
                content: null),
            PostgresErrorCodes.NotNullViolation);
        await ExecuteAsync(
            connection,
            ArtifactInsert(
                Guid.CreateVersion7(),
                organizationId.Value,
                pipeline.ProjectId.Value,
                executionId,
                researchStepId,
                "content"));
        await AssertFailureAsync(
            connection,
            ArtifactInsert(
                Guid.CreateVersion7(),
                organizationId.Value,
                pipeline.ProjectId.Value,
                executionId,
                researchStepId,
                "duplicate"),
            PostgresErrorCodes.UniqueViolation);
    }

    private static string ExecutionInsert(
        Guid executionId,
        Guid projectId,
        Guid pipelineId,
        int version)
    {
        return $"""
            INSERT INTO pipeline_executions
                (id, organization_id, project_id, pipeline_id, pipeline_version,
                 topic, status, created_at, created_by)
            VALUES
                ('{executionId}', '{Guid.CreateVersion7()}', '{projectId}', '{pipelineId}', {version},
                 'Topic', 'Pending', NOW(), 'integration-test');
            """;
    }

    private static string StepInsert(
        Guid stepId,
        Guid executionId,
        Guid pipelineStepId,
        int position)
    {
        return $"""
            INSERT INTO step_executions
                (id, pipeline_execution_id, pipeline_step_id, type, position, status)
            VALUES
                ('{stepId}', '{executionId}', '{pipelineStepId}', 'Research', {position}, 'Pending');
            """;
    }

    private static string ArtifactInsert(
        Guid artifactId,
        Guid organizationId,
        Guid projectId,
        Guid executionId,
        Guid stepId,
        string? content)
    {
        string sqlContent = content is null ? "NULL" : $"'{content}'";
        return $"""
            INSERT INTO artifacts
                (id, organization_id, project_id, pipeline_execution_id,
                 step_execution_id, type, content, created_at)
            VALUES
                ('{artifactId}', '{organizationId}', '{projectId}', '{executionId}',
                 '{stepId}', 'Research', {sqlContent}, NOW());
            """;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task AssertFailureAsync(
        NpgsqlConnection connection,
        string sql,
        string expectedSqlState)
    {
        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync(connection, sql));
        Assert.Equal(expectedSqlState, exception.SqlState);
    }
}
