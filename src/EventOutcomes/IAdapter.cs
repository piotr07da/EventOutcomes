using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public interface IAdapter
    {
        /// <summary>
        ///     Service provider for all the services needed to be injected in application code and test code.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        ///     Called before the test is executed. If scoped services are required then this is a perfect place to create a scope
        ///     and assign scoped service provider to the <see cref="ServiceProvider" /> property.
        /// </summary>
        /// <returns></returns>
        Task BeforeTestAsync();

        /// <summary>
        ///     Called after the test is completed. Any cleanup code goes here.
        /// </summary>
        /// <returns></returns>
        Task AfterTestAsync();

        /// <summary>
        ///     Saves the GIVEN events (events that already occurred) to the place from which Event Sourcing framework will read
        ///     them in order to rehydrate the domain objects (e.g. aggregates in DDD).
        /// </summary>
        /// <param name="events">The events that already occurred.</param>
        /// <returns></returns>
        Task SetGivenEventsAsync(IDictionary<string, IEnumerable<object>> events);

        /// <summary>
        ///     Gets the newly published events.
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<string, IEnumerable<object>>> GetPublishedEventsAsync();

        /// <summary>
        ///     Dispatches the command.
        /// </summary>
        /// <param name="command">Command to be dispatched.</param>
        /// <returns></returns>
        Task DispatchCommandAsync(object command);
    }
}
