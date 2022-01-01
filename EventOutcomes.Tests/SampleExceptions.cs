namespace EventOutcomes.Tests
{
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
