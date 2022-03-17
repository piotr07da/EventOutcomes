using Microsoft.Extensions.DependencyInjection;

namespace EventOutcomes.Tests;

public class EventOutcomesTesterAdapter : IAdapter
{
    public delegate void PublishEventsAction(Guid eventStreamId, params object[] events);

    public delegate void StubAction(IServiceProvider serviceProvider, string givenEventsStreamId, object[] givenEvents, object command, PublishEventsAction publishEventsAction);

    private readonly StubAction _stubAction;

    private readonly IServiceProvider _serviceProvider;
    private IServiceProvider? _scopedServiceProvider;

    private string? _givenEventsStreamId;
    private object[]? _givenEvents;
    private Guid? _publishedEventsStreamId;
    private object[]? _publishedEvents;

    public EventOutcomesTesterAdapter(StubAction stubAction)
    {
        _stubAction = stubAction;

        var services = new ServiceCollection();
        services.AddScoped<ICleverService, FakeCleverService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public IServiceProvider ServiceProvider => _scopedServiceProvider ?? throw new NullReferenceException($"Call {nameof(BeforeTestAsync)} first to initialize {nameof(ServiceProvider)}.");

    public Task BeforeTestAsync()
    {
        _scopedServiceProvider = _serviceProvider.CreateScope().ServiceProvider;
        return Task.CompletedTask;
    }

    public Task AfterTestAsync()
    {
        return Task.CompletedTask;
    }

    public Task SetGivenEventsAsync(string streamId, IEnumerable<object> events)
    {
        _givenEventsStreamId = streamId;
        _givenEvents = events.ToArray();
        return Task.CompletedTask;
    }

    public Task DispatchCommandAsync(object command)
    {
        _stubAction(ServiceProvider!, _givenEventsStreamId ?? string.Empty, _givenEvents ?? Array.Empty<object>(), command, (publishedEventsStreamId, publishedEvents) =>
        {
            _publishedEventsStreamId = publishedEventsStreamId;
            _publishedEvents = publishedEvents;
        });

        return Task.CompletedTask;
    }

    public async Task<IDictionary<string, IEnumerable<object>>> GetPublishedEventsAsync()
    {
        await Task.Delay(0);
        var result = new Dictionary<string, IEnumerable<object>>();
        if (_publishedEventsStreamId != null)
        {
            result.Add(_publishedEventsStreamId.Value.ToString(), _publishedEvents!.AsEnumerable());
        }

        return result;
    }

    public static EventOutcomesTesterAdapter Stub(Guid stubbedPublishedEventsStreamId, params object[] stubbedPublishedEvents) => Stub((serviceProvider, givenEventsStreamId, givenEvents, command, publishEventsAction) => publishEventsAction(stubbedPublishedEventsStreamId, stubbedPublishedEvents));

    public static EventOutcomesTesterAdapter Stub(Exception stubbedException) => Stub((serviceProvider, givenEventsStreamId, givenEvents, command, publishEventsAction) => throw stubbedException);

    public static EventOutcomesTesterAdapter Stub(StubAction stubAction) => new(stubAction);
}
