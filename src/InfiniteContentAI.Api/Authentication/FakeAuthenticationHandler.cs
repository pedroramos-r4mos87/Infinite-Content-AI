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
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims =
        [
            new("sub", FakeAuthenticationDefaults.Subject),
            new(
                ClaimTypes.NameIdentifier,
                FakeAuthenticationDefaults.UserId.ToString()),
            new(ClaimTypes.Name, FakeAuthenticationDefaults.Name),
            new(
                OrganizationClaimTypes.OrganizationId,
                FakeAuthenticationDefaults.OrganizationId.ToString()),
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
}
