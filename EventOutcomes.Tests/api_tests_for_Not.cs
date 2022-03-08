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
    public async Task having_events_published_but_non_of_them_qualifies_as_excluded_when_Test_for_Not_assertion_then_NO_exception_thrown()
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

        await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });
    }
}
