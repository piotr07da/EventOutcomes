namespace EventOutcomes;

internal sealed class EventMatchCheckersChain
{
    private readonly List<object> _checkers = new();

    public bool IsNone { get; private set; }

    public IReadOnlyList<object> Checkers => _checkers;

    public void AddNoneChecker()
    {
        if (_checkers.Count > 0)
        {
            throw new EventOutcomesException("Cannot set None if there are other checkers defined.");
        }

        IsNone = true;
    }

    public void AddPositiveMatchChecker(PositiveEventMatchChecker checker)
    {
        Add(checker);
    }

    public void AddNegativeMatcherChecker(NegativeEventMatchChecker checker)
    {
        Add(checker);
    }

    private void Add(object checker)
    {
        if (IsNone)
        {
            throw new EventOutcomesException("Cannot add any checker if None checker is set.");
        }

        if (_checkers.Count > 0)
        {
            var lastCheck = _checkers.Last();

            if (lastCheck is NegativeEventMatchChecker && checker is NegativeEventMatchChecker)
            {
                throw new EventOutcomesException("Cannot add two consecutive negative event match checkers (ThenAny or ThenNot).");
            }
        }

        _checkers.Add(checker);
    }
}
