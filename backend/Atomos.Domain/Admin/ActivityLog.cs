using Atomos.Domain.Users;

namespace Atomos.Domain.Admin;

public sealed class ActivityLog
{
    public long IDActivityLog { get; set; }
    public int? IDUser { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? MetadataJson { get; set; }
    public User? User { get; set; }
}
