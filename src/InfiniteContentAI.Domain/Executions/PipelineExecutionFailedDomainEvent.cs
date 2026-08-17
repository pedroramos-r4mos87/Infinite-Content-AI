using InfiniteContentAI.SharedKernel.Domain;

namespace InfiniteContentAI.Domain.Executions;

public sealed record PipelineExecutionFailedDomainEvent(
    Guid EventId,
    PipelineExecutionId PipelineExecutionId,
    string FailureCode,
    DateTimeOffset OccurredAt) : IDomainEvent;
