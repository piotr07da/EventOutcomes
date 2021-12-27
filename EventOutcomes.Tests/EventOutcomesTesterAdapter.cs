namespace EventOutcomes.Tests;

public class EventOutcomesTesterAdapter : IAdapter
{
    private readonly Guid _stubbedPublishedEventsStreamId;
    private readonly object[] _stubbedPublishedEvents;

    public EventOutcomesTesterAdapter(Guid stubbedPublishedEventsStreamId, object[] stubbedPublishedEvents)
    {
        _stubbedPublishedEventsStreamId = stubbedPublishedEventsStreamId;
        _stubbedPublishedEvents = stubbedPublishedEvents;
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

    public Task DispatchCommandAsync(object command)
    {
        return Task.CompletedTask;
    }

    public async Task<IDictionary<string, IEnumerable<object>>> GetPublishedEventsAsync()
    {
        await Task.Delay(0);
        return new Dictionary<string, IEnumerable<object>> { { _stubbedPublishedEventsStreamId.ToString(), _stubbedPublishedEvents.AsEnumerable() }, };
    }

    public static EventOutcomesTesterAdapter Stub(Guid stubbedPublishedEventsStreamId, params object[] stubbedThenEvents) => new(stubbedPublishedEventsStreamId, stubbedThenEvents);
}
