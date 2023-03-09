// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_combined_events_assertions
{
    private readonly Guid _streamId;

    public api_tests_for_combined_events_assertions()
    {
        _streamId = Guid.NewGuid();
    }

    [Fact]
    public async Task having_events_published_when_Test_for_Not_and_InOrder_assertions_then_no_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(8), new SecondSampleEvent("x"), new FirstSampleEvent(88), new SecondSampleEvent("xx"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenNot(
                e => e is FirstSampleEvent { V: 88, },
                e => e is SecondSampleEvent { V: "xx", })
            .ThenInOrder(new FirstSampleEvent(88), new SecondSampleEvent("xx"));


        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_events_published_when_Test_for_InOrder_and_Not_assertions_then_no_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new SecondSampleEvent("xx"), new FirstSampleEvent(8), new SecondSampleEvent("x"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(88), new SecondSampleEvent("xx"))
            .ThenNot(
                e => e is FirstSampleEvent { V: 88, },
                e => e is SecondSampleEvent { V: "xx", });


        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_events_published_when_Test_for_InOrder_and_InAnyOrder_assertions_then_no_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new SecondSampleEvent("xx"), new FirstSampleEvent(8), new SecondSampleEvent("x"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(88), new SecondSampleEvent("xx"))
            .ThenInAnyOrder(new FirstSampleEvent(8), new SecondSampleEvent("x"));

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_events_published_but_first_assertion_fails_when_Test_for_Not_and_InOrder_assertions_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(8), new SecondSampleEvent("x"), new FirstSampleEvent(88), new SecondSampleEvent("xx"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenNot(
                e => e is FirstSampleEvent { V: 8, },
                e => e is SecondSampleEvent { V: "xx", })
            .ThenInOrder(new FirstSampleEvent(88), new SecondSampleEvent("xx"));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.StartsWith($@"
--------------------------------------------------------
RESULT FOR STREAM: {_streamId}

Expected not to find any event matching 2 specified rules.

Unexpected published event found at [0].
Published events:", assertException.Message);
    }

    [Fact]
    public async Task having_events_published_but_second_assertion_fails_when_Test_for_Not_and_InOrder_assertions_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(8), new SecondSampleEvent("x"), new FirstSampleEvent(88), new SecondSampleEvent("xx"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenNot(
                e => e is FirstSampleEvent { V: 88, },
                e => e is SecondSampleEvent { V: "xx", })
            .ThenInOrder(new FirstSampleEvent(88), new SecondSampleEvent("xxXX"));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.StartsWith($@"
--------------------------------------------------------
RESULT FOR STREAM: {_streamId}

Expected following events in specified order:
0. [EventOutcomes.Tests.FirstSampleEvent]
{{""V"":88}}
1. [EventOutcomes.Tests.SecondSampleEvent]
{{""V"":""xxXX""}}

Expected series of events not found.
Published events:", assertException.Message);
    }

    [Fact]
    public async Task having_events_published_but_first_assertion_fails_when_Test_for_InOrder_and_Not_assertions_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new SecondSampleEvent("xx"), new FirstSampleEvent(8), new SecondSampleEvent("x"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(88), new SecondSampleEvent("xxXX"))
            .ThenNot(
                e => e is FirstSampleEvent { V: 88, },
                e => e is SecondSampleEvent { V: "xx", });


        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.StartsWith($@"
--------------------------------------------------------
RESULT FOR STREAM: {_streamId}

Expected following events in specified order:
0. [EventOutcomes.Tests.FirstSampleEvent]
{{""V"":88}}
1. [EventOutcomes.Tests.SecondSampleEvent]
{{""V"":""xxXX""}}

Unexpected published event found at [1].
Published events:", assertException.Message);
    }

    [Fact]
    public async Task having_events_published_but_second_assertion_fails_when_Test_for_InOrder_and_Not_assertions_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new SecondSampleEvent("xx"), new FirstSampleEvent(8), new SecondSampleEvent("x"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenInOrder(new FirstSampleEvent(88), new SecondSampleEvent("xx"))
            .ThenNot(
                e => e is FirstSampleEvent { V: 88, },
                e => e is SecondSampleEvent { V: "x", });


        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.StartsWith($@"
--------------------------------------------------------
RESULT FOR STREAM: {_streamId}

Expected not to find any event matching 2 specified rules.

Unexpected published event found at [3].
Published events:", assertException.Message);
    }

    [Fact]
    public async Task having_events_published_when_Test_for_Then_and_ThenInOrder_assertions_then_test_succeeded()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new FirstSampleEvent(222), new FirstSampleEvent(333));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .Then(new FirstSampleEvent(88))
            .ThenInOrder(new FirstSampleEvent(222), new FirstSampleEvent(333));

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_events_published_but_last_expected_event_does_not_exist_when_Test_for_Then_and_ThenInOrder_assertions_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new FirstSampleEvent(222));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .Then(new FirstSampleEvent(88))
            .ThenInOrder(new FirstSampleEvent(222), new FirstSampleEvent(333));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Contains(@"
Expected event [1] not found.
", assertException.Message);
    }

    [Fact]
    public async Task having_events_published_when_Test_for_Then_and_ThenInAnyOrder_assertions_then_test_succeeded()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new FirstSampleEvent(222), new FirstSampleEvent(333));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .Then(new FirstSampleEvent(88))
            .ThenInAnyOrder(new FirstSampleEvent(333), new FirstSampleEvent(222));

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_events_published_but_last_expected_event_does_not_exist_when_Test_for_Then_and_ThenInAnyOrder_assertions_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(88), new FirstSampleEvent(222));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .Then(new FirstSampleEvent(88))
            .ThenInAnyOrder(new FirstSampleEvent(222), new FirstSampleEvent(333));

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Contains(@"
Expected event [1] not found.
", assertException.Message);
    }
}
