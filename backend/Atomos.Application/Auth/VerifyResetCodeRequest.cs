namespace Atomos.Application.Auth;

public sealed class VerifyResetCodeRequest
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
