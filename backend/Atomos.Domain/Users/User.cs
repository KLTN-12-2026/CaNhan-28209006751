namespace Atomos.Domain.Users;

public sealed class User
{
    public int IDUser { get; set; }
    public int IDRole { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? MatKhau { get; set; }
    public string? MatKhauTruocDo { get; set; }
    public string? IDGoogle { get; set; }
    public string? HoTen { get; set; }
    public string? Avatar { get; set; }
    public DateTime? VipExpiresAt { get; set; }
    public bool DaKhoa { get; set; }
    public bool DaXoa { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public Role? Role { get; set; }
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<AuthSession> AuthSessions { get; set; } = [];
    public UsageQuota? UsageQuota { get; set; }
}
