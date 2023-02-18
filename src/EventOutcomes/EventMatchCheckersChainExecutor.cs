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

        var consecutiveNegativeAssertionCounter = 0;

        if (checkers.Count == 0)
        {
            throw new EventOutcomesException("Cannot assert without at least one checker defined.");
        }

        var eventPointerIndex = 0;

        for (var cIx = 0; cIx < checkers.Count; cIx++)
        {
            var check = checkers[cIx];
            if (check is PositiveEventMatchChecker positiveAssertion)
            {
                consecutiveNegativeAssertionCounter = 0;

                if (cIx > 0 && checkers[cIx - 1] is NegativeEventMatchChecker negativeCheck)
                {
                    var pr = positiveAssertion.CheckMatchUntilFoundOrEnd(publishedEvents, eventPointerIndex);
                    if (!pr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatPositiveEventMatchFail(streamId, publishedEvents, pr));
                    }

                    var nr = negativeCheck.CheckMatch(publishedEvents, eventPointerIndex, pr.MatchFrom);
                    if (!nr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatNegativeEventMatchFail(streamId, publishedEvents, nr));
                    }

                    eventPointerIndex = pr.MatchFrom + positiveAssertion.ExpectedEvents.Length;
                }
                else
                {
                    var pr = positiveAssertion.CheckMatch(publishedEvents, eventPointerIndex);
                    if (!pr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatPositiveEventMatchFail(streamId, publishedEvents, pr));
                    }

                    eventPointerIndex += positiveAssertion.ExpectedEvents.Length;
                }

                if (cIx == checkers.Count - 1)
                {
                    if (eventPointerIndex < publishedEvents.Length - 1)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, "Unexpected events found.");
                    }
                }
            }
            else if (check is NegativeEventMatchChecker negativeAssertion)
            {
                ++consecutiveNegativeAssertionCounter;
                if (consecutiveNegativeAssertionCounter > 1)
                {
                    throw new EventOutcomesException("Consecutive negative assertions detected.");
                }

                if (cIx == checkers.Count - 1)
                {
                    var nr = negativeAssertion.CheckMatch(publishedEvents, eventPointerIndex, publishedEvents.Length);
                    if (!nr.IsMatching)
                    {
                        return EventMatchCheckersChainExecutionResult.CreateFailed(streamId, EventMatchErrorMessageFormatter.FormatNegativeEventMatchFail(streamId, publishedEvents, nr));
                    }
                }
            }
            else
            {
                throw new EventOutcomesException($"{check.GetType().FullName} is not correct type of event assertion.");
            }
        }

        return EventMatchCheckersChainExecutionResult.CreateSucceeded(streamId);
    }
}
