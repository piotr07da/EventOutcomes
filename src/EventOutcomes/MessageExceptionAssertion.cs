using System;
using System.Text.RegularExpressions;

namespace EventOutcomes
{
    public sealed class MessageExceptionAssertion : IExceptionAssertion
    {
        private readonly string _expectedMessage;
        private readonly MessageExceptionAssertionType _matchingType;

        public MessageExceptionAssertion(string expectedMessage, MessageExceptionAssertionType matchingType)
        {
            _expectedMessage = expectedMessage;
            _matchingType = matchingType;
        }

        public void Assert(Exception exception)
        {
            var exceptionMessage = exception.Message;

            switch (_matchingType)
            {
                case MessageExceptionAssertionType.Equals:
                    if (!string.Equals(_expectedMessage, exceptionMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;

                case MessageExceptionAssertionType.Contains:
                    if (!exceptionMessage.ToLower().Contains(_expectedMessage.ToLower()))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;

                case MessageExceptionAssertionType.MatchesRegex:
                    if (!new Regex(_expectedMessage).IsMatch(exceptionMessage))
                    {
                        ThrowAssertException(_matchingType.ToString(), exception);
                    }

                    break;
            }
        }

        private void ThrowAssertException(string assertionTypeName, Exception thrownException)
        {
            throw new AssertException($"Exception with unexpected message was thrown.{Environment.NewLine}Exception message assertion of type {assertionTypeName} failed.{Environment.NewLine}Expected: {_expectedMessage}{Environment.NewLine}Actual: {thrownException.Message}{Environment.NewLine}Thrown exception:{Environment.NewLine}{thrownException}");
        }

        public static MessageExceptionAssertion Equals(string expectedMessage) => new MessageExceptionAssertion(expectedMessage, MessageExceptionAssertionType.Equals);

        public static MessageExceptionAssertion Contains(string expectedMessage) => new MessageExceptionAssertion(expectedMessage, MessageExceptionAssertionType.Contains);

        public static MessageExceptionAssertion MatchesRegex(string expectedMessage) => new MessageExceptionAssertion(expectedMessage, MessageExceptionAssertionType.MatchesRegex);
    }
}
