using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Infrastructure.ArtificialIntelligence;
using InfiniteContentAI.Infrastructure.Time;
using InfiniteContentAI.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IAIProvider, FakeAIProvider>();

        return services;
    }
}
