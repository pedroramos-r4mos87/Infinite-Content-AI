namespace InfiniteContentAI.Domain.Organizations;

public readonly record struct OrganizationId(Guid Value)
{
    public static readonly OrganizationId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }
}
