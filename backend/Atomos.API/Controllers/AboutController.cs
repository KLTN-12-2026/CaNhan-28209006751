using Atomos.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atomos.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOnly")]
[Route("api/about")]
public sealed class AboutController(IApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AboutContentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await dbContext.AboutContents
            .AsNoTracking()
            .Where(item => item.IsVisible)
            .OrderBy(item => item.Order)
            .Select(item => new AboutContentResponse(
                item.IDAboutContent,
                item.Title,
                item.Content,
                item.ImageUrl,
                item.Order))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
}

public sealed record AboutContentResponse(
    int Id,
    string Title,
    string Content,
    string? ImageUrl,
    int Order);
