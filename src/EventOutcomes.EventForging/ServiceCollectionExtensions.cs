using EventForging;
using Microsoft.Extensions.DependencyInjection;

namespace EventOutcomes.EventForging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventOutcomesForEventForging(IServiceCollection services)
    {
        services.AddSingleton<IEventDatabase, FakeEventDatabase>();
        FakeEventDatabase.Initialize();
        return services;
    }
}
