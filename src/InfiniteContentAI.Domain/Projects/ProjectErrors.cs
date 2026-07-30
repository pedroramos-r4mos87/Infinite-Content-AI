using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Domain.Projects;

public static class ProjectErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Project.NameRequired",
        "O nome do projeto é obrigatório.");

    public static readonly Error NameTooLong = Error.Validation(
        "Project.NameTooLong",
        $"O nome do projeto deve possuir no máximo {ProjectName.MaximumLength} caracteres.");

    public static readonly Error OrganizationRequired = Error.Validation(
        "Project.OrganizationRequired",
        "A organização do projeto é obrigatória.");

    public static readonly Error CreatedByRequired = Error.Validation(
        "Project.CreatedByRequired",
        "O autor da criação do projeto é obrigatório.");

    public static readonly Error DescriptionTooLong = Error.Validation(
        "Project.DescriptionTooLong",
        $"A descrição do projeto deve possuir no máximo {Project.MaximumDescriptionLength} caracteres.");

    public static readonly Error CreatedByTooLong = Error.Validation(
        "Project.CreatedByTooLong",
        $"O autor da criação do projeto deve possuir no máximo {Project.MaximumCreatedByLength} caracteres.");

    public static readonly Error NotFound = Error.NotFound(
        "Project.NotFound",
        "O projeto informado não foi encontrado.");
}
