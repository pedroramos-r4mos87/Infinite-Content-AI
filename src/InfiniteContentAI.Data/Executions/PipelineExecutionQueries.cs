using InfiniteContentAI.Application.Executions;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace InfiniteContentAI.Data.Executions;

internal sealed class PipelineExecutionQueries(ApplicationDbContext dbContext)
    : IPipelineExecutionQueries
{
    public async Task<PipelineExecutionDetails?> GetAsync(
        OrganizationId organizationId,
        PipelineExecutionId executionId,
        CancellationToken cancellationToken)
    {
        var execution = await dbContext.PipelineExecutions
            .AsNoTracking()
            .Where(candidate =>
                candidate.OrganizationId == organizationId &&
                candidate.Id == executionId)
            .Select(candidate => new
            {
                Details = new PipelineExecutionDetails(
                    candidate.Id.Value,
                    candidate.ProjectId.Value,
                    candidate.PipelineId.Value,
                    candidate.PipelineVersion,
                    candidate.Topic,
                    candidate.Status.ToString().ToLowerInvariant(),
                    candidate.CreatedAt,
                    candidate.StartedAt,
                    candidate.CompletedAt,
                    candidate.FailedAt,
                    candidate.FailureCode,
                    candidate.Steps
                        .OrderBy(step => step.Position)
                        .Select(step => new StepExecutionDetails(
                            step.Id.Value,
                            step.PipelineStepId.Value,
                            step.Type.ToString().ToLowerInvariant(),
                            step.Position,
                            step.Status.ToString().ToLowerInvariant(),
                            step.StartedAt,
                            step.CompletedAt,
                            step.FailedAt,
                            step.FailureCode))
                        .ToList(),
                    Array.Empty<ArtifactDetails>()),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (execution is null)
        {
            return null;
        }

        List<ArtifactDetails> artifacts = await dbContext.Artifacts
            .AsNoTracking()
            .Where(artifact =>
                artifact.OrganizationId == organizationId &&
                artifact.PipelineExecutionId == executionId)
            .OrderBy(artifact => artifact.CreatedAt)
            .ThenBy(artifact => artifact.Id)
            .Select(artifact => new ArtifactDetails(
                artifact.Id.Value,
                artifact.StepExecutionId.Value,
                artifact.Type.ToString().ToLowerInvariant(),
                artifact.Content,
                artifact.CreatedAt))
            .ToListAsync(cancellationToken);

        return execution.Details with { Artifacts = artifacts };
    }
}
