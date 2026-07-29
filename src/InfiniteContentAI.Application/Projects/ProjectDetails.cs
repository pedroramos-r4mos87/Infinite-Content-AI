namespace InfiniteContentAI.Application.Projects;

public sealed record ProjectDetails(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt);
