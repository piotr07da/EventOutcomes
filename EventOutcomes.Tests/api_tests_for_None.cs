// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests
{
    public class api_tests_for_None
    {
        private readonly Guid _streamId;

        public api_tests_for_None()
        {
            _streamId = Guid.NewGuid();
        }

        [Fact]
        public async Task having_NO_events_published_when_Test_for_None_assertion_then_NO_exception_thrown()
        {
            var having = EventOutcomesTesterAdapter.Stub(_streamId);

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenNone();

            await Tester.TestAsync(t, having);
        }

        [Fact]
        public async Task having_event_published_when_Test_for_None_assertion_then_exception_thrown()
        {
            var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(1));

            await Assert.ThrowsAsync<AssertException>(async () =>
            {
                var t = Test.For(_streamId)
                    .Given()
                    .When(new FirstCommand())
                    .ThenNone();

                await Tester.TestAsync(t, having);
            });
        }
    }
}
