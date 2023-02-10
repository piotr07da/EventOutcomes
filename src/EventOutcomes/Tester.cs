using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public sealed class Tester
    {
        private readonly Test _test;
        private readonly IAdapter _adapter;

        private Exception _thrownException;

        private Tester(Test test, IAdapter adapter)
        {
            _test = test ?? throw new ArgumentNullException(nameof(test));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public static async Task TestAsync(Guid eventStreamId, Func<Test, Test> testSetup, IAdapter adapter) => await TestAsync(eventStreamId.ToString(), testSetup, adapter);

        public static async Task TestAsync(string eventStreamId, Func<Test, Test> testSetup, IAdapter adapter)
        {
            var test = testSetup(Test.For(eventStreamId));
            var tester = new Tester(test, adapter);
            await tester.InternalTestAsync();
        }

        public static async Task TestAsync(Func<Test, Test> testSetup, IAdapter adapter)
        {
            var test = testSetup(Test.ForMany());
            var tester = new Tester(test, adapter);

            await tester.InternalTestAsync();
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

        private static void AssertEventStreamsAssertions(IDictionary<string, EventAssertionsChain> assertionsChainsForStreams, IDictionary<string, IEnumerable<object>> streamsWithPublishedEvents)
        {
            if (assertionsChainsForStreams is null) throw new ArgumentNullException(nameof(assertionsChainsForStreams));
            if (streamsWithPublishedEvents is null) throw new ArgumentNullException(nameof(streamsWithPublishedEvents));

            foreach (var ac in assertionsChainsForStreams)
            {
                var streamName = ac.Key;
                var assertionChain = ac.Value;
                if (!streamsWithPublishedEvents.TryGetValue(streamName, out var publishedEvents))
                {
                    publishedEvents = Array.Empty<object>();
                }

                EventAssertionsChainExecutor.Execute(assertionChain, publishedEvents);
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
}
