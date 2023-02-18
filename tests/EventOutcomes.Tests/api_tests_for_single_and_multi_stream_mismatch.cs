// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_single_and_multi_stream_mismatch
{
    [Fact]
    public void given_test_for_multi_stream_when_using_Given_without_eventStreamId_then_exception()
    {
        var exception = Assert.Throws<EventOutcomesException>(() =>
        {
            Test.ForMany().Given(new FirstSampleEvent(0));
        });

        Assert.Contains("If Test class was created using Test.ForMany() then you have to pass eventStreamId argument to the Given(...) method. Alternatively you can create the Test class specifying event stream id using Test.For(eventStreamId).", exception.Message);
    }
}
