using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.AddPipelineStep;

public sealed class AddPipelineStepHandler(
    ICurrentOrganization currentOrganization,
    IPipelineRepository pipelineRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<AddPipelineStepResult>> HandleAsync(
        AddPipelineStepCommand command,
        CancellationToken cancellationToken)
    {
        Result<PipelineStepType> stepType =
            AddPipelineStepValidator.Validate(command);
        if (stepType.IsFailure)
        {
            return Result.Failure<AddPipelineStepResult>(stepType.Error);
        }

        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<AddPipelineStepResult>(organization.Error);
        }

        Pipeline? pipeline = await pipelineRepository.GetForUpdateAsync(
            organization.Value,
            new PipelineId(command.PipelineId),
            cancellationToken);
        if (pipeline is null)
        {
            return Result.Failure<AddPipelineStepResult>(
                PipelineApplicationErrors.NotFound);
        }

        Result<PipelineStepId> addition = stepType.Value switch
        {
            PipelineStepType.Research => pipeline.AddResearchStep(command.Position),
            PipelineStepType.Script => pipeline.AddScriptStep(command.Position),
            _ => throw new InvalidOperationException("Tipo de etapa validado é inválido."),
        };

        if (addition.IsFailure)
        {
            return Result.Failure<AddPipelineStepResult>(addition.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new AddPipelineStepResult(
                addition.Value.Value,
                stepType.Value.ToString().ToLowerInvariant(),
                command.Position));
    }
}
