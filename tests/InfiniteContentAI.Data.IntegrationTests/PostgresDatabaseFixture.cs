using InfiniteContentAI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace InfiniteContentAI.Data.IntegrationTests;

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly string _databaseName =
        $"infinite_content_ai_data_tests_{Guid.NewGuid():N}";

    private ServiceProvider? _serviceProvider;

    public IServiceProvider Services =>
        _serviceProvider
        ?? throw new InvalidOperationException("The fixture has not been initialized.");

    public string ConnectionString =>
        $"Host=localhost;Port=5432;Database={_databaseName};Username=postgres;Password=postgres";

    public async Task InitializeAsync()
    {
        await using (var connection = new NpgsqlConnection(
                         "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"))
        {
            await connection.OpenAsync();
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        var services = new ServiceCollection();
        services.AddData(ConnectionString);
        _serviceProvider = services.BuildServiceProvider();

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        await using var connection = new NpgsqlConnection(
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres");
        await connection.OpenAsync();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText =
            $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity " +
            $"WHERE datname = '{_databaseName}' AND pid <> pg_backend_pid();";
        await command.ExecuteNonQueryAsync();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\"";
        await command.ExecuteNonQueryAsync();
    }
}
