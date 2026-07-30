using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Projects;

public sealed record ProjectCreatedDomainEvent(
    Guid EventId,
    ProjectId ProjectId,
    OrganizationId OrganizationId,
    DateTimeOffset OccurredAt) : IDomainEvent;
