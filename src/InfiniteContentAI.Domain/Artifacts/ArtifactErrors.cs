using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Domain.Artifacts;

public static class ArtifactErrors
{
    public static readonly Error OrganizationRequired = Error.Validation(
        "Artifact.OrganizationRequired",
        "A organização do artefato é obrigatória.");

    public static readonly Error ProjectRequired = Error.Validation(
        "Artifact.ProjectRequired",
        "O projeto do artefato é obrigatório.");

    public static readonly Error ExecutionRequired = Error.Validation(
        "Artifact.ExecutionRequired",
        "A execução do pipeline é obrigatória.");

    public static readonly Error StepExecutionRequired = Error.Validation(
        "Artifact.StepExecutionRequired",
        "A execução da etapa é obrigatória.");

    public static readonly Error TypeInvalid = Error.Validation(
        "Artifact.TypeInvalid",
        "O tipo do artefato é inválido.");

    public static readonly Error ContentRequired = Error.Validation(
        "Artifact.ContentRequired",
        "O conteúdo do artefato é obrigatório.");

    public static readonly Error ContentTooLong = Error.Validation(
        "Artifact.ContentTooLong",
        $"O conteúdo do artefato deve possuir no máximo {Artifact.MaximumContentLength} caracteres.");
}
