using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Domain.UnitTests.Pipelines;

public sealed class PipelineTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.CreateVersion7());
    private static readonly ProjectId ProjectId = ProjectId.New();
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateReturnsValidDraftPipelineWithCreationEvent()
    {
        var result = Pipeline.Create(
            OrganizationId,
            ProjectId,
            "  Pesquisa e Roteiro  ",
            "  Fluxo padrão de conteúdo.  ",
            "  user-123  ",
            new StubClock(CreatedAt));

        Assert.True(result.IsSuccess);
        Pipeline pipeline = result.Value;
        Assert.Equal(OrganizationId, pipeline.OrganizationId);
        Assert.Equal(ProjectId, pipeline.ProjectId);
        Assert.Equal("Pesquisa e Roteiro", pipeline.Name.Value);
        Assert.Equal("Fluxo padrão de conteúdo.", pipeline.Description);
        Assert.Equal(PipelineStatus.Draft, pipeline.Status);
        Assert.Equal(1, pipeline.Version);
        Assert.Equal(CreatedAt, pipeline.CreatedAt);
        Assert.Equal("user-123", pipeline.CreatedBy);
        Assert.Null(pipeline.PublishedAt);
        Assert.Empty(pipeline.Steps);
        Assert.Equal(7, pipeline.Id.Value.Version);

        PipelineCreatedDomainEvent domainEvent =
            Assert.IsType<PipelineCreatedDomainEvent>(Assert.Single(pipeline.DomainEvents));
        Assert.Equal(7, domainEvent.EventId.Version);
        Assert.Equal(pipeline.Id, domainEvent.PipelineId);
        Assert.Equal(OrganizationId, domainEvent.OrganizationId);
        Assert.Equal(ProjectId, domainEvent.ProjectId);
        Assert.Equal(CreatedAt, domainEvent.OccurredAt);
    }

    [Fact]
    public void CreateNormalizesWhitespaceDescriptionToNull()
    {
        Pipeline pipeline = CreatePipeline(description: "   ");

        Assert.Null(pipeline.Description);
    }

    [Fact]
    public void CreateRejectsMissingOrganization()
    {
        var result = Pipeline.Create(
            OrganizationId.Empty,
            ProjectId,
            "Pipeline",
            null,
            "user-123",
            new StubClock(CreatedAt));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.OrganizationRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsMissingProject()
    {
        var result = Pipeline.Create(
            OrganizationId,
            default,
            "Pipeline",
            null,
            "user-123",
            new StubClock(CreatedAt));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ProjectRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRejectsMissingName(string? name)
    {
        var result = Pipeline.Create(
            OrganizationId,
            ProjectId,
            name,
            null,
            "user-123",
            new StubClock(CreatedAt));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.NameRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsNameAboveMaximumLength()
    {
        var result = Pipeline.Create(
            OrganizationId,
            ProjectId,
            new string('a', PipelineName.MaximumLength + 1),
            null,
            "user-123",
            new StubClock(CreatedAt));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.NameTooLong, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRejectsMissingCreatedBy(string? createdBy)
    {
        var result = Pipeline.Create(
            OrganizationId,
            ProjectId,
            "Pipeline",
            null,
            createdBy,
            new StubClock(CreatedAt));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.CreatedByRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsDescriptionAboveMaximumLength()
    {
        var result = Pipeline.Create(
            OrganizationId,
            ProjectId,
            "Pipeline",
            new string('a', Pipeline.MaximumDescriptionLength + 1),
            "user-123",
            new StubClock(CreatedAt));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.DescriptionTooLong, result.Error);
    }

    [Fact]
    public void CreateRejectsCreatedByAboveMaximumLength()
    {
        var result = Pipeline.Create(
            OrganizationId,
            ProjectId,
            "Pipeline",
            null,
            new string('a', Pipeline.MaximumCreatedByLength + 1),
            new StubClock(CreatedAt));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.CreatedByTooLong, result.Error);
    }

    [Fact]
    public void AddResearchStepAddsValidStepWithVersionSevenId()
    {
        Pipeline pipeline = CreatePipeline();

        var result = pipeline.AddResearchStep(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value.Value.Version);
        PipelineStep step = Assert.Single(pipeline.Steps);
        Assert.Equal(result.Value, step.Id);
        Assert.Equal(PipelineStepType.Research, step.Type);
        Assert.Equal(1, step.Position);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddResearchStepRejectsNonPositivePosition(int position)
    {
        Pipeline pipeline = CreatePipeline();

        var result = pipeline.AddResearchStep(position);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.StepPositionInvalid, result.Error);
        Assert.Empty(pipeline.Steps);
    }

    [Fact]
    public void AddResearchStepRejectsSecondResearchStep()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);

        var result = pipeline.AddResearchStep(2);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ResearchStepAlreadyExists, result.Error);
        Assert.Single(pipeline.Steps);
    }

    [Fact]
    public void AddResearchStepRejectsDuplicatePosition()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);

        var result = pipeline.AddResearchStep(1);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.StepPositionAlreadyExists, result.Error);
        Assert.Single(pipeline.Steps);
    }

    [Fact]
    public void AddScriptStepAfterResearchAddsValidStep()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);

        var result = pipeline.AddScriptStep(2);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value.Value.Version);
        PipelineStep step = Assert.Single(
            pipeline.Steps,
            candidate => candidate.Type == PipelineStepType.Script);
        Assert.Equal(result.Value, step.Id);
        Assert.Equal(2, step.Position);
    }

    [Fact]
    public void AddScriptStepRejectsMissingResearchStep()
    {
        Pipeline pipeline = CreatePipeline();

        var result = pipeline.AddScriptStep(1);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ResearchStepRequired, result.Error);
        Assert.Empty(pipeline.Steps);
    }

    [Fact]
    public void AddScriptStepRejectsSecondScriptStep()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);

        var result = pipeline.AddScriptStep(3);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ScriptStepAlreadyExists, result.Error);
        Assert.Equal(2, pipeline.Steps.Count);
    }

    [Fact]
    public void AddScriptStepRejectsDuplicatePosition()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);

        var result = pipeline.AddScriptStep(1);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.StepPositionAlreadyExists, result.Error);
        Assert.Single(pipeline.Steps);
    }

    [Fact]
    public void AddScriptStepRejectsPositionBeforeResearch()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(2);

        var result = pipeline.AddScriptStep(1);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.InvalidStepOrder, result.Error);
        Assert.Single(pipeline.Steps);
    }

    [Fact]
    public void StepsCannotBeModifiedExternally()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);
        var collection = Assert.IsAssignableFrom<ICollection<PipelineStep>>(pipeline.Steps);

        Assert.True(collection.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => collection.Clear());
        Assert.Single(pipeline.Steps);
    }

    [Fact]
    public void PublishValidPipelineChangesStateAndRaisesEvent()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);
        pipeline.ClearDomainEvents();
        DateTimeOffset publishedAt = CreatedAt.AddMinutes(5);

        var result = pipeline.Publish(new StubClock(publishedAt));

        Assert.True(result.IsSuccess);
        Assert.Equal(PipelineStatus.Published, pipeline.Status);
        Assert.Equal(publishedAt, pipeline.PublishedAt);
        Assert.Equal(1, pipeline.Version);
        PipelinePublishedDomainEvent domainEvent =
            Assert.IsType<PipelinePublishedDomainEvent>(Assert.Single(pipeline.DomainEvents));
        Assert.Equal(7, domainEvent.EventId.Version);
        Assert.Equal(pipeline.Id, domainEvent.PipelineId);
        Assert.Equal(OrganizationId, domainEvent.OrganizationId);
        Assert.Equal(1, domainEvent.Version);
        Assert.Equal(publishedAt, domainEvent.OccurredAt);
    }

    [Fact]
    public void PublishRejectsEmptyPipeline()
    {
        Pipeline pipeline = CreatePipeline();

        var result = pipeline.Publish(new StubClock(CreatedAt.AddMinutes(5)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ResearchStepRequired, result.Error);
        Assert.Equal(PipelineStatus.Draft, pipeline.Status);
        Assert.Null(pipeline.PublishedAt);
    }

    [Fact]
    public void PublishRejectsMissingScriptStep()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);

        var result = pipeline.Publish(new StubClock(CreatedAt.AddMinutes(5)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.ScriptStepRequired, result.Error);
        Assert.Equal(PipelineStatus.Draft, pipeline.Status);
    }

    [Fact]
    public void PublishRejectsInvalidStepPositions()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(2);
        pipeline.AddScriptStep(3);

        var result = pipeline.Publish(new StubClock(CreatedAt.AddMinutes(5)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.InvalidStepOrder, result.Error);
        Assert.Equal(PipelineStatus.Draft, pipeline.Status);
    }

    [Fact]
    public void PublishRejectsSecondPublication()
    {
        Pipeline pipeline = CreatePublishedPipeline();
        DateTimeOffset firstPublishedAt = pipeline.PublishedAt!.Value;
        pipeline.ClearDomainEvents();

        var result = pipeline.Publish(new StubClock(firstPublishedAt.AddMinutes(5)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.AlreadyPublished, result.Error);
        Assert.Equal(firstPublishedAt, pipeline.PublishedAt);
        Assert.Empty(pipeline.DomainEvents);
    }

    [Fact]
    public void AddResearchStepRejectsPublishedPipeline()
    {
        Pipeline pipeline = CreatePublishedPipeline();

        var result = pipeline.AddResearchStep(3);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.NotDraft, result.Error);
        Assert.Equal(2, pipeline.Steps.Count);
    }

    [Fact]
    public void AddScriptStepRejectsPublishedPipeline()
    {
        Pipeline pipeline = CreatePublishedPipeline();

        var result = pipeline.AddScriptStep(3);

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineErrors.NotDraft, result.Error);
        Assert.Equal(2, pipeline.Steps.Count);
    }

    private static Pipeline CreatePipeline(string? description = null)
    {
        return Pipeline.Create(
            OrganizationId,
            ProjectId,
            "Pipeline",
            description,
            "user-123",
            new StubClock(CreatedAt)).Value;
    }

    private static Pipeline CreatePublishedPipeline()
    {
        Pipeline pipeline = CreatePipeline();
        pipeline.AddResearchStep(1);
        pipeline.AddScriptStep(2);
        pipeline.Publish(new StubClock(CreatedAt.AddMinutes(5)));

        return pipeline;
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
