# EventOutcomes

EventOutcomes is a free, open-source .NET library created to make it easier to write unit tests for event sourcing based applications.
It is based on the idea presented by Greg Young:
> GIVEN events
> WHEN command
> THEN events

EventOutcomes is MIT licensed.

## Status
[![build-n-publish](https://github.com/piotr07da/EventOutcomes/actions/workflows/build-n-publish.yml/badge.svg)](https://github.com/piotr07da/EventOutcomes/actions/workflows/build-n-publish.yml)

## Features

- GIVEN events, WHEN command, THEN events
- Additional exception assertions
- Additional arrangements and assertions on services, whether their real or fake implementations
- Support for dependency injection
- Independent of Event Sourcing and CQRS frameworks selection

## Using EventOutcomes

### Basic test

Let's start by creating the simplest possible unit test for an event-sourced application. Every test we will write later will be based on the following structure.
```csharp
[Fact]
public async Task given_bread_ingredients_mixed_when_BakeDough_for_25_minutes_then_bread_baked_and_fantastic_smell_produced()
{
    var id = Guid.NewGuid();
    var test = Test.For(id)
        .Given(new FlourAdded(id, 500), new WaterAdded(id, 300), new YeastAdded(id, 7), new SaltAdded(id, 2), new IngredientsMixed(id))
        .When(new BakeDough(id, 25))
        .ThenInAnyOrder(new BreadBaked(id, 1), new SmellProduced(id, TypeOfSmell.Fantastic));
        
    await Tester.TestAsync(test, new MyCustomAdapter());
}
```

As you can see above, the last line includes a class ``MyCustomAdapter``, which is a custom implementation of EventOutcomes ``IAdapeter`` interface. To start using EventOutcomes to write unit tests for your application, all you need to do is create your own implementation in your project.

### IAdapter

There are many ways to implement Event Sourcing and CQRS &ndash; you can use one of the existing frameworks or create your own implementation. EventOutcomes is designed to be framework-agnostic, meaning it can be used with any event sourcing and CQRS frameworks. However, there is one requirement before you can use EventOutcomes: you must implement the ``IAdapter`` interface. This interface acts as a common denominator between EventOutcomes and your event sourcing and CQRS frameworks of choice. Below is an explanation of what needs to be implemented in the interface.
- ``IServiceProvider ServiceProvider { get; }`` &ndash; Service provider for all the services that need to be injected into your application code.
- ``Task BeforeTestAsync();`` &ndash; A method that is called before the test is executed. If scoped services are required, this is the ideal place to create a scope and assign scoped service provider to the ``ServiceProvider`` property.
- ``Task AfterTestAsync();`` &ndash; A method that is called after the test is completed. This is where any cleanup code should go.
- ``Task SetGivenEventsAsync(IDictionary<string, IEnumerable<object>> events);`` &ndash; A method that saves the GIVEN events (events that have already occurred) to a location where they can be read by the Event Sourcing framework of your choice to rehydrate domain objects (e.g., aggregates in DDD). This can be a fake in-memory implementation of an ``IEventDatabase`` interface or a similiar interface used by your framework of choice.
- ``Task<IDictionary<string, IEnumerable<object>>> GetPublishedEventsAsync();`` &ndash; In every event sourcing framework, there is a component responsible for saving newly published events. Implement this method so that EventOutcomes can retrieve those newly published events.
- ``Task DispatchCommandAsync(object command);`` &ndash; Place your command dispatching code here. For example: ``await _commandDispatcher.Dispatch(command);`` or ``await _massTransitMediator.Publish(command);``.

Below is an example of how ``IAdapter`` can be implemented.
```csharp
public class MyAdapter : IAdapter
{
    public MyAdapter()
    {
        var services = new ServiceCollection();
        services.AddScoped<IEventDatabase, FakeEventDatabase>();
        // register all other services here - your main application registration code and all other fakes used for your tests
        ServiceProvider = services.BuildServiceProvider();
    }

    public IServiceProvider ServiceProvider { get; private set; }

    public Task BeforeTestAsync()
    {
        ServiceProvider = ServiceProvider.CreateScope().ServiceProvider;
        return Task.CompletedTask;
    }

    public Task AfterTestAsync()
    {
        return Task.CompletedTask;
    }

    public Task SetGivenEventsAsync(IDictionary<string, IEnumerable<object>> events)
    {
        var fakeEventDatabase = ServiceProvider.GetRequiredService<IEventDatabase>() as FakeEventDatabase;
        fakeEventDatabase.StubAlreadySavedEvents(events);
        return Task.CompletedTask;
    }

    public Task<IDictionary<string, IEnumerable<object>>> GetPublishedEventsAsync()
    {
        var fakeEventDatabase = ServiceProvider.GetRequiredService<IEventDatabase>() as FakeEventDatabase;
        return Task.FromResult(fakeEventDatabase.GetNewlySavedEvents());
    }

    public async Task DispatchCommandAsync(object command)
    {
        var massTransitMediator = ServiceProvider.GetRequiredService<IMediator>();
        await massTransitMediator.Publish(command);
    }
}
```


It is a good idea to use the DRY principle and write our own wrapper for the Tester class:
```csharp
public class MyCustomTesterWrapper
{
    public async Task TestAsync(Test test) => await Tester.TestAsync(test, new MyAdapter());
}
```
Thanks to this, we can execute our test from above by writing:
```csharp
[Fact]
public async Task given_bread_ingredients_mixed_when_BakeDough_for_25_minutes_then_bread_baked_and_fantastic_smell_produced()
{
    // ...
    await MyCustomTesterWrapper.TestAsync(test);
}
```

## Given
The ``Given`` method can be called multiple times on a single instance of the ``Test`` class.
```csharp
test.Given(new EventA()).Given(new EventB());
```
is equivalent to:
```csharp
test.Given(new EventA(), new EventB());
```
There are a few other options to arranging our tests and declaring what has already happend.

#### Given events
To declare what events have occured, use:
```csharp
.Given(new FirstEvent(), new SecondEvent(), new ThirdEvent() /*, ...*/)
```

#### Given an action on a service
To call any action on any service, use:
```csharp
.Given<IWeatherService>(s => s.ConfigurePressureUnit(PressureUnit.Hectopascal))
```

#### Given an action on a fake service
To call any action on any fake service, use:
```csharp
.Given<IGeoLocationService, FakeGeoLocationService>(s => s.StubLocation(53, Latitude.North, 18, Longitude.East))
```

## When
To specify a command that will be dispatched to your application code, use the ``When`` method.

## Then
To assert, use the ``Then`` method, which has many variations. All of them are described below:
- ``ThenNone()`` &ndash; the test passes if no event was published and no exception was thrown.
- ``ThenAny()`` &ndash; test passes if any events occured or if no events occured. This method only makes sense when it is combined with other ``Then`` methods. For example, if we want to check if ``FirstEventOccured`` and ``LastEventOccured`` occured, but we don't care about any events that may have occured in between, then we can write:
  ```csharp
  .Then(new FirstEventOccured())
  .ThenAny()
  .Then(new LastEventOccured())
  ```
- ``ThenNot(params Func<object, bool>[] excludedEventQualifiers)`` &ndash; the test passes if none of the events that occured match any of the ``excludedEventQualifiers``. For example:
  ```csharp
  .ThenNot(
      e => e is FirstEventOccured { V: 999, },
      e => e is SecondEventOccured { V: "x", })
  ```
- ``Then(object expectedEvent)`` &ndash; the test passes if exactly one event occured and that event is the same as the event specified in the ``Then`` method.
- ``ThenInOrder(params object[] expectedEvents)`` &ndash; the test passes if the same events occured in the specified order.
- ``ThenInAnyOrder(params object[] expectedEvents)`` &ndash; the test passes if the same events occured in any order.
- ``Then<TService>(Func<TService, AssertActionResult> assertAction)`` &ndash; the test passes if the assertion action returns ``true`` or ``AssertActionResult.Successful()``. There is also an async version of this method.
- ``Then<TService, TFakeService>(Func<TFakeService, AssertActionResult> assertAction)`` &ndash; the test passes if the assertion action returns ``true`` or ``AssertActionResult.Successful()``. There is also an async version of this method.
- ``Then(Func<IServiceProvider, AssertActionResult> assertAction)`` &ndash; the test passes if the assertion action returns ``true`` or ``AssertActionResult.Successful()``. There is also an async version of this method.
- ``ThenException(params IExceptionAssertion[] exceptionAssertions)`` &ndash; the test passes if an exception was thrown. There are two built-in implementations of ``IExceptionAssertion`` &ndash; ``ExceptionTypeAssertion`` and ``ExceptionMessageAssertion`` &ndash; but you can write your own implementations.
- ``ThenException<TExpectedException>(string expectedMessage, ExceptionMessageAssertionType matchingType)`` &ndash; the test passes if an exception of the specified type and with the specified message was thrown.
- ``ThenAnyException<TExpectedException>(string expectedMessage, ExceptionMessageAssertionType matchingType)`` &ndash; the test passes if an exception of the specified type or a derived type and with the specified message was thrown.

## Multistream tests

The domain logic you are writing tests for may depend on the state of multiple aggregates. EventOutcomes provides a way to use ``Given`` methods for a specified event stream id:
```csharp
var test = Test.ForMany()
    .Given(firstEventStreamId, new SomeEvent(firstEventStreamId, "some event data"))
    .Given(secondEventStreamId, new SomeEvent(secondEventStreamId, "some event data"))
```
instead of the standard single-stream method:
```csharp
var test = Test.For(eventStreamId)
    .Given(new SomeEvent(eventStreamId, "some event data"))
```

Altough this is generally bad idea to save two streams of events within single operation, EventOutcomes provides a way to write unit tests for such cases. To do this, you should use ``Then`` methods for a specified event stream id:
```csharp
var test = Test.ForMany()
    .Given(firstEventStreamId, new SomeEvent(firstEventStreamId, "some event data"))
    .Given(secondEventStreamId, new SomeEvent(secondEventStreamId, "some event data"))
    .When(new DoSomethingCommand())
    .Then(firstEventStreamId, new SomeOtherEvent(firstEventStreamId, "some other event data"))
    .Then(secondEventStreamId, new SomeOtherEvent(secondEventStreamId, "some other event data"))
```
instead of the standard single-stream method:
```csharp
var test = Test.For(eventStreamId)
    .Given(new SomeEvent(eventStreamId, "some event data"))
    .When(new DoSomethingCommand())
    .Given(new SomeOtherEvent(eventStreamId, "some other event data"))
```
