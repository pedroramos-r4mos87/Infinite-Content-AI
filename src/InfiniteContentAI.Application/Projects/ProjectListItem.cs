namespace InfiniteContentAI.Application.Projects;

public sealed record ProjectListItem(
    Guid Id,
    string Name,
    string Status,
    DateTimeOffset CreatedAt);
