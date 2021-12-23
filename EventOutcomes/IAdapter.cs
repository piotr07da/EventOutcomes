using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public interface IAdapter
    {
        Task BeforeTestAsync();
        Task AfterTestAsync();
        Task SetGivenEventsAsync(string streamId, IEnumerable<object> events);
        Task<IEnumerable<object>> GetThenEventsAsync(string streamId);
        Task DispatchCommandAsync(object command);
    }
}
