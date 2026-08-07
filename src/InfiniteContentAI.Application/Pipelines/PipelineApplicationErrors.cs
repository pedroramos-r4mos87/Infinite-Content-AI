using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines;

public static class PipelineApplicationErrors
{
    public static readonly Error ProjectNotFound = Error.NotFound(
        "Pipeline.ProjectNotFound",
        "O projeto informado não foi encontrado.");

    public static readonly Error NotFound = Error.NotFound(
        "Pipeline.NotFound",
        "O pipeline informado não foi encontrado.");

    public static readonly Error IdRequired = Error.Validation(
        "Pipeline.IdRequired",
        "O identificador do pipeline é obrigatório.");

    public static readonly Error StepTypeInvalid = Error.Validation(
        "Pipeline.StepTypeInvalid",
        "O tipo da etapa deve ser research ou script.");
}
