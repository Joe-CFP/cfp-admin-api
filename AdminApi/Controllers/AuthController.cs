using AdminApi.Security;
using AdminApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public class AuthController(JwtTokenService tokens, ISecurityService security) : ControllerBase
{
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        if (!string.Equals(request.grant_type, "password", StringComparison.Ordinal))
            return BadRequest("Unsupported grant_type.");

        if (string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
            return BadRequest("Username and password are required.");

        if (!Admins.IsAdmin(request.username))
            return Unauthorized("Invalid credentials.");

        if (!await security.VerifyPasswordAsync(request.username, request.password))
            return Unauthorized("Invalid credentials.");

        (string token, DateTimeOffset expiresUtc) = tokens.CreateAdminToken(request.username);
        int expiresIn = (int)Math.Max(0, (expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);

        return Ok(new { access_token = token, token_type = "Bearer", expires_in = expiresIn });
    }

    public sealed record TokenRequest(string? username, string? password, string? grant_type, string? ip);
}