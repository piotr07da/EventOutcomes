using EventForging;
using Microsoft.Extensions.DependencyInjection;

namespace EventOutcomes.EventForging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventOutcomesForEventForging(this IServiceCollection services)
    {
        services.AddSingleton<FakeEventDatabase>();
        services.AddSingleton<IEventDatabase>(sp => sp.GetRequiredService<FakeEventDatabase>());
        services.AddSingleton<IDestructiveEventDatabase>(sp => sp.GetRequiredService<FakeEventDatabase>());
        FakeEventDatabase.Initialize();
        return services;
    }
}
