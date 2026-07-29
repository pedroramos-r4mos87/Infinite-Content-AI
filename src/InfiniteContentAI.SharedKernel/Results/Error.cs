using System.Diagnostics.CodeAnalysis;

namespace InfiniteContentAI.SharedKernel.Results;

[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Error is the documented cross-layer result type name.")]
public sealed record Error(
    string Code,
    string Description,
    ErrorType Type)
{
    public static readonly Error None = new(
        string.Empty,
        string.Empty,
        ErrorType.None);

    public static Error Validation(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Validation);
    }

    public static Error NotFound(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.NotFound);
    }

    public static Error Conflict(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Conflict);
    }

    public static Error Unauthorized(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Unauthorized);
    }

    public static Error Forbidden(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Forbidden);
    }

    public static Error RateLimit(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.RateLimit);
    }

    public static Error Timeout(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Timeout);
    }

    public static Error Unavailable(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Unavailable);
    }

    public static Error Failure(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Failure);
    }
}
