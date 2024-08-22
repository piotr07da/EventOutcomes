using EventForging;

namespace EventOutcomes.EventForging;

public static class TestExtensions
{
    public static Test ThenAggregate<TAggregate>(this Test test, Func<TAggregate, bool> assertion)
    {
        return test.Then<IRepository<TAggregate>>(async r => assertion(await r.GetAsync(test.EventStreamId(), CancellationToken.None)));
    }

    public static Test ThenAggregate<TAggregate>(this Test test, Guid aggregateId, Func<TAggregate, bool> assertion)
    {
        return test.Then<IRepository<TAggregate>>(async r => assertion(await r.GetAsync(aggregateId, CancellationToken.None)));
    }
}

