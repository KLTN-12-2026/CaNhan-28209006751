namespace Atomos.Application.Auth;

public sealed class RegisterResponse
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public string Role { get; init; } = string.Empty;
}
