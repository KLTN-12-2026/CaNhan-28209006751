namespace Atomos.Domain.Chemistry;

public sealed class Reaction
{
    public int IDReaction { get; set; }
    public string Equation { get; set; } = string.Empty;
    public string? Phenomenon { get; set; }
    public string? Explanation { get; set; }
    public bool HasPrecipitate { get; set; }
    public bool HasGas { get; set; }
    public bool HasColorChange { get; set; }
    public bool IsExothermic { get; set; }
    public string? Note { get; set; }
    public string? RuleCode { get; set; }
    public bool IsSandboxOverride { get; set; }
    public int SandboxPriority { get; set; } = 50;
    public ICollection<ReactionParticipant> Participants { get; set; } = [];
}
