using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace EventOutcomes;

public sealed class Test
{
    private readonly string? _eventStreamId;

    private Test()
    {
    }

    private Test(string eventStreamId)
    {
        _eventStreamId = eventStreamId;
    }

    internal IList<Action<IServiceProvider>> ArrangeActions { get; } = new List<Action<IServiceProvider>>();
    internal IDictionary<string, IEnumerable<object>> ArrangeEvents { get; } = new Dictionary<string, IEnumerable<object>>();
    internal IList<object> ActCommands { get; } = new List<object>();
    internal IDictionary<string, EventMatchCheckersChain> AssertEventAssertionsChains { get; } = new Dictionary<string, EventMatchCheckersChain>();
    internal IList<Func<IServiceProvider, Task<AssertActionResult>>> AssertActions { get; } = new List<Func<IServiceProvider, Task<AssertActionResult>>>();
    internal IList<IExceptionAssertion> AssertExceptionAssertions { get; } = new List<IExceptionAssertion>();

    /// <summary>
    ///     Creates an instance of a test. The test will allow defining and expecting events for multiple event streams.
    ///     Therefore, it is necessary to specify the event stream identifier when using the Given and Then methods.
    /// </summary>
    public static Test ForMany() => new();

    /// <summary>
    ///     Creates an instance of a test. The test will be created for single event stream only. Therefore, when using Given
    ///     and Then methods to prepare and expect events, specifying the event stream identifier in those methods is optional.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    public static Test For(EventStreamId eventStreamId) => new(eventStreamId);

    /// <summary>
    ///     Registers a delegate that will be invoked to prepare the test. It allows stubbing data using a fake service
    ///     implementation.
    /// </summary>
    /// <typeparam name="TService">The type of service.</typeparam>
    /// <typeparam name="TFakeService">The type of the fake service implementation.</typeparam>
    /// <param name="arrangeAction">The action to be invoked to prepare the test.</param>
    /// <exception cref="EventOutcomesException">
    ///     An exception that will be thrown if an instance of the service resolved from
    ///     the service provider is not of the type of fake service.
    /// </exception>
    public Test Given<TService, TFakeService>(Action<TFakeService> arrangeAction)
        where TService : notnull
        where TFakeService : TService
    {
        return Given(sp =>
        {
            var service = sp.GetRequiredService<TService>();
            if (service is TFakeService fakeService)
            {
                arrangeAction(fakeService);
            }
            else
            {
                throw new EventOutcomesException($"Component of type {service.GetType().Name} (instead of {typeof(TFakeService).Name}) has been resolved for service of type {typeof(TService).Name}.");
            }
        });
    }

    /// <summary>
    ///     Registers a delegate that will be invoked to prepare the test. It allows invoking methods on a service.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="arrangeAction">The action to be invoked to prepare the test.</param>
    public Test Given<TService>(Action<TService> arrangeAction)
        where TService : notnull
    {
        return Given(sp =>
        {
            var service = sp.GetRequiredService<TService>();
            arrangeAction(service);
        });
    }

    /// <summary>
    ///     Registers a delegate that will be invoked to prepare the test. It allows resolving services and invoking methods on
    ///     the resolved services.
    /// </summary>
    /// <param name="arrangeAction">The action to be invoked to prepare the test.</param>
    public Test Given(Action<IServiceProvider> arrangeAction)
    {
        ArrangeActions.Add(arrangeAction);
        return this;
    }

    /// <summary>
    ///     Prepares the test by specifying events that already occurred, which represent initial state for the test.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="Given(EventOutcomes.EventStreamId, IEnumerable{object})" />, specifying the event stream
    ///     identifier.
    /// </remarks>
    /// <param name="initializationEvents">The list of events that have already occurred.</param>
    public Test Given(IEnumerable<object> initializationEvents) => Given(initializationEvents.ToArray());

    /// <summary>
    ///     Prepares the test by specifying events that already occurred, which represent initial state for the test.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="Given(EventOutcomes.EventStreamId, object[])" />, specifying the event stream identifier.
    /// </remarks>
    /// <param name="initializationEvents">The list of events that have already occurred.</param>
    public Test Given(params object[] initializationEvents) => Given(EventStreamId(), initializationEvents);

