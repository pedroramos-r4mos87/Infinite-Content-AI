using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.AddPipelineStep;

public static class AddPipelineStepValidator
{
    public static Result<PipelineStepType> Validate(AddPipelineStepCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PipelineId == Guid.Empty)
        {
            return Result.Failure<PipelineStepType>(
                PipelineApplicationErrors.IdRequired);
        }

        return MapType(command.Type);
    }

    private static Result<PipelineStepType> MapType(string? value)
    {
        string? normalized = value?.Trim();

        if (string.Equals(normalized, "research", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success(PipelineStepType.Research);
        }

        if (string.Equals(normalized, "script", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success(PipelineStepType.Script);
        }

        return Result.Failure<PipelineStepType>(
            PipelineApplicationErrors.StepTypeInvalid);
    }
}
