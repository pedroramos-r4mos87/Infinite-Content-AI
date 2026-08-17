using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Executions;

public sealed record PipelineExecutionCreatedDomainEvent(
    Guid EventId,
    PipelineExecutionId PipelineExecutionId,
    OrganizationId OrganizationId,
    ProjectId ProjectId,
    PipelineId PipelineId,
    DateTimeOffset OccurredAt) : IDomainEvent;
