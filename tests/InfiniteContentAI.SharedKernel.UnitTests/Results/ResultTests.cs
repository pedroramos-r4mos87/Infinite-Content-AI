using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.SharedKernel.UnitTests.Results;

public sealed class ResultTests
{
    [Fact]
    public void SuccessCreatesSuccessfulResult()
    {
        Result result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void FailureContainsError()
    {
        Error error = CreateError();

        Result result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void SuccessOfTContainsValue()
    {
        Result<int> result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ValueThrowsWhenResultIsFailure()
    {
        Result<int> result = Result.Failure<int>(CreateError());

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ConstructorThrowsWhenSuccessContainsError()
    {
        Assert.Throws<InvalidOperationException>(
            () => new TestResult(true, CreateError()));
    }

    [Fact]
    public void ConstructorThrowsWhenFailureContainsNoError()
    {
        Assert.Throws<InvalidOperationException>(
            () => new TestResult(false, Error.None));
    }

    [Fact]
    public void ConstructorThrowsWhenErrorIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new TestResult(false, null!));
    }

    [Fact]
    public void MatchInvokesSuccessFunctionWhenResultIsSuccessful()
    {
        Result<int> result = Result.Success(42);

        string matched = result.Match(
            value => $"success:{value}",
            error => $"failure:{error.Code}");

        Assert.Equal("success:42", matched);
    }

    [Fact]
    public void MatchInvokesFailureFunctionWhenResultIsFailure()
    {
        Error error = CreateError();
        Result<int> result = Result.Failure<int>(error);

        string matched = result.Match(
            value => $"success:{value}",
            failure => $"failure:{failure.Code}");

        Assert.Equal($"failure:{error.Code}", matched);
    }

    private static Error CreateError()
    {
        return Error.Failure(
            "Test.Failure",
            "Falha de teste.");
    }

    private sealed class TestResult : Result
    {
        public TestResult(
            bool isSuccess,
            Error error)
            : base(isSuccess, error)
        {
        }
    }
}
