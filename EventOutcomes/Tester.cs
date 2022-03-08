using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public class Tester
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
            var test = testSetup(new Test());
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
            }
            else
            {
                if (_thrownException == null)
                {
                    var streamsWithPublishedEvents = await _adapter.GetPublishedEventsAsync();

                    var savedAggregatesChanges = streamsWithPublishedEvents.SelectMany(streamKvp => streamKvp.Value);

                    throw new AssertException($"At least one exception assertion has been defined but no exception has been thrown. Following events were produced instead: [{string.Join(", ", savedAggregatesChanges.Select(e => e.GetType().Name))}].");
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

            if (assertionsChainsForStreams.Count != streamsWithPublishedEvents.Count)
            {
                throw new AssertException($"Number of assertions chains is different than number of streams with published events. Expected number of streams: {assertionsChainsForStreams.Count}. Actual number of streams with published events: {streamsWithPublishedEvents.Count}.");
            }

            foreach (var ac in assertionsChainsForStreams)
            {
                var streamName = ac.Key;
                var assertionChain = ac.Value;
                if (!streamsWithPublishedEvents.TryGetValue(streamName, out var publishedEvents))
                {
                    throw new AssertException($"Expected stream of events not found / not published. Stream name is {streamName}.");
                }

                EventAssertionsChainExecutor.Execute(assertionChain, publishedEvents);
            }
        }
    }
}
