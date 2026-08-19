using InfiniteContentAI.Application.Executions.ExecutePipeline;
using InfiniteContentAI.Domain.Executions;

namespace InfiniteContentAI.Application.UnitTests.Executions;

public sealed class ExecutePipelineValidatorTests
{
    [Fact]
    public void ValidateRejectsEmptyPipelineId()
    {
        var result = ExecutePipelineValidator.Validate(
            new ExecutePipelineCommand(Guid.Empty, "Tema"));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.PipelineRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRejectsMissingTopic(string? topic)
    {
        var result = ExecutePipelineValidator.Validate(
            new ExecutePipelineCommand(Guid.CreateVersion7(), topic));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.TopicRequired, result.Error);
    }

    [Fact]
    public void ValidateRejectsTopicAboveDomainLimit()
    {
        var result = ExecutePipelineValidator.Validate(
            new ExecutePipelineCommand(
                Guid.CreateVersion7(),
                new string('a', PipelineExecution.MaximumTopicLength + 1)));

        Assert.True(result.IsFailure);
        Assert.Equal(PipelineExecutionErrors.TopicTooLong, result.Error);
    }
}
