// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_Not
{
    private readonly Guid _streamId;

    public api_tests_for_Not()
    {
        _streamId = Guid.NewGuid();
    }

    [Fact]
    public async Task having_events_published_but_none_of_them_qualifies_as_excluded_when_Test_for_Not_assertion_then_NO_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(8), new SecondSampleEvent("x"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenNot(
                e => e is FirstSampleEvent { V: 999, },
                e => e is SecondSampleEvent { V: "abc", });

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_events_published_but_one_of_them_qualifies_as_excluded_when_Test_for_Not_assertion_then_exception_thrown()
    {
        var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(8), new SecondSampleEvent("x"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenNot(
                e => e is FirstSampleEvent { V: 999, },
                e => e is SecondSampleEvent { V: "x", });

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Equal($@"
--------------------------------------------------------
RESULT FOR STREAM: {_streamId}

Expected not to find any event matching 2 specified rules.

Unexpected published event found at [1].
Published events are:
0. [EventOutcomes.Tests.FirstSampleEvent]
{{""V"":8}}
1. [EventOutcomes.Tests.SecondSampleEvent]
{{""V"":""x""}}

--------------------------------------------------------
Events were published to the following streams:
- {_streamId}
--------------------------------------------------------
", assertException.Message);
    }
}
