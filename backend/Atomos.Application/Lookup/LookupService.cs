namespace Atomos.Application.Lookup;

public sealed class LookupService(ILookupQueries lookupQueries)
{
    public async Task<List<ElementResponse>> GetElementsAsync(CancellationToken cancellationToken) =>
        await lookupQueries.GetElementsAsync(cancellationToken);

    public async Task<List<ChemicalResponse>> GetChemicalsAsync(CancellationToken cancellationToken) =>
        await lookupQueries.GetChemicalsAsync(cancellationToken);
}

public sealed record ElementResponse(
    int Id,
    int AtomicNumber,
    string Symbol,
    string Name,
    string? EnglishName,
    decimal? AtomicMass,
    byte? Period,
    byte? Group,
    string? Block,
    string? Type,
    string? State,
    string? Color,
    decimal? Electronegativity,
    string? ElectronConfiguration,
    string? Description,
    string? Applications);

public sealed record ChemicalResponse(
    int Id,
    string Name,
    string Formula,
    string? Type,
    decimal? MolarMass,
    string? State,
    string? Color,
    decimal? DefaultPh,
    decimal? DefaultConcentration,
    string? Solubility,
    string? Properties,
    string? Applications,
    string? Description,
    string? Phenomenon);
