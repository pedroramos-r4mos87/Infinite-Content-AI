using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Api.Errors;

internal static class ResultExtensions
{
    public static IResult ToProblem(this Error error)
    {
        (int Status, string Slug, string Title) problem = error.Type switch
        {
            ErrorType.Validation => (
                StatusCodes.Status400BadRequest,
                "validation",
                "A requisição possui dados inválidos."),
            ErrorType.Unauthorized => (
                StatusCodes.Status401Unauthorized,
                "unauthorized",
                "A autenticação é necessária para acessar este recurso."),
            ErrorType.Forbidden => (
                StatusCodes.Status403Forbidden,
                "forbidden",
                "A identidade atual não possui permissão para esta operação."),
            ErrorType.NotFound => (
                StatusCodes.Status404NotFound,
                "not-found",
                "O recurso não foi encontrado."),
            ErrorType.Conflict => (
                StatusCodes.Status409Conflict,
                "conflict",
                "A operação conflita com o estado atual do recurso."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "failure",
                "Ocorreu uma falha inesperada ao processar a requisição."),
        };

        string detail = problem.Status == StatusCodes.Status500InternalServerError
            ? "Ocorreu uma falha interna. Tente novamente mais tarde."
            : error.Description;

        return Results.Problem(
            type: $"https://errors.infinitecontent.ai/{problem.Slug}",
            title: problem.Title,
            detail: detail,
            statusCode: problem.Status,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
