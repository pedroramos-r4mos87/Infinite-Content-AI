using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Executions;

public sealed record PipelineExecutionCompletedDomainEvent(
    Guid EventId,
    PipelineExecutionId PipelineExecutionId,
    DateTimeOffset OccurredAt) : IDomainEvent;
