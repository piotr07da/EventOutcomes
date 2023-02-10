using System;
using System.Linq;

namespace EventOutcomes
{
    internal sealed class NegativeEventAssertion
    {
        public NegativeEventAssertion(Func<object, bool>[] excludedEventQualifiers)
        {
            ExcludedEventQualifiers = excludedEventQualifiers;
        }

        public Func<object, bool>[] ExcludedEventQualifiers { get; }

        public bool Assert(object[] publishedEvents, out int failAtIndex)
        {
            for (var peIx = 0; peIx < publishedEvents.Length; ++peIx)
            {
                var e = publishedEvents[peIx];

                if (ExcludedEventQualifiers.Any(eeq => eeq(e)))
                {
                    failAtIndex = peIx;
                    return false;
                }
            }

            failAtIndex = -1;
            return true;
        }
    }
}
