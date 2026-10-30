using Atomos.Domain.Chemistry;
using Atomos.Domain.Users;

namespace Atomos.Domain.Challenges;

public sealed class WorkshopChallenge
{
    public int IDChallenge { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string Difficulty { get; set; } = "De";
    public DateTime? ChallengeDate { get; set; }
    public int? DailySlot { get; set; }
    public string? Seed { get; set; }
    public string? GeneratorVersion { get; set; }
    public string? MetadataJson { get; set; }
    public string Source { get; set; } = "AdminLegacy";
    public int TimeLimitSeconds { get; set; } = 300;
    public int TargetChemicalId { get; set; }
    public Chemical? TargetChemical { get; set; }
    public decimal TargetAmount { get; set; }
    public string TargetUnit { get; set; } = "g";
    public int BaseScore { get; set; } = 100;
    public int EarlyBonusScore { get; set; }
    public int EarlyBonusThresholdSeconds { get; set; }
    public int LatePenaltyScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<WorkshopChallengeResource> Resources { get; set; } = [];
    public ICollection<WorkshopChallengeSession> Sessions { get; set; } = [];
}

public sealed class WorkshopChallengeResource
{
    public int IDResource { get; set; }
    public int IDChallenge { get; set; }
    public WorkshopChallenge? Challenge { get; set; }
    public int ChemicalId { get; set; }
    public Chemical? Chemical { get; set; }
    public decimal Amount { get; set; }
    public string Unit { get; set; } = "g";
    public int Order { get; set; }
}

public sealed class WorkshopChallengeSession
{
    public int IDSession { get; set; }
    public int IDChallenge { get; set; }
    public WorkshopChallenge? Challenge { get; set; }
    public int IDUser { get; set; }
    public User? User { get; set; }
    public string Status { get; set; } = "Running";
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InventoryJson { get; set; } = "[]";
    public decimal? TargetAmountAchieved { get; set; }
    public bool TargetAchieved { get; set; }
    public int ReactionCount { get; set; }
    public int MistakeCount { get; set; }
    public string SuspicionStatus { get; set; } = "Normal";
    public string EventHistoryJson { get; set; } = "[]";
    public int BaseScoreAwarded { get; set; }
    public int EarlyBonusAwarded { get; set; }
    public int LatePenaltyApplied { get; set; }
    public int FinalScore { get; set; }
    public int? DurationSeconds { get; set; }
    public string? FailureReason { get; set; }
}
