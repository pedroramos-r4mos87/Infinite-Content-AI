namespace InfiniteContentAI.SharedKernel.Pagination;

public sealed record PaginatedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(
                TotalCount /
                (double)PageSize);
}
