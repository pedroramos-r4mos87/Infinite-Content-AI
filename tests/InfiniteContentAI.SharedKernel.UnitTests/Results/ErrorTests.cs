using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.SharedKernel.UnitTests.Results;

public sealed class ErrorTests
{
    public static TheoryData<Error, ErrorType> Factories =>
        new()
        {
            {
                Error.Validation("Test.Validation", "Validation"),
                ErrorType.Validation
            },
            {
                Error.NotFound("Test.NotFound", "Not found"),
                ErrorType.NotFound
            },
            {
                Error.Conflict("Test.Conflict", "Conflict"),
                ErrorType.Conflict
            },
            {
                Error.Unauthorized("Test.Unauthorized", "Unauthorized"),
                ErrorType.Unauthorized
            },
            {
                Error.Forbidden("Test.Forbidden", "Forbidden"),
                ErrorType.Forbidden
            },
            {
                Error.RateLimit("Test.RateLimit", "Rate limit"),
                ErrorType.RateLimit
            },
            {
                Error.Timeout("Test.Timeout", "Timeout"),
                ErrorType.Timeout
            },
            {
                Error.Unavailable("Test.Unavailable", "Unavailable"),
                ErrorType.Unavailable
            },
            {
                Error.Failure("Test.Failure", "Failure"),
                ErrorType.Failure
            },
        };

    [Theory]
    [MemberData(nameof(Factories))]
    public void FactoryCreatesExpectedErrorType(
        Error error,
        ErrorType expectedType)
    {
        Assert.Equal(expectedType, error.Type);
        Assert.StartsWith("Test.", error.Code);
        Assert.NotEmpty(error.Description);
    }
}
