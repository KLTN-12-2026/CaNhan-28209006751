namespace Atomos.Domain.Chemistry;

public sealed class PeriodicElement
{
    public int IDElement { get; set; }
    public int AtomicNumber { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public decimal? AtomicMass { get; set; }
    public byte? Period { get; set; }
    public byte? Group { get; set; }
    public string? Block { get; set; }
    public string? Type { get; set; }
    public string? State { get; set; }
    public string? Color { get; set; }
    public decimal? Electronegativity { get; set; }
    public string? ElectronConfiguration { get; set; }
    public string? Description { get; set; }
    public string? Applications { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<ElementOxidationState> OxidationStates { get; set; } = [];
    public ICollection<ChemicalOxidationState> ChemicalOxidationStates { get; set; } = [];
}
