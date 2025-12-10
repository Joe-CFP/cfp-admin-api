using AdminApi.Repositories;
using AdminApi.Security;
using AdminApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public class AuthController(JwtTokenService tokens, ISecurityService security, IDatabaseRepository db, ILogger<AuthController> log) : ControllerBase
{
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        if (string.Equals(request.grant_type, "password", StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
                return BadRequest("Username and password are required.");

            if (!Admins.IsAdmin(request.username))
                return Unauthorized("Invalid credentials.");

            var member = await security.AuthenticateAsync(request.username, request.password);
            if (member is null)
                return Unauthorized("Invalid credentials.");

            string loginIp = GetServerObservedIp();
            string loginUrl = Request.Headers.Referer.ToString();
            const string referCode = "";

            try
            {
                await db.InsertLoginHistoryAsync(member.Id, loginIp, loginUrl, referCode, HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "InsertLoginHistory failed for {Username}", member.Username);
            }

            string refreshToken = await RefreshTokenService.IssueAsync(db, member.Id, HttpContext.RequestAborted);

            (string token, DateTimeOffset expiresUtc) = tokens.CreateAdminToken(member.Id, member.Username, member.FirstName, member.LastName);
            int expiresIn = (int)Math.Max(0, (expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);

            return Ok(new { access_token = token, refresh_token = refreshToken, token_type = "Bearer", expires_in = expiresIn });
        }

        if (string.Equals(request.grant_type, "refresh_token", StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(request.refresh_token))
                return BadRequest("Refresh token is required.");

            var rotated = await RefreshTokenService.RotateAsync(db, request.refresh_token, HttpContext.RequestAborted);
            if (rotated is null)
                return Unauthorized("Invalid credentials.");

            var member = await db.GetMemberSecurityRecordByIdAsync(rotated.Value.MemberId);
            if (member is null)
                return Unauthorized("Invalid credentials.");

            if (!Admins.IsAdmin(member.Username))
                return Unauthorized("Invalid credentials.");

            (string token, DateTimeOffset expiresUtc) = tokens.CreateAdminToken(member.Id, member.Username, member.FirstName, member.LastName);
            int expiresIn = (int)Math.Max(0, (expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);

            return Ok(new { access_token = token, refresh_token = rotated.Value.RefreshToken, token_type = "Bearer", expires_in = expiresIn });
        }

        return BadRequest("Unsupported grant_type.");
    }

    private string GetServerObservedIp()
    {
        string forwarded = Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            string first = forwarded.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(first)) return first;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    // ReSharper disable InconsistentNaming // The names are fixed OAuth2 fields, including case
    public sealed record TokenRequest(string? username, string? password, string? grant_type, string? refresh_token);
}
