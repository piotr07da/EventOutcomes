using System;
using System.Collections.Generic;
using System.Linq;

namespace EventOutcomes
{
    public class Test
    {
        private readonly Guid? _eventStreamId;

        public Test()
        {
        }

        public Test(Guid? eventStreamId)
        {
            _eventStreamId = eventStreamId;
        }

        public IList<object> ActCommands { get; } = new List<object>();

        public IDictionary<string, EventAssertionChain> AssertChecks { get; } = new Dictionary<string, EventAssertionChain>();

        public Guid EventStreamId => _eventStreamId ?? throw new Exception("Event stream Id has not been defined. Either call appropriate method overload or use Test.For method to create the Test object for specified stream Id.");

        public static Test For(Guid? eventStreamId) => new Test(eventStreamId);

        public Test Given(IEnumerable<object> initializationEvents) => Given(EventStreamId, initializationEvents.ToArray());

        public Test Given(Guid eventStreamId, IEnumerable<object> initializationEvents) => Given(eventStreamId, initializationEvents.ToArray());

        public Test Given(params object[] initializationEvents) => Given(EventStreamId, initializationEvents);

        public Test Given(Guid eventStreamId, params object[] initializationEvents)
        {
            throw new NotImplementedException();
        }

        public Test When(object commandToExecute)
        {
            ActCommands.Add(commandToExecute);
            return this;
        }

        public Test AllowExtra() => AllowExtra(EventStreamId);

        public Test AllowExtra(Guid eventStreamId)
        {
            throw new NotImplementedException();
        }

        public Test ThenAny() => ThenAny(EventStreamId);

        public Test ThenAny(Guid eventStreamId) => throw new NotImplementedException();

        public Test ThenNot(params Func<object, bool>[] excludedEventQualifiers) => ThenNot(EventStreamId, excludedEventQualifiers);

        public Test ThenNot(Guid eventStreamId, params Func<object, bool>[] excludedEventQualifiers) => throw new NotImplementedException();

        public Test Then(object expectedEvent) => Then(EventStreamId, expectedEvent);

        public Test Then(Guid eventStreamId, object expectedEvent) => ThenInOrder(eventStreamId, expectedEvent);

        public Test ThenInOrder(params object[] expectedEvents) => ThenInOrder(EventStreamId, expectedEvents);

        public Test ThenInOrder(Guid eventStreamId, params object[] expectedEvents) => ThenPositiveEventAssertion(eventStreamId, expectedEvents, PositiveEventAssertionOrder.InOrder);

        public Test ThenOutOfOrder(params object[] expectedEvents) => ThenOutOfOrder(EventStreamId, expectedEvents);

        public Test ThenOutOfOrder(Guid eventStreamId, params object[] expectedEvents) => throw new NotImplementedException();

        public Test ThenNone() => ThenNone(EventStreamId);

        public Test ThenNone(Guid eventStreamId)
        {
            throw new NotImplementedException();
        }

        private Test ThenPositiveEventAssertion(Guid eventStreamId, object[] expectedEvents, PositiveEventAssertionOrder order)
        {
            var checkChain = GetEventAssertionChain(eventStreamId);

            checkChain.AddPositiveAssertion(new PositiveEventAssertion(expectedEvents, order));

            return this;
        }

        private EventAssertionChain GetEventAssertionChain(Guid eventStreamId)
        {
            var key = eventStreamId.ToString();
            if (!AssertChecks.TryGetValue(key, out var checkChain))
            {
                checkChain = new EventAssertionChain();
                AssertChecks.Add(key, checkChain);
            }

            return checkChain;
        }
    }
}
