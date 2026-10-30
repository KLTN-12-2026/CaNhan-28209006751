using System.Security.Claims;
using Atomos.Application.Auth;
using Atomos.API.Security;
using Atomos.Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atomos.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    AuthSessionCookieIssuer cookieIssuer,
    IAuthSessionService sessions) : ControllerBase
{
    [HttpGet("session")]
    [AllowAnonymous]
    public ActionResult<AuthBootstrapResponse> Session()
    {
        var identity = User.Identities.FirstOrDefault(current =>
            current.IsAuthenticated
            && string.Equals(
                current.AuthenticationType,
                AtomosAuthenticationDefaults.CookieScheme,
                StringComparison.Ordinal));
        if (identity is null) return Ok(AuthBootstrapResponse.Anonymous);

        var userIdValue = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = identity.FindFirst(ClaimTypes.Name)?.Value;
        var email = identity.FindFirst(ClaimTypes.Email)?.Value;
        var role = identity.FindFirst(ClaimTypes.Role)?.Value;
        if (!int.TryParse(userIdValue, out var userId)
            || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(role))
        {
            return Ok(AuthBootstrapResponse.Anonymous);
        }

        return Ok(new AuthBootstrapResponse(
            true,
            new AuthBootstrapUser(
                userId,
                username,
                email,
                identity.FindFirst("full_name")?.Value,
                identity.FindFirst("avatar")?.Value),
            role));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var usesCookieAuthentication = User.Identities.Any(identity =>
            identity.IsAuthenticated
            && string.Equals(
                identity.AuthenticationType,
                AtomosAuthenticationDefaults.CookieScheme,
                StringComparison.Ordinal));
        if (usesCookieAuthentication)
        {
            var handle = Request.Cookies[AtomosAuthenticationDefaults.CookieName];
            if (handle is not null)
            {
                await sessions.RevokeSessionAsync(
                    handle,
                    AuthSessionRevokeReasons.Logout,
                    cancellationToken);
            }
        }

        await HttpContext.SignOutAsync(AtomosAuthenticationDefaults.CookieScheme);

        return NoContent();
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message,
                code = result.Code
            });
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message,
                code = result.Code
            });
        }

        await cookieIssuer.IssueAsync(
            HttpContext,
            result.Data!.UserId,
            AuthSessionMethod.Password,
            cancellationToken);

        return Ok(result.Data);
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<GoogleAuthResponse>> Google(
        GoogleAuthRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.GoogleAuthAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message,
                code = result.Code
            });
        }

        if (!result.Data!.RequiresUsername && result.Data.UserId is int userId)
        {
            await cookieIssuer.IssueAsync(
                HttpContext,
                userId,
                AuthSessionMethod.Google,
                cancellationToken);
        }

        return Ok(result.Data);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.ForgotPasswordAsync(
            new ForgotPasswordRequest
            {
                UsernameOrEmail = request.UsernameOrEmail,
                IpAddress = GetClientIpAddress(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            },
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message,
                code = result.Code
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyResetCodeResponse>> VerifyResetCode(
        VerifyResetCodeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.VerifyResetCodeAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message,
                code = result.Code
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message,
                code = result.Code
            });
        }

        return Ok(result.Data);
    }

    private string? GetClientIpAddress()
        => HttpContext.Connection.RemoteIpAddress?.ToString();
}

public sealed record AuthBootstrapResponse(
    bool Authenticated,
    AuthBootstrapUser? User,
    string? Role)
{
    public static AuthBootstrapResponse Anonymous { get; } = new(false, null, null);
}

public sealed record AuthBootstrapUser(
    int Id,
    string Username,
    string Email,
    string? FullName,
    string? Avatar);
