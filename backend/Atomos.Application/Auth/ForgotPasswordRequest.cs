namespace Atomos.Application.Auth;

public sealed class ForgotPasswordRequest
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
