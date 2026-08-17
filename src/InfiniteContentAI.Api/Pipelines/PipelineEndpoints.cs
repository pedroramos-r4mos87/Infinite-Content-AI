using InfiniteContentAI.Api.Errors;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Application.Pipelines.AddPipelineStep;
using InfiniteContentAI.Application.Pipelines.CreatePipeline;
using InfiniteContentAI.Application.Pipelines.GetPipeline;
using InfiniteContentAI.Application.Pipelines.ListPipelines;
using InfiniteContentAI.Application.Pipelines.PublishPipeline;
using InfiniteContentAI.SharedKernel.Pagination;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Api.Pipelines;

public static class PipelineEndpoints
{
    public static IEndpointRouteBuilder MapPipelineEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder projectPipelines = endpoints
            .MapGroup("/api/v1/projects/{projectId:guid}/pipelines")
            .RequireAuthorization()
            .WithTags("Pipelines");
        RouteGroupBuilder pipelines = endpoints
            .MapGroup("/api/v1/pipelines")
            .RequireAuthorization()
            .WithTags("Pipelines");

        projectPipelines.MapPost(
                "/",
                CreatePipelineAsync)
            .WithName("CreatePipeline")
            .WithSummary("Cria um Pipeline dentro de um Project.")
            .Produces<CreatePipelineResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        projectPipelines.MapGet(
                "/",
                ListPipelinesAsync)
            .WithName("ListPipelinesByProject")
            .WithSummary("Lista os Pipelines de um Project.")
            .Produces<PaginatedResult<PipelineListItemResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        pipelines.MapGet(
                "/{pipelineId:guid}",
                GetPipelineAsync)
            .WithName("GetPipeline")
            .WithSummary("Consulta a configuração de um Pipeline.")
            .Produces<PipelineResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        pipelines.MapPost(
                "/{pipelineId:guid}/steps",
                AddPipelineStepAsync)
            .WithName("AddPipelineStep")
            .WithSummary("Adiciona uma etapa a um Pipeline em Draft.")
            .Produces<AddPipelineStepResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        pipelines.MapPost(
                "/{pipelineId:guid}/publish",
                PublishPipelineAsync)
            .WithName("PublishPipeline")
            .WithSummary("Publica um Pipeline válido.")
            .Produces<PublishPipelineResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> CreatePipelineAsync(
        Guid projectId,
        CreatePipelineRequest request,
        CreatePipelineHandler handler,
        CancellationToken cancellationToken)
    {
        Result<CreatePipelineResult> result = await handler.HandleAsync(
            new CreatePipelineCommand(
                projectId,
                request.Name,
                request.Description),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        CreatePipelineResult pipeline = result.Value;
        return Results.Created(
            $"/api/v1/pipelines/{pipeline.Id}",
            new CreatePipelineResponse(
                pipeline.Id,
                pipeline.ProjectId,
                pipeline.Name,
                pipeline.Description,
                pipeline.Status,
                pipeline.Version,
                pipeline.CreatedAt));
    }

    private static async Task<IResult> AddPipelineStepAsync(
        Guid pipelineId,
        AddPipelineStepRequest request,
        AddPipelineStepHandler handler,
        CancellationToken cancellationToken)
    {
        Result<AddPipelineStepResult> result = await handler.HandleAsync(
            new AddPipelineStepCommand(
                pipelineId,
                request.Type,
                request.Position),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        AddPipelineStepResult step = result.Value;
        return Results.Ok(
            new AddPipelineStepResponse(
                step.StepId,
                step.Type,
                step.Position));
    }

    private static async Task<IResult> PublishPipelineAsync(
        Guid pipelineId,
        PublishPipelineHandler handler,
        CancellationToken cancellationToken)
    {
        Result<PublishPipelineResult> result = await handler.HandleAsync(
            new PublishPipelineCommand(pipelineId),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        PublishPipelineResult pipeline = result.Value;
        return Results.Ok(
            new PublishPipelineResponse(
                pipeline.PipelineId,
                pipeline.Status,
                pipeline.Version,
                pipeline.PublishedAt));
    }

    private static async Task<IResult> GetPipelineAsync(
        Guid pipelineId,
        GetPipelineHandler handler,
        CancellationToken cancellationToken)
    {
        Result<PipelineDetails> result = await handler.HandleAsync(
            new GetPipelineQuery(pipelineId),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        PipelineDetails pipeline = result.Value;
        return Results.Ok(
            new PipelineResponse(
                pipeline.Id,
                pipeline.ProjectId,
                pipeline.Name,
                pipeline.Description,
                pipeline.Status,
                pipeline.Version,
                pipeline.CreatedAt,
                pipeline.CreatedBy,
                pipeline.PublishedAt,
                pipeline.Steps
                    .Select(step => new PipelineStepResponse(
                        step.Id,
                        step.Type,
                        step.Position))
                    .ToArray()));
    }

    private static async Task<IResult> ListPipelinesAsync(
        Guid projectId,
        int? page,
        int? pageSize,
        ListPipelinesHandler handler,
        CancellationToken cancellationToken)
    {
        Result<PaginatedResult<PipelineListItem>> result =
            await handler.HandleAsync(
                new ListPipelinesQuery(
                    projectId,
                    page ?? 1,
                    pageSize ?? 20),
                cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        PaginatedResult<PipelineListItem> pipelines = result.Value;
        return Results.Ok(
            new PaginatedResult<PipelineListItemResponse>(
                pipelines.Items
                    .Select(pipeline => new PipelineListItemResponse(
                        pipeline.Id,
                        pipeline.ProjectId,
                        pipeline.Name,
                        pipeline.Status,
                        pipeline.Version,
                        pipeline.CreatedAt,
                        pipeline.PublishedAt))
                    .ToArray(),
                pipelines.Page,
                pipelines.PageSize,
                pipelines.TotalCount));
    }
}
