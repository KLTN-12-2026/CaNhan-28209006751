namespace Atomos.Application.Auth;

public sealed class LoginRequest
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
