using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Executions;

public static class PipelineExecutionApplicationErrors
{
    public static readonly Error PipelineNotFound = Error.NotFound(
        "PipelineExecution.PipelineNotFound",
        "O pipeline informado não foi encontrado.");

    public static readonly Error PipelineNotPublished = Error.Conflict(
        "PipelineExecution.PipelineNotPublished",
        "Somente um pipeline publicado pode ser executado.");
}
