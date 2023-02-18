using System.Reflection;
using System.Text;

namespace EventOutcomes;

public sealed class Tester
{
    private readonly Test _test;
    private readonly IAdapter _adapter;

    private Exception? _thrownException;

    private Tester(Test test, IAdapter adapter)
    {
        _test = test ?? throw new ArgumentNullException(nameof(test));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public static async Task TestAsync(EventStreamId eventStreamId, Func<Test, Test> testSetup, IAdapter adapter)
    {
        var test = testSetup(Test.For(eventStreamId));
        await TestAsync(test, adapter);
    }

    public static async Task TestAsync(Func<Test, Test> testSetup, IAdapter adapter)
    {
        var test = testSetup(Test.ForMany());
        await TestAsync(test, adapter);
    }

    public static async Task TestAsync(Test test, IAdapter adapter)
    {
        var tester = new Tester(test, adapter);
        await tester.InternalTestAsync();
    }

    private async Task InternalTestAsync()
    {
        await _adapter.BeforeTestAsync();

        try
        {
            await ArrangeAsync();
            await ActAsync();
            await AssertAsync();
        }
        finally
        {
            await _adapter.AfterTestAsync();
        }
    }

    private async Task ArrangeAsync()
    {
        foreach (var arrangeAction in _test.ArrangeActions)
        {
            arrangeAction(_adapter.ServiceProvider);
        }

        await _adapter.SetGivenEventsAsync(_test.ArrangeEvents);
        await Task.CompletedTask;
    }

    private async Task ActAsync()
    {
        try
        {
            foreach (var command in _test.ActCommands)
            {
                await _adapter.DispatchCommandAsync(command);
            }
        }
        catch (TargetInvocationException tiEx)
        {
            _thrownException = tiEx.InnerException;
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    private async Task AssertAsync()
    {
        var exceptionAssertions = _test.AssertExceptionAssertions;
        if (exceptionAssertions.Count == 0)
        {
            if (_thrownException != null)
            {
                throw new AssertException(_thrownException.ToString());
            }

            var streamsWithPublishedEvents = await _adapter.GetPublishedEventsAsync();
            AssertEventStreamsAssertions(_test.AssertEventAssertionsChains, streamsWithPublishedEvents);
            await AssertAssertActionsAsync(_adapter.ServiceProvider, _test.AssertActions);
        }
        else
        {
            if (_thrownException == null)
            {
                var streamsWithPublishedEvents = await _adapter.GetPublishedEventsAsync();

                var savedAggregatesChanges = streamsWithPublishedEvents.SelectMany(streamKvp => streamKvp.Value).ToArray();

                var messageBuilder = new StringBuilder("Exception was expected but no exception was thrown.");
                if (savedAggregatesChanges.Length > 0)
                {
                    messageBuilder.Append($" Following events were produced instead: [{string.Join(", ", savedAggregatesChanges.Select(e => e.GetType().Name))}].");
                }

                throw new AssertException(messageBuilder.ToString());
            }

            foreach (var exceptionAssertion in exceptionAssertions)
            {
                exceptionAssertion.Assert(_thrownException);
            }
        }
    }

    private static void AssertEventStreamsAssertions(IDictionary<string, EventMatchCheckersChain> assertionsChainsForStreams, IDictionary<string, IEnumerable<object>> streamsWithPublishedEvents)
    {
        if (assertionsChainsForStreams is null) throw new ArgumentNullException(nameof(assertionsChainsForStreams));
        if (streamsWithPublishedEvents is null) throw new ArgumentNullException(nameof(streamsWithPublishedEvents));

        var executionResults = new List<EventMatchCheckersChainExecutionResult>();

        foreach (var ac in assertionsChainsForStreams)
        {
            var streamId = ac.Key;
            var assertionChain = ac.Value;
            if (!streamsWithPublishedEvents.TryGetValue(streamId, out var publishedEvents))
            {
                publishedEvents = Array.Empty<object>();
            }

            var executionResult = EventMatchCheckersChainExecutor.Execute(streamId, assertionChain, publishedEvents);
            executionResults.Add(executionResult);
        }

        if (executionResults.Any(er => !er.Succeeded))
        {
            var exceptionMessageBuilder = new StringBuilder();
            foreach (var executionResult in executionResults)
            {
                exceptionMessageBuilder.AppendLine("--------------------------------------------------------");
                exceptionMessageBuilder.AppendLine($"RESULT FOR STREAM: {executionResult.StreamId}");
                exceptionMessageBuilder.AppendLine();
                if (executionResult.Succeeded)
                {
                    exceptionMessageBuilder.AppendLine("OK");
                    exceptionMessageBuilder.AppendLine();
                }
                else
                {
                    exceptionMessageBuilder.Append(executionResult.ErrorMessage);
                    exceptionMessageBuilder.AppendLine();
                }
            }

            exceptionMessageBuilder.AppendLine("--------------------------------------------------------");
            if (streamsWithPublishedEvents.Any())
            {
                exceptionMessageBuilder.AppendLine("Events were published to the following streams:");
                foreach (var streamId in streamsWithPublishedEvents.Keys)
                {
                    exceptionMessageBuilder.AppendLine($"- {streamId}");
                }
            }
            else
            {
                exceptionMessageBuilder.AppendLine("No events were published to any stream.");
            }

            exceptionMessageBuilder.AppendLine("--------------------------------------------------------");

            throw new AssertException(exceptionMessageBuilder.ToString());
        }
    }

    private static async Task AssertAssertActionsAsync(IServiceProvider serviceProvider, IEnumerable<Func<IServiceProvider, Task<AssertActionResult>>> assertActions)
    {
        foreach (var assertAction in assertActions)
        {
            var result = await assertAction(serviceProvider);
            if (!result)
            {
                throw new AssertException($"Assert action failed. {result.FailMessage}");
            }
        }
    }
}
