using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Pipelines;

public sealed record PipelineCreatedDomainEvent(
    Guid EventId,
    PipelineId PipelineId,
    OrganizationId OrganizationId,
    ProjectId ProjectId,
    DateTimeOffset OccurredAt) : IDomainEvent;
