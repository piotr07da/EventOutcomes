// ReSharper disable InconsistentNaming

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_calling_arrange_actions_defined_in_Given
{
    private readonly Guid _streamId;

    public api_tests_for_calling_arrange_actions_defined_in_Given()
    {
        _streamId = Guid.NewGuid();
    }

    [Fact]
    public async Task arrange_action_on_scoped_service_passed_to_Given_shall_be_called()
    {
        var t = Test.For(_streamId)
            .Given<IFirstSampleService>(s => s.SetValue(987))
            .When(new FirstCommand())
            .ThenAny();

        var stubAdapter = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEvents) =>
        {
            publishEvents(_streamId);
        });

        await Tester.TestAsync(t, stubAdapter);

        var arrangeActionService = stubAdapter.ServiceProvider.GetRequiredService<IFirstSampleService>() as FakeTransientFirstSampleService;
        Assert.Equal(987, arrangeActionService!.Value);
    }

    [Fact]
    public async Task arrange_action_on_singleton_AsyncLocal_service_passed_to_Given_shall_be_available_from_assertion_method()
    {
        var t = Test.For(_streamId)
            .Given<ISecondSampleService>(s => s.SetValue(987))
            .When(new FirstCommand())
            .Then<ISecondSampleService, FakeAsyncLocalSecondSampleService>(s => s.GetValue() == 987);

        var stubAdapter = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEvents) =>
        {
            publishEvents(_streamId);
        });

        await Tester.TestAsync(t, stubAdapter);
    }
}
