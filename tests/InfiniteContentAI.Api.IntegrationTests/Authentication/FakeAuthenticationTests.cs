using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using InfiniteContentAI.Api;
using InfiniteContentAI.Api.Authentication;
using InfiniteContentAI.Application.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InfiniteContentAI.Api.IntegrationTests.Authentication;

public sealed class FakeAuthenticationTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Test")]
    public async Task FakeAuthenticationWorksInAllowedEnvironment(
        string environmentName)
    {
        await using WebApplication application =
            await CreateTestApplicationAsync(environmentName);
        using HttpClient client = application.GetTestClient();

        using HttpResponseMessage response =
            await client.GetAsync("/_tests/identity");
        AuthenticationSnapshot? snapshot =
            await response.Content.ReadFromJsonAsync<AuthenticationSnapshot>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(snapshot);
        Assert.True(snapshot.IsAuthenticated);
        Assert.True(snapshot.HasOrganization);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.Subject));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.Name));
        Assert.True(Guid.TryParse(snapshot.UserId, out _));
        Assert.True(Guid.TryParse(snapshot.OrganizationClaim, out _));
        Assert.Equal(
            snapshot.OrganizationClaim,
            snapshot.CurrentOrganization);
    }

    [Fact]
    public async Task FakeOrganizationIsStableBetweenRequests()
    {
        await using WebApplication application =
            await CreateTestApplicationAsync(Environments.Development);
        using HttpClient client = application.GetTestClient();

        AuthenticationSnapshot? first = await client
            .GetFromJsonAsync<AuthenticationSnapshot>("/_tests/identity");
        AuthenticationSnapshot? second = await client
            .GetFromJsonAsync<AuthenticationSnapshot>("/_tests/identity");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.OrganizationClaim, second.OrganizationClaim);
        Assert.Equal(first.UserId, second.UserId);
    }

    [Fact]
    public void FakeAuthenticationCannotBeEnabledInProduction()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                EnvironmentName = Environments.Production,
            });

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => builder.Services.AddApiServices(
                    builder.Configuration,
                    builder.Environment));

        Assert.Contains(
            "outside Development and Test",
            exception.Message,
            StringComparison.Ordinal);
    }

    private static async Task<WebApplication> CreateTestApplicationAsync(
        string environmentName)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                EnvironmentName = environmentName,
            });
        builder.WebHost.UseTestServer();
        builder.Configuration["ConnectionStrings:Database"] =
            "Host=localhost;Database=test;Username=test;Password=test";
        builder.Services.AddApiServices(
            builder.Configuration,
            builder.Environment);

        WebApplication application = builder.Build();
        application.UseAuthentication();
        application.UseAuthorization();
        application
            .MapGet(
                "/_tests/identity",
                (
                    ClaimsPrincipal user,
                    ICurrentOrganization currentOrganization) =>
                {
                    return new AuthenticationSnapshot(
                        user.Identity?.IsAuthenticated == true,
                        user.FindFirst("sub")?.Value,
                        user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        user.Identity?.Name,
                        user.FindFirst(
                            OrganizationClaimTypes.OrganizationId)?.Value,
                        currentOrganization.OrganizationId?.ToString(),
                        currentOrganization.IsAvailable);
                })
            .RequireAuthorization();

        await application.StartAsync();
        return application;
    }

    public sealed record AuthenticationSnapshot(
        bool IsAuthenticated,
        string? Subject,
        string? UserId,
        string? Name,
        string? OrganizationClaim,
        string? CurrentOrganization,
        bool HasOrganization);
}
