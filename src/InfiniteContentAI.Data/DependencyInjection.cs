using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Data.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(
            options => options.UseNpgsql(connectionString));
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectQueries, ProjectQueries>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
