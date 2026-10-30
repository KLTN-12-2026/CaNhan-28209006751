using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atomos.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class CsrfController(IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet("csrf")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ActionResult<CsrfTokenResponse> GetToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new CsrfTokenResponse(
            tokens.RequestToken
                ?? throw new InvalidOperationException("Antiforgery request token was not generated.")));
    }
}

public sealed record CsrfTokenResponse(string Token);
