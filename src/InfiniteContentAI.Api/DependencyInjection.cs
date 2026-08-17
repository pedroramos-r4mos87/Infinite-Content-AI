using InfiniteContentAI.Api.Authentication;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines.AddPipelineStep;
using InfiniteContentAI.Application.Pipelines.CreatePipeline;
using InfiniteContentAI.Application.Pipelines.GetPipeline;
using InfiniteContentAI.Application.Pipelines.ListPipelines;
using InfiniteContentAI.Application.Pipelines.PublishPipeline;
using InfiniteContentAI.Application.Projects.CreateProject;
using InfiniteContentAI.Application.Projects.GetProject;
using InfiniteContentAI.Application.Projects.ListProjects;
using InfiniteContentAI.Data;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.AspNetCore.Authentication;

namespace InfiniteContentAI.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!environment.IsDevelopment() &&
            !environment.IsEnvironment("Test"))
        {
            throw new InvalidOperationException(
                "A non-fake authentication scheme must be configured outside Development and Test.");
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentOrganization, HttpCurrentOrganization>();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton<IClock, global::InfiniteContentAI.Infrastructure.Time.SystemClock>();
        services.AddScoped<CreateProjectHandler>();
        services.AddScoped<GetProjectHandler>();
        services.AddScoped<ListProjectsHandler>();
        services.AddScoped<CreatePipelineHandler>();
        services.AddScoped<AddPipelineStepHandler>();
        services.AddScoped<PublishPipelineHandler>();
        services.AddScoped<GetPipelineHandler>();
        services.AddScoped<ListPipelinesHandler>();
        services.AddData(
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "The Database connection string is required."));
        services.AddProblemDetails();
        services.AddOpenApi();

        services
            .AddAuthentication(FakeAuthenticationDefaults.Scheme)
            .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
                FakeAuthenticationDefaults.Scheme,
                configureOptions: null);

        services.AddAuthorization();

        return services;
    }
}
