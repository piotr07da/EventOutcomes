namespace EventOutcomes;

internal sealed class PositiveEventMatchChecker
{
    public PositiveEventMatchChecker(object[] expectedEvents, PositiveEventMatchOrder order)
    {
        if (expectedEvents == null || expectedEvents.Length == 0) throw new ArgumentException("Expected events collection is empty.", nameof(expectedEvents));

        ExpectedEvents = expectedEvents;
        Order = order;
    }

    public object[] ExpectedEvents { get; }
    public PositiveEventMatchOrder Order { get; }

    public PositiveEventMatchResult CheckMatchUntilFoundOrEnd(object[] publishedEvents, int checkFrom)
    {
        for (var eIx = checkFrom; eIx < publishedEvents.Length - ExpectedEvents.Length + 1; ++eIx)
        {
            var checkResult = CheckMatch(publishedEvents, eIx);
            if (checkResult.IsMatching)
            {
                return checkResult;
            }
        }

        return NotMatchingResult(-1, -1);
    }

    public PositiveEventMatchResult CheckMatch(object[] publishedEvents, int checkFrom)
    {
        if (Order == PositiveEventMatchOrder.InOrder)
        {
            return CheckInOrder(publishedEvents, checkFrom);
        }

        if (Order == PositiveEventMatchOrder.InAnyOrder)
        {
            return CheckInAnyOrder(publishedEvents, checkFrom);
        }

        throw new EventOutcomesException($"Unknown {nameof(PositiveEventMatchOrder)} specified: {Order}.");
    }

    private PositiveEventMatchResult CheckInOrder(object[] publishedEvents, int checkFrom)
    {
        var comparableExpectedEvents = ExpectedEvents.Select(ComparableEventDocument.From).ToArray();
        var comparablePublishedEvents = publishedEvents.Skip(checkFrom).Select(ComparableEventDocument.From).ToArray();

        var expectedEventIndex = 0;
        foreach (var comparablePublishedEvent in comparablePublishedEvents)
        {
            if (comparablePublishedEvent != comparableExpectedEvents[expectedEventIndex])
            {
                return NotMatchingResult(-1, expectedEventIndex + checkFrom);
            }

            ++expectedEventIndex;

            if (expectedEventIndex == ExpectedEvents.Length)
            {
                return MatchingResult(checkFrom);
            }
        }

        return NotMatchingResult(expectedEventIndex, -1);
    }

    private PositiveEventMatchResult CheckInAnyOrder(object[] publishedEvents, int checkFrom)
    {
        var comparableExpectedEvents = ExpectedEvents.Select(ComparableEventDocument.From).ToArray();
        var comparablePublishedEvents = publishedEvents.Select(ComparableEventDocument.From).ToArray();

        var comparableExpectedEventsLeft = comparableExpectedEvents.ToHashSet();
        for (var publishedEventIndex = 0; publishedEventIndex < publishedEvents.Length; ++publishedEventIndex)
        {
            var serializedPublishedEvent = comparablePublishedEvents[publishedEventIndex];

            if (!comparableExpectedEventsLeft.Contains(serializedPublishedEvent))
            {
                return NotMatchingResult(-1, publishedEventIndex + checkFrom);
            }

            comparableExpectedEventsLeft.Remove(serializedPublishedEvent);

            if (comparableExpectedEventsLeft.Count == 0)
            {
                return MatchingResult(checkFrom);
            }
        }

        return NotMatchingResult(Array.FindIndex(comparableExpectedEvents, ee => ee == comparableExpectedEventsLeft.First()), -1);
    }

    private PositiveEventMatchResult MatchingResult(int matchFrom) => PositiveEventMatchResult.Matching(ExpectedEvents, Order, matchFrom);

    private PositiveEventMatchResult NotMatchingResult(int indexOfNotFoundExpectedEvent, int indexOfUnexpectedPublishedEvent) => PositiveEventMatchResult.NotMatching(ExpectedEvents, Order, indexOfNotFoundExpectedEvent, indexOfUnexpectedPublishedEvent);
}
