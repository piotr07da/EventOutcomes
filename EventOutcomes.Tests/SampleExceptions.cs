namespace EventOutcomes.Tests
{
    public class ExceptionalException : Exception
    {
        public ExceptionalException(string message)
            : base(message)
        {
        }
    }

    public class UnbelievableException : Exception
    {
        public UnbelievableException(string message)
            : base(message)
        {
        }
    }

    public class DerivedUnbelievableException : UnbelievableException
    {
        public DerivedUnbelievableException(string message)
            : base(message)
        {
        }
    }
}
