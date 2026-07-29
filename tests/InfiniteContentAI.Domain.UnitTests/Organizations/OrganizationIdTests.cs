using InfiniteContentAI.Domain.Organizations;

namespace InfiniteContentAI.Domain.UnitTests.Organizations;

public sealed class OrganizationIdTests
{
    [Fact]
    public void OrganizationIdPreservesValidGuid()
    {
        Guid value = Guid.CreateVersion7();

        var organizationId = new OrganizationId(value);

        Assert.Equal(value, organizationId.Value);
        Assert.NotEqual(OrganizationId.Empty, organizationId);
    }

    [Fact]
    public void OrganizationIdsWithSameValueAreEqual()
    {
        Guid value = Guid.CreateVersion7();

        var first = new OrganizationId(value);
        var second = new OrganizationId(value);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }
}
