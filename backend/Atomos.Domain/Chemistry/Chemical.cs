namespace Atomos.Domain.Chemistry;

public sealed class Chemical
{
    public int IDChemical { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public string? Type { get; set; }
    public decimal? MolarMass { get; set; }
    public string? State { get; set; }
    public string? Color { get; set; }
    public string? Properties { get; set; }
    public string? Applications { get; set; }
    public string? Description { get; set; }
    public string? SandboxCategory { get; set; }
    public bool AlwaysVisibleInSandbox { get; set; }
    public bool IsSandboxStarter { get; set; }
    public bool CanDrag { get; set; } = true;
    public decimal? DefaultPh { get; set; }
    public decimal? DefaultConcentration { get; set; }
    public string? RuleCation { get; set; }
    public int? RuleCationCharge { get; set; }
    public string? RuleAnion { get; set; }
    public int? RuleAnionCharge { get; set; }
    public int? AcidHydrogenCount { get; set; }
    public int? BaseHydroxideCount { get; set; }
    public int? MetalActivityRank { get; set; }
    public string? Solubility { get; set; }
    public string? SandboxPhenomenon { get; set; }
    public bool AllowRuleEngine { get; set; } = true;
    public ICollection<ReactionParticipant> ReactionParticipants { get; set; } = [];
    public ICollection<ChemicalOxidationState> OxidationStates { get; set; } = [];
    public ICollection<ChemicalRedoxCapability> RedoxCapabilities { get; set; } = [];
    public ICollection<ChemicalWaterReactivity> WaterReactivities { get; set; } = [];
    public ICollection<ChemicalAmphotericBehavior> AmphotericBehaviors { get; set; } = [];
    public ICollection<ChemicalAcidBehavior> AcidBehaviors { get; set; } = [];
    public ICollection<ChemicalProductFamily> ProductFamilies { get; set; } = [];
}
