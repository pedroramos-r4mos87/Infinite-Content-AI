using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Domain.Executions;

namespace InfiniteContentAI.Data.Executions;

internal sealed class PipelineExecutionRepository(ApplicationDbContext dbContext)
    : IPipelineExecutionRepository
{
    public async Task AddAsync(
        PipelineExecution execution,
        CancellationToken cancellationToken)
    {
        await dbContext.PipelineExecutions.AddAsync(execution, cancellationToken);
    }
}
