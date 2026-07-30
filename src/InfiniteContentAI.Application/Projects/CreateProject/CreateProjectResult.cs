namespace InfiniteContentAI.Application.Projects.CreateProject;

public sealed record CreateProjectResult(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt);
