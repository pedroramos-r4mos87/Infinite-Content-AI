namespace InfiniteContentAI.Api.Authentication;

internal static class FakeAuthenticationDefaults
{
    public const string Scheme = "Fake";
    public const string Subject = "development-user";
    public const string Name = "Development User";

    public static readonly Guid UserId =
        Guid.Parse("019c0000-0000-7000-8000-000000000001");

    public static readonly Guid OrganizationId =
        Guid.Parse("019c0000-0000-7000-8000-000000000002");
}
