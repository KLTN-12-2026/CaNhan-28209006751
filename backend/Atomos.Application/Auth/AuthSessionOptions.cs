namespace Atomos.Application.Auth;

public sealed class AuthSessionOptions
{
    public const string SectionName = "AuthSession";
    public int LifetimeMinutes { get; init; } = 60;

    public TimeSpan Lifetime => TimeSpan.FromMinutes(LifetimeMinutes);
}
