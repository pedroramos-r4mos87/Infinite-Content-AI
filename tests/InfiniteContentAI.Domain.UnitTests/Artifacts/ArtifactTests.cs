using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.UnitTests.Artifacts;

public sealed class ArtifactTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.CreateVersion7());
    private static readonly ProjectId ProjectId = ProjectId.New();
    private static readonly PipelineExecutionId ExecutionId = PipelineExecutionId.New();
    private static readonly StepExecutionId StepExecutionId = StepExecutionId.New();
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(ArtifactType.Research)]
    [InlineData(ArtifactType.Script)]
    public void CreateReturnsImmutableArtifactAndPreservesContent(ArtifactType type)
    {
        const string content = "  # Conteúdo\n\nTexto com Markdown.  ";

        var result = Artifact.Create(
            OrganizationId,
            ProjectId,
            ExecutionId,
            StepExecutionId,
            type,
            content,
            new StubClock(CreatedAt));

        Assert.True(result.IsSuccess);
        Artifact artifact = result.Value;
        Assert.Equal(7, artifact.Id.Value.Version);
        Assert.Equal(OrganizationId, artifact.OrganizationId);
        Assert.Equal(ProjectId, artifact.ProjectId);
        Assert.Equal(ExecutionId, artifact.PipelineExecutionId);
        Assert.Equal(StepExecutionId, artifact.StepExecutionId);
        Assert.Equal(type, artifact.Type);
        Assert.Equal(content, artifact.Content);
        Assert.Equal(CreatedAt, artifact.CreatedAt);
        Assert.Empty(artifact.DomainEvents);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n\t")]
    public void CreateRejectsMissingContent(string? content)
    {
        var result = CreateResult(content: content);

        Assert.True(result.IsFailure);
        Assert.Equal(ArtifactErrors.ContentRequired, result.Error);
    }

    [Fact]
    public void CreateAcceptsContentAtMaximumLength()
    {
        string content = new('a', Artifact.MaximumContentLength);

        var result = CreateResult(content: content);

        Assert.True(result.IsSuccess);
        Assert.Equal(content, result.Value.Content);
    }

    [Fact]
    public void CreateRejectsContentAboveMaximumLength()
    {
        var result = CreateResult(
            content: new string('a', Artifact.MaximumContentLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal(ArtifactErrors.ContentTooLong, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingOrganization()
    {
        var result = CreateResult(organizationId: OrganizationId.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(ArtifactErrors.OrganizationRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingProject()
    {
        var result = CreateResult(projectId: new ProjectId(Guid.Empty));

        Assert.True(result.IsFailure);
        Assert.Equal(ArtifactErrors.ProjectRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingExecution()
    {
        var result = CreateResult(
            executionId: new PipelineExecutionId(Guid.Empty));

        Assert.True(result.IsFailure);
        Assert.Equal(ArtifactErrors.ExecutionRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingStepExecution()
    {
        var result = CreateResult(
            stepExecutionId: new StepExecutionId(Guid.Empty));

        Assert.True(result.IsFailure);
        Assert.Equal(ArtifactErrors.StepExecutionRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsInvalidType()
    {
        var result = CreateResult(type: (ArtifactType)999);

        Assert.True(result.IsFailure);
        Assert.Equal(ArtifactErrors.TypeInvalid, result.Error);
    }

    private static InfiniteContentAI.SharedKernel.Results.Result<Artifact> CreateResult(
        OrganizationId? organizationId = null,
        ProjectId? projectId = null,
        PipelineExecutionId? executionId = null,
        StepExecutionId? stepExecutionId = null,
        ArtifactType type = ArtifactType.Research,
        string? content = "Conteúdo")
    {
        return Artifact.Create(
            organizationId ?? OrganizationId,
            projectId ?? ProjectId,
            executionId ?? ExecutionId,
            stepExecutionId ?? StepExecutionId,
            type,
            content,
            new StubClock(CreatedAt));
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
