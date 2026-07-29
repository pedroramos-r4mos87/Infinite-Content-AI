using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.Identity;

public interface ICurrentOrganization
{
    OrganizationId? OrganizationId { get; }

    bool IsAvailable { get; }

    Result<OrganizationId> Require();
}
