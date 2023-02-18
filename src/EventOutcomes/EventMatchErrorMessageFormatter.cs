using System.Text;

namespace EventOutcomes;

internal sealed class EventMatchErrorMessageFormatter
{
    public static string FormatPositiveEventMatchFail(string streamId, object[] publishedEvents, PositiveEventMatchResult result)
    {
        var sb = new StringBuilder();

        sb.Append("Expected following events ");
        switch (result.Order)
        {
            case PositiveEventMatchOrder.InOrder:
                sb.AppendLine("in specified order:");
                break;

            case PositiveEventMatchOrder.InAnyOrder:
                sb.AppendLine("in any order:");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result.Order));
        }

        var serializedExpectedEvents = result.ExpectedEvents.Select(ComparableEventDocument.From);
        sb.Append(string.Join(Environment.NewLine, serializedExpectedEvents.Select((pe, ix) => $"{ix}. [{pe.EventType}]{Environment.NewLine}{pe.Content}")));
        sb.AppendLine();
        sb.AppendLine();

        if (publishedEvents.Length == 0)
        {
            sb.AppendLine($"No events were published to the stream '{streamId}'.");
        }
        else
        {
            if (result.IndexOfNotFoundExpectedEvent >= 0)
            {
                sb.AppendLine($"Expected event [{result.IndexOfNotFoundExpectedEvent}] not found.");
            }
            else if (result.IndexOfUnexpectedPublishedEvent >= 0)
            {
                sb.AppendLine($"Unexpected published event found at [{result.IndexOfUnexpectedPublishedEvent}].");
            }
            else
            {
                sb.AppendLine("Expected series of events not found.");
            }

            sb.Append(FormatPublishedEventsInfo(publishedEvents));
        }

        return sb.ToString();
    }

    public static string FormatNegativeEventMatchFail(string streamId, object[] publishedEvents, NegativeEventMatchResult result)
    {
        var sb = new StringBuilder();

        sb.Append($"Expected not to find any event matching {result.ExcludedEventQualifiersCount} specified rule{(result.ExcludedEventQualifiersCount == 1 ? string.Empty : "s")}.");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"Unexpected published event found at [{result.IndexOfUnexpectedPublishedEvent}].");
        sb.Append(FormatPublishedEventsInfo(publishedEvents));

        return sb.ToString();
    }

    public static string FormatNoEventsExpected(object[] publishedEvents)
    {
        var sb = new StringBuilder();

        sb.AppendLine("No events expected.");
        sb.Append(FormatPublishedEventsInfo(publishedEvents));

        return sb.ToString();
    }

    private static string FormatPublishedEventsInfo(object[] publishedEvents)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Published events:");
        var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From).ToArray();
        sb.AppendLine(string.Join(Environment.NewLine, serializedPublishedEvents.Select((pe, ix) => $"{ix}. [{pe.EventType}]{Environment.NewLine}{pe.Content}")));
        return sb.ToString();
    }
}
