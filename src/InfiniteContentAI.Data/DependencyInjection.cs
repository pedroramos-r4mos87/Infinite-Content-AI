using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Artifacts;
using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Data.Artifacts;
using InfiniteContentAI.Data.Executions;
using InfiniteContentAI.Data.Pipelines;
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
        services.AddScoped<IPipelineRepository, PipelineRepository>();
        services.AddScoped<IPipelineQueries, PipelineQueries>();
        services.AddScoped<IPipelineExecutionRepository, PipelineExecutionRepository>();
        services.AddScoped<IPipelineExecutionQueries, PipelineExecutionQueries>();
        services.AddScoped<IArtifactRepository, ArtifactRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
