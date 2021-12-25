using System;
using System.Linq;

namespace EventOutcomes
{
    public class PositiveEventAssertion
    {
        public PositiveEventAssertion(object[] expectedEvents, PositiveEventAssertionOrder order)
        {
            if (expectedEvents == null || expectedEvents.Length == 0) throw new ArgumentException("Expected events collection is empty.", nameof(expectedEvents));

            ExpectedEvents = expectedEvents;
            Order = order;
        }

        public object[] ExpectedEvents { get; }
        public PositiveEventAssertionOrder Order { get; }

        public int FindAssertionIndex(object[] events)
        {
            for (var eIx = 0; eIx < events.Length - ExpectedEvents.Length + 1; ++eIx)
            {
                var eventsToAssert = events.Range(eIx, eIx + ExpectedEvents.Length);
                if (Assert(eventsToAssert))
                {
                    return eIx;
                }
            }

            return -1;
        }

        public bool Assert(object[] events)
        {
            if (events.Length > 0 && ExpectedEvents.Length == 0)
            {
                return false;
            }

            if (events.Length < ExpectedEvents.Length)
            {
                return false;
            }

            if (Order == PositiveEventAssertionOrder.InOrder)
            {
                return CheckInOrder(events);
            }

            if (Order == PositiveEventAssertionOrder.OutOfOrder)
            {
                return CheckOutOfOrder(events);
            }

            return false;
        }

        private bool CheckInOrder(object[] publishedEvents)
        {
            var serializedExpectedEvents = ExpectedEvents.Select(ComparableEventDocument.From).ToArray();
            var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From).ToArray();

            var expectedEventIndex = 0;
            foreach (var serializedPublishedEvent in serializedPublishedEvents)
            {
                if (serializedPublishedEvent != serializedExpectedEvents[expectedEventIndex])
                {
                    return false;
                }

                ++expectedEventIndex;

                if (expectedEventIndex == ExpectedEvents.Length)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckOutOfOrder(object[] publishedEvents)
        {
            var serializedExpectedEvents = ExpectedEvents.Select(ComparableEventDocument.From).ToArray();
            var serializedPublishedEvents = publishedEvents.Select(ComparableEventDocument.From).ToArray();

            var serializedExpectedEventsLeft = serializedExpectedEvents.ToHashSet();
            foreach (var serializedPublishedEvent in serializedPublishedEvents)
            {
                if (!serializedExpectedEventsLeft.Contains(serializedPublishedEvent))
                {
                    return false;
                }

                serializedExpectedEventsLeft.Remove(serializedPublishedEvent);

                if (serializedExpectedEventsLeft.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            var serializedExpectedEvents = ExpectedEvents.Select(ComparableEventDocument.From);
            return string.Join(Environment.NewLine, serializedExpectedEvents.Select((pe, ix) => $"[{pe.EventType}]{Environment.NewLine}{pe.Content}"));
        }
    }
}
