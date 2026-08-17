using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.CreatePipeline;

public static class CreatePipelineValidator
{
    public static Result Validate(CreatePipelineCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ProjectId == Guid.Empty)
        {
            return Result.Failure(PipelineErrors.ProjectRequired);
        }

        Result<PipelineName> name = PipelineName.Create(command.Name);
        return name.IsSuccess
            ? Result.Success()
            : Result.Failure(name.Error);
    }
}
