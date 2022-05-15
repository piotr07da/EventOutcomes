# EventOutcomes

EventOutcomes is a free, open-source .NET library created to make writing unit tests for event sourced applications easier.
It is based on the idea presented by Greg Young:
> GIVEN events
> WHEN command
> THEN events

and therefore full credit for that main idea  goes to Greg Young.

EventOutcomes is MIT licensed.

## Features

- GIVEN events, WHEN command, THEN events
- Additional exception assertions
- Additional arrangements and assertions on real and fake services
- Support for dependency injection
- Independent of Event Sourcing and CQRS frameworks selection

## Using EventOutcomes

### Basic test

Lets start from creating the simplest possible unit test for event sourced application. Every test we will write later will be based on the following structure.
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

As you can see above, in the last line there is a class ``MyCustomAdapter`` which is a custom implementation of EventOutcomes ``IAdapeter`` interface. Creating such implementation in your project is the only thing you need to do before you will be able to start writing unit tests using EventOutcomes.

### IAdapter

There are many ways to implement Event Sourcing and CQRS -- you can use one of the existing frameworks or you can make your own implementation. EventOutcomes is designed to be independent of a frameworks selection. Because of that there is one thing required until you will be able to use EventOutcomes. You have to implement `IAdapter` interface. You can think of it as a common denominator for all the frameworks. Below is the explanation of what has to be implemented.
- ``IServiceProvider ServiceProvider { get; }`` -- Service provider for all the services you need to inject in your application code.
- ``Task BeforeTestAsync();`` -- Method called before the test is executed. If scoped services are required then this is a perfect place to create a scope and assign scoped service provider to the ``ServiceProvider`` property.
- ``Task AfterTestAsync();`` -- Method called after the test is completed. Any cleanup code can goes here.
- ``Task SetGivenEventsAsync(IDictionary<string, IEnumerable<object>> events);`` -- Method that saves the GIVEN events (events that already occurred) to the place from which your Event Sourcing framework will read them in order to rehydrate the domain objects (e.g. aggregates in DDD). It can be a fake in memory implementation of some ``IEventDatabase`` interface or something similiar used by your framework.
- ``Task<IDictionary<string, IEnumerable<object>>> GetPublishedEventsAsync();`` - In each event sourcing framework there is some component responsible for saving new published events. Implement this method so EventOutcomes can retrieve those newly published events.
- ``Task DispatchCommandAsync(object command);`` - Put your command dispatching code here. For example ``await _commandDispatcher.Dispatch(command);`` or ``await massTransitMediator.Publish(command);``.

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
        fakeEventDatabase.FakeAlreadySavedEvents(events);
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


It is good idea to use DRY principle and write our own wrapper for Tester class:
```csharp
public class MyCustomTesterWrapper
{
    public async Task TestAsync(Test test) => await Tester.TestAsync(test, new MyAdapter());
}
```
Thanks to that we can execute our test from above writing:
```csharp
[Fact]
public async Task given_bread_ingredients_mixed_when_BakeDough_for_25_minutes_then_bread_baked_and_fantastic_smell_produced()
{
    // ...
    await MyCustomTesterWrapper.TestAsync(test);
}
```

## Given
Given method can be called multiple times on single instance of the ``Test`` class.
```csharp
test.Given(new EventA()).Given(new EventB());
```
is equivalent to:
```csharp
test.Given(new EventA(), new EventB());
```
There are few options to arrange our tests and declare what already happend.

#### Given events
To declare what events occured use:
```csharp
.Given(new FirstEvent(), new SecondEvent(), new ThirdEvent() /*, ...*/)
```

#### Given an action on a service
To call any action on any service use:
```csharp
.Given<IWeatherService>(s => s.ConfigurePressureUnit(PressureUnit.Hectopascal))
```

#### Given an action on a fake service
To call any action on any fake service use:
```csharp
.Given<IGeoLocationService, FakeGeoLocationService>(s => s.StubLocation(53, Latitude.North, 18, Longitude.East))
```

## When
To specify command that will be dispatched to your application code use ``When`` method.

## Then
To assert use ``Then`` method which has many variants. All of them are described below.
- ``ThenNone()`` -- test passes if no event and no exception has been thrown.
- ``ThenAny()`` -- test passes if any or no events occured. This method only makes sense if it is combined with other ``Then`` methods. For example if we want to check if FirstEventOccured and LastEventOccured but we don't care what if any events occured in between the we can write:
  ```csharp
  .Then(new FirstEventOccured())
  .ThenAny()
  .Then(new LastEventOccured())
  ```
- ``ThenNot(params Func<object, bool>[] excludedEventQualifiers)`` -- test passes if none of the events that occured matches any of ``excludedEventQualifiers``. For example
  ```csharp
  .ThenNot(
      e => e is FirstEventOccured { V: 999, },
      e => e is SecondEventOccured { V: "x", })
  ```
- ``Then(object expectedEvent)`` -- test passes if exactly one event occured and that event is the same as the event specified in ``Then`` method.
- ``ThenInOrder(params object[] expectedEvents)`` -- test passes if the same events occured in specified order.
- ``ThenInAnyOrder(params object[] expectedEvents)`` -- test passes if the same events occured in any order.
- ``Then<TService>(Func<TService, AssertActionResult> assertAction)`` -- test passes if assertion action returns ``true`` or ``AssertActionResult.Successful()``. There is also async version of this method.
- ``Test Then<TService, TFakeService>(Func<TFakeService, AssertActionResult> assertAction)`` -- test passes if assertion action returns ``true`` or ``AssertActionResult.Successful()``. There is also async version of this method.
- ``Test Then(Func<IServiceProvider, AssertActionResult> assertAction)`` -- test passes if assertion action returns ``true`` or ``AssertActionResult.Successful()``. There is also async version of this method.

## Multistream tests

Altough this is generally bad idea to save two streams of events within single operation, EventOutcomes provides way to write unit tests for such cases. To do this you should write:
```csharp
var test = new Test()
    .Given(firstEventStreamId, new SomeEvent(firstEventStreamId, "some event data"))
    .Given(secondEventStreamId, new SomeEvent(secondEventStreamId, "some event data"))
    // ...
```
instead of standard single-stream way:
```csharp
var test = Test.For(eventStreamId)
    .Given(new SomeEvent(eventStreamId, "some event data"))
    // ...
```
