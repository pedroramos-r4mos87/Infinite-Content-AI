using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Api.Errors;

internal static class ResultExtensions
{
    public static IResult ToProblem(this Error error)
    {
        int status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        string slug = error.Type switch
        {
            ErrorType.Validation => "validation",
            ErrorType.NotFound => "not-found",
            ErrorType.Unauthorized => "unauthorized",
            _ => "failure",
        };

        return Results.Problem(
            type: $"https://errors.infinitecontent.ai/{slug}",
            title: status switch
            {
                StatusCodes.Status400BadRequest => "A requisição possui dados inválidos.",
                StatusCodes.Status404NotFound => "O recurso não foi encontrado.",
                StatusCodes.Status401Unauthorized =>
                    "A identidade atual não possui uma organização válida.",
                _ => "Não foi possível processar a requisição.",
            },
            detail: error.Description,
            statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
