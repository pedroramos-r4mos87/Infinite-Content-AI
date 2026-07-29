using InfiniteContentAI.SharedKernel.Pagination;

namespace InfiniteContentAI.SharedKernel.UnitTests.Pagination;

public sealed class PaginatedResultTests
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(100, 20, 5)]
    public void TotalPagesRoundsUp(
        long totalCount,
        int pageSize,
        int expectedTotalPages)
    {
        var result = new PaginatedResult<int>(
            [],
            1,
            pageSize,
            totalCount);

        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TotalPagesIsZeroWhenPageSizeIsNotPositive(
        int pageSize)
    {
        var result = new PaginatedResult<int>(
            [],
            1,
            pageSize,
            10);

        Assert.Equal(0, result.TotalPages);
    }
}
