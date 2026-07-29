namespace InfiniteContentAI.Domain.Projects;

public readonly record struct ProjectId(Guid Value)
{
    public static ProjectId New()
    {
        return new ProjectId(Guid.CreateVersion7());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
