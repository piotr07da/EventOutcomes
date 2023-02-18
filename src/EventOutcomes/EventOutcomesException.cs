namespace EventOutcomes;

public sealed class EventOutcomesException : Exception
{
    public EventOutcomesException(string message)
        : base(message)
    {
    }
}
