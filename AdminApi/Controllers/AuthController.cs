using AdminApi.Security;
using AdminApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public class AuthController(JwtTokenService tokens, ISecurityService passwords) : ControllerBase
{
    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest? request)
    {
        string? email = request?.email;
        string? password = request?.password;

        if (Request.HasFormContentType)
        {
            IFormCollection form = await Request.ReadFormAsync();
            email ??= form["email"].ToString();
            email = string.IsNullOrWhiteSpace(email) ? null : email;
            password ??= form["password"].ToString();
            password = string.IsNullOrWhiteSpace(password) ? null : password;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email and password are required.");

        if (!Admins.IsAllowed(email))
            return Unauthorized("Invalid credentials.");

        if (!await passwords.VerifyPasswordAsync(email, password))
            return Unauthorized("Invalid credentials.");

        (string token, DateTimeOffset expiresUtc) = tokens.CreateAdminToken(email);
        int expiresIn = (int)Math.Max(0, (expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);

        return Ok(new { access_token = token, token_type = "Bearer", expires_in = expiresIn });
    }

    public sealed record TokenRequest(string? email, string? password);
}