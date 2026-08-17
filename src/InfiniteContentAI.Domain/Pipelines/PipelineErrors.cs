using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Domain.Pipelines;

public static class PipelineErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Pipeline.NameRequired",
        "O nome do pipeline é obrigatório.");

    public static readonly Error NameTooLong = Error.Validation(
        "Pipeline.NameTooLong",
        $"O nome do pipeline deve possuir no máximo {PipelineName.MaximumLength} caracteres.");

    public static readonly Error OrganizationRequired = Error.Validation(
        "Pipeline.OrganizationRequired",
        "A organização do pipeline é obrigatória.");

    public static readonly Error ProjectRequired = Error.Validation(
        "Pipeline.ProjectRequired",
        "O projeto do pipeline é obrigatório.");

    public static readonly Error CreatedByRequired = Error.Validation(
        "Pipeline.CreatedByRequired",
        "O autor da criação do pipeline é obrigatório.");

    public static readonly Error DescriptionTooLong = Error.Validation(
        "Pipeline.DescriptionTooLong",
        $"A descrição do pipeline deve possuir no máximo {Pipeline.MaximumDescriptionLength} caracteres.");

    public static readonly Error CreatedByTooLong = Error.Validation(
        "Pipeline.CreatedByTooLong",
        $"O autor da criação do pipeline deve possuir no máximo {Pipeline.MaximumCreatedByLength} caracteres.");

    public static readonly Error NotDraft = Error.Conflict(
        "Pipeline.NotDraft",
        "Somente um pipeline em rascunho pode ser alterado.");

    public static readonly Error AlreadyPublished = Error.Conflict(
        "Pipeline.AlreadyPublished",
        "O pipeline já foi publicado.");

    public static readonly Error StepPositionInvalid = Error.Validation(
        "Pipeline.StepPositionInvalid",
        "A posição da etapa deve ser maior que zero.");

    public static readonly Error StepPositionAlreadyExists = Error.Conflict(
        "Pipeline.StepPositionAlreadyExists",
        "Já existe uma etapa na posição informada.");

    public static readonly Error ResearchStepAlreadyExists = Error.Conflict(
        "Pipeline.ResearchStepAlreadyExists",
        "O pipeline já possui uma etapa de pesquisa.");

    public static readonly Error ScriptStepAlreadyExists = Error.Conflict(
        "Pipeline.ScriptStepAlreadyExists",
        "O pipeline já possui uma etapa de roteiro.");

    public static readonly Error ResearchStepRequired = Error.Validation(
        "Pipeline.ResearchStepRequired",
        "O pipeline deve possuir uma etapa de pesquisa.");

    public static readonly Error ScriptStepRequired = Error.Validation(
        "Pipeline.ScriptStepRequired",
        "O pipeline deve possuir uma etapa de roteiro.");

    public static readonly Error InvalidStepOrder = Error.Validation(
        "Pipeline.InvalidStepOrder",
        "A sequência deve possuir Research na posição 1 e Script na posição 2.");
}
