using System.ComponentModel;

namespace EventOutcomes;

public sealed class TestForSingleStream
{
    private readonly string _eventStreamId;

    internal TestForSingleStream(Test test, string eventStreamId)
    {
        Test = test ?? throw new ArgumentNullException(nameof(test));
        _eventStreamId = eventStreamId ?? throw new ArgumentNullException(nameof(eventStreamId));
    }

    internal Test Test { get; }

    public TestForSingleStream Given<TService, TFakeService>(Action<TFakeService> arrangeAction)
        where TService : notnull
        where TFakeService : TService
    {
        Test.Given<TService, TFakeService>(arrangeAction);
        return this;
    }

    public TestForSingleStream Given<TService>(Action<TService> arrangeAction)
        where TService : notnull
    {
        Test.Given(arrangeAction);
        return this;
    }

    public TestForSingleStream Given(Action<IServiceProvider> arrangeAction)
    {
        Test.Given(arrangeAction);
        return this;
    }

    public TestForSingleStream Given(IEnumerable<object> initializationEvents) => Given(_eventStreamId, initializationEvents.ToArray());

    public TestForSingleStream Given(params object[] initializationEvents)
    {
        Test.Given(_eventStreamId, initializationEvents);
        return this;
    }

    public TestForSingleStream When(object commandToExecute)
    {
        Test.When(commandToExecute);
        return this;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TestForSingleStream AllowExtra()
    {
        Test.AllowExtra(_eventStreamId);
        return this;
    }

    public TestForSingleStream ThenAny()
    {
        Test.ThenAny(_eventStreamId);
        return this;
    }

    public TestForSingleStream ThenNot(params Func<object, bool>[] excludedEventQualifiers)
    {
        Test.ThenNot(_eventStreamId, excludedEventQualifiers);
        return this;
    }

    public TestForSingleStream Then(object expectedEvent)
    {
        Test.Then(_eventStreamId, expectedEvent);
        return this;
    }

    public TestForSingleStream ThenInOrder(params object[] expectedEvents)
    {
        Test.ThenInOrder(_eventStreamId, expectedEvents);
        return this;
    }

    public TestForSingleStream ThenInAnyOrder(params object[] expectedEvents)
    {
        Test.ThenInAnyOrder(_eventStreamId, expectedEvents);
        return this;
    }

    public TestForSingleStream ThenNone()
    {
        Test.ThenNone(_eventStreamId);
        return this;
    }

    public TestForSingleStream Then<TService, TFakeService>(Func<TFakeService, AssertActionResult> assertAction)
        where TService : notnull
        where TFakeService : TService
    {
        Test.Then<TService, TFakeService>(assertAction);
        return this;
    }

    public TestForSingleStream Then<TService, TFakeService>(Func<TFakeService, Task<AssertActionResult>> assertAction)
        where TService : notnull
        where TFakeService : TService
    {
        Test.Then<TService, TFakeService>(assertAction);
        return this;
    }

    public TestForSingleStream Then<TService>(Func<TService, AssertActionResult> assertAction)
        where TService : notnull
    {
        Test.Then(assertAction);
        return this;
    }

    public TestForSingleStream Then<TService>(Func<TService, Task<AssertActionResult>> assertAction)
        where TService : notnull
    {
        Test.Then(assertAction);
        return this;
    }

    public TestForSingleStream Then(Func<IServiceProvider, AssertActionResult> assertAction)
    {
        Test.Then(assertAction);
        return this;
    }

    public TestForSingleStream Then(Func<IServiceProvider, Task<AssertActionResult>> assertAction)
    {
        Test.Then(assertAction);
        return this;
    }

    public TestForSingleStream ThenException(Func<Exception, bool> expectedExceptionCondition)
    {
        Test.ThenException(expectedExceptionCondition);
        return this;
    }

    public TestForSingleStream ThenAnyException<TExpectedException>(Func<TExpectedException, bool> expectedExceptionCondition)
        where TExpectedException : Exception
    {
        Test.ThenAnyException(expectedExceptionCondition);
        return this;
    }

    public TestForSingleStream ThenException<TExpectedException>(Func<TExpectedException, bool> expectedExceptionCondition)
        where TExpectedException : Exception
    {
        Test.ThenException(expectedExceptionCondition);
        return this;
    }

    public TestForSingleStream ThenAnyException<TExpectedException>(string expectedMessage, MessageExceptionAssertionType matchingType)
        where TExpectedException : Exception
    {
        Test.ThenAnyException<TExpectedException>(expectedMessage, matchingType);
        return this;
    }

    public TestForSingleStream ThenException<TExpectedException>(string expectedMessage, MessageExceptionAssertionType matchingType)
        where TExpectedException : Exception
    {
        Test.ThenException<TExpectedException>(expectedMessage, matchingType);
        return this;
    }

    public TestForSingleStream ThenException<TExpectedException>()
        where TExpectedException : Exception
    {
        Test.ThenException<TExpectedException>();
        return this;
    }

    public TestForSingleStream ThenAnyException<TExpectedException>()
        where TExpectedException : Exception
    {
        Test.ThenAnyException<TExpectedException>();
        return this;
    }

    public TestForSingleStream ThenException(string expectedMessage, MessageExceptionAssertionType matchingType)
    {
        Test.ThenException(expectedMessage, matchingType);
        return this;
    }

    public TestForSingleStream ThenException(params IExceptionAssertion[] exceptionAssertions)
    {
        Test.ThenException(exceptionAssertions);
        return this;
    }
}
