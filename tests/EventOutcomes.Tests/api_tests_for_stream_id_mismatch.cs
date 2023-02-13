using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_stream_id_mismatch
{
    [Fact]
    public async Task having_events_published_in_one_stream_while_expecting_events_in_different_stream_results_in_exception()
    {
        var publishedStreamId = Guid.NewGuid();
        var having = EventOutcomesTesterAdapter.Stub(publishedStreamId, new FirstSampleEvent(8));

        var expectedStreamId = Guid.NewGuid();
        var t = Test.ForMany()
            .When(new FirstCommand())
            .Then(expectedStreamId, new FirstSampleEvent(8));

        var exception = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Contains($"No events were published to the stream '{expectedStreamId}'", exception.Message);
        Assert.Contains($"Events were published to the following streams:{Environment.NewLine}- {publishedStreamId}", exception.Message);
    }
}
