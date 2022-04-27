// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_Exceptions_by_type
{
    private readonly Guid _streamId;

    public api_tests_for_Exceptions_by_type()
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
            .ThenException<UnbelievableException>();

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_exception_derived_from_expected_Exception_thrown_when_Test_for_Exception_assertion_then_assertion_failed()
    {
        var having = EventOutcomesTesterAdapter.Stub(new DerivedUnbelievableException("abc"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenException<UnbelievableException>();

        await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });
    }

    [Fact]
    public async Task having_expected_Exception_thrown_when_Test_for_AnyException_assertion_then_assertion_succeeded()
    {
        var having = EventOutcomesTesterAdapter.Stub(new UnbelievableException("abc"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenAnyException<UnbelievableException>();

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_exception_derived_from_expected_Exception_thrown_when_Test_for_AnyException_assertion_then_assertion_succeeded()
    {
        var having = EventOutcomesTesterAdapter.Stub(new DerivedUnbelievableException("abc"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenAnyException<UnbelievableException>();

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task having_NOT_expected_Exception_thrown_when_Test_for_Exception_assertion_then_assertion_failed()
    {
        var having = EventOutcomesTesterAdapter.Stub(new ExceptionalException("abc"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenException<UnbelievableException>();

        await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });
    }

    [Fact]
    public async Task having_NOT_expected_Exception_thrown_when_Test_for_AnyException_assertion_then_assertion_failed()
    {
        var having = EventOutcomesTesterAdapter.Stub(new ExceptionalException("abc"));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .ThenAnyException<UnbelievableException>();

        await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });
    }
}
