using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Domain.Executions;

public static class PipelineExecutionErrors
{
    public static readonly Error OrganizationRequired = Error.Validation(
        "PipelineExecution.OrganizationRequired",
        "A organização da execução é obrigatória.");

    public static readonly Error ProjectRequired = Error.Validation(
        "PipelineExecution.ProjectRequired",
        "O projeto da execução é obrigatório.");

    public static readonly Error PipelineRequired = Error.Validation(
        "PipelineExecution.PipelineRequired",
        "O pipeline da execução é obrigatório.");

    public static readonly Error PipelineVersionInvalid = Error.Validation(
        "PipelineExecution.PipelineVersionInvalid",
        "A versão do pipeline deve ser maior que zero.");

    public static readonly Error TopicRequired = Error.Validation(
        "PipelineExecution.TopicRequired",
        "O tema da execução é obrigatório.");

    public static readonly Error TopicTooLong = Error.Validation(
        "PipelineExecution.TopicTooLong",
        $"O tema da execução deve possuir no máximo {PipelineExecution.MaximumTopicLength} caracteres.");

    public static readonly Error CreatedByRequired = Error.Validation(
        "PipelineExecution.CreatedByRequired",
        "O autor da execução é obrigatório.");

    public static readonly Error CreatedByTooLong = Error.Validation(
        "PipelineExecution.CreatedByTooLong",
        $"O autor da execução deve possuir no máximo {PipelineExecution.MaximumCreatedByLength} caracteres.");

    public static readonly Error CannotStart = Error.Conflict(
        "PipelineExecution.CannotStart",
        "Somente uma execução pendente pode ser iniciada.");

    public static readonly Error CannotComplete = Error.Conflict(
        "PipelineExecution.CannotComplete",
        "A execução somente pode ser concluída quando estiver em andamento e todas as etapas estiverem concluídas.");

    public static readonly Error AlreadyCompleted = Error.Conflict(
        "PipelineExecution.AlreadyCompleted",
        "A execução já foi concluída.");

    public static readonly Error AlreadyFailed = Error.Conflict(
        "PipelineExecution.AlreadyFailed",
        "A execução já falhou.");

    public static readonly Error FailureCodeRequired = Error.Validation(
        "PipelineExecution.FailureCodeRequired",
        "O código da falha é obrigatório.");

    public static readonly Error FailureCodeTooLong = Error.Validation(
        "PipelineExecution.FailureCodeTooLong",
        $"O código da falha deve possuir no máximo {PipelineExecution.MaximumFailureCodeLength} caracteres.");

    public static readonly Error FailureMessageTooLong = Error.Validation(
        "PipelineExecution.FailureMessageTooLong",
        $"A mensagem da falha deve possuir no máximo {PipelineExecution.MaximumFailureMessageLength} caracteres.");

    public static readonly Error StepPositionInvalid = Error.Validation(
        "PipelineExecution.StepPositionInvalid",
        "A posição da etapa deve ser maior que zero.");

    public static readonly Error StepPositionAlreadyExists = Error.Conflict(
        "PipelineExecution.StepPositionAlreadyExists",
        "Já existe uma etapa de execução na posição informada.");

    public static readonly Error ResearchStepAlreadyExists = Error.Conflict(
        "PipelineExecution.ResearchStepAlreadyExists",
        "A execução já possui uma etapa de pesquisa.");

    public static readonly Error ScriptStepAlreadyExists = Error.Conflict(
        "PipelineExecution.ScriptStepAlreadyExists",
        "A execução já possui uma etapa de roteiro.");

    public static readonly Error InvalidStepOrder = Error.Validation(
        "PipelineExecution.InvalidStepOrder",
        "A execução deve possuir Research na posição 1 e Script na posição 2.");

    public static readonly Error StepsLocked = Error.Conflict(
        "PipelineExecution.StepsLocked",
        "As etapas não podem ser alteradas depois que a execução deixa o estado pendente.");

    public static readonly Error StepNotFound = Error.NotFound(
        "PipelineExecution.StepNotFound",
        "A etapa de execução informada não foi encontrada.");
}
