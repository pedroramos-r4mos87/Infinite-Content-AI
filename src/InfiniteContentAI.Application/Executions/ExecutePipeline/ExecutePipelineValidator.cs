using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Executions.ExecutePipeline;

public static class ExecutePipelineValidator
{
    public static Result Validate(ExecutePipelineCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PipelineId == Guid.Empty)
        {
            return Result.Failure(PipelineExecutionErrors.PipelineRequired);
        }

        if (string.IsNullOrWhiteSpace(command.Topic))
        {
            return Result.Failure(PipelineExecutionErrors.TopicRequired);
        }

        return command.Topic.Trim().Length > PipelineExecution.MaximumTopicLength
            ? Result.Failure(PipelineExecutionErrors.TopicTooLong)
            : Result.Success();
    }
}
