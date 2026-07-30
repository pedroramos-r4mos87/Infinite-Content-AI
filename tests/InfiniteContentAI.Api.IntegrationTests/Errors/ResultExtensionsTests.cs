using System.Reflection;
using System.Text.Json;
using InfiniteContentAI.SharedKernel.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteContentAI.Api.IntegrationTests.Errors;

public sealed class ResultExtensionsTests
{
    [Fact]
    public async Task ValidationMapsToExpectedProblem()
    {
        await AssertProblemAsync(
            Error.Validation("Test.Validation", "Detalhe de validação."),
            StatusCodes.Status400BadRequest,
            "validation",
            "A requisição possui dados inválidos.",
            "Detalhe de validação.");
    }

    [Fact]
    public async Task UnauthorizedMapsToExpectedProblem()
    {
        await AssertProblemAsync(
            Error.Unauthorized("Test.Unauthorized", "Detalhe de autenticação."),
            StatusCodes.Status401Unauthorized,
            "unauthorized",
            "A autenticação é necessária para acessar este recurso.",
            "Detalhe de autenticação.");
    }

    [Fact]
    public async Task ForbiddenMapsToExpectedProblem()
    {
        await AssertProblemAsync(
            Error.Forbidden("Test.Forbidden", "Detalhe de autorização."),
            StatusCodes.Status403Forbidden,
            "forbidden",
            "A identidade atual não possui permissão para esta operação.",
            "Detalhe de autorização.");
    }

    [Fact]
    public async Task NotFoundMapsToExpectedProblem()
    {
        await AssertProblemAsync(
            Error.NotFound("Test.NotFound", "Detalhe de recurso ausente."),
            StatusCodes.Status404NotFound,
            "not-found",
            "O recurso não foi encontrado.",
            "Detalhe de recurso ausente.");
    }

    [Fact]
    public async Task ConflictMapsToExpectedProblem()
    {
        await AssertProblemAsync(
            Error.Conflict("Test.Conflict", "Detalhe de conflito."),
            StatusCodes.Status409Conflict,
            "conflict",
            "A operação conflita com o estado atual do recurso.",
            "Detalhe de conflito.");
    }

    [Fact]
    public async Task FailureMapsToGenericProblemWithoutInternalDescription()
    {
        const string internalDescription = "Connection string: secret";
        JsonElement problem = await ExecuteAsync(
            Error.Failure("Test.InternalFailure", internalDescription));

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.GetProperty("status").GetInt32());
        Assert.Equal(
            "https://errors.infinitecontent.ai/failure",
            problem.GetProperty("type").GetString());
        Assert.Equal(
            "Ocorreu uma falha inesperada ao processar a requisição.",
            problem.GetProperty("title").GetString());
        Assert.Equal(
            "Ocorreu uma falha interna. Tente novamente mais tarde.",
            problem.GetProperty("detail").GetString());
        Assert.Equal("Test.InternalFailure", problem.GetProperty("code").GetString());
        Assert.DoesNotContain(internalDescription, problem.GetRawText(), StringComparison.Ordinal);
    }

    private static async Task AssertProblemAsync(
        Error error,
        int expectedStatus,
        string expectedSlug,
        string expectedTitle,
        string expectedDetail)
    {
        JsonElement problem = await ExecuteAsync(error);

        Assert.Equal(expectedStatus, problem.GetProperty("status").GetInt32());
        Assert.Equal(
            $"https://errors.infinitecontent.ai/{expectedSlug}",
            problem.GetProperty("type").GetString());
        Assert.Equal(expectedTitle, problem.GetProperty("title").GetString());
        Assert.Equal(expectedDetail, problem.GetProperty("detail").GetString());
        Assert.Equal(error.Code, problem.GetProperty("code").GetString());
    }

    private static async Task<JsonElement> ExecuteAsync(Error error)
    {
        Type extensionsType = typeof(Program).Assembly.GetRequiredType(
            "InfiniteContentAI.Api.Errors.ResultExtensions");
        MethodInfo toProblem = extensionsType.GetRequiredMethod("ToProblem");
        var result = Assert.IsAssignableFrom<IResult>(
            toProblem.Invoke(null, [error]));

        await using ServiceProvider services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();
        await using var responseBody = new MemoryStream();
        var context = new DefaultHttpContext
        {
            RequestServices = services,
            Response = { Body = responseBody },
        };

        await result.ExecuteAsync(context);
        responseBody.Position = 0;
        return await JsonSerializer.DeserializeAsync<JsonElement>(responseBody);
    }
}

internal static class ReflectionExtensions
{
    public static Type GetRequiredType(this Assembly assembly, string name)
    {
        return assembly.GetType(name)
            ?? throw new InvalidOperationException($"Type '{name}' was not found.");
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name)
    {
        return type.GetMethod(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Method '{name}' was not found.");
    }
}
