using InfiniteContentAI.Api.Authentication;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.AspNetCore.Authentication;

namespace InfiniteContentAI.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
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

        services
            .AddAuthentication(FakeAuthenticationDefaults.Scheme)
            .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
                FakeAuthenticationDefaults.Scheme,
                configureOptions: null);

        services.AddAuthorization();

        return services;
    }
}
