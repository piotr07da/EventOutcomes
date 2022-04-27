// ReSharper disable InconsistentNaming

using Xunit;

namespace EventOutcomes.Tests;

public class api_tests_for_passing_correct_command_defined_in_When
{
    private readonly Guid _streamId;

    public api_tests_for_passing_correct_command_defined_in_When()
    {
        _streamId = Guid.NewGuid();
    }

    [Fact]
    public async Task command_passed_to_When_shall_be_dispatched_on_the_Adapter()
    {
        var whenCommand = new FirstCommand(123);

        var t = Test.For(_streamId)
            .Given()
            .When(whenCommand)
            .ThenAny();

        object? dispatchedCommand = null;

        var stubAdapter = EventOutcomesTesterAdapter.Stub((serviceProvider, givenEvents, command, publishEvents) =>
        {
            dispatchedCommand = command;
            publishEvents(_streamId);
        });

        await Tester.TestAsync(t, stubAdapter);

        Assert.Same(whenCommand, dispatchedCommand);
    }
}
