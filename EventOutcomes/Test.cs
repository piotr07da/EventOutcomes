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
        public ExceptionExpectation AssertException { get; private set; }

        public string EventStreamId => _eventStreamId ?? throw new Exception("Event stream Id has not been defined. Either call appropriate method overload or use Test.For method to create the Test object for specified stream Id.");

        public static Test For(Guid eventStreamId) => For(eventStreamId.ToString());

        public static Test For(string eventStreamId) => new Test(eventStreamId);

        public Test Given(IEnumerable<object> initializationEvents) => Given(EventStreamId, Enumerable.ToArray(initializationEvents));

        public Test Given(params object[] initializationEvents) => Given(EventStreamId, initializationEvents);

        public Test Given(Guid eventStreamId, IEnumerable<object> initializationEvents) => Given(eventStreamId.ToString(), Enumerable.ToArray(initializationEvents));

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

        public Test ThenException<TException>(IExceptionAssertion<TException> exceptionAssertion)
            where TException : Exception
        {
            if (AssertException != null)
                throw new InvalidOperationException("There is expected exception associated with this Test. You cannot expect another exception. Remove previous expectation if needed.");

            AssertException = new ExceptionExpectation(typeof(TException), exceptionAssertion);

            return this;
        }
    }

    public interface IExceptionAssertion<in TException>
        where TException : Exception
    {
        void Assert(TException thrownException);
    }

    public class ExceptionAssertion<TException> : IExceptionAssertion<TException>
        where TException : Exception
    {
        private readonly ExceptionMessageAssertion _messageAssertion;

        private ExceptionAssertion(ExceptionMessageAssertion messageAssertion)
        {
            _messageAssertion = messageAssertion ?? throw new ArgumentNullException(nameof(messageAssertion));
        }

        public void Assert(TException thrownException)
        {
            _messageAssertion.Assert(thrownException);
        }

        public static ExceptionAssertion<TException> For(ExceptionMessageAssertion messageAssertion)
        {
            return new ExceptionAssertion<TException>(messageAssertion);
        }
    }
}
