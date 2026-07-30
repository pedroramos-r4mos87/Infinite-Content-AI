using InfiniteContentAI.Domain.Projects;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Projects.CreateProject;

public static class CreateProjectValidator
{
    public static Result Validate(CreateProjectCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        Result<ProjectName> name = ProjectName.Create(command.Name);
        return name.IsSuccess
            ? Result.Success()
            : Result.Failure(name.Error);
    }
}
