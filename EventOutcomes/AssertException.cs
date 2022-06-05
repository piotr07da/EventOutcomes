using System;

namespace EventOutcomes
{
    public sealed class AssertException : Exception
    {
        public AssertException(string message)
            : base(message)
        {
        }
    }
}
