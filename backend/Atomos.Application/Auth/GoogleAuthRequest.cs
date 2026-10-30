namespace Atomos.Application.Auth;

public sealed class GoogleAuthRequest
{
    public string IdToken { get; init; } = string.Empty;
    public string? Username { get; init; }
}
