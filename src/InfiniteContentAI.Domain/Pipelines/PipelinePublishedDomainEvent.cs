using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Pipelines;

public sealed record PipelinePublishedDomainEvent(
    Guid EventId,
    PipelineId PipelineId,
    OrganizationId OrganizationId,
    int Version,
    DateTimeOffset OccurredAt) : IDomainEvent;
