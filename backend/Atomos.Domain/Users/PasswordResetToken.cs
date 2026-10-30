namespace Atomos.Domain.Users;

public sealed class PasswordResetToken
{
    public int IDReset { get; set; }
    public int IDUser { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime HetHan { get; set; }
    public bool DaSuDung { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime NgayTao { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public User? User { get; set; }
}
