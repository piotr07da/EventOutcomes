using System.Runtime.CompilerServices;
using EventForging;

namespace EventOutcomes.EventForging;

public class FakeEventDatabase : IEventDatabase
{
    private static readonly AsyncLocal<Dictionary<string, IEnumerable<object>>> _alreadySavedEvents = new();
    private static readonly AsyncLocal<Dictionary<string, IEnumerable<object>>> _newlySavedEvents = new();

    internal Dictionary<string, IEnumerable<object>> AlreadySavedEvents => _alreadySavedEvents.Value ?? throw new NullReferenceException("Not initialized.");

    internal Dictionary<string, IEnumerable<object>> NewlySavedEvents => _newlySavedEvents.Value ?? throw new NullReferenceException("Not initialized.");

    public async IAsyncEnumerable<object> ReadAsync<TAggregate>(string aggregateId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var records = ReadRecordsAsync<TAggregate>(aggregateId, cancellationToken);
        await foreach (var record in records)
        {
            yield return record.EventData;
        }
    }

    public async IAsyncEnumerable<EventDatabaseRecord> ReadRecordsAsync<TAggregate>(string aggregateId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var alreadySavedEvents = AlreadySavedEvents.TryGetValue(aggregateId, out var asEvents) ? asEvents.ToArray() : Array.Empty<object>();
        var newlySavedEvents = NewlySavedEvents.TryGetValue(aggregateId, out var nsEvents) ? nsEvents.ToArray() : Array.Empty<object>();
        var events = alreadySavedEvents.Concat(newlySavedEvents).ToArray();
        for (var eIx = 0; eIx < events.Length; ++eIx)
        {
            var e = events[eIx];

            yield return new EventDatabaseRecord(
                Guid.NewGuid(),
                eIx,
                e.GetType().FullName!,
                DateTime.UtcNow,
                e,
                Guid.Empty,
                Guid.Empty,
                new Dictionary<string, string>()
            );
        }

        await Task.CompletedTask;
    }

    public Task WriteAsync<TAggregate>(string aggregateId, IReadOnlyList<object> events, AggregateVersion retrievedVersion, ExpectedVersion expectedVersion, Guid conversationId, Guid initiatorId, IDictionary<string, string> customProperties, CancellationToken cancellationToken = default)
    {
        long currentVersion;

        if (AlreadySavedEvents.TryGetValue(aggregateId, out var asEvents))
        {
            currentVersion = asEvents.Count() - 1;
        }
        else
        {
            currentVersion = -1;
        }

        if ((expectedVersion == ExpectedVersion.None && currentVersion != -1) || (expectedVersion.IsDefined && expectedVersion != currentVersion))
            throw new EventForgingUnexpectedVersionException(aggregateId, null, expectedVersion, retrievedVersion, currentVersion);

        NewlySavedEvents[aggregateId] = events.ToArray(); // makes copy of events
        return Task.CompletedTask;
    }

    public static void Initialize()
    {
        _alreadySavedEvents.Value = new Dictionary<string, IEnumerable<object>>();
        _newlySavedEvents.Value = new Dictionary<string, IEnumerable<object>>();
    }

    public void StubAlreadySavedEvents(IDictionary<string, IEnumerable<object>> events)
    {
        foreach (var kvp in events)
        {
            var streamId = kvp.Key;
            var streamEvents = kvp.Value;
            
            AlreadySavedEvents.Add(streamId, streamEvents);
        }
    }

    public IDictionary<string, IEnumerable<object>> GetNewlySavedEvents()
    {
        return NewlySavedEvents.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
