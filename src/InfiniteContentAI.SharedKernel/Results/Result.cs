namespace InfiniteContentAI.SharedKernel.Results;

public class Result
{
    protected Result(
        bool isSuccess,
        Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException(
                "Um resultado de sucesso não pode possuir erro.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException(
                "Um resultado de falha deve possuir erro.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success()
    {
        return new Result(
            true,
            Error.None);
    }

    public static Result Failure(
        Error error)
    {
        return new Result(
            false,
            error);
    }

    public static Result<TValue> Success<TValue>(
        TValue value)
    {
        return Result<TValue>.Success(value);
    }

    public static Result<TValue> Failure<TValue>(
        Error error)
    {
        return Result<TValue>.Failure(error);
    }
}
