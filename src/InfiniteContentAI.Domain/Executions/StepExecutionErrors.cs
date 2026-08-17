using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Domain.Executions;

public static class StepExecutionErrors
{
    public static readonly Error PipelineStepRequired = Error.Validation(
        "StepExecution.PipelineStepRequired",
        "A etapa de pipeline é obrigatória.");

    public static readonly Error TypeInvalid = Error.Validation(
        "StepExecution.TypeInvalid",
        "O tipo da etapa de execução é inválido.");

    public static readonly Error PositionInvalid = Error.Validation(
        "StepExecution.PositionInvalid",
        "A posição da etapa deve ser maior que zero.");

    public static readonly Error CannotStart = Error.Conflict(
        "StepExecution.CannotStart",
        "Somente uma etapa pendente pode ser iniciada.");

    public static readonly Error CannotComplete = Error.Conflict(
        "StepExecution.CannotComplete",
        "Somente uma etapa em andamento pode ser concluída.");

    public static readonly Error CannotFail = Error.Conflict(
        "StepExecution.CannotFail",
        "Uma etapa finalizada não pode falhar.");

    public static readonly Error FailureCodeRequired = Error.Validation(
        "StepExecution.FailureCodeRequired",
        "O código da falha é obrigatório.");

    public static readonly Error FailureCodeTooLong = Error.Validation(
        "StepExecution.FailureCodeTooLong",
        $"O código da falha deve possuir no máximo {StepExecution.MaximumFailureCodeLength} caracteres.");

    public static readonly Error FailureMessageTooLong = Error.Validation(
        "StepExecution.FailureMessageTooLong",
        $"A mensagem da falha deve possuir no máximo {StepExecution.MaximumFailureMessageLength} caracteres.");
}
