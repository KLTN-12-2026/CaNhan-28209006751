namespace Atomos.Application.Auth;

public sealed class ResetPasswordRequest
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
