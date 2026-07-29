using System.Diagnostics.CodeAnalysis;

namespace InfiniteContentAI.SharedKernel.Results;

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(
        TValue? value,
        bool isSuccess,
        Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "O valor de um resultado com falha não pode ser acessado.");

    [SuppressMessage(
        "Design",
        "CA1000:Do not declare static members on generic types",
        Justification = "The static factory is part of the documented Result<T> API.")]
    public static Result<TValue> Success(
        TValue value)
    {
        return new Result<TValue>(
            value,
            true,
            Error.None);
    }

    [SuppressMessage(
        "Design",
        "CA1000:Do not declare static members on generic types",
        Justification = "The static factory is part of the documented Result<T> API.")]
    public static new Result<TValue> Failure(
        Error error)
    {
        return new Result<TValue>(
            default,
            false,
            error);
    }

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
            ? onSuccess(Value)
            : onFailure(Error);
    }
}
