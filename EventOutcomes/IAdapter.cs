using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public interface IAdapter
    {
        IServiceProvider ServiceProvider { get; }
        Task BeforeTestAsync();
        Task AfterTestAsync();
        Task SetGivenEventsAsync(IDictionary<string, IEnumerable<object>> events);
        Task<IDictionary<string, IEnumerable<object>>> GetPublishedEventsAsync();
        Task DispatchCommandAsync(object command);
    }
}
