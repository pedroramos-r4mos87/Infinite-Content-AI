namespace InfiniteContentAI.Api.Projects;

public sealed record CreateProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt);
