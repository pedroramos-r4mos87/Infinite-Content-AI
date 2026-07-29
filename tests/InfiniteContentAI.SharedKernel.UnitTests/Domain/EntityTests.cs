using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.SharedKernel.UnitTests.Domain;

public sealed class EntityTests
{
    [Fact]
    public void EntitiesWithSameIdAreEqual()
    {
        Guid id = Guid.CreateVersion7();
        var entityA = new TestEntity(id);
        var entityB = new TestEntity(id);

        Assert.Equal(entityA, entityB);
        Assert.True(entityA == entityB);
        Assert.False(entityA != entityB);
        Assert.Equal(entityA.GetHashCode(), entityB.GetHashCode());
    }

    [Fact]
    public void EntitiesWithDifferentIdsAreNotEqual()
    {
        var entityA = new TestEntity(Guid.CreateVersion7());
        var entityB = new TestEntity(Guid.CreateVersion7());

        Assert.NotEqual(entityA, entityB);
        Assert.False(entityA == entityB);
        Assert.True(entityA != entityB);
    }

    private sealed class TestEntity(Guid id) : Entity<Guid>(id);
}
