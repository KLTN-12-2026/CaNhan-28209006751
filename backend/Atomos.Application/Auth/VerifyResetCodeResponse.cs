namespace Atomos.Application.Auth;

public sealed class VerifyResetCodeResponse
{
    public string Message { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
}
