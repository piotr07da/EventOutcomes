using System;
using System.Linq;

namespace EventOutcomes
{
    public class NegativeEventAssertion
    {
        public NegativeEventAssertion(Func<object, bool>[] excludedEventQualifiers)
        {
            ExcludedEventQualifiers = excludedEventQualifiers;
        }

        public Func<object, bool>[] ExcludedEventQualifiers { get; }

        public static NegativeEventAssertion Empty => new NegativeEventAssertion(Array.Empty<Func<object, bool>>());

        public bool Assert(object[] publishedEvents)
        {
            foreach (var e in publishedEvents)
            {
                if (ExcludedEventQualifiers.Any(eeq => eeq(e)))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "NegativeCheck";
        }
    }
}
