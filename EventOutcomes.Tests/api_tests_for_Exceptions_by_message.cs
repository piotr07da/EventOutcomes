// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests
{
    public class api_tests_for_Exceptions_by_message
    {
        private readonly Guid _streamId;

        public api_tests_for_Exceptions_by_message()
        {
            _streamId = Guid.NewGuid();
        }

        [Fact]
        public async Task having_Exception_with_equal_message_thrown_when_Test_for_Exception_with_Equal_message_assertion_then_assertion_succeeded()
        {
            var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc def ghi"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenException("abc def ghi", ExceptionMessageAssertionType.Equals);

            await Tester.TestAsync(t, having);
        }

        [Fact]
        public async Task having_Exception_with_message_containing_expected_string_thrown_when_Test_for_Exception_with_Contains_message_assertion_then_assertion_succeeded()
        {
            var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc def ghi"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenException("bc de", ExceptionMessageAssertionType.Contains);

            await Tester.TestAsync(t, having);
        }

        [Fact]
        public async Task having_Exception_with_message_matching_expected_regex_thrown_when_Test_for_Exception_with_MatchesRegex_message_assertion_then_assertion_succeeded()
        {
            var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc def ghi"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenException("[a-c]{3}\\s[d-f]{3}\\s[g-i]{3}", ExceptionMessageAssertionType.MatchesRegex);

            await Tester.TestAsync(t, having);
        }

        [Fact]
        public async Task having_Exception_with_NOT_equal_message_thrown_when_Test_for_Exception_with_Equal_message_assertion_then_assertion_failed()
        {
            var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc def ghi"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenException("abc def ghi jkl", ExceptionMessageAssertionType.Equals);

            await Assert.ThrowsAsync<AssertException>(async () =>
            {
                await Tester.TestAsync(t, having);
            });
        }

        [Fact]
        public async Task having_Exception_with_message_NOT_containing_expected_string_thrown_when_Test_for_Exception_with_Contains_message_assertion_then_assertion_failed()
        {
            var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc def ghi"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenException("bc ee", ExceptionMessageAssertionType.Contains);

            await Assert.ThrowsAsync<AssertException>(async () =>
            {
                await Tester.TestAsync(t, having);
            });
        }

        [Fact]
        public async Task having_Exception_with_message_NOT_matching_expected_regex_thrown_when_Test_for_Exception_with_MatchesRegex_message_assertion_then_assertion_failed()
        {
            var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc def ghi"));

            var t = Test.For(_streamId)
                .Given()
                .When(new FirstCommand())
                .ThenException("[a-c]{4}\\s[d-f]{3}\\s[g-i]{3}", ExceptionMessageAssertionType.MatchesRegex);

            await Assert.ThrowsAsync<AssertException>(async () =>
            {
                await Tester.TestAsync(t, having);
            });
        }
    }
}
