namespace EventOutcomes;

internal sealed class EventMatchCheckersChainExecutor
{
    public static EventMatchCheckersChainExecutionResult Execute(string streamId, EventMatchCheckersChain checkersChain, IEnumerable<object> publishedEvents)
    {
        return Execute(streamId, checkersChain, publishedEvents.ToArray());
    }

    public static EventMatchCheckersChainExecutionResult Execute(string streamId, EventMatchCheckersChain checkersChain, object[] publishedEvents)
    {
        if (checkersChain.IsNone)
        {
            if (publishedEvents.Length > 0)
            {
                return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatNoEventsExpected(publishedEvents));
            }

            return EventMatchCheckersChainExecutionResult.CreateSucceeded(streamId);
        }

        var checkers = checkersChain.Checkers;

        var consecutiveNegativeCheckerCounter = 0;

        if (checkers.Count == 0)
        {
            throw new EventOutcomesException("Cannot assert without at least one checker defined.");
        }

        var eventPointerIndex = 0;

        for (var cIx = 0; cIx < checkers.Count; cIx++)
        {
            var checker = checkers[cIx];
            if (checker is PositiveEventMatchChecker positiveChecker)
            {
                consecutiveNegativeCheckerCounter = 0;

                if (cIx > 0 && checkers[cIx - 1] is NegativeEventMatchChecker negativeCheck)
                {
                    var pr = positiveChecker.CheckMatchUntilFoundOrEnd(publishedEvents, eventPointerIndex);
                    if (!pr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatPositiveEventMatchFail(streamId, publishedEvents, pr));
                    }

                    var nr = negativeCheck.CheckMatch(publishedEvents, eventPointerIndex, pr.MatchFrom);
                    if (!nr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatNegativeEventMatchFail(streamId, publishedEvents, nr));
                    }

                    eventPointerIndex = pr.MatchFrom + positiveChecker.ExpectedEvents.Length;
                }
                else
                {
                    var pr = positiveChecker.CheckMatch(publishedEvents, eventPointerIndex);
                    if (!pr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatPositiveEventMatchFail(streamId, publishedEvents, pr));
                    }

                    eventPointerIndex += positiveChecker.ExpectedEvents.Length;
                }

                if (cIx == checkers.Count - 1)
                {
                    if (eventPointerIndex < publishedEvents.Length - 1)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, "Unexpected events found.");
                    }
                }
            }
            else if (checker is NegativeEventMatchChecker negativeChecker)
            {
                ++consecutiveNegativeCheckerCounter;
                if (consecutiveNegativeCheckerCounter > 1)
                {
                    throw new EventOutcomesException("Consecutive negative checkers detected.");
                }

                if (cIx == checkers.Count - 1)
                {
                    var nr = negativeChecker.CheckMatch(publishedEvents, eventPointerIndex, publishedEvents.Length);
                    if (!nr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatNegativeEventMatchFail(streamId, publishedEvents, nr));
                    }
                }
            }
            else
            {
                throw new EventOutcomesException($"{checker.GetType().FullName} is not correct type of event checker.");
            }
        }

        return EventMatchCheckersChainExecutionResult.CreateSucceeded(streamId);
    }
}
