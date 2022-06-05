using System;
using System.Collections.Generic;
using System.Linq;

namespace EventOutcomes
{
    public sealed class EventAssertionsChain
    {
        private readonly List<object> _assertions = new List<object>();

        public bool IsNone { get; private set; }

        public IReadOnlyList<object> Assertions => _assertions;

        public void AddNoneAssertion()
        {
            if (_assertions.Count > 0)
            {
                throw new Exception("Cannot set None if there are other assertions defined.");
            }

            IsNone = true;
        }

        public void AddPositiveAssertion(PositiveEventAssertion assertion)
        {
            Add(assertion);
        }

        public void AddNegativeAssertion(NegativeEventAssertion assertion)
        {
            Add(assertion);
        }

        private void Add(object assertion)
        {
            if (IsNone)
            {
                throw new Exception("Cannot add any assertion if None assertion is set.");
            }

            if (_assertions.Count > 0)
            {
                var lastCheck = _assertions.Last();

                if (lastCheck is NegativeEventAssertion && assertion is NegativeEventAssertion)
                {
                    throw new Exception("Cannot add two consecutive negative event assertions (ThenAny, ThenNot).");
                }
            }

            _assertions.Add(assertion);
        }
    }
}
