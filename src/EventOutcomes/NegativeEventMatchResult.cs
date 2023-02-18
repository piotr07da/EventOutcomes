namespace EventOutcomes;

internal sealed record NegativeEventMatchResult(bool IsMatching, int ExcludedEventQualifiersCount, int IndexOfUnexpectedPublishedEvent)
{
    public static NegativeEventMatchResult Matching(int excludedEventQualifiersCount) => new(true, excludedEventQualifiersCount, -1);

    public static NegativeEventMatchResult NotMatching(int excludedEventQualifiersCount, int indexOfUnexpectedPublishedEvent) => new(false, excludedEventQualifiersCount, indexOfUnexpectedPublishedEvent);
}
