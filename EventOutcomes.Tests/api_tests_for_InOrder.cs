// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests
{
    public class api_tests_for_InOrder
    {
        private readonly Guid _streamId;

        public api_tests_for_InOrder()
        {
            _streamId = Guid.NewGuid();
        }

        [Fact]
        public async Task given_the_same_events_in_the_same_order_when_Test_for_InOrder_check_then_NO_exception_thrown()
        {
            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenInOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

            await Tester.TestAsync(t, EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123")));
        }

        [Fact]
        public async Task given_the_same_events_in_different_order_when_Test_for_InOrder_check_then_exception_thrown()
        {
            await Assert.ThrowsAsync<AssertException>(async () =>
            {
                var t = Test.For(Guid.NewGuid())
                    .Given()
                    .When(new FirstCommand())
                    .ThenInOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

                await Tester.TestAsync(t, EventOutcomesTesterAdapter.Stub(_streamId, new SecondSampleEvent("abc123"), new FirstSampleEvent(1), new FirstSampleEvent(999)));
            });
        }

        [Fact]
        public async Task given_events_of_the_same_type_with_the_same_order_but_with_different_data_when_Test_for_InOrder_check_then_exception_thrown()
        {
            await Assert.ThrowsAsync<AssertException>(async () =>
            {
                var t = Test.For(Guid.NewGuid())
                    .Given()
                    .When(new FirstCommand())
                    .ThenInOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

                await Tester.TestAsync(t, EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(111), new FirstSampleEvent(999), new SecondSampleEvent("abc123")));
            });
        }
    }
}
