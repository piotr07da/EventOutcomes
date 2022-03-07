using System;
using System.Collections.Generic;
using System.Linq;

namespace EventOutcomes
{
    public class Test
    {
        private readonly string _eventStreamId;

        public Test()
        {
        }

        public Test(string eventStreamId)
        {
            _eventStreamId = eventStreamId;
        }

        public IList<object> ActCommands { get; } = new List<object>();
        public IDictionary<string, EventAssertionsChain> AssertEventAssertionsChains { get; } = new Dictionary<string, EventAssertionsChain>();
        public IList<IExceptionAssertion> AssertExceptionAssertions { get; } = new List<IExceptionAssertion>();

        public string EventStreamId => _eventStreamId ?? throw new Exception("Event stream Id has not been defined. Either call appropriate method overload or use Test.For method to create the Test object for specified stream Id.");

        public static Test For(Guid eventStreamId) => For(eventStreamId.ToString());

        public static Test For(string eventStreamId) => new Test(eventStreamId);

        public Test Given(IEnumerable<object> initializationEvents) => Given(EventStreamId, initializationEvents.ToArray());

        public Test Given(params object[] initializationEvents) => Given(EventStreamId, initializationEvents);

        public Test Given(Guid eventStreamId, IEnumerable<object> initializationEvents) => Given(eventStreamId.ToString(), initializationEvents.ToArray());

        public Test Given(Guid eventStreamId, params object[] initializationEvents) => Given(eventStreamId.ToString(), initializationEvents);

        public Test Given(string eventStreamId, params object[] initializationEvents)
        {
            // TODO
            return this;
        }

        public Test When(object commandToExecute)
        {
            ActCommands.Add(commandToExecute);
            return this;
        }

        public Test AllowExtra() => AllowExtra(EventStreamId);

        public Test AllowExtra(Guid eventStreamId) => AllowExtra(eventStreamId.ToString());

        public Test AllowExtra(string eventStreamId)
        {
            throw new NotImplementedException();
        }

        public Test ThenAny() => ThenAny(EventStreamId);

        public Test ThenAny(Guid eventStreamId) => ThenAny(eventStreamId.ToString());

        public Test ThenAny(string eventStreamId) => ThenNegativeEventAssertion(eventStreamId, Array.Empty<Func<object, bool>>());

        public Test ThenNot(params Func<object, bool>[] excludedEventQualifiers) => ThenNot(EventStreamId, excludedEventQualifiers);

        public Test ThenNot(Guid eventStreamId, params Func<object, bool>[] excludedEventQualifiers) => ThenNot(eventStreamId.ToString(), excludedEventQualifiers);

        public Test ThenNot(string eventStreamId, params Func<object, bool>[] excludedEventQualifiers) => ThenNegativeEventAssertion(eventStreamId, excludedEventQualifiers);

        public Test Then(object expectedEvent) => Then(EventStreamId, expectedEvent);

        public Test Then(Guid eventStreamId, object expectedEvent) => Then(eventStreamId.ToString(), expectedEvent);

        public Test Then(string eventStreamId, object expectedEvent) => ThenInOrder(eventStreamId, expectedEvent);

        public Test ThenInOrder(params object[] expectedEvents) => ThenInOrder(EventStreamId, expectedEvents);

        public Test ThenInOrder(Guid eventStreamId, params object[] expectedEvents) => ThenInOrder(eventStreamId.ToString(), expectedEvents);

        public Test ThenInOrder(string eventStreamId, params object[] expectedEvents) => ThenPositiveEventAssertion(eventStreamId, expectedEvents, PositiveEventAssertionOrder.InOrder);

        public Test ThenOutOfOrder(params object[] expectedEvents) => ThenOutOfOrder(EventStreamId, expectedEvents);

        public Test ThenOutOfOrder(Guid eventStreamId, params object[] expectedEvents) => ThenOutOfOrder(eventStreamId.ToString(), expectedEvents);

        public Test ThenOutOfOrder(string eventStreamId, params object[] expectedEvents) => ThenPositiveEventAssertion(eventStreamId, expectedEvents, PositiveEventAssertionOrder.OutOfOrder);

        public Test ThenNone() => ThenNone(EventStreamId);

        public Test ThenNone(Guid eventStreamId) => ThenNone(eventStreamId.ToString());

        public Test ThenNone(string eventStreamId)
        {
            var checkChain = GetEventAssertionChain(eventStreamId);

            checkChain.AddNoneAssertion();

            return this;
        }

        private Test ThenNegativeEventAssertion(string eventStreamId, Func<object, bool>[] excludedEventQualifiers)
        {
            var checkChain = GetEventAssertionChain(eventStreamId);

            checkChain.AddNegativeAssertion(new NegativeEventAssertion(excludedEventQualifiers));

            return this;
        }

        private Test ThenPositiveEventAssertion(string eventStreamId, object[] expectedEvents, PositiveEventAssertionOrder order)
        {
            var checkChain = GetEventAssertionChain(eventStreamId);

            checkChain.AddPositiveAssertion(new PositiveEventAssertion(expectedEvents, order));

            return this;
        }

        private EventAssertionsChain GetEventAssertionChain(string eventStreamId)
        {
            var key = eventStreamId;
            if (!AssertEventAssertionsChains.TryGetValue(key, out var checkChain))
            {
                checkChain = new EventAssertionsChain();
                AssertEventAssertionsChains.Add(key, checkChain);
            }

            return checkChain;
        }

        public Test ThenAnyException<TExpectedException>(string expectedMessage, ExceptionMessageAssertionMatchingType matchingType)
            where TExpectedException : Exception
        {
            return ThenException<TExpectedException>(true, expectedMessage, matchingType);
        }

        public Test ThenException<TExpectedException>(string expectedMessage, ExceptionMessageAssertionMatchingType matchingType)
            where TExpectedException : Exception
        {
            return ThenException<TExpectedException>(false, expectedMessage, matchingType);
        }

        private Test ThenException<TExpectedException>(bool anyDerived, string expectedMessage, ExceptionMessageAssertionMatchingType matchingType)
            where TExpectedException : Exception
        {
            return ThenException(
                new ExceptionTypeAssertion(typeof(TExpectedException), anyDerived),
                new ExceptionMessageAssertion(expectedMessage, matchingType));
        }

        public Test ThenException<TExpectedException>()
            where TExpectedException : Exception
        {
            return ThenException<TExpectedException>(false);
        }

        public Test ThenAnyException<TExpectedException>()
            where TExpectedException : Exception
        {
            return ThenException<TExpectedException>(true);
        }

        private Test ThenException<TExpectedException>(bool anyDerived)
            where TExpectedException : Exception
        {
            return ThenException(new ExceptionTypeAssertion(typeof(TExpectedException), anyDerived));
        }

        public Test ThenException(string expectedMessage, ExceptionMessageAssertionMatchingType matchingType)
        {
            return ThenException(new ExceptionMessageAssertion(expectedMessage, matchingType));
        }

        public Test ThenException(params IExceptionAssertion[] exceptionAssertions)
        {
            foreach (var exceptionAssertion in exceptionAssertions)
            {
                AssertExceptionAssertions.Add(exceptionAssertion);
            }

            return this;
        }
    }
}
