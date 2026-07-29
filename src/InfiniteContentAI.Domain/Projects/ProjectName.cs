using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Domain.Projects;

public sealed record ProjectName
{
    public const int MaximumLength = 200;

    private ProjectName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProjectName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ProjectName>(ProjectErrors.NameRequired);
        }

        string normalized = value.Trim();

        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<ProjectName>(ProjectErrors.NameTooLong);
        }

        return Result.Success(new ProjectName(normalized));
    }

    public override string ToString()
    {
        return Value;
    }
}
