namespace EventOutcomes;

public sealed class AssertException : Exception
{
    public AssertException(string message)
        : base($"{Environment.NewLine}{message}")
    {
    }
}
