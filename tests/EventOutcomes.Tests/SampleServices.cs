namespace EventOutcomes.Tests;

public interface IFirstSampleService
{
    void SetValue(int v);
    Task SetValueAsync(int v);
}

public sealed class FakeTransientFirstSampleService : IFirstSampleService
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

public interface ISecondSampleService
{
    int GetValue();
    void SetValue(int v);
    Task SetValueAsync(int v);
}

public sealed class FakeAsyncLocalSecondSampleService : ISecondSampleService
{
    private static readonly AsyncLocal<ValueHolder<int>> _value = new();

    public int GetValue()
    {
        return _value.Value!.Value;
    }

    public void SetValue(int v)
    {
        _value.Value = new ValueHolder<int>(v);
    }

    public Task SetValueAsync(int v)
    {
        _value.Value = new ValueHolder<int>(v);
        return Task.CompletedTask;
    }

    private sealed record ValueHolder<T>(T Value);
}
