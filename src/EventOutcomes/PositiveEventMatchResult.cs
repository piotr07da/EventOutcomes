namespace EventOutcomes;

internal sealed record PositiveEventMatchResult(bool IsMatching, object[] ExpectedEvents, PositiveEventMatchOrder Order, int MatchFrom, int IndexOfNotFoundExpectedEvent, int IndexOfUnexpectedPublishedEvent)
{
    public static PositiveEventMatchResult Matching(object[] expectedEvents, PositiveEventMatchOrder order, int matchFrom) => new(true, expectedEvents, order, matchFrom, -1, -1);

    public static PositiveEventMatchResult NotMatching(object[] expectedEvents, PositiveEventMatchOrder order, int indexOfNotFoundExpectedEvent, int indexOfUnexpectedPublishedEvent) => new(false, expectedEvents, order, -1, indexOfNotFoundExpectedEvent, indexOfUnexpectedPublishedEvent);
}
