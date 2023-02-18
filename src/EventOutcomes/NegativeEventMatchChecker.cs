namespace EventOutcomes;

internal sealed class NegativeEventMatchChecker
{
    public NegativeEventMatchChecker(Func<object, bool>[] excludedEventQualifiers)
    {
        ExcludedEventQualifiers = excludedEventQualifiers;
    }

    public Func<object, bool>[] ExcludedEventQualifiers { get; }

    public NegativeEventMatchResult CheckMatch(object[] publishedEvents, int checkFrom, int checkTo)
    {
        for (var peIx = checkFrom; peIx < checkTo; ++peIx)
        {
            var e = publishedEvents[peIx];

            if (ExcludedEventQualifiers.Any(eeq => eeq(e)))
            {
                return NotMatchingResult(peIx);
            }
        }

        return MatchingResult();
    }

    private NegativeEventMatchResult MatchingResult() => NegativeEventMatchResult.Matching(ExcludedEventQualifiers.Length);

    private NegativeEventMatchResult NotMatchingResult(int indexOfUnexpectedPublishedEvent) => NegativeEventMatchResult.NotMatching(ExcludedEventQualifiers.Length, indexOfUnexpectedPublishedEvent);
}
