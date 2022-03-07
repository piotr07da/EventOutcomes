using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventOutcomes
{
    public class EventAssertionsChainExecutor
    {
        public static void Execute(EventAssertionsChain assertionChain, IEnumerable<object> events)
        {
            Execute(assertionChain, events.ToArray());
        }

        public static void Execute(EventAssertionsChain assertionChain, object[] events)
        {
            if (assertionChain.IsNone)
            {
                if (events.Length > 0)
                {
                    Fail(events, 0, "None", 0, events.Length - 1);
                }

                return;
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
                            Fail(originalEvents, cIx, positiveAssertion.ToString(), originalEvents.Length - events.Length, originalEvents.Length - 1);
                        }

                        if (!negativeCheck.Assert(events.RangeTo(aIx)))
                        {
                            Fail(originalEvents, cIx - 1, negativeCheck.ToString(), originalEvents.Length - events.Length, aIx - 1);
                        }

                        events = events.RangeFrom(aIx + positiveAssertion.ExpectedEvents.Length);
                    }
                    else
                    {
                        if (!positiveAssertion.Assert(events))
                        {
                            Fail(originalEvents, cIx, positiveAssertion.ToString(), originalEvents.Length - events.Length, originalEvents.Length - events.Length + positiveAssertion.ExpectedEvents.Length - 1);
                        }

                        events = events.RangeFrom(positiveAssertion.ExpectedEvents.Length);
                    }

                    if (cIx == assertions.Count - 1)
                    {
                        if (events.Length > 0)
                        {
                            Fail(originalEvents, cIx, positiveAssertion.ToString(), originalEvents.Length - events.Length, originalEvents.Length - 1);
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
                        if (!negativeAssertion.Assert(events))
                        {
                            Fail(originalEvents, cIx, negativeAssertion.ToString(), originalEvents.Length - events.Length, originalEvents.Length - 1);
                        }
                    }
                }
            }
        }

        private static void Fail(IEnumerable<object> publishedEvents, int assertionIndex, string assertionInfo, int failFrom, int failTo)
        {
            throw new AssertException(FormatExpectedAndPublishedEventsMessage(publishedEvents, assertionIndex, assertionInfo, failFrom, failTo));
        }

        private static string FormatExpectedAndPublishedEventsMessage(IEnumerable<object> publishedEvents, int assertionIndex, string assertionInfo, int failFrom, int failTo)
        {
            var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From);

            var sb = new StringBuilder();
            sb.AppendLine($"Assertion with index {assertionIndex} failed between event {failFrom} and {failTo}.");
            sb.AppendLine($"Assertion with index {assertionIndex} is:");
            sb.AppendLine($"{assertionInfo}");
            sb.AppendLine();
            sb.AppendLine($"Published events are:{Environment.NewLine}{string.Join(Environment.NewLine, serializedPublishedEvents.Select((pe, ix) => $"{ix}. [{pe.EventType}]{Environment.NewLine}{pe.Content}"))}");
            return sb.ToString();
        }
    }
}
