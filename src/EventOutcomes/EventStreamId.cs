namespace EventOutcomes;

public struct EventStreamId
{
    private readonly string _eventStreamId;

    private EventStreamId(string eventStreamId)
    {
        _eventStreamId = eventStreamId ?? throw new ArgumentNullException(nameof(eventStreamId));
    }

    public static implicit operator EventStreamId(Guid eventStreamId) => new(eventStreamId.ToString());
    public static implicit operator EventStreamId(string eventStreamId) => new(eventStreamId);
    public static implicit operator string(EventStreamId eventStreamId) => eventStreamId._eventStreamId;
}
