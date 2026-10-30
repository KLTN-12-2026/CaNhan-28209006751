namespace Atomos.Domain.Chemistry;

public sealed class ChemicalComponent
{
    public int IDChemical { get; set; }
    public int IDElement { get; set; }
    public int AtomCount { get; set; }
    public Chemical? Chemical { get; set; }
    public PeriodicElement? Element { get; set; }
}
