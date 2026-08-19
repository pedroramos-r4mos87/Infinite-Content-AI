using InfiniteContentAI.Api.Errors;
using InfiniteContentAI.Application.Executions.ExecutePipeline;
using InfiniteContentAI.Application.Executions.GetPipelineExecution;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Api.Executions;

public static class ExecutionEndpoints
{
    public static IEndpointRouteBuilder MapExecutionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder pipelines = endpoints
            .MapGroup("/api/v1/pipelines")
            .RequireAuthorization()
            .WithTags("Executions");
        RouteGroupBuilder executions = endpoints
            .MapGroup("/api/v1/executions")
            .RequireAuthorization()
            .WithTags("Executions");

        pipelines.MapPost(
                "/{pipelineId:guid}/executions",
                ExecutePipelineAsync)
            .WithName("ExecutePipeline")
            .WithSummary("Executa sincronamente um Pipeline publicado.")
            .Produces<ExecutePipelineResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        executions.MapGet(
                "/{executionId:guid}",
                GetPipelineExecutionAsync)
            .WithName("GetPipelineExecution")
            .WithSummary("Consulta uma Pipeline Execution e seus resultados.")
            .Produces<PipelineExecutionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> ExecutePipelineAsync(
        Guid pipelineId,
        ExecutePipelineRequest request,
        ExecutePipelineHandler handler,
        CancellationToken cancellationToken)
    {
        Result<ExecutePipelineResult> result = await handler.HandleAsync(
            new ExecutePipelineCommand(pipelineId, request.Topic),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        ExecutePipelineResult execution = result.Value;
        return Results.Created(
            $"/api/v1/executions/{execution.ExecutionId}",
            new ExecutePipelineResponse(
                execution.ExecutionId,
                execution.PipelineId,
                execution.PipelineVersion,
                execution.Status,
                execution.CreatedAt,
                execution.StartedAt,
                execution.CompletedAt,
                execution.FailedAt,
                execution.FailureCode));
    }

    private static async Task<IResult> GetPipelineExecutionAsync(
        Guid executionId,
        GetPipelineExecutionHandler handler,
        CancellationToken cancellationToken)
    {
        Result<GetPipelineExecutionResult> result = await handler.HandleAsync(
            new GetPipelineExecutionQuery(executionId),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        GetPipelineExecutionResult execution = result.Value;
        return Results.Ok(
            new PipelineExecutionResponse(
                execution.ExecutionId,
                execution.ProjectId,
                execution.PipelineId,
                execution.PipelineVersion,
                execution.Topic,
                execution.Status,
                execution.CreatedAt,
                execution.StartedAt,
                execution.CompletedAt,
                execution.FailedAt,
                execution.FailureCode,
                execution.Steps
                    .Select(step => new StepExecutionResponse(
                        step.StepExecutionId,
                        step.PipelineStepId,
                        step.Type,
                        step.Position,
                        step.Status,
                        step.StartedAt,
                        step.CompletedAt,
                        step.FailedAt,
                        step.FailureCode))
                    .ToArray(),
                execution.Artifacts
                    .Select(artifact => new ArtifactResponse(
                        artifact.ArtifactId,
                        artifact.StepExecutionId,
                        artifact.Type,
                        artifact.Content,
                        artifact.CreatedAt))
                    .ToArray()));
    }
}
