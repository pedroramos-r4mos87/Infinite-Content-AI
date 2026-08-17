using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Executions;

public sealed record PipelineExecutionStartedDomainEvent(
    Guid EventId,
    PipelineExecutionId PipelineExecutionId,
    DateTimeOffset OccurredAt) : IDomainEvent;
