namespace Atomos.Application.Lookup;

public interface ILookupQueries
{
    Task<List<ElementResponse>> GetElementsAsync(CancellationToken cancellationToken);

    Task<List<ChemicalResponse>> GetChemicalsAsync(CancellationToken cancellationToken);
}
