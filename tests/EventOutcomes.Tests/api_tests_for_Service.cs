// ReSharper disable InconsistentNaming

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_Service
{
    private readonly Guid _streamId = Guid.NewGuid();

    [Fact]
    public async Task given_fake_service_having_expected_operation_done_on_that_service_when_Test_for_Service_assertion_then_assertion_succeeded()
    {
        var having = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEventsAction) => serviceProvider.GetRequiredService<IFirstSampleService>().SetValue(12345));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .Then<IFirstSampleService, FakeTransientFirstSampleService>(f => f.Value == 12345);

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task given_fake_AsyncLocal_based_service_having_expected_operation_done_on_that_service_when_Test_for_Service_assertion_then_assertion_succeeded()
    {
        var having = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEventsAction) => serviceProvider.GetRequiredService<ISecondSampleService>().SetValue(12345));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .Then<ISecondSampleService, FakeAsyncLocalSecondSampleService>(f => f.GetValue() == 12345);

        await Tester.TestAsync(t, having);
    }

    [Fact]
    public async Task given_fake_service_having_NOT_expected_operation_done_on_that_service_when_Test_for_Service_assertion_then_assertion_failed()
    {
        var having = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEventsAction) => serviceProvider.GetRequiredService<IFirstSampleService>().SetValue(-999));

        var t = Test.For(_streamId)
            .Given()
            .When(new FirstCommand())
            .Then<IFirstSampleService, FakeTransientFirstSampleService>(f => f.Value == 12345);

        await Assert.ThrowsAsync<AssertException>(async () =>
        {
            await Tester.TestAsync(t, having);
        });
    }
}
