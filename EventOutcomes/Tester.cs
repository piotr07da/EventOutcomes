using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public class Tester
    {
        private readonly IAdapter _adapter;

        public Tester(IAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public static async Task TestAsync(Guid eventStreamId, Func<Test, Test> testSetup, IAdapter adapter) => await TestAsync(eventStreamId.ToString(), testSetup, adapter);

        public static async Task TestAsync(string eventStreamId, Func<Test, Test> testSetup, IAdapter adapter)
        {
            var tester = new Tester(adapter);
            var gwt = testSetup(Test.For(eventStreamId));
            await tester.InternalTestAsync(gwt);
        }

        public static async Task TestAsync(Func<Test, Test> testSetup, IAdapter adapter)
        {
            var tester = new Tester(adapter);
            var gwt = testSetup(new Test());
            await tester.InternalTestAsync(gwt);
        }

        public static async Task TestAsync(Test test, IAdapter adapter)
        {
            var tester = new Tester(adapter);
            await tester.InternalTestAsync(test);
        }

        private async Task InternalTestAsync(Test test)
        {
            await _adapter.BeforeTestAsync();

            // GIVEN

            // TODO

            // WHEN

            // TODO - exceptions expectations

            foreach (var command in test.ActCommands)
            {
                await _adapter.DispatchCommandAsync(command);
            }

            // THEN

            var streamsWithPublishedEvents = await _adapter.GetPublishedEventsAsync();
            AssertEventStreamsAssertions(test.AssertEventAssertionsChains, streamsWithPublishedEvents);
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

                AssertionExecutor.Execute(publishedEvents, assertionChain);
            }
        }
    }
}
