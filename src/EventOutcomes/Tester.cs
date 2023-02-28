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
            // The following two methods are not called after ArrangeAsync because when one async method is called after another, they do not belong to the same asynchronous flow.
            // Therefore if someone uses AsyncLocal in the fake or real implementations of their services, the values initialized in Given<IService, FakeService>() will be lost.
            // For that reason ActAsync() asn AssertAsync() methods are called internally inside the ArrangeAsync() method to maintain the same asynchronous flow.
            // This ensures that when ArrangeAsync() is called, ActAsync() and AssertAsync() are also called in the same asynchronous flow,
            // preventing the loss of values set in Given<IService, FakeService>().
            // The other solution for that would be to extract the code from all three methods and place that code here directly one after another.

            await ArrangeAsync(async () =>
            {
                await ActAsync(async () =>
                {
                    await AssertAsync();
                });
            });
        }
        finally
        {
            await _adapter.AfterTestAsync();
        }
    }

    private async Task ArrangeAsync(Func<Task> internalContinuation)
    {
        foreach (var arrangeAction in _test.ArrangeActions)
        {
            arrangeAction(_adapter.ServiceProvider);
        }

        await _adapter.SetGivenEventsAsync(_test.ArrangeEvents);

        await internalContinuation();
    }

    private async Task ActAsync(Func<Task> internalContinuation)
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

        await internalContinuation();
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
