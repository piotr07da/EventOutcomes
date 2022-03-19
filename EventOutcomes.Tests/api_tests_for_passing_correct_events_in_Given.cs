// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_passing_correct_events_in_Given
{
    private readonly Guid _streamId;

    public api_tests_for_passing_correct_events_in_Given()
    {
        _streamId = Guid.NewGuid();
    }

    [Fact]
    public async Task events_passed_to_Given_shall_be_set_on_the_Adapter()
    {
        var expectedGivenEvents = new object[] { new FirstSampleEvent(18), new FirstSampleEvent(-10), new SecondSampleEvent("xyz"), };

        var t = Test.For(_streamId)
            .Given(expectedGivenEvents)
            .When(new FirstCommand())
            .ThenAny();

        IDictionary<string, IEnumerable<object>>? setGivenEvents = null;

        var stubAdapter = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEvents) =>
        {
            setGivenEvents = givenEvents;
            publishEvents(_streamId);
        });

        await Tester.TestAsync(t, stubAdapter);

        Assert.NotNull(setGivenEvents);
        Assert.True(setGivenEvents!.Count == 1);
        Assert.True(setGivenEvents!.ContainsKey(_streamId.ToString()));
        Assert.Collection(
            setGivenEvents![_streamId.ToString()],
            expectedGivenEvents.Select(expectedGivenEvent => new Action<object>(setGivenEvent => Assert.Same(expectedGivenEvent, setGivenEvent))).ToArray());
    }
}
