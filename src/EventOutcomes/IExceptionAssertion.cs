namespace EventOutcomes;

public interface IExceptionAssertion
{
    void Assert(Exception thrownException);
}
