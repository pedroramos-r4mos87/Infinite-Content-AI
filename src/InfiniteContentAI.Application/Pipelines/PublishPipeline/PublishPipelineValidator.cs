using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.PublishPipeline;

public static class PublishPipelineValidator
{
    public static Result Validate(PublishPipelineCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return command.PipelineId == Guid.Empty
            ? Result.Failure(PipelineApplicationErrors.IdRequired)
            : Result.Success();
    }
}
