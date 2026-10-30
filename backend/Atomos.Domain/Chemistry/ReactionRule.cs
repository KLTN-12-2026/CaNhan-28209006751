namespace Atomos.Domain.Chemistry;

public sealed class ReactionRule
{
    public int IDRule { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 100;
    public bool? RequiresHeat { get; set; }
    public bool? OutcomeHasGas { get; set; }
    public bool? OutcomeHasPrecipitate { get; set; }
    public bool? OutcomeHasColorChange { get; set; }
    public bool? OutcomeIsExothermic { get; set; }
    public decimal? OutcomeTemperatureDeltaC { get; set; }
    public string? PhenomenonTemplate { get; set; }
    public string? ExplanationTemplate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<ReactionRuleSlot> Slots { get; set; } = [];
    public ICollection<ReactionRuleCondition> Conditions { get; set; } = [];
    public ICollection<ReactionRuleProduct> Products { get; set; } = [];
    public ICollection<ReactionRuleOutcome> Outcomes { get; set; } = [];
}

public sealed class ReactionRuleSlot
{
    public int IDSlot { get; set; }
    public int IDRule { get; set; }
    public string SlotKey { get; set; } = string.Empty;
    public string Role { get; set; } = "Reactant";
    public int Order { get; set; }
    public ReactionRule? Rule { get; set; }
}

public sealed class ReactionRuleCondition
{
    public int IDCondition { get; set; }
    public int IDRule { get; set; }
    public string TargetKind { get; set; } = "Slot";
    public string? TargetSlot { get; set; }
    public string? TargetProduct { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "Equals";
    public string ValueKind { get; set; } = "Literal";
    public string? ValueText { get; set; }
    public string? CompareSlot { get; set; }
    public string? CompareField { get; set; }
    public string? GroupKey { get; set; }
    public string? GroupOperator { get; set; }
    public ReactionRule? Rule { get; set; }
}

public sealed class ReactionRuleProduct
{
    public int IDProduct { get; set; }
    public int IDRule { get; set; }
    public int Order { get; set; }
    public string ProductType { get; set; } = "FixedFormula";
    public string? FixedFormula { get; set; }
    public string? CationSourceSlot { get; set; }
    public string? AnionSourceSlot { get; set; }
    public string? SourceField { get; set; }
    public int? CoefficientHint { get; set; }
    public ReactionRule? Rule { get; set; }
}

public sealed class ReactionRuleOutcome
{
    public int IDOutcome { get; set; }
    public int IDRule { get; set; }
    public int Order { get; set; }
    public string OutputField { get; set; } = string.Empty;
    public bool? BoolValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public string? TextValue { get; set; }
    public string TargetKind { get; set; } = "Product";
    public string? TargetSlot { get; set; }
    public string? TargetProduct { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "Equals";
    public string ValueKind { get; set; } = "Literal";
    public string? ValueText { get; set; }
    public string? CompareSlot { get; set; }
    public string? CompareField { get; set; }
    public ReactionRule? Rule { get; set; }
}
