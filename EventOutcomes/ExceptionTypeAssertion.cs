using System;

namespace EventOutcomes
{
    public class ExceptionTypeAssertion : IExceptionAssertion
    {
        private readonly Type _expectedExceptionType;
        private readonly bool _anyDerived;

        public ExceptionTypeAssertion(Type expectedExceptionType, bool anyDerived)
        {
            _expectedExceptionType = expectedExceptionType ?? throw new ArgumentNullException(nameof(expectedExceptionType));
            _anyDerived = anyDerived;
        }

        public void Assert(Exception thrownException)
        {
            var thrownExceptionType = thrownException.GetType();

            if ((!_anyDerived && thrownExceptionType != _expectedExceptionType) || (_anyDerived && !_expectedExceptionType.IsAssignableFrom(thrownExceptionType)))
            {
                throw new AssertException($"Exception assertion failed. Expected exception type was {_expectedExceptionType.Name} but type of thrown exception was {thrownExceptionType.Name}.");
            }
        }
    }
}
