namespace Atomos.Domain.Chemistry;

public sealed class ElementOxidationState
{
    public int IDState { get; set; }
    public int IDElement { get; set; }
    public int OxidationState { get; set; }
    public PeriodicElement? Element { get; set; }
}

public sealed class ChemicalOxidationState
{
    public int IDState { get; set; }
    public int IDChemical { get; set; }
    public int IDElement { get; set; }
    public int OxidationState { get; set; }
    public Chemical? Chemical { get; set; }
    public PeriodicElement? Element { get; set; }
}

public sealed class ChemicalRedoxCapability
{
    public int IDCapability { get; set; }
    public int IDChemical { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public Chemical? Chemical { get; set; }
}

public sealed class ChemicalWaterReactivity
{
    public int IDReactivity { get; set; }
    public int IDChemical { get; set; }
    public string WaterPhase { get; set; } = string.Empty;
    public decimal? MinTemperatureC { get; set; }
    public decimal? MaxTemperatureC { get; set; }
    public string ProductFamily { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Chemical? Chemical { get; set; }
}

public sealed class ChemicalAmphotericBehavior
{
    public int IDBehavior { get; set; }
    public int IDChemical { get; set; }
    public string ChemicalForm { get; set; } = string.Empty;
    public string Medium { get; set; } = string.Empty;
    public bool RequiresWater { get; set; }
    public string ProductFamily { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Chemical? Chemical { get; set; }
}

public sealed class ChemicalAcidBehavior
{
    public int IDBehavior { get; set; }
    public int IDChemical { get; set; }
    public string AcidKind { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public string? ReductionProduct { get; set; }
    public Chemical? Chemical { get; set; }
}

public sealed class ChemicalProductFamily
{
    public int IDProductFamily { get; set; }
    public int IDChemical { get; set; }
    public string ProductFamily { get; set; } = string.Empty;
    public Chemical? Chemical { get; set; }
}
