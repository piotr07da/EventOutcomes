using EventOutcomes;

public class EventOutcomesTesterAdapter : IAdapter
{
    private readonly object[] _stubbedThenEvents;

    public EventOutcomesTesterAdapter(object[] stubbedThenEvents)
    {
        _stubbedThenEvents = stubbedThenEvents;
    }

    public Task BeforeTestAsync()
    {
        return Task.CompletedTask;
    }

    public Task AfterTestAsync()
    {
        return Task.CompletedTask;
    }

    public Task SetGivenEventsAsync(string streamId, IEnumerable<object> events)
    {
        return Task.CompletedTask;
    }

    public Task<IEnumerable<object>> GetThenEventsAsync(string streamId)
    {
        return Task.FromResult(_stubbedThenEvents.AsEnumerable());
    }

    public Task DispatchCommandAsync(object command)
    {
        return Task.CompletedTask;
    }

    public static EventOutcomesTesterAdapter Stub(params object[] stubbedThenEvents) => new(stubbedThenEvents);
}
