namespace EventOutcomes;

public sealed class ComparableEventDocument
{
    public ComparableEventDocument(string eventType, string content)
    {
        EventType = eventType;
        Content = content;
    }

    public string EventType { get; }

    public string Content { get; }

    public static ComparableEventDocument From(object e)
    {
        return new ComparableEventDocument(e.GetType().FullName!, EventSerializer.Serialize(e));
    }

    public override string ToString() => EventType;

    public override bool Equals(object? obj)
    {
        return obj != null && this == (ComparableEventDocument)obj;
    }

    public override int GetHashCode()
    {
        return $"{EventType};{Content}".GetHashCode();
    }

    public static bool operator !=(ComparableEventDocument lhs, ComparableEventDocument rhs) => !(lhs == rhs);

    public static bool operator ==(ComparableEventDocument lhs, ComparableEventDocument rhs) => lhs.EventType == rhs.EventType && lhs.Content == rhs.Content;
}
