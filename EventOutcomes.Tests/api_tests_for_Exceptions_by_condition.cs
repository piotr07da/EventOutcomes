// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_Exceptions_by_condition
{
    private readonly Guid _streamId;

    public api_tests_for_Exceptions_by_condition()
    {
        _streamId = Guid.NewGuid();
    }

    [Fact]
    public async Task having_expected_Exception_thrown_when_Test_for_Exception_assertion_then_assertion_succeeded()
    {
        var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenException(e => e is UnbelievableException { Message: "abc", });

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_unexpected_exception_when_Test_for_Exception_assertion_then_assertion_failed()
    {
        var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenException(e => e is UnbelievableException { Message: "abc def", });

        var assertException = await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });

        Assert.Equal(@"
Unexpected exception was thrown. Thrown exception did not match specified condition.", assertException.Message);
    }
}
