namespace EventOutcomes;

internal sealed class EventAssertionsChainExecutionResult
{
    private EventAssertionsChainExecutionResult(string streamId, bool succeeded, string? errorMessage)
    {
        StreamId = streamId;
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    public string StreamId { get; }
    public bool Succeeded { get; }
    public string? ErrorMessage { get; }

    public static EventAssertionsChainExecutionResult CreateSucceeded(string streamId) => new(streamId, true, null);
    public static EventAssertionsChainExecutionResult CreateFailed(string streamId, string errorMessage) => new(streamId, false, errorMessage);
}
