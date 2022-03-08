// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests
{
    public class api_tests_for_OutOfOrder
    {
        private readonly Guid _streamId;

        public api_tests_for_OutOfOrder()
        {
            _streamId = Guid.NewGuid();
        }

        [Fact]
        public async Task having_the_same_events_in_the_same_order_when_Test_for_OutOfOrder_assertion_then_NO_exception_thrown()
        {
            var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenOutOfOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

            await Tester.TestAsync(t, having);
        }

        [Fact]
        public async Task having_the_same_events_in_different_order_when_Test_for_OutOfOrder_assertion_then_NO_exception_thrown()
        {
            var having = EventOutcomesTesterAdapter.Stub(_streamId, new SecondSampleEvent("abc123"), new FirstSampleEvent(1), new FirstSampleEvent(999));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenOutOfOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

            await Tester.TestAsync(t, having);
        }

        [Fact]
        public async Task having_events_of_the_same_type_with_the_same_order_but_with_different_data_when_Test_for_OutOfOrder_assertion_then_exception_thrown()
        {
            var having = EventOutcomesTesterAdapter.Stub(_streamId, new FirstSampleEvent(111), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenOutOfOrder(new FirstSampleEvent(1), new FirstSampleEvent(999), new SecondSampleEvent("abc123"));

            await Assert.ThrowsAsync<AssertException>(async () =>
            {
                await Tester.TestAsync(t, having);
            });
        }
    }
}