    /// <summary>
    ///     Prepares the test by specifying events that already occurred, which represent initial state for the test.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    /// <param name="initializationEvents">The list of events that have already occurred.</param>
    public Test Given(EventStreamId eventStreamId, IEnumerable<object> initializationEvents) => Given(eventStreamId, initializationEvents.ToArray());

    /// <summary>
    ///     Prepares the test by specifying events that already occurred, which represent initial state for the test.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    /// <param name="initializationEvents">The list of events that have already occurred.</param>
    public Test Given(EventStreamId eventStreamId, params object[] initializationEvents)
    {
        if (ArrangeEvents.TryGetValue(eventStreamId, out var currentInitializationEvents))
        {
            var list = currentInitializationEvents.ToList();
            list.AddRange(initializationEvents);
            ArrangeEvents[eventStreamId] = list.ToArray();
        }
        else
        {
            ArrangeEvents.Add(eventStreamId, initializationEvents);
        }

        return this;
    }

    /// <summary>
    ///     Prepares the test by specifying the command that will be executed.
    /// </summary>
    /// <param name="commandToExecute">The command to be executed.</param>
    public Test When(object commandToExecute)
    {
        ActCommands.Add(commandToExecute);
        return this;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Test AllowExtra() => AllowExtra(EventStreamId());

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Test AllowExtra(EventStreamId eventStreamId)
    {
        // The idea is to allow for extra events in ThenInOrder and ThenInAnyOrder. For example, after enabling AllowExtra the following chain of events A, X, B, Y, C, Z matches following assertion ThenInOrder(A, B, C).
        // Maybe instead of changing behavior globally by calling AllowExtra there should rather be a parameter in ThenInOrder and ThenInAnyOrder methods.
        throw new NotImplementedException();
    }

    /// <summary>
    ///     The test passes if any events occurred or if no events occurred. This method only makes sense when it is combined
    ///     with
    ///     other Then methods.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="ThenAny(EventOutcomes.EventStreamId)" />, specifying the event stream identifier.
    /// </remarks>
    public Test ThenAny() => ThenAny(EventStreamId());

    /// <summary>
    ///     The test passes if any events occurred or if no events occurred. This method only makes sense when it is combined
    ///     with
    ///     other Then methods.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    public Test ThenAny(EventStreamId eventStreamId) => ThenNegativeEventMatchChecker(eventStreamId, Array.Empty<Func<object, bool>>());

    /// <summary>
    ///     The test passes if none of the events that occurred matches any of the <see cref="excludedEventQualifiers" />.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="ThenNot(EventOutcomes.EventStreamId, Func{object, bool}[])" />, specifying the event
    ///     stream identifier.
    /// </remarks>
    /// <param name="excludedEventQualifiers">List of the qualifiers of not expected events to occur.</param>
    public Test ThenNot(params Func<object, bool>[] excludedEventQualifiers) => ThenNot(EventStreamId(), excludedEventQualifiers);

    /// <summary>
    ///     The test passes if none of the events that occurred matches any of the <see cref="excludedEventQualifiers" />.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    /// <param name="excludedEventQualifiers">List of the qualifiers of not expected events to occur.</param>
    public Test ThenNot(EventStreamId eventStreamId, params Func<object, bool>[] excludedEventQualifiers) => ThenNegativeEventMatchChecker(eventStreamId, excludedEventQualifiers);

    /// <summary>
    ///     The test passes if exactly one event occurred and that event is the same as the event specified in the
    ///     <see cref="expectedEvent" /> argument.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="Then(EventOutcomes.EventStreamId, object)" />, specifying the event stream identifier.
    /// </remarks>
    /// <param name="expectedEvent">The expected event.</param>
    public Test Then(object expectedEvent) => Then(EventStreamId(), expectedEvent);

    /// <summary>
    ///     The test passes if exactly one event occurred and that event is the same as the event specified in the
    ///     <see cref="expectedEvent" /> argument.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    /// <param name="expectedEvent">The expected event.</param>
    public Test Then(EventStreamId eventStreamId, object expectedEvent) => ThenInOrder(eventStreamId, expectedEvent);

    /// <summary>
    ///     The test passes if the same events occurred in the specified order.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="ThenInOrder(EventOutcomes.EventStreamId, object[])" />, specifying the event stream
    ///     identifier.
    /// </remarks>
    /// <param name="expectedEvents">The expected events.</param>
    public Test ThenInOrder(params object[] expectedEvents) => ThenInOrder(EventStreamId(), expectedEvents);

    /// <summary>
    ///     The test passes if the same events occurred in the specified order.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    /// <param name="expectedEvents">The expected events.</param>
    public Test ThenInOrder(EventStreamId eventStreamId, params object[] expectedEvents) => ThenPositiveEventMatchChecker(eventStreamId, expectedEvents, PositiveEventMatchOrder.InOrder);

    /// <summary>
    ///     The test passes if the same events occurred in any order.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="ThenInAnyOrder(EventOutcomes.EventStreamId, object[])" />, specifying the event stream
    ///     identifier.
    /// </remarks>
    /// <param name="expectedEvents">The expected events.</param>
    public Test ThenInAnyOrder(params object[] expectedEvents) => ThenInAnyOrder(EventStreamId(), expectedEvents);

    /// <summary>
    ///     The test passes if the same events occurred in any order.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    /// <param name="expectedEvents">The expected events.</param>
    public Test ThenInAnyOrder(EventStreamId eventStreamId, params object[] expectedEvents) => ThenPositiveEventMatchChecker(eventStreamId, expectedEvents, PositiveEventMatchOrder.InAnyOrder);

    /// <summary>
    ///     The test passes if no event was published and no exception was thrown.
    /// </summary>
    /// <remarks>
    ///     Use this method only if the test instance was created using <see cref="For(EventOutcomes.EventStreamId)" /> method.
    ///     Otherwise, use <see cref="ThenNone(EventOutcomes.EventStreamId)" />, specifying the event stream identifier.
    /// </remarks>
    public Test ThenNone() => ThenNone(EventStreamId());

    /// <summary>
    ///     The test passes if no event was published and no exception was thrown.
    /// </summary>
    /// <param name="eventStreamId">The event stream identifier.</param>
    public Test ThenNone(EventStreamId eventStreamId)
    {
        var checkChain = GetEventAssertionChain(eventStreamId);

        checkChain.AddNoneChecker();

        return this;
    }

    public Test Then<TService, TFakeService>(Func<TFakeService, AssertActionResult> assertAction)
        where TService : notnull
        where TFakeService : TService
    {
        return Then<TService, TFakeService>(fakeService => Task.FromResult(assertAction(fakeService)));
    }

    public Test Then<TService, TFakeService>(Func<TFakeService, Task<AssertActionResult>> assertAction)
        where TService : notnull
        where TFakeService : TService
    {
        return Then(sp =>
        {
            var service = sp.GetRequiredService<TService>();
            if (service is TFakeService fakeService)
            {
                return assertAction(fakeService);
            }

            throw new EventOutcomesException($"Component of type {service.GetType().Name} (instead of {typeof(TFakeService).Name}) has been resolved for service of type {typeof(TService).Name}.");
        });
    }

    public Test Then<TService>(Func<TService, AssertActionResult> assertAction)
        where TService : notnull
    {
        return Then<TService>(service => Task.FromResult(assertAction(service)));
    }

    public Test Then<TService>(Func<TService, Task<AssertActionResult>> assertAction)
        where TService : notnull
    {
        return Then(sp =>
        {
            var service = sp.GetRequiredService<TService>();
            return assertAction(service);
        });
    }

    public Test Then(Func<IServiceProvider, AssertActionResult> assertAction)
    {
        return Then(sp => Task.FromResult(assertAction(sp)));
    }

    public Test Then(Func<IServiceProvider, Task<AssertActionResult>> assertAction)
    {
        AssertActions.Add(assertAction);
        return this;
    }

    public Test ThenException(Func<Exception, bool> expectedExceptionCondition)
    {
        ThenException(new ConditionExceptionAssertion(expectedExceptionCondition));
        return this;
    }

    public Test ThenAnyException<TExpectedException>(Func<TExpectedException, bool> expectedExceptionCondition)
        where TExpectedException : Exception
    {
        ThenException(
            new TypeExceptionAssertion(typeof(TExpectedException), true),
            new ConditionExceptionAssertion(e => expectedExceptionCondition(e as TExpectedException ?? throw new AssertException($"Exception of type '{e.GetType().FullName ?? "EMPTY"}' is not of type '{typeof(TExpectedException).FullName ?? "EMPTY"}'."))));
        return this;
    }

    public Test ThenException<TExpectedException>(Func<TExpectedException, bool> expectedExceptionCondition)
        where TExpectedException : Exception
    {
        ThenException(
            new TypeExceptionAssertion(typeof(TExpectedException), false),
            new ConditionExceptionAssertion(e => expectedExceptionCondition(e as TExpectedException ?? throw new AssertException($"Exception of type '{e.GetType().FullName ?? "EMPTY"}' is not of type '{typeof(TExpectedException).FullName ?? "EMPTY"}'."))));
        return this;
    }

    public Test ThenAnyException<TExpectedException>(string expectedMessage, MessageExceptionAssertionType matchingType)
        where TExpectedException : Exception
    {
        return ThenException<TExpectedException>(true, expectedMessage, matchingType);
    }

    public Test ThenException<TExpectedException>(string expectedMessage, MessageExceptionAssertionType matchingType)
        where TExpectedException : Exception
    {
        return ThenException<TExpectedException>(false, expectedMessage, matchingType);
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

    public Test ThenException(string expectedMessage, MessageExceptionAssertionType matchingType)
    {
        return ThenException(new MessageExceptionAssertion(expectedMessage, matchingType));
    }

    public Test ThenException(params IExceptionAssertion[] exceptionAssertions)
    {
        foreach (var exceptionAssertion in exceptionAssertions)
        {
            AssertExceptionAssertions.Add(exceptionAssertion);
        }

        return this;
    }

    private Test ThenNegativeEventMatchChecker(string eventStreamId, Func<object, bool>[] excludedEventQualifiers)
    {
        var checkChain = GetEventAssertionChain(eventStreamId);

        checkChain.AddNegativeMatcherChecker(new NegativeEventMatchChecker(excludedEventQualifiers));

        return this;
    }

    private Test ThenPositiveEventMatchChecker(string eventStreamId, object[] expectedEvents, PositiveEventMatchOrder order)
    {
        var checkChain = GetEventAssertionChain(eventStreamId);

        checkChain.AddPositiveMatchChecker(new PositiveEventMatchChecker(expectedEvents, order));

        return this;
    }

    private EventMatchCheckersChain GetEventAssertionChain(string eventStreamId)
    {
        if (!AssertEventAssertionsChains.TryGetValue(eventStreamId, out var checkChain))
        {
            checkChain = new EventMatchCheckersChain();
            AssertEventAssertionsChains.Add(eventStreamId, checkChain);
        }

        return checkChain;
    }

    private Test ThenException<TExpectedException>(bool anyDerived, string expectedMessage, MessageExceptionAssertionType matchingType)
        where TExpectedException : Exception
    {
        return ThenException(
            new TypeExceptionAssertion(typeof(TExpectedException), anyDerived),
            new MessageExceptionAssertion(expectedMessage, matchingType));
    }

    private Test ThenException<TExpectedException>(bool anyDerived)
        where TExpectedException : Exception
    {
        return ThenException(new TypeExceptionAssertion(typeof(TExpectedException), anyDerived));
    }

    public string EventStreamId([CallerMemberName] string callerMemberName = "") => _eventStreamId ?? throw new EventOutcomesException($"If Test class was created using Test.ForMany() then you have to pass eventStreamId argument to the {callerMemberName}(...) method. Alternatively you can create the Test class specifying event stream id using Test.For(eventStreamId).");
}
