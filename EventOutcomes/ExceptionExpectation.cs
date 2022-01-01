using System;

namespace EventOutcomes
{
    public class ExceptionExpectation
    {
        public ExceptionExpectation(Type exceptionType, object exceptionAssertion)
        {
            ExceptionType = exceptionType ?? throw new ArgumentNullException(nameof(exceptionType));
            ExceptionAssertion = exceptionAssertion ?? throw new ArgumentNullException(nameof(exceptionAssertion));
        }

        public Type ExceptionType { get; set; }
        public object ExceptionAssertion { get; set; }
    }
}
