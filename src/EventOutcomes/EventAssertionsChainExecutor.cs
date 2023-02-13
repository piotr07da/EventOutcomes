using System.Text;

namespace EventOutcomes;

internal sealed class EventAssertionsChainExecutor
{
    public static EventAssertionsChainExecutionResult Execute(string streamId, EventAssertionsChain assertionChain, IEnumerable<object> events)
    {
        return Execute(streamId, assertionChain, events.ToArray());
    }

    public static EventAssertionsChainExecutionResult Execute(string streamId, EventAssertionsChain assertionChain, object[] events)
    {
        if (assertionChain.IsNone)
        {
            if (events.Length > 0)
            {
                return FailAtNegative(streamId, events, "Expected no events.", 0);
            }

            return EventAssertionsChainExecutionResult.CreateSucceeded(streamId);
        }

        var originalEvents = events;
        var assertions = assertionChain.Assertions;

        var consecutiveNegativeAssertionCounter = 0;

        if (assertions.Count == 0)
        {
            throw new Exception("Cannot assert without at least one assertion defined.");
        }

        for (var cIx = 0; cIx < assertions.Count; cIx++)
        {
            var check = assertions[cIx];
            if (check is PositiveEventAssertion positiveAssertion)
            {
                consecutiveNegativeAssertionCounter = 0;

                if (cIx > 0 && assertions[cIx - 1] is NegativeEventAssertion negativeCheck)
                {
                    var aIx = positiveAssertion.FindAssertionIndex(events);
                    if (aIx < 0)
                    {
                        return FailAtPositive(streamId, originalEvents, positiveAssertion, originalEvents.Length - events.Length, originalEvents.Length - 1);
                    }

                    if (!negativeCheck.Assert(events.RangeTo(aIx - 1), out var failAt))
                    {
                        return FailAtNegative(streamId, originalEvents, negativeCheck, originalEvents.Length - events.Length + failAt);
                    }

                    events = events.RangeFrom(aIx + positiveAssertion.ExpectedEvents.Length);
                }
                else
                {
                    if (!positiveAssertion.Assert(events))
                    {
                        return FailAtPositive(
                            streamId,
                            originalEvents,
                            positiveAssertion,
                            Math.Min(originalEvents.Length - events.Length, originalEvents.Length - 1),
                            Math.Min(originalEvents.Length - events.Length + positiveAssertion.ExpectedEvents.Length - 1, originalEvents.Length - 1));
                    }

                    events = events.RangeFrom(positiveAssertion.ExpectedEvents.Length);
                }

                if (cIx == assertions.Count - 1)
                {
                    if (events.Length > 0)
                    {
                        return FailAtPositive(streamId, originalEvents, positiveAssertion, originalEvents.Length - events.Length, originalEvents.Length - 1);
                    }
                }
            }
            else if (check is NegativeEventAssertion negativeAssertion)
            {
                ++consecutiveNegativeAssertionCounter;
                if (consecutiveNegativeAssertionCounter > 1)
                {
                    throw new Exception("Consecutive negative assertions detected.");
                }

                if (cIx == assertions.Count - 1)
                {
                    if (!negativeAssertion.Assert(events, out var failAt))
                    {
                        return FailAtNegative(streamId, originalEvents, negativeAssertion, originalEvents.Length - events.Length + failAt);
                    }
                }
            }
            else
            {
                throw new Exception($"{check.GetType().FullName} is not correct type of event assertion.");
            }
        }

        return EventAssertionsChainExecutionResult.CreateSucceeded(streamId);
    }

    private static EventAssertionsChainExecutionResult FailAtPositive(string streamId, IEnumerable<object> publishedEvents, PositiveEventAssertion positiveAssertion, int failFrom, int failTo)
    {
        return FailAtPositive(streamId, publishedEvents, PositiveAssertionExpectationInfo(positiveAssertion), failFrom, failTo);
    }

    private static EventAssertionsChainExecutionResult FailAtPositive(string streamId, IEnumerable<object> publishedEvents, string positiveAssertionInfo, int failFrom, int failTo)
    {
        var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From).ToArray();
        var sb = new StringBuilder();
        sb.AppendLine($"{positiveAssertionInfo}");
        sb.AppendLine();

        if (serializedPublishedEvents.Length == 0)
        {
            sb.AppendLine($"No events were published to the stream '{streamId}'.");
        }
        else
        {
            if (failFrom == failTo)
            {
                sb.AppendLine($"Unexpected published event found at [{failFrom}].");
            }
            else
            {
                sb.AppendLine($"Unexpected published events found in range [{failFrom}..{failTo}].");
            }

            sb.AppendLine("Published events are:");
            sb.AppendLine(string.Join(Environment.NewLine, serializedPublishedEvents.Select((pe, ix) => $"{ix}. [{pe.EventType}]{Environment.NewLine}{pe.Content}")));
        }

        return EventAssertionsChainExecutionResult.CreateFailed(streamId, sb.ToString());
    }

    private static string PositiveAssertionExpectationInfo(PositiveEventAssertion positiveAssertion)
    {
        var builder = new StringBuilder();
        builder.Append("Expected following events ");
        switch (positiveAssertion.Order)
        {
            case PositiveEventAssertionOrder.InOrder:
                builder.AppendLine("in specified order:");
                break;

            case PositiveEventAssertionOrder.InAnyOrder:
                builder.AppendLine("in any order:");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(positiveAssertion.Order));
        }

        var serializedExpectedEvents = positiveAssertion.ExpectedEvents.Select(ComparableEventDocument.From);
        builder.Append(string.Join(Environment.NewLine, serializedExpectedEvents.Select((pe, ix) => $"[{pe.EventType}]{Environment.NewLine}{pe.Content}")));
        return builder.ToString();
    }

    private static EventAssertionsChainExecutionResult FailAtNegative(string streamId, IEnumerable<object> publishedEvents, NegativeEventAssertion negativeAssertion, int failAt)
    {
        return FailAtNegative(streamId, publishedEvents, NegativeAssertionExpectationInfo(negativeAssertion), failAt);
    }

    private static EventAssertionsChainExecutionResult FailAtNegative(string streamId, IEnumerable<object> publishedEvents, string negativeAssertionInfo, int failAt)
    {
        var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From);
        var sb = new StringBuilder();
        sb.AppendLine($"{negativeAssertionInfo}");
        sb.AppendLine();
        sb.AppendLine($"Unexpected published event found at [{failAt}].");
        sb.AppendLine("Published events are:");
        sb.AppendLine(string.Join(Environment.NewLine, serializedPublishedEvents.Select((pe, ix) => $"{ix}. [{pe.EventType}]{Environment.NewLine}{pe.Content}")));

        return EventAssertionsChainExecutionResult.CreateFailed(streamId, sb.ToString());
    }

    private static string NegativeAssertionExpectationInfo(NegativeEventAssertion negativeAssertion)
    {
        var builder = new StringBuilder();
        builder.Append($"Expected not to find any event matching {negativeAssertion.ExcludedEventQualifiers.Length} specified rule{(negativeAssertion.ExcludedEventQualifiers.Length == 1 ? string.Empty : "s")}.");
        return builder.ToString();
    }
}
