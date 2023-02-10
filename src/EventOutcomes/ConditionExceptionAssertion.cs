using System;

namespace EventOutcomes
{
    public sealed class ConditionExceptionAssertion : IExceptionAssertion
    {
        private readonly Func<Exception, bool> _expectedExceptionCondition;

        public ConditionExceptionAssertion(Func<Exception, bool> expectedExceptionCondition)
        {
            _expectedExceptionCondition = expectedExceptionCondition;
        }

        public void Assert(Exception thrownException)
        {
            if (!_expectedExceptionCondition(thrownException))
            {
                throw new AssertException("Unexpected exception was thrown. Thrown exception did not match specified condition.");
            }
        }
    }
}
