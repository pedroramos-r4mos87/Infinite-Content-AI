using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Pipelines.ListPipelines;

public sealed class ListPipelinesHandler(
    ICurrentOrganization currentOrganization,
    IProjectQueries projectQueries,
    IPipelineQueries pipelineQueries)
{
    public async Task<Result<PaginatedResult<PipelineListItem>>> HandleAsync(
        ListPipelinesQuery query,
        CancellationToken cancellationToken)
    {
        Result validation = ListPipelinesValidator.Validate(query);
        if (validation.IsFailure)
        {
            return Result.Failure<PaginatedResult<PipelineListItem>>(validation.Error);
        }

        var organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<PaginatedResult<PipelineListItem>>(organization.Error);
        }

        var projectId = new ProjectId(query.ProjectId);
        ProjectDetails? project = await projectQueries.GetAsync(
            organization.Value,
            projectId,
            cancellationToken);
        if (project is null)
        {
            return Result.Failure<PaginatedResult<PipelineListItem>>(
                PipelineApplicationErrors.ProjectNotFound);
        }

        return Result.Success(
            await pipelineQueries.ListByProjectAsync(
                organization.Value,
                projectId,
                query.Page,
                query.PageSize,
                cancellationToken));
    }
}
