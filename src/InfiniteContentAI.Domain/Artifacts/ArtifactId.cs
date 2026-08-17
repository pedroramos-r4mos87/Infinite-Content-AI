namespace InfiniteContentAI.Domain.Artifacts;

public readonly record struct ArtifactId(Guid Value)
{
    public static ArtifactId New()
    {
        return new ArtifactId(Guid.CreateVersion7());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
