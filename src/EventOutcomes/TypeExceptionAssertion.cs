using System;

namespace EventOutcomes
{
    public sealed class TypeExceptionAssertion : IExceptionAssertion
    {
        private readonly Type _expectedExceptionType;
        private readonly bool _anyDerived;

        public TypeExceptionAssertion(Type expectedExceptionType, bool anyDerived)
        {
            _expectedExceptionType = expectedExceptionType ?? throw new ArgumentNullException(nameof(expectedExceptionType));
            _anyDerived = anyDerived;
        }

        public void Assert(Exception thrownException)
        {
            var thrownExceptionType = thrownException.GetType();

            if ((!_anyDerived && thrownExceptionType != _expectedExceptionType) || (_anyDerived && !_expectedExceptionType.IsAssignableFrom(thrownExceptionType)))
            {
                throw new AssertException($"Exception of unexpected type was thrown.{Environment.NewLine}Expected: {_expectedExceptionType.Name}.{Environment.NewLine}Actual: {thrownExceptionType.Name}.");
            }
        }
    }
}
