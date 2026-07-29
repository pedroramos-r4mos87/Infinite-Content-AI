using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Api.Authentication;

public sealed class HttpCurrentOrganization(
    IHttpContextAccessor httpContextAccessor) : ICurrentOrganization
{
    public OrganizationId? OrganizationId
    {
        get
        {
            string? claimValue = httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(OrganizationClaimTypes.OrganizationId)?
                .Value;

            return Guid.TryParse(claimValue, out Guid value) &&
                   value != Guid.Empty
                ? new OrganizationId(value)
                : null;
        }
    }

    public bool IsAvailable => OrganizationId.HasValue;

    public Result<OrganizationId> Require()
    {
        return OrganizationId is { } organizationId
            ? Result.Success(organizationId)
            : Result.Failure<OrganizationId>(
                IdentityErrors.OrganizationRequired);
    }
}
