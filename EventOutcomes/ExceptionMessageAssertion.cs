using System;
using System.Text.RegularExpressions;

namespace EventOutcomes
{
    public class ExceptionMessageAssertion : IExceptionAssertion
    {
        private readonly string _expectedMessage;
        private readonly ExceptionMessageAssertionType _matchingType;

        public ExceptionMessageAssertion(string expectedMessage, ExceptionMessageAssertionType matchingType)
        {
            _expectedMessage = expectedMessage;
            _matchingType = matchingType;
        }

        public void Assert(Exception exception)
        {
            var exceptionMessage = exception.Message;

            switch (_matchingType)
            {
                case ExceptionMessageAssertionType.Equals:
                    if (!string.Equals(_expectedMessage, exceptionMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;

                case ExceptionMessageAssertionType.Contains:
                    if (!exceptionMessage.ToLower().Contains(_expectedMessage.ToLower()))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;

                case ExceptionMessageAssertionType.MatchesRegex:
                    if (!new Regex(_expectedMessage).IsMatch(exceptionMessage))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;
            }
        }

        private void ThrowAssertException(string assertionTypeName, Exception thrownException)
        {
            throw new AssertException($"Exception message assertion of type {assertionTypeName} failed.{Environment.NewLine}Expected: {_expectedMessage}{Environment.NewLine}Actual: {thrownException.Message}{Environment.NewLine}Thrown exception:{Environment.NewLine}{thrownException}");
        }

        public static ExceptionMessageAssertion Equals(string expectedMessage) => new ExceptionMessageAssertion(expectedMessage, ExceptionMessageAssertionType.Equals);

        public static ExceptionMessageAssertion Contains(string expectedMessage) => new ExceptionMessageAssertion(expectedMessage, ExceptionMessageAssertionType.Contains);

        public static ExceptionMessageAssertion MatchesRegex(string expectedMessage) => new ExceptionMessageAssertion(expectedMessage, ExceptionMessageAssertionType.MatchesRegex);
    }
}
