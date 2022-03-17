namespace EventOutcomes.Tests;

public class FirstSampleEvent
{
    public FirstSampleEvent(int v)
    {
        V = v;
    }

    public int V { get; set; }
}

public class SecondSampleEvent
{
    public SecondSampleEvent(string v)
    {
        V = v;
    }

    public string V { get; set; }
}
