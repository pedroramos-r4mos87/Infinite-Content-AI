using System.Security.Claims;
using InfiniteContentAI.Api.Authentication;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Domain.Organizations;
using Microsoft.AspNetCore.Http;

namespace InfiniteContentAI.Api.IntegrationTests.Authentication;

public sealed class HttpCurrentOrganizationTests
{
    [Fact]
    public void ValidClaimProducesOrganizationId()
    {
        Guid value = Guid.CreateVersion7();
        HttpCurrentOrganization currentOrganization =
            CreateCurrentOrganization(value.ToString());

        Assert.True(currentOrganization.IsAvailable);
        Assert.Equal(
            new OrganizationId(value),
            currentOrganization.OrganizationId);
        Assert.Equal(
            new OrganizationId(value),
            currentOrganization.Require().Value);
    }

    [Fact]
    public void MissingClaimProducesUnavailableContext()
    {
        HttpCurrentOrganization currentOrganization =
            CreateCurrentOrganization(claimValue: null);

        Assert.False(currentOrganization.IsAvailable);
        Assert.Null(currentOrganization.OrganizationId);
        Assert.Equal(
            IdentityErrors.OrganizationRequired,
            currentOrganization.Require().Error);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void InvalidClaimProducesUnavailableContext(string claimValue)
    {
        HttpCurrentOrganization currentOrganization =
            CreateCurrentOrganization(claimValue);

        Assert.False(currentOrganization.IsAvailable);
        Assert.Null(currentOrganization.OrganizationId);
        Assert.Equal(
            "Identity.OrganizationRequired",
            currentOrganization.Require().Error.Code);
    }

    private static HttpCurrentOrganization CreateCurrentOrganization(
        string? claimValue)
    {
        Claim[] claims = claimValue is null
            ? []
            : [new Claim(OrganizationClaimTypes.OrganizationId, claimValue)];
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "Test")),
        };
        var accessor = new HttpContextAccessor
        {
            HttpContext = context,
        };

        return new HttpCurrentOrganization(accessor);
    }
}
