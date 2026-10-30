namespace Atomos.Domain.Users;

public sealed class AuthSession
{
    public long IDAuthSession { get; set; }
    public int IDUser { get; set; }
    public byte[] SessionSecretHash { get; set; } = [];
    public AuthSessionMethod AuthMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokeReason { get; set; }
    public User? User { get; set; }
}

public enum AuthSessionMethod
{
    Password,
    Google
}
