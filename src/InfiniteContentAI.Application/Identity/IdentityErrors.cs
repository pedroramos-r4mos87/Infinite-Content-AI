using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Identity;

public static class IdentityErrors
{
    public static readonly Error OrganizationRequired = Error.Unauthorized(
        "Identity.OrganizationRequired",
        "Não foi possível identificar a organização atual.");
}
