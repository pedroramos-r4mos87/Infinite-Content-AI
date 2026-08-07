using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Domain.Pipelines;

public sealed record PipelineName
{
    public const int MaximumLength = 200;

    private PipelineName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PipelineName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PipelineName>(PipelineErrors.NameRequired);
        }

        string normalized = value.Trim();

        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<PipelineName>(PipelineErrors.NameTooLong);
        }

        return Result.Success(new PipelineName(normalized));
    }

    public override string ToString()
    {
        return Value;
    }
}
