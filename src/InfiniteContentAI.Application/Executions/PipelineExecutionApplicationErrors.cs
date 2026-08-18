using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Executions;

public static class PipelineExecutionApplicationErrors
{
    public static readonly Error ExecutionRequired = Error.Validation(
        "PipelineExecution.ExecutionRequired",
        "O identificador da execução é obrigatório.");

    public static readonly Error NotFound = Error.NotFound(
        "PipelineExecution.NotFound",
        "A execução de pipeline informada não foi encontrada.");

    public static readonly Error PipelineNotFound = Error.NotFound(
        "PipelineExecution.PipelineNotFound",
        "O pipeline informado não foi encontrado.");

    public static readonly Error PipelineNotPublished = Error.Conflict(
        "PipelineExecution.PipelineNotPublished",
        "Somente um pipeline publicado pode ser executado.");
}
