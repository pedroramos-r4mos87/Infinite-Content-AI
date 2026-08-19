using InfiniteContentAI.Api;
using InfiniteContentAI.Api.Executions;
using InfiniteContentAI.Api.Pipelines;
using InfiniteContentAI.Api.Projects;
using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace InfiniteContentAI.Api.IntegrationTests.Projects;

public sealed class ProjectApiFixture : IAsyncLifetime
{
    private const string ServerConnectionString =
        "Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=postgres;SSL Mode=Disable";

    private readonly string _databaseName =
        $"infinite_content_ai_api_tests_{Guid.NewGuid():N}";

    private WebApplication? _application;

    public static Guid CurrentOrganizationId { get; } =
        Guid.Parse("019c0000-0000-7000-8000-000000000002");

    public HttpClient Client =>
        _application?.GetTestClient()
        ?? throw new InvalidOperationException("The fixture has not been initialized.");

    public IServiceProvider Services =>
        _application?.Services
        ?? throw new InvalidOperationException("The fixture has not been initialized.");

    public HttpClient CreateClient(
        Guid organizationId,
        Guid? userId = null)
    {
        HttpClient client = _application?.GetTestClient()
            ?? throw new InvalidOperationException("The fixture has not been initialized.");
        client.DefaultRequestHeaders.Add(
            "X-Test-Organization-Id",
            organizationId.ToString());
        if (userId.HasValue)
        {
            client.DefaultRequestHeaders.Add(
                "X-Test-User-Id",
                userId.Value.ToString());
        }

        return client;
    }

    private string ConnectionString =>
        $"Host=127.0.0.1;Port=5432;Database={_databaseName};Username=postgres;Password=postgres;SSL Mode=Disable";

    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();

        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Test" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseTestServer();
        builder.Configuration["ConnectionStrings:Database"] = ConnectionString;
        builder.Services.AddApiServices(builder.Configuration, builder.Environment);

        _application = builder.Build();
        _application.UseExceptionHandler();
        _application.UseAuthentication();
        _application.UseAuthorization();
        _application.MapOpenApi();
        _application.MapProjectEndpoints();
        _application.MapPipelineEndpoints();
        _application.MapExecutionEndpoints();

        await using (AsyncServiceScope scope = _application.Services.CreateAsyncScope())
        {
            ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        await _application.StartAsync();
    }

    public async Task<TestApiApplication> CreateApplicationAsync(
        IAIProvider aiProvider)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Test" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseTestServer();
        builder.Configuration["ConnectionStrings:Database"] = ConnectionString;
        builder.Services.AddApiServices(builder.Configuration, builder.Environment);
        builder.Services.RemoveAll<IAIProvider>();
        builder.Services.AddSingleton(aiProvider);

        WebApplication application = builder.Build();
        application.UseExceptionHandler();
        application.UseAuthentication();
        application.UseAuthorization();
        application.MapOpenApi();
        application.MapProjectEndpoints();
        application.MapPipelineEndpoints();
        application.MapExecutionEndpoints();
        await application.StartAsync();

        return new TestApiApplication(application);
    }

    public async Task DisposeAsync()
    {
        if (_application is not null)
        {
            await _application.DisposeAsync();
        }

        await using var connection = new NpgsqlConnection(ServerConnectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText =
            $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity " +
            $"WHERE datname = '{_databaseName}' AND pid <> pg_backend_pid();";
        await command.ExecuteNonQueryAsync();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\"";
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ServerConnectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
        await command.ExecuteNonQueryAsync();
    }

    public sealed class TestApiApplication(
        WebApplication application) : IAsyncDisposable
    {
        public HttpClient Client { get; } = application.GetTestClient();

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await application.DisposeAsync();
        }
    }
}
