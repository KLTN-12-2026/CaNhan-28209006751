namespace Atomos.Domain.Users;

public sealed class UsageQuota
{
    public int IDUser { get; set; }
    public DateTime? SandboxWindowStartedAt { get; set; }
    public int SandboxUsedSeconds { get; set; }
    public DateTime? SandboxHeartbeatAt { get; set; }
    public bool SandboxIsActive { get; set; }
    public User? User { get; set; }
}
