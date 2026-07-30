using InfiniteContentAI.Api.Errors;
using InfiniteContentAI.Application.Projects;
using InfiniteContentAI.Application.Projects.CreateProject;
using InfiniteContentAI.Application.Projects.GetProject;
using InfiniteContentAI.Application.Projects.ListProjects;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Api.Projects;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/projects")
            .RequireAuthorization()
            .WithTags("Projects");

        group.MapPost(
                "/",
                async (
                    CreateProjectRequest request,
                    CreateProjectHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    Result<CreateProjectResult> result = await handler.HandleAsync(
                        new CreateProjectCommand(request.Name, request.Description),
                        cancellationToken);

                    if (result.IsFailure)
                    {
                        return result.Error.ToProblem();
                    }

                    CreateProjectResult project = result.Value;
                    return Results.Created(
                        $"/api/v1/projects/{project.Id}",
                        new CreateProjectResponse(
                            project.Id,
                            project.Name,
                            project.Description,
                            project.Status,
                            project.CreatedAt));
                })
            .WithName("CreateProject")
            .Produces<CreateProjectResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet(
                "/{projectId:guid}",
                async (
                    Guid projectId,
                    GetProjectHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    Result<ProjectDetails> result = await handler.HandleAsync(
                        new GetProjectQuery(projectId),
                        cancellationToken);
                    return result.IsFailure
                        ? result.Error.ToProblem()
                        : Results.Ok(result.Value);
                })
            .WithName("GetProject")
            .Produces<ProjectDetails>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    ListProjectsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    Result<PaginatedResult<ProjectListItem>> result =
                        await handler.HandleAsync(
                            new ListProjectsQuery(page ?? 1, pageSize ?? 20),
                            cancellationToken);
                    return result.IsFailure
                        ? result.Error.ToProblem()
                        : Results.Ok(result.Value);
                })
            .WithName("ListProjects")
            .Produces<PaginatedResult<ProjectListItem>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
