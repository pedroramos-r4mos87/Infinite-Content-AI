using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.SharedKernel.UnitTests.Domain;

public sealed class AggregateRootTests
{
    [Fact]
    public void RaiseDomainEventStoresEvent()
    {
        var aggregate = new TestAggregate();
        var domainEvent = new TestDomainEvent(
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow);

        aggregate.ExecuteOperation(domainEvent);

        IDomainEvent storedEvent = Assert.Single(aggregate.DomainEvents);
        Assert.Same(domainEvent, storedEvent);
    }

    [Fact]
    public void ClearDomainEventsRemovesStoredEvents()
    {
        var aggregate = new TestAggregate();
        aggregate.ExecuteOperation(
            new TestDomainEvent(
                Guid.CreateVersion7(),
                DateTimeOffset.UtcNow));

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed class TestAggregate()
        : AggregateRoot<Guid>(Guid.CreateVersion7())
    {
        public void ExecuteOperation(IDomainEvent domainEvent)
        {
            RaiseDomainEvent(domainEvent);
        }
    }

    private sealed record TestDomainEvent(
        Guid EventId,
        DateTimeOffset OccurredAt) : IDomainEvent;
}
