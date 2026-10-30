using Atomos.Application.Lookup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atomos.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOnly")]
[Route("api/lookup")]
public sealed class LookupController(LookupService lookupService) : ControllerBase
{
    [HttpGet("elements")]
    public async Task<ActionResult<List<ElementResponse>>> GetElements(CancellationToken cancellationToken) =>
        Ok(await lookupService.GetElementsAsync(cancellationToken));

    [HttpGet("chemicals")]
    public async Task<ActionResult<List<ChemicalResponse>>> GetChemicals(CancellationToken cancellationToken) =>
        Ok(await lookupService.GetChemicalsAsync(cancellationToken));
}
