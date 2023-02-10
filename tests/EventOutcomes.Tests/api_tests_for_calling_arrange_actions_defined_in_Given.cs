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
    public async Task arrange_action_passed_to_Given_shall_be_called()
    {
        var t = Test.For(_streamId)
            .Given<ICleverService>(s => s.SetValue(987))
            .When(new FirstCommand())
            .ThenAny();

        var stubAdapter = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEvents) =>
        {
            publishEvents(_streamId);
        });

        await Tester.TestAsync(t, stubAdapter);

        var arrangeActionService = stubAdapter.ServiceProvider.GetRequiredService<ICleverService>() as FakeCleverService;
        Assert.Equal(987, arrangeActionService!.Value);
    }
}
