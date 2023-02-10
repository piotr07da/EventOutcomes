namespace EventOutcomes.Tests;

public interface ICleverService
{
    void SetValue(int v);
    Task SetValueAsync(int v);
}

public class FakeCleverService : ICleverService
{
    public int Value { get; private set; }

    public void SetValue(int v)
    {
        Value = v;
    }

    public Task SetValueAsync(int v)
    {
        Value = v;
        return Task.CompletedTask;
    }
}
