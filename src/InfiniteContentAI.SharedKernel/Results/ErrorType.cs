namespace InfiniteContentAI.SharedKernel.Results;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    RateLimit = 6,
    Timeout = 7,
    Unavailable = 8,
    Failure = 9,
}
