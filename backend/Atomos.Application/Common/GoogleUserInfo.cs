namespace Atomos.Application.Common;

public sealed class GoogleUserInfo
{
    public string GoogleId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public string? Avatar { get; init; }
}
