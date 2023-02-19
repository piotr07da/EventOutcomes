// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_InOrder
{
    private readonly Guid _streamId;

    public api_tests_for_InOrder()
    {
        _streamId = Guid.NewGuid();
    }

    [Fact]
    public async Task having_the_same_events_in_the_same_order_when_Test_for_InOrder_assertion_then_NO_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_no_events_published_when_Test_for_InOrder_assertion_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, Array.Empty<object>());

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Equal($@"
--------------------------------------------------------
RESULT FOR STREAM: {_streamId}

Expected following events in specified order:
0. [EventOutcomes.Tests.FirstSampleEvent]
{{""V"":1}}
1. [EventOutcomes.Tests.FirstSampleEvent]
{{""V"":999}}
2. [EventOutcomes.Tests.SecondSampleEvent]
{{""V"":""abc123""}}

No events were published to the stream '{_streamId}'.

--------------------------------------------------------
Events were published to the following streams:
- {_streamId}
--------------------------------------------------------
", assertException.Message);
    }

    [Fact]
    public async Task having_the_same_events_in_different_order_when_Test_for_InOrder_assertion_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new SecondSampleEvent("abc123"), new FirstSampleEvent(1), new FirstSampleEvent(999));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Contains(@"
Unexpected published event found at [0].
", assertException.Message);
    }


    [Fact]
    public async Task having_events_of_the_same_type_with_the_same_order_but_with_different_data_when_Test_for_InOrder_assertion_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(1), new FirstSampleEvent(888), new SecondSampleEvent("abc123"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Contains(@"
Unexpected published event found at [1].
", assertException.Message);
    }

    [Fact]
    public async Task having_only_first_expected_event_published_when_Test_for_InOrder_assertion_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(111));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(111), new SecondSampleEvent("abc123"));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Contains(@"
Expected event [1] not found.
", assertException.Message);
    }
}
