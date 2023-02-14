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
    internal IDictionary<string, EventAssertionsChain> AssertEventAssertionsChains { get; } = new Dictionary<string, EventAssertionsChain>();
    internal IList<Func<IServiceProvider, Task<AssertActionResult>>> AssertActions { get; } = new List<Func<IServiceProvider, Task<AssertActionResult>>>();
    internal IList<IExceptionAssertion> AssertExceptionAssertions { get; } = new List<IExceptionAssertion>();

    public static Test ForMany() => new();

    public static Test For(EventStreamId eventStreamId) => new(eventStreamId);

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
                throw new Exception($"Component of type {service.GetType().Name} (instead of {typeof(TFakeService).Name}) has been resolved for service of type {typeof(TService).Name}.");
            }
        });
    }

    public Test Given<TService>(Action<TService> arrangeAction)
        where TService : notnull
    {
        return Given(sp =>
        {
            var service = sp.GetRequiredService<TService>();
            arrangeAction(service);
        });
    }

    public Test Given(Action<IServiceProvider> arrangeAction)
    {
        ArrangeActions.Add(arrangeAction);
        return this;
    }

    public Test Given(IEnumerable<object> initializationEvents) => Given(initializationEvents.ToArray());

    public Test Given(params object[] initializationEvents) => Given(EventStreamId(), initializationEvents);

    public Test Given(EventStreamId eventStreamId, IEnumerable<object> initializationEvents) => Given(eventStreamId, initializationEvents.ToArray());

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

    public Test ThenAny() => ThenAny(EventStreamId());

    public Test ThenAny(EventStreamId eventStreamId) => ThenNegativeEventAssertion(eventStreamId, Array.Empty<Func<object, bool>>());

    public Test ThenNot(params Func<object, bool>[] excludedEventQualifiers) => ThenNot(EventStreamId(), excludedEventQualifiers);

    public Test ThenNot(EventStreamId eventStreamId, params Func<object, bool>[] excludedEventQualifiers) => ThenNegativeEventAssertion(eventStreamId, excludedEventQualifiers);

    public Test Then(object expectedEvent) => Then(EventStreamId(), expectedEvent);

    public Test Then(EventStreamId eventStreamId, object expectedEvent) => ThenInOrder(eventStreamId, expectedEvent);

    public Test ThenInOrder(params object[] expectedEvents) => ThenInOrder(EventStreamId(), expectedEvents);

    public Test ThenInOrder(EventStreamId eventStreamId, params object[] expectedEvents) => ThenPositiveEventAssertion(eventStreamId, expectedEvents, PositiveEventAssertionOrder.InOrder);

    public Test ThenInAnyOrder(params object[] expectedEvents) => ThenInAnyOrder(EventStreamId(), expectedEvents);

    public Test ThenInAnyOrder(EventStreamId eventStreamId, params object[] expectedEvents) => ThenPositiveEventAssertion(eventStreamId, expectedEvents, PositiveEventAssertionOrder.InAnyOrder);

    public Test ThenNone() => ThenNone(EventStreamId());

    public Test ThenNone(EventStreamId eventStreamId)
    {
        var checkChain = GetEventAssertionChain(eventStreamId);

        checkChain.AddNoneAssertion();

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

            throw new Exception($"Component of type {service.GetType().Name} (instead of {typeof(TFakeService).Name}) has been resolved for service of type {typeof(TService).Name}.");
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
        if (!AssertEventAssertionsChains.TryGetValue(eventStreamId, out var checkChain))
        {
            checkChain = new EventAssertionsChain();
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

    private string EventStreamId([CallerMemberName] string callerMemberName = "") => _eventStreamId ?? throw new Exception($"If Test class was created using Test.ForMany() then you have to pass eventStreamId argument to the {callerMemberName}(...) method. Alternatively you can create the Test class specifying event stream id using Test.For(eventStreamId).");
}
