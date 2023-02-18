namespace EventOutcomes;

internal sealed class EventMatchCheckersChainExecutionResult
{
    private EventMatchCheckersChainExecutionResult(string streamId, bool succeeded, string? errorMessage)
    {
        StreamId = streamId;
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    public string StreamId { get; }
    public bool Succeeded { get; }
    public string? ErrorMessage { get; }

    public static EventMatchCheckersChainExecutionResult CreateSucceeded(string streamId) => new(streamId, true, null);
    public static EventMatchCheckersChainExecutionResult CreateFailed(string streamId, string errorMessage) => new(streamId, false, errorMessage);
}
