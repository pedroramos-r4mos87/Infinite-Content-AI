using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace InfiniteContentAI.Api.Authentication;

internal sealed class FakeAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        options,
        logger,
        encoder)
{
    private const string TestOrganizationHeader = "X-Test-Organization-Id";
    private const string TestUserHeader = "X-Test-User-Id";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Guid organizationId = ReadGuidHeader(
            TestOrganizationHeader,
            FakeAuthenticationDefaults.OrganizationId);
        Guid userId = ReadGuidHeader(
            TestUserHeader,
            FakeAuthenticationDefaults.UserId);
        Claim[] claims =
        [
            new("sub", FakeAuthenticationDefaults.Subject),
            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),
            new(ClaimTypes.Name, FakeAuthenticationDefaults.Name),
            new(
                OrganizationClaimTypes.OrganizationId,
                organizationId.ToString()),
        ];

        var identity = new ClaimsIdentity(
            claims,
            FakeAuthenticationDefaults.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(
            principal,
            FakeAuthenticationDefaults.Scheme);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }

    private Guid ReadGuidHeader(string name, Guid fallback)
    {
        return Request.Headers.TryGetValue(name, out var values) &&
               Guid.TryParse(values.ToString(), out Guid value) &&
               value != Guid.Empty
            ? value
            : fallback;
    }
}
