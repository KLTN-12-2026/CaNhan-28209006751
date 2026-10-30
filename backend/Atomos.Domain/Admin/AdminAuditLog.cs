namespace Atomos.Domain.Admin;

public sealed class AdminAuditLog
{
    public long IDAuditLog { get; set; }
    public int IDAdmin { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int? TargetId { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedAt { get; set; }
}
