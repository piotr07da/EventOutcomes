using System;
using System.Text.RegularExpressions;

namespace EventOutcomes
{
    public class ExceptionMessageAssertion
    {
        private readonly string _expectedMessage;
        private readonly MatchingType _matchingType;

        private ExceptionMessageAssertion(string expectedMessage, MatchingType matchingType)
        {
            _expectedMessage = expectedMessage;
            _matchingType = matchingType;
        }

        public void Assert(Exception exception)
        {
            var exceptionMessage = exception.Message;

            switch (_matchingType)
            {
                case MatchingType.Equals:
                    if (!string.Equals(_expectedMessage, exceptionMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;

                case MatchingType.Contains:
                    if (!exceptionMessage.ToLower().Contains(_expectedMessage.ToLower()))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;

                case MatchingType.MatchesRegex:
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

        public static ExceptionMessageAssertion Equals(string expectedMessage) => new ExceptionMessageAssertion(expectedMessage, MatchingType.Equals);

        public static ExceptionMessageAssertion Contains(string expectedMessage) => new ExceptionMessageAssertion(expectedMessage, MatchingType.Contains);

        public static ExceptionMessageAssertion MatchesRegex(string expectedMessage) => new ExceptionMessageAssertion(expectedMessage, MatchingType.MatchesRegex);

        private enum MatchingType
        {
            Equals,
            Contains,
            MatchesRegex,
        }
    }
}
