using InfiniteContentAI.Domain.Executions;

namespace InfiniteContentAI.Application.Executions;

public interface IPipelineExecutionRepository
{
    Task AddAsync(
        PipelineExecution execution,
        CancellationToken cancellationToken);
}
