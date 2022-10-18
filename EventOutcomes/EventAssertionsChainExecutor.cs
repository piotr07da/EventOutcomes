using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventOutcomes
{
    internal sealed class EventAssertionsChainExecutor
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
                    FailAtNegative(events, "Expected no events.", 0);
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
                            FailAtPositive(originalEvents, positiveAssertion, originalEvents.Length - events.Length, originalEvents.Length - 1);
                        }

                        if (!negativeCheck.Assert(events.RangeTo(aIx - 1), out var failAt))
                        {
                            FailAtNegative(originalEvents, negativeCheck, originalEvents.Length - events.Length + failAt);
                        }

                        events = events.RangeFrom(aIx + positiveAssertion.ExpectedEvents.Length);
                    }
                    else
                    {
                        if (!positiveAssertion.Assert(events))
                        {
                            FailAtPositive(
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
                            FailAtPositive(originalEvents, positiveAssertion, originalEvents.Length - events.Length, originalEvents.Length - 1);
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
                            FailAtNegative(originalEvents, negativeAssertion, originalEvents.Length - events.Length + failAt);
                        }
                    }
                }
                else
                {
                    throw new Exception($"{check.GetType().FullName} is not correct type of event assertion.");
                }
            }
        }

        private static void FailAtPositive(IEnumerable<object> publishedEvents, PositiveEventAssertion positiveAssertion, int failFrom, int failTo)
        {
            FailAtPositive(publishedEvents, PositiveAssertionExpectationInfo(positiveAssertion), failFrom, failTo);
        }

        private static void FailAtPositive(IEnumerable<object> publishedEvents, string positiveAssertionInfo, int failFrom, int failTo)
        {
            var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From).ToArray();
            var sb = new StringBuilder();
            sb.AppendLine($"{positiveAssertionInfo}");
            sb.AppendLine();

            if (serializedPublishedEvents.Length == 0)
            {
                sb.AppendLine("No events were published.");
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

            throw new AssertException(sb.ToString());
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

        private static void FailAtNegative(IEnumerable<object> publishedEvents, NegativeEventAssertion negativeAssertion, int failAt)
        {
            FailAtNegative(publishedEvents, NegativeAssertionExpectationInfo(negativeAssertion), failAt);
        }

        private static void FailAtNegative(IEnumerable<object> publishedEvents, string negativeAssertionInfo, int failAt)
        {
            var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From);
            var sb = new StringBuilder();
            sb.AppendLine($"{negativeAssertionInfo}");
            sb.AppendLine();
            sb.AppendLine($"Unexpected published event found at [{failAt}].");
            sb.AppendLine("Published events are:");
            sb.AppendLine(string.Join(Environment.NewLine, serializedPublishedEvents.Select((pe, ix) => $"{ix}. [{pe.EventType}]{Environment.NewLine}{pe.Content}")));

            throw new AssertException(sb.ToString());
        }

        private static string NegativeAssertionExpectationInfo(NegativeEventAssertion negativeAssertion)
        {
            var builder = new StringBuilder();
            builder.Append($"Expected not to find any event matching {negativeAssertion.ExcludedEventQualifiers.Length} specified rule{(negativeAssertion.ExcludedEventQualifiers.Length == 1 ? string.Empty : "s")}.");
            return builder.ToString();
        }
    }
}
